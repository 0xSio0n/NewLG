using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputUIModule", menuName = "AddOn Module/Game UI", order = 1)]
public class InputUIModule : ScriptableObject {
    public Action OnReset { get; set; }
    public Action OnSubmit { get; set; }
    public Action OnCancel { get; set; }

    private GameInput input;

    private void OnEnable() {
        input = new();
        input.Enable();

        input.GameUI.Submit.performed += (e) => {
            OnSubmit?.Invoke();
        };

        input.GameUI.Cancel.performed += (e) => {
            OnCancel?.Invoke();
        };

        input.GameUI.Back.performed += (e) => {
            OnReset?.Invoke();
        };
    }

    private void OnDisable() {
        input.Disable();
    }
}