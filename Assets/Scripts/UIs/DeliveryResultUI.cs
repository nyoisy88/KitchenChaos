using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
    private const string POPUP = "Popup";

    [SerializeField] private Animator animator;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI deliveryResultText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Color successColor;
    [SerializeField] private Color failColor;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failSprite;

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailure += DeliveryManager_OnRecipeFailure;
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
     
    }

    private void DeliveryManager_OnRecipeFailure(object sender, EventArgs e)
    {
        Show();
        backgroundImage.color = failColor;
        iconImage.sprite = failSprite;
        deliveryResultText.text = "DELIVERY\nFAIL";
        animator.SetTrigger(POPUP);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        Show();
        backgroundImage.color = successColor;
        iconImage.sprite = successSprite;
        deliveryResultText.text = "DELIVERY\nSUCCESS";
        animator.SetTrigger(POPUP);
    }
}
