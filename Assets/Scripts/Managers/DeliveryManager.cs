using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailure;
    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    private List<RecipeSO> waitingRecipeSOList = new();

    float spawnRecipeTimer;
    float spawnRecipeTimerMax = 4f;
    int waitingRecipeCountMax = 4;
    int recipesDelivered;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (waitingRecipeSOList.Count >= waitingRecipeCountMax)
        {
            return;
        }
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            RecipeSO spawnRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
            waitingRecipeSOList.Add(spawnRecipeSO);

            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
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
                if (!ingredientFound) {
                    everyIngredientMatched = false;
                    break;
                };
            }

            if (everyIngredientMatched)
            {
                Debug.Log("Delivery Success");
                waitingRecipeSOList.RemoveAt(i);
                recipesDelivered++;

                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        // Recipe not matched at all
        Debug.Log("Delivery Fail");
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
