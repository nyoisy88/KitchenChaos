using System;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    public event EventHandler OnPlateAdded;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float plateSpawnTimer;
    private float plateSpawnTimerMax = 2f;
    private int spawnPlateCount;
    private int spawnPlateCountMax = 4;


    private void Update()
    {
        if (!IsServer) return;
        if (!KitchenGameManager.Instance.IsGamePlaying() || spawnPlateCount >= spawnPlateCountMax)
        {
            return;
        }

        plateSpawnTimer += Time.deltaTime;
        if (plateSpawnTimer >= plateSpawnTimerMax)
        {
            plateSpawnTimer = 0;
            
            SpawnPlateServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        spawnPlateCount++;
        OnPlateAdded?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject() && spawnPlateCount > 0)
        {
            InteractLogicServerRpc(player.GetNetworkObject());
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc(NetworkObjectReference playerNetworkObjectRef)
    {
        if (playerNetworkObjectRef.TryGet(out NetworkObject playerNetworkObject))
        {
            Player player = playerNetworkObject.GetComponent<Player>();
            if (!player.HasKitchenObject())
            {
                InteractLogicClientRpc();
            }
        }
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        spawnPlateCount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
