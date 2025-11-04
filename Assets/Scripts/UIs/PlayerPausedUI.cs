using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPausedUI : MonoBehaviour
{
    [SerializeField] private Player player;
    private void Start()
    {
        KitchenGameManager.Instance.OnAnyPlayerTogglePause += KitchenGameManager_OnAnyPlayerTogglePause;
        Hide();
    }

    private void KitchenGameManager_OnAnyPlayerTogglePause(object sender, OnAnyPlayerTogglePauseEventArgs e)
    {
        if (e.AreAllPlayersPaused)
        {
            Show();
        }
        else if (e.Player == player)
        {
            if (e.IsPlayerGamePaused)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
