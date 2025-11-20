using TMPro;
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
        Hide();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
