using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetcodeTestingUI : MonoBehaviour
{
    [SerializeField] private Button hostStartBtn;
    [SerializeField] private Button clientStartBtn;

    void Start()
    {
        hostStartBtn.onClick.AddListener(() =>
        {
            Debug.Log("Host");
            KitchenGameMultiplayer.Instance.StartHost();
            Hide();
        });
        clientStartBtn.onClick.AddListener(() =>
        {
            Debug.Log("Client");
            KitchenGameMultiplayer.Instance.StartClient();
            Hide();
        });
    }

    private void Show()
    {
        gameObject.SetActive(true);
        hostStartBtn.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
