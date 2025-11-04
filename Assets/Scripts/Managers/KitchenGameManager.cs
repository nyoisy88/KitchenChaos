using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler<OnAnyPlayerTogglePauseEventArgs> OnAnyPlayerTogglePause;

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> state = new(State.WaitingToStart);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPauseDictionary;

    private NetworkVariable<float> startCountdownTimer = new(3f);
    private NetworkVariable<float> playingTimer = new(0f);
    [SerializeField] private float playingTimerMax = 60f;
    private bool isLocalPlayerReady = false;
    private bool isLocalGamePaused = false;

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // Spawn player objects for each client that completed loading the game scene
        if (sceneName == Loader.Scene.GameScene.ToString())
        {
            foreach (ulong clientId in clientsCompleted)
            {
                Transform playerTransform = Instantiate(playerPrefab);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }

    private void State_OnValueChanged(State previousStateValue, State newStateValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPauseDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool areAllPlayersReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // There is at least one player who is not ready
                areAllPlayersReady = false;
                break;
            }
        }
        if (areAllPlayersReady)
        {
            state.Value = State.CountdownToStart;
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                startCountdownTimer.Value -= Time.deltaTime;
                if (startCountdownTimer.Value <= 0f)
                {
                    playingTimer.Value = playingTimerMax;
                    state.Value = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                playingTimer.Value -= Time.deltaTime;
                if (playingTimer.Value <= 0f)
                {
                    state.Value = State.GameOver;

                }
                break;
            case State.GameOver:
                break;
        }

    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (!isLocalGamePaused)
        {
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        SetPlayerPauseServerRpc(!isLocalGamePaused, Player.LocalInstance.GetNetworkObject());
        isLocalGamePaused = !isLocalGamePaused;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerPauseServerRpc(bool isSenderPaused, NetworkObjectReference playerNetworkObjectRef, ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = isSenderPaused;

        bool allPlayersPaused = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerPauseDictionary.ContainsKey(clientId) || !playerPauseDictionary[clientId])
            {
                // There is at least one player who is not paused
                allPlayersPaused = false;
                break;
            }
        }
        SetPlayerPauseClientRpc(allPlayersPaused, isSenderPaused, playerNetworkObjectRef);
    }

    [ClientRpc]
    private void SetPlayerPauseClientRpc(bool allPlayersPaused, bool isSenderPaused, NetworkObjectReference playerNetworkObjectRef)
    {
        if (allPlayersPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        playerNetworkObjectRef.TryGet(out NetworkObject playerNetworkObject);
        Player player = playerNetworkObject.GetComponent<Player>();

        OnAnyPlayerTogglePause?.Invoke(this, new OnAnyPlayerTogglePauseEventArgs
        {
            AreAllPlayersPaused = allPlayersPaused,
            IsPlayerGamePaused = isSenderPaused,
            Player = player
        });
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }
    public float GetStartCountdownTimer()
    {
        return startCountdownTimer.Value;
    }

    public float GetPlayingTimerNormalized()
    {
        return 1 - (playingTimer.Value / playingTimerMax);
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public bool IsLocalGamePaused()
    {
        return isLocalGamePaused;
    }

}

public class OnAnyPlayerTogglePauseEventArgs : EventArgs
{
    public bool AreAllPlayersPaused;
    public bool IsPlayerGamePaused;
    public Player Player;
}