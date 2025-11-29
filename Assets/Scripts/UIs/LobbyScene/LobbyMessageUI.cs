using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    public static LobbyMessageUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeBtn;

    public event Action OnCloseBtnAction;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFail;
        KitchenGameLobby.Instance.OnQuickJoinFailed += KitchenGameLobby_OnQuickJoinFailed;
        KitchenGameLobby.Instance.OnJoinWithCodeFailed += KitchenGameLobby_OnJoinWithCodeFailed;
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        closeBtn.onClick.AddListener(Hide);
        Hide();
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        UpdateMessage("Creating lobby...");
    }

    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            UpdateMessage("Failed to connect.");
        }
        else
        {
            UpdateMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void KitchenGameLobby_OnJoinWithCodeFailed(object sender, EventArgs e)
    {
        UpdateMessage("Failed to join lobby with code.");
    }

    private void KitchenGameLobby_OnQuickJoinFailed(object sender, EventArgs e)
    {
        UpdateMessage("Failed to quick join lobby.");
    }

    private void KitchenGameLobby_OnCreateLobbyFail(object sender, EventArgs e)
    {
        UpdateMessage("Failed to create lobby.");
    }

    private void UpdateMessage(string message)
    {
        messageText.text = message;
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        closeBtn.Select();
    }
    private void Hide()
    {
        gameObject.SetActive(false);
        OnCloseBtnAction?.Invoke();
    }

    private void OnDestroy()
    {
        KitchenGameLobby.Instance.OnCreateLobbyFailed -= KitchenGameLobby_OnCreateLobbyFail;
        KitchenGameLobby.Instance.OnQuickJoinFailed -= KitchenGameLobby_OnQuickJoinFailed;
        KitchenGameLobby.Instance.OnJoinWithCodeFailed -= KitchenGameLobby_OnJoinWithCodeFailed;
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
    }
}
