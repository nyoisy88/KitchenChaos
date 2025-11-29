using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startMultiplayerBtn;
    [SerializeField] private Button startSingleplayerBtn;
    [SerializeField] private Button quitBtn;

    private void Start()
    {
        startMultiplayerBtn.Select();
        startMultiplayerBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.IsMultiplayerMode = true;
            Loader.LoadScene(Loader.Scene.LobbyScene);
        });

        startSingleplayerBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.IsMultiplayerMode = false;
            Loader.LoadScene(Loader.Scene.LobbyScene);
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        Time.timeScale = 1f; // Ensure time scale is reset when entering the main menu
    }
}
