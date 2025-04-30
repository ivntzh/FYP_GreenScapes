using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class NetworkedPlayer : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Animator fullBodyAnimator;

    private PhotonView photonView;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XROrigin xrOrigin = FindAnyObjectByType<XROrigin>();
        headRig = xrOrigin.transform.Find("Camera Offset/Main Camera");
        leftHandRig = xrOrigin.transform.Find("Camera Offset/Left Controller");
        rightHandRig = xrOrigin.transform.Find("Camera Offset/Right Controller");

        if (photonView.IsMine)
        {
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = false;
            }
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            UpdateFullBodyAnimator();
        }
    }

    void UpdateFullBodyAnimator()
    {
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        float leftTrigger = 0f, leftGrip = 0f;
        if (leftDevice.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger))
        {
            fullBodyAnimator.SetFloat("Left Pinch", leftTrigger);
        }
        else
        {
            fullBodyAnimator.SetFloat("Left Pinch", 0f);
        }
        if (leftDevice.TryGetFeatureValue(CommonUsages.grip, out leftGrip))
        {
            fullBodyAnimator.SetFloat("Left Grab", leftGrip);
        }
        else
        {
            fullBodyAnimator.SetFloat("Left Grab", 0f);
        }

        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        float rightTrigger = 0f, rightGrip = 0f;
        if (rightDevice.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger))
        {
            fullBodyAnimator.SetFloat("Right Pinch", rightTrigger);
        }
        else
        {
            fullBodyAnimator.SetFloat("Right Pinch", 0f);
        }
        if (rightDevice.TryGetFeatureValue(CommonUsages.grip, out rightGrip))
        {
            fullBodyAnimator.SetFloat("Right Grab", rightGrip);
        }
        else
        {
            fullBodyAnimator.SetFloat("Right Grab", 0f);
        }
    }

    void MapPosition(Transform target, Transform xrOriginTransform)
    {
        target.position = xrOriginTransform.position;
        target.rotation = xrOriginTransform.rotation;
    }
}
