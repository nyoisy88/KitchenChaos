using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField] private Player player;

    private static readonly int Walking = Animator.StringToHash("IsWalking");
    private Animator _playerAnimator;

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        _playerAnimator.SetBool(Walking, player.IsWalking());   
    }

}
