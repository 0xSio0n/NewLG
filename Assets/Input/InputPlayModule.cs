using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputPlayModule", menuName = "AddOn Module/Game Play", order = 0)]
public class InputPlayModule : ScriptableObject {
    public Action<ActionState> OnAction { get; set; }
    public Action<int>[] OnPocket { get; set; }

    public Vector3 MoveHandler {
        get {
            var axisX = input.GamePlay.Movement.ReadValue<Vector2>().x;
            var axisZ = input.GamePlay.Movement.ReadValue<Vector2>().y;
            return new Vector3(axisX, 0, axisZ);
        }
    }

    public Vector3 LookHandler {
        get {
            var axisX = input.GamePlay.Look.ReadValue<Vector2>().x;
            var axisY = input.GamePlay.Look.ReadValue<Vector2>().y;
            return new Vector3(axisX, axisY);
        }
    }

    //public bool RightHoldHandler => input.GamePlay.CamRight.ReadValue<float>() > 0;
    //public bool LeftHoldHandler => input.GamePlay.CamLeft.ReadValue<float>() > 0;
    public bool CastHoldHandler => input.GamePlay.Skill.ReadValue<float>() > 0;

    public InputAction Test(string name) {
        return input.FindAction(name); 
    }

    private GameInput input;

    private void ActionAwake() {
        OnPocket = new Action<int>[4];

        input.GamePlay.Pocket_1.performed += (e) => {
            OnPocket[0]?.Invoke(0);
        };

        input.GamePlay.Pocket_2.performed += (e) => {
            OnPocket[1]?.Invoke(1);
        };

        input.GamePlay.Pocket_3.performed += (e) => {
            OnPocket[2]?.Invoke(2);
        };

        input.GamePlay.Pocket_4.performed += (e) => {
            OnPocket[3]?.Invoke(3);
        };

        input.GamePlay.Interaction.performed += (e) => {
            OnAction?.Invoke(ActionState.Interaction);
        };

        input.GamePlay.Abort.performed += (e) => {
            OnAction?.Invoke(ActionState.Abort);
        };

        input.GamePlay.Switch.performed += (e) => {
            OnAction?.Invoke(ActionState.Switch);
        };

        input.GamePlay.Target.performed += (e) => {
            OnAction?.Invoke(ActionState.Target);
        };

        input.GamePlay.Attack.performed += (e) => {
            OnAction?.Invoke(ActionState.Attack);
        };

        input.GamePlay.Skill.canceled += (e) => {
            OnAction?.Invoke(ActionState.Skill);
        };

        input.GamePlay.Placement.performed += (e) => {
            OnAction?.Invoke(ActionState.Left);
        };

        //input.GamePlay.CamRight.performed += (e) => {
        //    OnAction?.Invoke(ActionState.Right);
        //};

        input.GamePlay.Select.performed += (e) => {
            OnAction?.Invoke(ActionState.Select);
        };

        input.GamePlay.Pause.performed += (e) => {
            OnAction?.Invoke(ActionState.Pause);
        };
    }

    private void OnEnable() {
        input = new();
        input.Enable();
        ActionAwake();
    }

    private void OnDisable() {
        input.Disable();
    }
}
