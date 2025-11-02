using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State State;
    }

    public enum State
    {
        Idle,
        Frying,
        Burning,
        Burnt,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;


    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            State = newValue
        });
        if (state.Value == State.Idle || state.Value == State.Burnt)
        {
            ResetProgress();
        }
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningTimer != null ? burningRecipeSO.burningTimerMax : 1f;
        SetProgress(newValue, burningTimerMax);
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;
        SetProgress(newValue, fryingTimerMax);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (!HasKitchenObject())
        {
            return;
        }

        switch (state.Value)
        {
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer.Value += Time.deltaTime;
                if (fryingTimer.Value >= fryingRecipeSO.fryingTimerMax)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                    burningTimer.Value = 0f;
                    SetBurningRecipeSOClientRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));
                    state.Value = State.Burning;
                }

                break;
            case State.Burning:
                burningTimer.Value += Time.deltaTime;
                if (burningTimer.Value >= burningRecipeSO.burningTimerMax)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                    state.Value = State.Burnt;
                }
                break;
            case State.Burnt:
                break;
        }

    }

    public override void Interact(Player player)
    {
        // Player is holding Kitchen Object
        if (player.HasKitchenObject())
        {
            // Counter is empty
            if (!HasKitchenObject())
            {
                if (HasFryingRecipe(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParentRpc(this);
                    InteractLogicObjectTransferServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                        );
                }
                return;
            }

            KitchenObject playerKO = player.GetKitchenObject();
            KitchenObject thisKO = GetKitchenObject();

            // Player is holding a plate and counter has Kitchen Object
            if (playerKO.TryGetPlate(out PlateKitchenObject playerPlate))
            {
                if (playerPlate.TryAddIngredientRpc(thisKO.GetKitchenObjectSO()))
                {
                    thisKO.DestroySelf();

                    InteractResetStateServerRpc();
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

                InteractResetStateServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractResetStateServerRpc()
    {
        fryingTimer.Value = 0f;
        burningTimer.Value = 0f;
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicObjectTransferServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0f;
        state.Value = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        fryingRecipeSO = GetFryingRecipeSOFromInput(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOFromInput(kitchenObjectSO);
    }

    private void ResetProgress()
    {
        SetProgress(0f, 1f);
    }

    private void SetProgress(float timer, float timerMax)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = timer / timerMax,
        });
    }

    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        FryingRecipeSO FryingRecipeSO = GetFryingRecipeSOFromInput(kitchenObjectSO);
        return (FryingRecipeSO != null) ? FryingRecipeSO.output : null;
    }

    private bool HasFryingRecipe(KitchenObjectSO kitchenObjectSO)
    {
        return GetFryingRecipeSOFromInput(kitchenObjectSO) != null;
    }

    private bool TryGetFryingRecipeSOFromInput(KitchenObjectSO kitchenObjectSO, out FryingRecipeSO FryingRecipeSO)
    {
        FryingRecipeSO = GetFryingRecipeSOFromInput(kitchenObjectSO);
        return FryingRecipeSO != null;
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

    public bool IsBurning()
    {
        return state.Value == State.Burning;
    }
}
