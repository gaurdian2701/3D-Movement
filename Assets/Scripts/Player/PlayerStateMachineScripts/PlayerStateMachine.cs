using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    private PlayerController controller;
    private IState currentState;
    private Dictionary<PlayerStates, IState> playerStates;

    public PlayerStateMachine(PlayerController controller, PlayerIKController playerIKController)
    {
        this.controller = controller;
        playerStates = new Dictionary<PlayerStates, IState>();
        AddStates(playerIKController);
        ChangeState(PlayerStates.WALKING);
    }

    private void AddStates(PlayerIKController playerIKController)
    {
        playerStates.Add(PlayerStates.WALKING, new PlayerWalkingState(controller));
        playerStates.Add(PlayerStates.HANGING, new PlayerHangingState(controller, playerIKController));
    }
    public void UpdateStateMachine(Vector2 inputData) => currentState.UpdateMovement(inputData);

    public void ChangeState(PlayerStates state)
    {
        currentState?.ExitState();
        currentState = playerStates[state];
        controller.SetCurrentPlayerState(state);
        currentState.EnterState();
    }
}
