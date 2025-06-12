using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && HasOutputFromInput(player.GetKitchenObject().GetKitchenObjectSO()))
        {
            if (!HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
        else
        {
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    public override void InteractAlt()
    {
        if (HasKitchenObject() && TryGetOutputForInput(GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO inputKitchenObjectSO))
        {
            GetKitchenObject().DestroySelf();
            KitchenObjectSO output = GetOutputFromInput(inputKitchenObjectSO);
            KitchenObject.SpawnKitchenObject(output, this);
        }
    }

    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (CuttingRecipeSO recipe in cuttingRecipeSOArray)
        {
            if (kitchenObjectSO == recipe.input)
            {
                return recipe.output;
            }
        }
        return null;
    }

    private bool HasOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        return GetOutputFromInput(kitchenObjectSO) != null;
    }

    private bool TryGetOutputForInput(KitchenObjectSO kitchenObjectSO, out KitchenObjectSO output)
    {
        foreach (CuttingRecipeSO recipe in cuttingRecipeSOArray)
        {
            if (kitchenObjectSO == recipe.input)
            {
                output = recipe.output;
                return true;
            }
        }

        output = null;
        return false;
    }
}
