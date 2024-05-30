using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWalkingState : IState
{
    private PlayerController controller;
    private CharacterController characterController;
    private Animator animator;
    private Transform playerTransform;
    private Vector3 currentPosition;
    private string speedParam;
    private float normalSpeed;
    private float rotationSpeed;

    public PlayerWalkingState(PlayerController controller)
    {
        this.controller = controller;
        playerTransform = controller.transform;
        animator = controller.GetAnimator();
        normalSpeed = controller.GetNormalSpeed();
        rotationSpeed = controller.GetRotationSpeed();
        speedParam = controller.GetSpeedParam();
        characterController = controller.GetCharacterController();
    }

    public void EnterState()
    {
        controller.SetHangingFlag(false);
    }

    public void UpdateMovement(Vector2 inputData)
    {
        UpdateSpeed(inputData);
        if(currentPosition != Vector3.zero)
            UpdateRotation();
    }

    private void UpdateSpeed(Vector2 inputData)
    {
        Vector3 forwardMovementRelativeToCamera = controller.GetCamForward() * inputData.y;
        Vector3 sidewaysMovementRelativeToCamera = controller.GetCamRight() * inputData.x; //Rotating the vector input from standard X-Z axes to the X-Z axes of the camera
        currentPosition = forwardMovementRelativeToCamera + sidewaysMovementRelativeToCamera; //New vector obtained in camera's axis by adding the X and Z components

        animator.SetFloat(speedParam, currentPosition.magnitude);
        characterController.Move(CalculatePosition(controller.jumpVector));
    }
    private void UpdateRotation()
    {
        Quaternion lookRotation = Quaternion.LookRotation(currentPosition, Vector3.up);
        playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    private Vector3 CalculatePosition(Vector3 jumpVector) => ((normalSpeed * currentPosition) + jumpVector) * Time.deltaTime;

    public void ExitState()
    {
    }
}
