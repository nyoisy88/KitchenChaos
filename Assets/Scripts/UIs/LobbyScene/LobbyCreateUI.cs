using System;
using TMPro;
using UIs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button createPublicBtn;
    [SerializeField] private Button createPrivateBtn;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Button closeBtn;

    private void Awake()
    {
        createPublicBtn.onClick.AddListener(() =>
        {
            string lobbyName = lobbyNameInputField.text;
            KitchenGameLobby.Instance.CreateLobby(lobbyName, false);
        });
        createPrivateBtn.onClick.AddListener(() =>
        {
            string lobbyName = lobbyNameInputField.text;
            KitchenGameLobby.Instance.CreateLobby(lobbyName, true);
        });
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        LobbyMessageUI.Instance.OnCloseBtnAction += LobbyMessageUI_onCloseBtnAction;
        Hide();
    }

    private void LobbyMessageUI_onCloseBtnAction()
    {
        if (this.gameObject.activeSelf)
        {
            createPublicBtn.Select();
        }
    }

    public void Show(Action onCloseBtnAction)
    {
        this.gameObject.SetActive(true);
        createPublicBtn.Select();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
            onCloseBtnAction?.Invoke();
        });
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
