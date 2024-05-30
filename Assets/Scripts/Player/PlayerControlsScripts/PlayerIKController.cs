using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerIKController : MonoBehaviour
{
    [Header("SCRIPTS")]
    [SerializeField] private PlayerController playerController;

    [Header("IK REFERENCES AND BONES")]
    [SerializeField] private Rig environmentInteractionsRig;
    [SerializeField] private Transform leftArmIKTarget;
    [SerializeField] private Transform rightArmIKTarget;
    [SerializeField] private Transform leftHandBoneTransform;
    [SerializeField] private Transform rightHandBoneTransform;

    private Vector3 defaultLeftArmIKTargetLocalPos;
    private Vector3 defaultRightArmIKTargetLocalPos;
    private readonly Vector3 defaultRightArmIKTargetHangingPos = new Vector3(0.297f, 1.283f, 0.257f);
    private readonly Vector3 defaultLeftArmIKTargetHangingPos = new Vector3(-0.364f, 1.283f, 0.249f);
    private const float handGrabOffset = 0.15f;
    private string groundMask;

    private void Awake()
    {
        groundMask = playerController.GetGroundMask();
        defaultLeftArmIKTargetLocalPos = leftArmIKTarget.localPosition;
        defaultRightArmIKTargetLocalPos = rightArmIKTarget.localPosition;
        ToggleRigWeight(false);
    }

    public void ToggleRigWeight(bool toggle)
    {
        environmentInteractionsRig.weight = toggle ? 1 : 0;
        if (!toggle)
        {
            leftArmIKTarget.localPosition = defaultLeftArmIKTargetLocalPos;  
            rightArmIKTarget.localPosition = defaultRightArmIKTargetLocalPos;
        }
    }
    public void SwitchArmIKTargetsToHanging()
    {
        leftArmIKTarget.localPosition = defaultLeftArmIKTargetHangingPos;
        rightArmIKTarget.localPosition = defaultRightArmIKTargetHangingPos;
    }

    public void DoLeftHandGrab()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(leftHandBoneTransform.position, leftHandBoneTransform.forward, out hitInfo, 1f, LayerMask.GetMask(groundMask)))
        {
            Vector3 grabPoint = hitInfo.point;
            grabPoint.z += handGrabOffset;
            SetLeftHandIKTarget(grabPoint);
        }
    }

    public void DoRightHandGrab()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(rightHandBoneTransform.position, rightHandBoneTransform.forward, out hitInfo, 1f, LayerMask.GetMask(groundMask)))
        {
            Vector3 grabPoint = hitInfo.point;
            grabPoint.z += handGrabOffset;
            SetRightHandIKTarget(grabPoint);
        }
    }

    public void SetLeftHandIKTargetToDefaultHang() => leftArmIKTarget.localPosition = defaultLeftArmIKTargetHangingPos;
    public void SetRightHandIKTargetToDefaultHang() => rightArmIKTarget.localPosition = defaultRightArmIKTargetHangingPos;

    public void ResetArmIK()
    {
        leftArmIKTarget.localPosition = defaultLeftArmIKTargetLocalPos;
        rightArmIKTarget.localPosition = defaultRightArmIKTargetLocalPos;
    }

    private void SetLeftHandIKTarget(Vector3 leftPos) => leftArmIKTarget.position = leftPos;
    private void SetRightHandIKTarget(Vector3 rightPos) => rightArmIKTarget.position = rightPos;
}
