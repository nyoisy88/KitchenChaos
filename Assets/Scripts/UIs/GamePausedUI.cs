using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GamePausedUI : MonoBehaviour
{
    public static GamePausedUI Instance { get; private set; }

    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button mainMenuBtn;

    private void Awake()
    {
        Instance = this;
        resumeBtn.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePause();
        });
        optionsBtn.onClick.AddListener(() =>
        {
            OptionsUI.Instance.Show(Show);
            Hide();
        });
        mainMenuBtn.onClick.AddListener(() =>
        {
            Loader.LoadScene(Loader.Scene.MainMenuScene);
            Time.timeScale = 1f;
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;


        Hide();
    }

    private void KitchenGameManager_OnGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void KitchenGameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        resumeBtn.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
