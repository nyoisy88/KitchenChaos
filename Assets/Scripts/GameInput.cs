using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAltAction;
    public event EventHandler OnPauseAction;

    //[SerializeField] private Player player;
    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        Instance = this;
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Interact.performed += Interact_performed;
        _playerInputActions.Player.InteractAlt.performed += InteractAlt_performed;
        _playerInputActions.Player.Pause.performed += Pause_performed;
    }

    private void OnDestroy()
    {
        if (_playerInputActions != null)
        {
            _playerInputActions.Player.Interact.performed -= Interact_performed;
            _playerInputActions.Player.InteractAlt.performed -= InteractAlt_performed;
            _playerInputActions.Player.Pause.performed -= Pause_performed;
            _playerInputActions.Dispose();
        }
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlt_performed(InputAction.CallbackContext context)
    {
        OnInteractAltAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        return _playerInputActions.Player.Move.ReadValue<Vector2>();
    }
}
