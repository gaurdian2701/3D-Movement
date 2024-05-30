using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerHangingState : IState
{
    private PlayerController playerController;
    private PlayerIKController playerIKController;
    private Transform playerTransform;
    private Animator animator;
    private string hangParam;
    private string hangSpeedParam;
    private string groundMask;
    private float currentHangingMovementSpeedModifier = 0f;
    private Vector3 ledgeEndRaycastOrigin;
    private RaycastHit ledgeHitInfo;
    private const float ledgeEndCheckRaycastHorizontalOffset = 0.5f;
    private const float ledgeEndCheckRaycastVerticalOffset = 2.2f;
    private const float defaultHangingMovementSpeedModifier = 0.38f;

    public PlayerHangingState(PlayerController controller, PlayerIKController playerIKController)
    {
        this.playerController = controller;
        this.playerIKController = playerIKController;
        animator = controller.GetAnimator();
        hangParam = controller.GetHangParam();
        hangSpeedParam = controller.GetHangSpeedParam();
        groundMask = controller.GetGroundMask();
        playerTransform = controller.transform;
        currentHangingMovementSpeedModifier = defaultHangingMovementSpeedModifier;
    }
    public void EnterState()
    {
        animator.SetBool(hangParam, true);
        playerController.SetHangingFlag(true);
        playerIKController.SwitchArmIKTargetsToHanging();
        playerIKController.ToggleRigWeight(true);
    }

    //Unable to access updated transform.forward value even though TRANSFORM ITSELF IS GETTING UPDATED WTF UNITY
    public void UpdateMovement(Vector2 inputData)
    {
        float horizontalInput = Mathf.Round(inputData.x);
        CheckForShimmy(horizontalInput);
        animator.SetFloat(hangSpeedParam, Mathf.Round(horizontalInput));
        Vector3 shimmyAxisVector = ledgeHitInfo.normal;
        shimmyAxisVector.Normalize();
        shimmyAxisVector = Quaternion.AngleAxis(-90f, Vector3.up) * shimmyAxisVector;
        Vector3 hangingMovement = new Vector3(inputData.x, 0f, 0f);
        playerTransform.position += currentHangingMovementSpeedModifier * Time.deltaTime * hangingMovement;
    }

    private void CheckForShimmy(float moveDirection)
    {
        ledgeEndRaycastOrigin = playerTransform.position;
        ledgeEndRaycastOrigin.x += (ledgeEndCheckRaycastHorizontalOffset * moveDirection);
        ledgeEndRaycastOrigin.y += ledgeEndCheckRaycastVerticalOffset;
        currentHangingMovementSpeedModifier = Physics.Raycast(ledgeEndRaycastOrigin, playerTransform.forward, out ledgeHitInfo, 1f, LayerMask.GetMask(groundMask)) ? 
            defaultHangingMovementSpeedModifier : 0f;
    }

    public void ExitState()
    {
        animator.SetBool(hangParam, false);
        playerController.SetHangingFlag(false);
        playerIKController.ToggleRigWeight(false);
        playerIKController.ResetArmIK();
    }
}
