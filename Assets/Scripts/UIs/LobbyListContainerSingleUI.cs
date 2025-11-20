using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListContainerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameTxt;
    public Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinLobbyById(lobby.Id);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameTxt.text = lobby.Name;
    }
}
