using UnityEngine;

public class ClearCounter : MonoBehaviour
{
    [SerializeField] private Transform kitchenObjectPrefab;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private bool testing = false;
    [SerializeField] private ClearCounter secondClearCounter;

    private KitchenObject _kitchenObject;

    private void Update()
    {
        if (testing && Input.GetKeyDown(KeyCode.T))
        {
            if (_kitchenObject != null)
            {
                _kitchenObject.SetKitchenObjectParent(secondClearCounter);
            }
        }
    }

    public void Interact()
    {
        if (_kitchenObject == null)
        {
            Transform kitchenObjectTransform = Instantiate(kitchenObjectPrefab, counterTopPoint);
            kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(this);
        }
        else
        {
            Debug.Log(_kitchenObject.gameObject);
        }
        
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;

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
}
