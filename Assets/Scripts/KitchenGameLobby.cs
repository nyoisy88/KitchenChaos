using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinWithCodeFailed;
    public event EventHandler<LobbyListChangedEventArgs> OnLobbyListChanged;
    public class LobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> LobbyList;
    }


    private const string RELAY_JOIN_CODE = "RelayJoinCode";
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyRefreshTimer;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeUnityAuthentication();
    }


    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicLobbyRefresh();
    }

    private void HandlePeriodicLobbyRefresh()
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.LobbyScene.ToString() 
            || joinedLobby != null || !AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

        lobbyRefreshTimer -= Time.deltaTime;
        if (lobbyRefreshTimer < 0f)
        {
            float lobbyRefreshTimerMax = 3f;
            lobbyRefreshTimer = lobbyRefreshTimerMax;
            LobbiesList();
        }
    }

    private void HandleHeartbeat()
    {
        if (!IsLobbyHost())
        {
            return;
        }
        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer < 0f)
        {
            float heartbeatTimerMax = 15f;
            heartbeatTimer = heartbeatTimerMax;
            LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && AuthenticationService.Instance.PlayerId == joinedLobby.HostId;
    }

    private async void InitializeUnityAuthentication()
    {
        // Only initialize if not already initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(0, 1000).ToString());
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }
    }

    public async void LobbiesList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
            OnLobbyListChanged?.Invoke(this, new LobbyListChangedEventArgs { LobbyList = queryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e}");
        }
    }

    private async Task<Allocation> AllocateRelay() {
        try { 
            Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(KitchenGameMultiplayer.MAX_PLAYER_COUNT - 1);
            return allocation;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to allocate relay: {e}");
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation) {
        try {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to get relay join code: {e}");
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string relayJoinCode) {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to join relay: {e}");
            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.MAX_PLAYER_COUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        RELAY_JOIN_CODE, new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: relayJoinCode)
                    }
                }
            });
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                allocation,
                "dtls"
            ));


            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e}");

            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);

        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data[RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation,
                "dtls"
            ));


            KitchenGameMultiplayer.Instance.StartClient();
            // No need to load scene here, client will be moved to CharacterSelectScene by server
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to quick join lobby: {e}");

            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinLobbyWithCode(string joinCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);

            string relayJoinCode = joinedLobby.Data[RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation,
                "dtls"
            ));

            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby with code: {e}");
            OnJoinWithCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = joinedLobby.Data[RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation,
                "dtls"
            ));

            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby with code: {e}");
            OnJoinWithCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void DeleteLobby()
    {
        if (!IsLobbyHost())
        {
            return;
        }
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to delete lobby: {e}");
            return;
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby == null)
        {
            return;
        }
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to leave lobby: {e}");
            return;
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (!IsLobbyHost() || playerId == joinedLobby.HostId)
        {
            return;
        }
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to kick player: {e}");
            return;
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }
}
