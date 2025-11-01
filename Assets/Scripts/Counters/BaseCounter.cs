using System;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectDroppedHere;
    public static void ResetStaticData()
    {
        OnAnyObjectDroppedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;
    private KitchenObject _kitchenObject;
    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseCounter.Interact()");
    }

    public virtual void InteractAlt()
    {
        Debug.Log("BaseCounter.InteractAlt()");
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;

        if (_kitchenObject != null)
        {
            OnAnyObjectDroppedHere?.Invoke(this, EventArgs.Empty);
        }

    }
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public bool HasKitchenObject()
    {
        return _kitchenObject != null;
    }

    public KitchenObject GetKitchenObject()
    {
        return _kitchenObject;
    }

    public void ClearKitchenObject()
    {
        _kitchenObject = null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
