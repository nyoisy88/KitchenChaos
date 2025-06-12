using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKitchenObjectParent
{
    public void SetKitchenObject(KitchenObject kitchenObject);
    public Transform GetKitchenObjectFollowTransform();
    public bool HasKitchenObject();
    public KitchenObject GetKitchenObject();
    public void ClearKitchenObject();
}
