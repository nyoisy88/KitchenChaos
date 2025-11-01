using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
    public void SetKitchenObject(KitchenObject kitchenObject);
    public Transform GetKitchenObjectFollowTransform();
    public bool HasKitchenObject();
    public KitchenObject GetKitchenObject();
    public void ClearKitchenObject();
    public NetworkObject GetNetworkObject();
}
