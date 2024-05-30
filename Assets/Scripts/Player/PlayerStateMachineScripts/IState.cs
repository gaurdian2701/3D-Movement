using UnityEngine;

public interface IState
{
    public void EnterState();

    public void UpdateMovement(Vector2 inputData);
    public void ExitState();
}