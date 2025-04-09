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
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);

            MapPosition(head,headRig);
            MapPosition(leftHand,leftHandRig);
            MapPosition(rightHand,rightHandRig);
        }
    }

    void MapPosition(Transform target,Transform xrOriginTransform)
	{
		target.position = xrOriginTransform.position;
        target.rotation = xrOriginTransform.rotation;
	}
}