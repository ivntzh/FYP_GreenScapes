using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SoilGrowthOnParticle : MonoBehaviourPunCallbacks
{
    [Header("Growth Objects")]
    public GameObject smallPlantObject;      // child under this transform
    public GameObject mediumPlantObject;     // child under this transform
    [Header("Final Plant Prefab (in Resources/)")]
    public string finalPlantPrefabName;

    [Header("Soil Visuals")]
    public Material wetSoilMaterial;
    public Material drySoilMaterial;
    public MeshRenderer soilRenderer;

    [Header("Seed Setup")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor seedSocket;
    public string seedTag = "Seed";

    [Header("Timers")]
    public float waterTimeRequired = 2f;
    public float timeToMediumPlant   = 7f;
    public float timeToFinalReplace  = 10f;

    [Header("UI Panels")]
    public GameObject waterUI;
    public GameObject timer1UI;
    public GameObject timer2UI;
    public GameObject timer3UI;

    // internal state (driven only on host)
    private bool   isWatering        = false;
    private bool   hasGrowthStarted = false;
    private float  waterTimer       = 0f;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (isWatering)
        {
            waterTimer += Time.deltaTime;
            if (waterTimer >= waterTimeRequired && !hasGrowthStarted)
            {
                hasGrowthStarted = true;
                waterTimer       = 0f;
                photonView.RPC(nameof(RPC_GrowSmall), RpcTarget.AllBuffered);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (PhotonNetwork.IsMasterClient)
            BeginWateringLocal();
        else
            photonView.RPC(nameof(RequestBeginWatering), RpcTarget.MasterClient);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (PhotonNetwork.IsMasterClient)
            StopWateringLocal();
        else
            photonView.RPC(nameof(RequestStopWatering), RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RequestBeginWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        BeginWateringLocal();
    }

    [PunRPC]
    public void RequestStopWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StopWateringLocal();
    }

    void BeginWateringLocal()
    {
        isWatering        = true;
        waterTimer        = 0f;
        hasGrowthStarted  = false;
        waterUI?.SetActive(true);
    }

    void StopWateringLocal()
    {
        isWatering = false;
        waterTimer = 0f;
        waterUI?.SetActive(false);
    }

    [PunRPC]
    public void RPC_GrowSmall()
    {
        // Destroy the socketed seed
        if (seedSocket.hasSelection && seedSocket.firstInteractableSelected != null)
        {
            var seedGO = seedSocket.firstInteractableSelected.transform.gameObject;
            if (seedGO.CompareTag(seedTag))
            {
                var pv = seedGO.GetComponent<PhotonView>();
                if (pv != null) PhotonNetwork.Destroy(seedGO);
                else           Destroy(seedGO);
            }
        }

        // Activate your small child
        if (smallPlantObject != null) smallPlantObject.SetActive(true);

        // Soil & UI
        soilRenderer.material = wetSoilMaterial;
        waterUI?.SetActive(false);
        timer1UI?.SetActive(true);

        // Schedule medium growth
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayedMedium());
    }

    private IEnumerator DelayedMedium()
    {
        yield return new WaitForSeconds(timeToMediumPlant);
        photonView.RPC(nameof(RPC_GrowMedium), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_GrowMedium()
    {
        // Swap small → medium
        smallPlantObject?.SetActive(false);
        if (mediumPlantObject != null) mediumPlantObject.SetActive(true);

        soilRenderer.material = wetSoilMaterial;
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayedFinal());
    }

    private IEnumerator DelayedFinal()
    {
        yield return new WaitForSeconds(timeToFinalReplace);
        photonView.RPC(nameof(RPC_GrowFinal), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_GrowFinal()
    {
        // Clean up any small/medium children
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(false);

        // Instantiate the final plant prefab under this pot
        var prefab = Resources.Load<GameObject>(finalPlantPrefabName);
        if (prefab != null)
            PhotonNetwork.Instantiate(
                finalPlantPrefabName,
                transform.position,
                transform.rotation
            );
        else
            Debug.LogError($"Missing Resources/{finalPlantPrefabName}");

        soilRenderer.material = drySoilMaterial;
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(true);
    }
}
