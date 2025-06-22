using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public enum State
    {
        Idle,
        Frying,
        Burning,
        Burnt,
    }

    private State state;

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private float fryingTimer;
    private float burningTimer;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;
    
    private void Start()
    {
        state = State.Idle;
    }
    private void Update()
    {
        if (!HasKitchenObject())
        {
            return;
        }

        switch (state)
        {
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;
                SetProgress(fryingTimer, fryingRecipeSO.fryingTimerMax);
                if (fryingTimer >= fryingRecipeSO.fryingTimerMax)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject friedKitchenObject = KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                    state = State.Burning;
                    burningTimer = 0;
                    burningRecipeSO = GetBurningRecipeSOFromInput(friedKitchenObject.GetKitchenObjectSO());
                }

                break;
            case State.Burning:
                burningTimer += Time.deltaTime;
                SetProgress(burningTimer, burningRecipeSO.burningTimerMax);
                if (burningTimer >= burningRecipeSO.burningTimerMax)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject burntKitchenObject = KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                    state = State.Burnt;
                }
                break;
            case State.Burnt: 
                break;
        }

    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {

            if (TryGetFryingRecipeSOFromInput(player.GetKitchenObject().GetKitchenObjectSO(),
                out FryingRecipeSO FryingRecipeSO) && !HasKitchenObject())
            {
                fryingRecipeSO = FryingRecipeSO;
                player.GetKitchenObject().SetKitchenObjectParent(this);
                state = State.Frying;
                fryingTimer = 0;
            }
        }
        else
        {
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                fryingRecipeSO = null;
                burningRecipeSO = null;
                state = State.Idle;
                ResetProgress();
            }
        }
    }

    private void ResetProgress()
    {
        fryingTimer = 0f;
        burningTimer = 0f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = 0
        });
    }
    private void SetProgress(float timer, float timerMax)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = timer/timerMax,
        });
    }

    private FryingRecipeSO GetFryingRecipeSOFromInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (FryingRecipeSO recipe in fryingRecipeSOArray)
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
        FryingRecipeSO FryingRecipeSO = GetFryingRecipeSOFromInput(kitchenObjectSO);
        return (FryingRecipeSO != null) ? FryingRecipeSO.output : null;
    }

    private bool HasOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        return GetFryingRecipeSOFromInput(kitchenObjectSO) != null;
    }

    private bool TryGetFryingRecipeSOFromInput(KitchenObjectSO kitchenObjectSO, out FryingRecipeSO FryingRecipeSO)
    {
        FryingRecipeSO = GetFryingRecipeSOFromInput(kitchenObjectSO);
        return FryingRecipeSO != null;
    }

    private BurningRecipeSO GetBurningRecipeSOFromInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (BurningRecipeSO recipe in burningRecipeSOArray)
        {
            if (kitchenObjectSO == recipe.input)
            {
                return recipe;
            }
        }
        return null;
    }
}
