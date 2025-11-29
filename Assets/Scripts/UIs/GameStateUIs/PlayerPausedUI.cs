using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPausedUI : MonoBehaviour
{
    [SerializeField] private Player player;
    private void Start()
    {
        player.OnPlayerTogglePause += Player_OnPlayerTogglePause;
        Hide();
    }

    private void Player_OnPlayerTogglePause(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.isPlayerPaused(player.NetworkObject.OwnerClientId))
        {
            Show();
        }
        else
        {
            Hide();
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
