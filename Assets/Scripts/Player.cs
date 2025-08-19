using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public event EventHandler OnPlayerGrabbedObject;
    public static Player Instance { get; private set; }
    
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private bool _isWalking;
    private Vector3 _lastInteractDir;
    private BaseCounter _selectedCounter;
    private KitchenObject _kitchenObject;

    #region unity methods

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("More than 1 player detected!!");
        }
        Instance = this;
    }

    private void Start()
    {
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
        HandleMovement();
        HandleInteraction();
    }

    #endregion
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

        float playerHeight = 2f;
        float playerRadius = 0.6f;
        float moveDistance = Time.deltaTime * moveSpeed;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + (Vector3.up * playerHeight),
            playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            // Check wall direction to move alongside
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + (Vector3.up * playerHeight),
                playerRadius, moveDirX, moveDistance);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + (Vector3.up * playerHeight),
                    playerRadius, moveDirZ, moveDistance);
                if ( canMove )
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
        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);
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
}

public class OnSelectedCounterChangedEventArgs : EventArgs
{
    public BaseCounter SelectedCounter;
}
