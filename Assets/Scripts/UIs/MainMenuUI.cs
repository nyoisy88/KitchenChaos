using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Button quitBtn;

    private void Start()
    {
        startBtn.onClick.AddListener(() =>
        {
            Loader.LoadScene(Loader.Scene.LobbyScene);
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        Time.timeScale = 1f; // Ensure time scale is reset when entering the main menu
    }
}
