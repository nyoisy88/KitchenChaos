using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailure;
    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    private List<RecipeSO> waitingRecipeSOList = new();

    float spawnRecipeTimer = 2f;
    float spawnRecipeTimerMax = 4f;
    int waitingRecipeCountMax = 4;
    int recipesDelivered;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (!KitchenGameManager.Instance.IsGamePlaying() || waitingRecipeSOList.Count >= waitingRecipeCountMax)
        {
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            int recipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

            SpawnRecipeClientRpc(recipeSOIndex);

        }
    }

    [ClientRpc]
    private void SpawnRecipeClientRpc(int recipeSOIndex)
    {
        RecipeSO spawnRecipeSO = recipeListSO.recipeSOList[recipeSOIndex];
        waitingRecipeSOList.Add(spawnRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject, Player player)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            bool numberOfIngredientsMatched = false;
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                numberOfIngredientsMatched = true;
            }
            if (!numberOfIngredientsMatched) continue;

            bool everyIngredientMatched = true;
            //Cycle through every ingredient in recipeSO
            foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
            {
                bool ingredientFound = false;
                //Cycle through every ingredient in plate
                foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                {
                    if (recipeKitchenObjectSO == plateKitchenObjectSO)
                    {
                        ingredientFound = true;
                        break;
                    }
                }
                if (!ingredientFound)
                {
                    everyIngredientMatched = false;
                    break;
                }
                ;
            }

            if (everyIngredientMatched)
            {
                //Debug.Log("Delivery Success");
                RecipeDeliverSuccessServerRpc(i, player.NetworkObject);
                return;
            }
        }

        // Recipe not matched at all
        Debug.Log("Delivery Fail");
        RecipeDeliverFailureServerRpc(player.NetworkObject);

    }

    [ServerRpc(RequireOwnership = false)]
    private void RecipeDeliverSuccessServerRpc(int recipeIndex, NetworkObjectReference playerNetworkObjectRef)
    {
        Player player = playerNetworkObjectRef.TryGet(out NetworkObject playerNetworkObject) ?
            playerNetworkObject.GetComponent<Player>() : null;
        if (!player.HasKitchenObject())
        {
            return;
        }
        RecipeDeliverSuccessClientRpc(recipeIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecipeDeliverFailureServerRpc(NetworkObjectReference playerNetworkObjectRef)
    {
        Player player = playerNetworkObjectRef.TryGet(out NetworkObject playerNetworkObject) ?
            playerNetworkObject.GetComponent<Player>() : null;
        if (!player.HasKitchenObject())
        {
            return;
        }
        RecipeDeliverFailureClientRpc();
    }

    [ClientRpc]
    private void RecipeDeliverSuccessClientRpc(int recipeIndex)
    {
        waitingRecipeSOList.RemoveAt(recipeIndex);
        recipesDelivered++;
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void RecipeDeliverFailureClientRpc()
    {
        OnRecipeFailure?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetRecipesDeliverdNumber()
    {
        return recipesDelivered;
    }

}
