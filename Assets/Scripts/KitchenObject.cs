using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent _kitchenObjectParent;
    private FollowTransform _followTranform;

    protected virtual void Awake()
    {
        _followTranform = GetComponent<FollowTransform>();
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }


    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParentRpc(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectRef);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObjectRef.TryGet(out NetworkObject kitchenObjectParentNetworkObject) ?
            kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>() : null;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("This kitchenObjectParent already has kitchen object!");
            return;
        }

        if (_kitchenObjectParent != null)
        {
            _kitchenObjectParent.ClearKitchenObject();
        }
        _kitchenObjectParent = kitchenObjectParent;
        _kitchenObjectParent.SetKitchenObject(this);
        _followTranform.SetTargetTransform(_kitchenObjectParent.GetKitchenObjectFollowTransform());
        //transform.parent = _kitchenObjectParent.GetKitchenObjectFollowTransform();
        //transform.localPosition = Vector3.zero;
    }

    public void DestroySelf()
    {
        DestroySelfServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroySelfServerRpc()
    {
        if (this == null)
        {
            return;
        }
        ClearKitchenObjectOnParentClientRpc();
        this.NetworkObject.Despawn();
    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc()
    {
        _kitchenObjectParent.ClearKitchenObject();
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        plateKitchenObject = null;
        return false;
    }
}
