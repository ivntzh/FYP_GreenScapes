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

    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    private PhotonView photonView;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XROrigin xrOrigin = FindAnyObjectByType<XROrigin>();
        headRig = xrOrigin.transform.Find("Camera Offset/Main Camera");
        leftHandRig = xrOrigin.transform.Find("Camera Offset/Left Controller");
        rightHandRig = xrOrigin.transform.Find("Camera Offset/Right Controller");

        if(photonView.IsMine)
        {
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
			    item.enabled = false;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            MapPosition(head,headRig);
            MapPosition(leftHand,leftHandRig);
            MapPosition(rightHand,rightHandRig);

            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand),leftHandAnimator);
            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand),rightHandAnimator);
        }
    }

    void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
    {
        if(targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }

    void MapPosition(Transform target,Transform xrOriginTransform)
	{
		target.position = xrOriginTransform.position;
        target.rotation = xrOriginTransform.rotation;
	}
}