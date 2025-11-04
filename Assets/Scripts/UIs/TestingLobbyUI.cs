using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameBtn;
    [SerializeField] private Button joinGameBtn;

    private void Start()
    {
        createGameBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });

        joinGameBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
            // No need to load scene here, client will be moved to CharacterSelectScene by server
        });
    }
}
