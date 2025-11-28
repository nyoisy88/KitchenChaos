using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public const int MAX_PLAYER_COUNT = 4;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static KitchenGameMultiplayer Instance { get; private set; }
    public string PlayerName
    {
        get => playerName;
        set
        {
            playerName = value;
            PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsMultiplayerMode;
    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnPlayerColorChanged;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;
    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "Player" + UnityEngine.Random.Range(1000, 9999).ToString());
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;

    }

    private void Start()
    {
        if (!IsMultiplayerMode)
        {
            StartHost();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_Server_OnConnectionEvent;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnConnectionEvent(NetworkManager arg1, ConnectionEventData connectionEventData)
    {
        switch (connectionEventData.EventType)
        {
            case ConnectionEvent.ClientConnected:
                playerDataNetworkList.Add(new PlayerData()
                {
                    clientId = connectionEventData.ClientId,
                    colorIndex = GetFirstUnusedColor(),
                    playerName = (IsHost? playerName : ""),
                    playerId = IsHost? AuthenticationService.Instance.PlayerId : ""
                });
                Debug.Log($"Client connected: {connectionEventData.ClientId}");
                break;
            case ConnectionEvent.ClientDisconnected:
                if (!NetworkManager.Singleton.IsConnectedClient)
                {
                    return;
                }
                foreach (PlayerData playerData in playerDataNetworkList)
                {
                    if (playerData.clientId == connectionEventData.ClientId)
                    {
                        playerDataNetworkList.Remove(playerData);
                        break;
                    }
                }
                Debug.Log($"Client disconnected: {connectionEventData.ClientId}");
                break;
        }
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (request.ClientNetworkId == OwnerClientId)
        {
            // Host connection — always approved
            response.Approved = true;
            return;
        }
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started!";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= MAX_PLAYER_COUNT)
        {
            response.Approved = false;
            response.Reason = "Server is full!";
            return;
        }
        response.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_Client_OnConnectionEvent;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                SetPlayerNameServerRpc(playerName);
                SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
                Debug.Log($"Client connected to server: {data.ClientId}");
                break;
            case ConnectionEvent.ClientDisconnected:
                OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
                Debug.Log($"Client disconnected from server: {data.ClientId}");
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        if (playerDataIndex != -1)
        {
            PlayerData playerData = playerDataNetworkList[playerDataIndex];
            playerData.playerName = playerName;
            playerDataNetworkList[playerDataIndex] = playerData;
        }
        Debug.Log($"Set playerName for clientId {clientId} to {playerName}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        if (playerDataIndex != -1)
        {
            PlayerData playerData = playerDataNetworkList[playerDataIndex];
            playerData.playerId = playerId;
            playerDataNetworkList[playerDataIndex] = playerData;
        }
        Debug.Log($"Set playerId for clientId {clientId} to {playerId}");
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        int kitchenObjectSOIndex = GetKitchenObjectSOIndex(kitchenObjectSO);
        SpawnKitchenObjectServerRpc(kitchenObjectSOIndex, kitchenObjectParent.GetNetworkObject());

    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectListSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        KitchenObjectSO kitchenObjectSO = kitchenObjectListSO.kitchenObjectSOList[kitchenObjectListSOIndex];

        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObjectRef.TryGet(out NetworkObject kitchenObjectParentNetworkObject) ?
    kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>() : null;

        if (kitchenObjectParent.HasKitchenObject())
        {
            // Parent already has a kitchen object
            return;
        }

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);

        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);


        KitchenObject spawnKitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        spawnKitchenObject.SetKitchenObjectParentRpc(kitchenObjectParent);
    }

    public void ChangePlayerColorRpc(int colorIndex)
    {
        ChangePlayerColorServerRpc(colorIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!IsColorAvailable(colorIndex))
        {
            return;
        }
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                PlayerData playerData = playerDataNetworkList[i];
                playerData.colorIndex = colorIndex;
                playerDataNetworkList[i] = playerData;
                break;
            }
        }
        OnPlayerColorChanged?.Invoke(this, EventArgs.Empty);
    }

    public void KickPlayer(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogError("Can't kick self!");
            return;
        }
        NetworkManager.Singleton.DisconnectClient(clientId);
        Debug.Log($"Kicked client: {clientId}, number of remaining client: {NetworkManager.Singleton.ConnectedClientsList.Count}");
        NetworkManager_Server_OnConnectionEvent(NetworkManager.Singleton, new ConnectionEventData()
        {
            ClientId = clientId,
            EventType = ConnectionEvent.ClientDisconnected
        });
    }


    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerDataNetworkList.Count > playerIndex;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    public int GetPlayerIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsColorAvailable(int colorIndex)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.colorIndex == colorIndex)
            {
                return false;
            }
        }
        return true;
    }

    public int GetFirstUnusedColor()
    {
        for (int i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
}
