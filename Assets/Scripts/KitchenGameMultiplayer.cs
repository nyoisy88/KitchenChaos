using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    private const int MAX_PLAYER_COUNT = 4;
    public static KitchenGameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnPlayerColorChanged;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;

    private void Awake()
    {
        Instance = this;
        playerDataNetworkList = new NetworkList<PlayerData>();

        playerDataNetworkList.OnListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;

        DontDestroyOnLoad(gameObject);
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager arg1, ConnectionEventData connectionEventData)
    {
        switch (connectionEventData.EventType)
        {
            case ConnectionEvent.ClientConnected:
                playerDataNetworkList.Add(new PlayerData()
                {
                    clientId = connectionEventData.ClientId,
                    colorIndex = GetFirstUnusedColor()
                });
                Debug.Log($"Client connected: {connectionEventData.ClientId}");
                break;
            case ConnectionEvent.ClientDisconnected:
                foreach (PlayerData playerData in playerDataNetworkList)
                {
                    if (playerData.clientId == connectionEventData.ClientId)
                    {
                        playerDataNetworkList.Remove(playerData);
                        Debug.Log($"Client disconnected: {connectionEventData.ClientId}");
                        break;
                    }
                }
                break;
        }
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
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
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
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
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);

        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);

        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObjectRef.TryGet(out NetworkObject kitchenObjectParentNetworkObject) ?
            kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>() : null;

        KitchenObject spawnKitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        spawnKitchenObject.SetKitchenObjectParentRpc(kitchenObjectParent);
    }

    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
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

    [ClientRpc]
    private void ChangePlayerColorClientRpc(int colorIndex, ulong clientId)
    {

    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        Debug.Log($"Kicked client: {clientId}, number of remaining client: {NetworkManager.Singleton.ConnectedClientsList.Count}");
        NetworkManager_OnConnectionEvent(NetworkManager.Singleton, new ConnectionEventData()
        {
            ClientId = clientId,
            EventType = ConnectionEvent.ClientDisconnected
        });
    }
}
