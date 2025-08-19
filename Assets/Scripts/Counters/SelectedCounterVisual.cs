using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private Transform[] visualGameObjects;
    [SerializeField] private BaseCounter baseCounter;

    private void Start()
    {
        Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }

    private void Player_OnSelectedCounterChanged(object sender, OnSelectedCounterChangedEventArgs e)
    {
        if (e.SelectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Hide()
    {
        foreach (var i in visualGameObjects)
        {
            i.gameObject.SetActive(false);
        }
    }

    private void Show()
    {
        foreach (var i in visualGameObjects)
        {
            i.gameObject.SetActive(true);
        }
    }
}
