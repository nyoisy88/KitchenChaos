using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private Transform iconTemplate;

    private void Start()
    {
        plateKitchenObject.OnIngredientChanged += PlateKitchenObject_OnIngredientChanged;

        iconTemplate.gameObject.SetActive(false);
    }

    private void PlateKitchenObject_OnIngredientChanged(object sender, PlateKitchenObject.OnIngredientChangedEventArgs e)
    {
        foreach (Transform child in transform)
        {
            if (child == iconTemplate) continue;

            Destroy(child.gameObject);

        }
        foreach (KitchenObjectSO kitchenObjectSO in e.KitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectS0(kitchenObjectSO);
            iconTransform.gameObject.SetActive(true);
        }
    }
}
