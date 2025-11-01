using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetcodeTestingUI : MonoBehaviour
{
    [SerializeField] private Button hostStartBtn;
    [SerializeField] private Button clientStartBtn;
    [SerializeField] private Button serverStartBtn;

    void Start()
    {
        hostStartBtn.onClick.AddListener(() =>
        {
            Unity.Netcode.NetworkManager.Singleton.StartHost();
            Hide();
        });
        clientStartBtn.onClick.AddListener(() =>
        {
            Unity.Netcode.NetworkManager.Singleton.StartClient();
            Hide();
        });
        serverStartBtn.onClick.AddListener(() =>
        {
            Unity.Netcode.NetworkManager.Singleton.StartServer();
        });
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        KitchenGameManager.Instance.GameInput_OnInteractAction(null, System.EventArgs.Empty);
    }
}
