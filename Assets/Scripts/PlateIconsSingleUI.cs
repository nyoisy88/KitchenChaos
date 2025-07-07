using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateIconsSingleUI : MonoBehaviour
{
    [SerializeField] private Image iconImg;

    public void SetKitchenObjectS0(KitchenObjectSO kitchenObjectSO)
    {
        iconImg.sprite = kitchenObjectSO.sprite;
    }
}
