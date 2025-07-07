using System;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private Transform sizzleParticle;
    [SerializeField] private Transform stoveOnVisual;

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        if (e.State == StoveCounter.State.Frying || e.State == StoveCounter.State.Burning)
        {
            StoveOn();
        }
        else
        {
            StoveOff();
        }
            
    }

    private void StoveOff()
    {
        stoveOnVisual.gameObject.SetActive(false);
        sizzleParticle.gameObject.SetActive(false);
    }

    private void StoveOn()
    {
        stoveOnVisual.gameObject.SetActive(true);
        sizzleParticle.gameObject.SetActive(true);
    }
}
