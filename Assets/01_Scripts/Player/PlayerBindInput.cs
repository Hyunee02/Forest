using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerBindInput : MonoBehaviour
{
    private PlayerInput playerInput;

    private Vector2 moveInput = Vector2.zero;
    private bool bRun;

    public Vector2 MoveInput => moveInput;
    public bool BRun => bRun;

    public event Action OnInventoryInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        InputActionMap actionMap = playerInput.actions.FindActionMap("Player");

        // Move
        {
            InputAction action = actionMap.FindAction("Move");
            action.performed += context => moveInput = context.ReadValue<Vector2>();
            action.canceled += context => moveInput = Vector2.zero;
        }

        // Run
        {
            InputAction action = actionMap.FindAction("Sprint");
            action.performed += context => bRun = true;
            action.canceled += context => bRun = false;
        }

        // Inventory
        {
            InputAction action = actionMap.FindAction("Inventory");
            action.performed += context => OnInventoryInput?.Invoke();
        }
    }
}
