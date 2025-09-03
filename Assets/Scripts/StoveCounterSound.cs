using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource audioSource;
    private bool playWarningSound = false;
    private float warningSoundTimer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void Update()
    {
        if (!playWarningSound)
        {
            return;
        }
        warningSoundTimer -= Time.deltaTime;
        if (warningSoundTimer <= 0f)
        {
            SoundManager.Instance.PlayWarningSound(transform.position);
            warningSoundTimer = 0.2f;
        }
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = 0.5f;
        playWarningSound = stoveCounter.IsBurning() && e.progressNormalized >= burnShowProgressAmount;

    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        if (e.State == StoveCounter.State.Frying || e.State == StoveCounter.State.Burning)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
}
