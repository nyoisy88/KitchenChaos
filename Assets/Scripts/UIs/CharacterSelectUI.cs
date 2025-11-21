using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
namespace UIs
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private Button mainMenuBtn;
        [SerializeField] private Button readyBtn;
        [SerializeField] private TextMeshProUGUI lobbyNameTxt;
        [SerializeField] private TextMeshProUGUI lobbyCodeTxt;

        private void Awake()
        {
            mainMenuBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.Shutdown();
                KitchenGameLobby.Instance.LeaveLobby();
                Loader.LoadScene(Loader.Scene.MainMenuScene);
            });

            readyBtn.onClick.AddListener(() =>
            {
                CharacterSelectReady.Instance.SetPlayerReady();
            });
        }

        private void Start()
        {
            Lobby lobby = KitchenGameLobby.Instance.GetLobby();
            lobbyNameTxt.text = $"Lobby Name: {lobby.Name}";
            lobbyCodeTxt.text = $"Lobby Code: {lobby.LobbyCode}";
        }
    }
}
