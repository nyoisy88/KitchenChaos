using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ClearCounter : MonoBehaviour
{
    [SerializeField] private Transform kitchenObjectPrefab;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private bool testing = false;
    [SerializeField] private ClearCounter clearCounter2;

    private KitchenObject _kitchenObject;

    private void Update()
    {
        if (testing && clearCounter2)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _kitchenObject.SetKitchenObjectParent(clearCounter2);
            }
        }
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;
        
    }

    public void Interact()
    {
        Debug.Log("Interact");
        if (_kitchenObject)
        {
            return;
        }
        Transform kitchenObjectTransform = Instantiate(kitchenObjectPrefab, counterTopPoint);
        kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(this);
    }

    public Transform GetKitchenObjectPoint()
    {
        return counterTopPoint;
    }

    public bool HasKitchenObject()
    {
        return _kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return _kitchenObject;
    }

    public void ClearKitchenObject()
    {
        _kitchenObject = null;
    }
}
