using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Button returnBtn;
    private Action onReturnBtnAction;

    private void Awake()
    {
        Instance = this;
        soundSlider.onValueChanged.AddListener(value => SoundManager.Instance.SetSoundVolume(value));
        musicSlider.onValueChanged.AddListener(value => MusicManager.Instance.SetMusicVolume(value));
        returnBtn.onClick.AddListener(() =>
        {
            Hide();
            onReturnBtnAction();
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;
        soundSlider.value = SoundManager.Instance.GetSoundVolume();
        musicSlider.value = MusicManager.Instance.GetMusicVolume();
        Hide();
    }

    private void KitchenGameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(Action onReturnBtnAction)
    {
        this.onReturnBtnAction = onReturnBtnAction;
        gameObject.SetActive(true);
        soundSlider.Select();
    }
}
