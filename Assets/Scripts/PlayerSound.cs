using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private Player player;
    private float footStepTimer;
    private float footStepTimerMax = 0.7f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }
    private void Update()
    {
        if (!player.IsWalking())
        {
            footStepTimer = 0f;
            return;
        }
        footStepTimer -= Time.deltaTime;
        if (footStepTimer <= 0f)
        {
            footStepTimer = footStepTimerMax;

            SoundManager.Instance.PlayFootstepSound(player.transform.position);
        }
    }
}
