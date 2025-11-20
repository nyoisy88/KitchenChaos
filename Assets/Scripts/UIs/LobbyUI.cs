using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private Button joinCodeBtn;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private TMP_InputField playerNameTxt;
    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        mainMenuBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.LeaveLobby();
            Loader.LoadScene(Loader.Scene.MainMenuScene);
        });
        createLobbyBtn.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });
        quickJoinBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.QuickJoin();
        });
        joinCodeBtn.onClick.AddListener(() =>
        {
            string joinCode = joinCodeInputField.text;
            KitchenGameLobby.Instance.JoinLobbyWithCode(joinCode);
        });
        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameTxt.text = KitchenGameMultiplayer.Instance.PlayerName;
        playerNameTxt.onValueChanged.AddListener((string newText) =>
        {
            KitchenGameMultiplayer.Instance.PlayerName = newText;
        });
        KitchenGameLobby.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;
        UpdateLobbies(new List<Lobby>());
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, KitchenGameLobby.LobbyListChangedEventArgs e)
    {
        UpdateLobbies(e.LobbyList);
    }

    private void UpdateLobbies(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyListContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyListContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListContainerSingleUI>().SetLobby(lobby);
        }
    }
}
