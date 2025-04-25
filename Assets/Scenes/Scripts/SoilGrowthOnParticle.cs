using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SoilGrowthOnParticle : MonoBehaviourPunCallbacks
{
    [Header("Growth Objects")]
    public GameObject smallPlantObject;
    public GameObject mediumPlantObject;
    [Header("Final Plant Prefab (in Resources/PhotonPrefabs)")]
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

    // internal state (only master drives growth)
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
        photonView.RPC(nameof(RPC_BeginWatering), RpcTarget.AllBuffered);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        photonView.RPC(nameof(RPC_StopWatering), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_BeginWatering(PhotonMessageInfo info)
    {
        // show UI everywhere
        waterUI?.SetActive(true);
        // only master updates timer
        if (PhotonNetwork.IsMasterClient)
        {
            isWatering       = true;
            hasGrowthStarted = false;
            waterTimer       = 0f;
        }
    }

    [PunRPC]
    public void RPC_StopWatering(PhotonMessageInfo info)
    {
        waterUI?.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
            isWatering = false;
    }

    [PunRPC]
    public void RPC_GrowSmall()
    {
        // remove socketed seed
        if (seedSocket.hasSelection && seedSocket.firstInteractableSelected != null)
        {
            var seedGO = seedSocket.firstInteractableSelected.transform.gameObject;
            if (seedGO.CompareTag(seedTag))
            {
                PhotonNetwork.Destroy(seedGO);
            }
        }

        smallPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;
        timer1UI?.SetActive(true);

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
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(true);
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
        mediumPlantObject?.SetActive(false);
        smallPlantObject?.SetActive(false);

        PhotonNetwork.Instantiate(
            finalPlantPrefabName,
            transform.position,
            transform.rotation
        );

        soilRenderer.material = drySoilMaterial;
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(true);
    }
}