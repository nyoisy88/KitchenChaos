using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public static event EventHandler OnAnyCut;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cutProgress;
    private CuttingRecipeSO currentCuttingRecipeSO;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {

            if (!HasKitchenObject())
            {
                if (TryGetCuttingRecipeSOFromInput(player.GetKitchenObject().GetKitchenObjectSO(),
                    out CuttingRecipeSO cuttingRecipeSO))
                {
                    currentCuttingRecipeSO = cuttingRecipeSO;
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                }

                return;
            }

            KitchenObject playerKO = player.GetKitchenObject();
            KitchenObject thisKO = GetKitchenObject();

            // Case 1: Player is holding a plate
            if (playerKO.TryGetPlate(out PlateKitchenObject playerPlate))
            {
                if (playerPlate.TryAddIngredient(thisKO.GetKitchenObjectSO()))
                {
                    thisKO.DestroySelf();
                    ResetProgress();
                }
                return;
            }

            // Case 2: This counter has a plate
            if (thisKO.TryGetPlate(out PlateKitchenObject counterPlate))
            {
                if (counterPlate.TryAddIngredient(playerKO.GetKitchenObjectSO()))
                {
                    playerKO.DestroySelf();
                    ResetProgress();
                }
                return;
            }
        }
        else
        {
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                currentCuttingRecipeSO = null;
                ResetProgress();
            }
        }
    }

    public override void InteractAlt()
    {
        if (HasKitchenObject() && currentCuttingRecipeSO != null)
        {

            cutProgress++;

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
            {
                progressNormalized = (float)cutProgress / currentCuttingRecipeSO.cuttingNumberMax
            });
            if (cutProgress >= currentCuttingRecipeSO.cuttingNumberMax)
            {
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(currentCuttingRecipeSO.output, this);
                currentCuttingRecipeSO = null;
            }

        }
    }

    private void ResetProgress()
    {
        cutProgress = 0;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = 0
        });
    }

    private CuttingRecipeSO GetCuttingRecipeSOFromInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (CuttingRecipeSO recipe in cuttingRecipeSOArray)
        {
            if (kitchenObjectSO == recipe.input)
            {
                return recipe;
            }
        }
        return null;
    }

    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOFromInput(kitchenObjectSO);
        return (cuttingRecipeSO != null) ? cuttingRecipeSO.output : null;
    }

    private bool HasOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        return GetCuttingRecipeSOFromInput(kitchenObjectSO) != null;
    }

    private bool TryGetCuttingRecipeSOFromInput(KitchenObjectSO kitchenObjectSO, out CuttingRecipeSO cuttingRecipeSO)
    {
        cuttingRecipeSO = GetCuttingRecipeSOFromInput(kitchenObjectSO);
        return cuttingRecipeSO != null;
    }
}
