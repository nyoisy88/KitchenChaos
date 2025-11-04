using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static Player LocalInstance { get; private set; }

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPlayerGrabbedObject;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyPlayerGrabbedObject = null;
    }

    public event EventHandler OnPlayerGrabbedObject;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    //public event EventHandler OnPlayerTogglePause;

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisionLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> playerSpawnLocationsList;

    private GameInput gameInput;
    private bool _isWalking;
    private Vector3 _lastInteractDir;
    private BaseCounter _selectedCounter;
    private KitchenObject _kitchenObject;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
        }
        int localPlayerId = (int)NetworkObject.OwnerClientId;
        transform.position = playerSpawnLocationsList[localPlayerId];

        if (IsServer)
        {
            //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
        }
    }


    private void NetworkManager_OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if (data.EventType == ConnectionEvent.ClientDisconnected && data.ClientId == OwnerClientId && HasKitchenObject())
        {
            GetKitchenObject().DestroySelf();
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            GetKitchenObject().DestroySelf();
        }
    }

    #region unity methods


    private void Start()
    {
        gameInput = GameInput.Instance;
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAltAction += GameInput_OnInteractAltAction;

    }


    private void GameInput_OnInteractAltAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (_selectedCounter)
        {
            _selectedCounter.InteractAlt();
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (_selectedCounter != null)
        {
            _selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        //HandleMovementServerAuth();
        HandleMovement();
        HandleInteraction();
    }

    #endregion

    private void HandleMovementServerAuth()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        HandleMovementServerRpc(inputVector);

    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector2 inputVector)
    {
        HandleMovement();
    }

    private void HandleInteraction()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            _lastInteractDir = moveDir;
        }

        float interactDistance = 1.5f;
        if (Physics.Raycast(transform.position, _lastInteractDir, out RaycastHit raycastHit,
                interactDistance, counterLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter counter))
            {
                if (_selectedCounter != counter)
                {
                    SetSelectedCounter(counter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //float playerHeight = 2f;
        float playerRadius = 0.6f;
        float moveDistance = Time.deltaTime * moveSpeed;
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionLayerMask);

        if (!canMove)
        {
            // Check wall direction to move alongside
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = moveDir.x != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionLayerMask);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = moveDir.z != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionLayerMask);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDir * moveDistance;
        }

        _isWalking = moveDir != Vector3.zero;

        const float rotationSpeed = 10f;
        if (_isWalking)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);
        }
    }

    public bool IsWalking()
    {
        return _isWalking;
    }

    public void SetSelectedCounter(BaseCounter selectedCounter)
    {
        _selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs()
        {
            SelectedCounter = selectedCounter
        });
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;

        if (_kitchenObject != null)
        {
            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public bool HasKitchenObject()
    {
        return (_kitchenObject != null);
    }

    public KitchenObject GetKitchenObject()
    {
        return _kitchenObject;
    }

    public void ClearKitchenObject()
    {
        _kitchenObject = null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}

public class OnSelectedCounterChangedEventArgs : EventArgs
{
    public BaseCounter SelectedCounter;
}
