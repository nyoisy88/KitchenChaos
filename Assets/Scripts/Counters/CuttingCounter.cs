using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public static event EventHandler OnAnyCut;

    public static new void ResetStaticData()
    {
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cutProgress;
    private CuttingRecipeSO cuttingRecipeSO;

    public override void Interact(Player player)
    {
        // Player is holding Kitchen Object
        if (player.HasKitchenObject())
        {
            // Counter is empty
            if (!HasKitchenObject())
            {
                // Check if the Kitchen Object can be cut
                if (HasOutputFromInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParentRpc(this);
                    InteractLogicObjectTransferServerRpc();
                }

                return;
            }

            KitchenObject playerKO = player.GetKitchenObject();
            KitchenObject thisKO = GetKitchenObject();

            // Player is holding a plate and counter has Kitchen Object
            if (playerKO.TryGetPlate(out PlateKitchenObject playerPlateKO))
            {
                if (playerPlateKO.TryAddIngredientRpc(thisKO.GetKitchenObjectSO()))
                {
                    thisKO.DestroySelf();
                }
                return;
            }

        }
        else
        {
            // Player is not holding anything and counter has Kitchen Object
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParentRpc(player);

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicObjectTransferServerRpc()
    {
        InteractLogicObjectTransferClientRpc();
    }

    [ClientRpc]
    private void InteractLogicObjectTransferClientRpc()
    {
        cutProgress = 0;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = 0
        });
    }

    public override void InteractAlt()
    {
        if (HasKitchenObject() && HasOutputFromInput(GetKitchenObject().GetKitchenObjectSO()))
        {

            CutObjectServerRpc();
            TestCuttingObjectProgressServerRpc();

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cutProgress++;

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
        //Debug.Log(OnAnyCut.GetInvocationList().Length + " subscribers to OnAnyCut");
        cuttingRecipeSO = GetCuttingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = (float)cutProgress / cuttingRecipeSO.cuttingNumberMax
        });
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingObjectProgressServerRpc()
    {
        if (cutProgress >= cuttingRecipeSO.cuttingNumberMax)
        {
            GetKitchenObject().DestroySelf();
            KitchenObject.SpawnKitchenObject(cuttingRecipeSO.output, this);
        }
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
