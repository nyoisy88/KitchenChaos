using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public event EventHandler<OnIngredientChangedEventArgs> OnIngredientChanged;
    public class OnIngredientChangedEventArgs : EventArgs
    {
        public List<KitchenObjectSO> KitchenObjectSOList;
    }

    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO KitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    private List<KitchenObjectSO> kitchenObjectSOList;

    protected override void Awake()
    {
        base.Awake();
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO ingredient)
    {
        if (!validKitchenObjectSOList.Contains(ingredient))
        {
            return false;
        }
        if (!kitchenObjectSOList.Contains(ingredient))
        {
            kitchenObjectSOList.Add(ingredient);

            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
            {
                KitchenObjectSO = ingredient,
            });

            OnIngredientChanged?.Invoke(this, new OnIngredientChangedEventArgs
            {
                KitchenObjectSOList = kitchenObjectSOList,
            });
            return true;
        }
        return false;
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}
