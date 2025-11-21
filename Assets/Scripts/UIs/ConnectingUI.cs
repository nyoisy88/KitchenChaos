using System;
using UnityEngine;

namespace UIs
{
    public class ConnectingUI : MonoBehaviour
    {

        void Start()
        {
            KitchenGameMultiplayer.Instance.OnTryingToJoinGame += KitchenGameMultiplayer_OnTryingToJoinGame;
            KitchenGameMultiplayer.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
            Hide();
        }

        private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
        {
            Hide();
        }

        private void KitchenGameMultiplayer_OnTryingToJoinGame(object sender, EventArgs e)
        {
            Debug.Log("Showing Connecting UI");
            Show();
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }
        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            KitchenGameMultiplayer.Instance.OnTryingToJoinGame -= KitchenGameMultiplayer_OnTryingToJoinGame;
            KitchenGameMultiplayer.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
        }
    }
}