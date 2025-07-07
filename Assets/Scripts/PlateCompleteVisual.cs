using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;

    [Serializable]
    public struct GameObject_KitchenObjectSO {
        public GameObject GameObject;
        public KitchenObjectSO KitchenObjectSO;
    }

    [SerializeField] private List<GameObject_KitchenObjectSO> gameObject_KitchenObjectSOList;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;

        foreach (GameObject_KitchenObjectSO gameObject_KitchenObjectSO in gameObject_KitchenObjectSOList)
        {
            gameObject_KitchenObjectSO.GameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (GameObject_KitchenObjectSO gameObject_KitchenObjectSO in gameObject_KitchenObjectSOList)
        {
            if (gameObject_KitchenObjectSO.KitchenObjectSO == e.KitchenObjectSO)
            {
                gameObject_KitchenObjectSO.GameObject.SetActive(true);
                return;
            }
        }
    }
}
