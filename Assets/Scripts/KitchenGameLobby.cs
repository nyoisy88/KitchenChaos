using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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
        if (joinedLobby != null || !AuthenticationService.Instance.IsSignedIn)
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

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.MAX_PLAYER_COUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

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
