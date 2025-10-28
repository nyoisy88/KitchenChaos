using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    public event EventHandler OnStateChanged;

    public enum State
    {
        WaitingToStart,
        StartCountdown,
        GamePlaying,
        GameOver
    }

    private State state;

    private float startCountdownTimer = 3f;
    private float playingTimer;
    private float playingTimerMax = 30f;
    private bool isGamePaused = false;



    private void Awake()
    {
        Instance = this;

        state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            state = State.StartCountdown;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                
                break;
            case State.StartCountdown:
                startCountdownTimer -= Time.deltaTime;
                if (startCountdownTimer <= 0f)
                {
                    state = State.GamePlaying;
                    playingTimer = playingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                playingTimer -= Time.deltaTime;
                if (playingTimer <= 0f)
                {
                    state = State.GameOver;

                    OnStateChanged?.Invoke(this, EventArgs.Empty);
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
        if (isGamePaused)
        {
            Time.timeScale = (float)1.0f;

            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = (float)0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        isGamePaused = !isGamePaused;
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsStartCountdowntActive()
    {
        return state == State.StartCountdown;
    }

    public float GetStartCountdownTimer()
    {
        return startCountdownTimer;
    }

    public float GetPlayingTimerNormalized()
    {
        return 1 - (playingTimer / playingTimerMax);
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }
}
