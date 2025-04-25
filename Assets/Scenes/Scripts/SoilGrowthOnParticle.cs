using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SoilGrowthOnParticle : MonoBehaviourPunCallbacks
{
    [Header("Growth Objects")]
    public GameObject smallPlantObject;
    public GameObject mediumPlantObject;
    [Header("Final Prefab (in Resources/PhotonPrefabs)")]
    public string finalPlantPrefabName;

    [Header("Soil Visuals")]
    public MeshRenderer soilRenderer;
    public Material wetSoilMaterial;
    public Material drySoilMaterial;

    [Header("Seed Setup")]
    public XRSocketInteractor seedSocket;
    public string seedTag = "Seed";

    [Header("UI & Timers")]
    public GameObject waterUI;
    public GameObject timer1UI;
    public GameObject timer2UI;
    public GameObject timer3UI;

    [Header("Timing Settings")]
    public float waterTimeRequired = 5f;
    public float timer1Duration     = 7f;
    public float timer2Duration     = 7f;
    public float timer3Duration     = 7f;

    [Header("External Pot Pieces")]
    public PotManager potManager;
    public Dirt       dirtScript;

    private int   growthStage = 0;
    private bool  isWatering  = false;
    private float waterTimer  = 0f;

    void Start()
    {
        // initial state: everything off
        waterUI?.SetActive(false);
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(false);
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(false);
        if (soilRenderer != null && drySoilMaterial != null)
            soilRenderer.material = drySoilMaterial;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isWatering) return;
        waterTimer += Time.deltaTime;
        if (waterTimer >= waterTimeRequired)
        {
            isWatering = false;
            waterTimer = 0f;
            photonView.RPC(nameof(RPC_HideWaterUI), RpcTarget.All);
            photonView.RPC(nameof(RPC_StartTimerForStage), RpcTarget.All, growthStage + 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (PhotonNetwork.IsMasterClient)
        {
            BeginWatering();
            photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.All);
        }
        else
            photonView.RPC(nameof(RequestBeginWatering), RpcTarget.MasterClient);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water") || !PhotonNetwork.IsMasterClient) return;
        isWatering = false;
        waterTimer = 0f;
        photonView.RPC(nameof(RPC_HideWaterUI), RpcTarget.All);
    }

    [PunRPC]
    public void RequestBeginWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        BeginWatering();
        photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.All);
    }

    void BeginWatering()
    {
        isWatering = true;
        waterTimer = 0f;
    }

    [PunRPC] void RPC_ShowWaterUI() => waterUI?.SetActive(true);
    [PunRPC] void RPC_HideWaterUI() => waterUI?.SetActive(false);

    [PunRPC]
    public void RPC_StartTimerForStage(int stage)
    {
        switch (stage)
        {
            case 1:
                ActivateTimer(timer1UI, timer1UI?.GetComponent<Timer>());
                if (PhotonNetwork.IsMasterClient)
                    StartCoroutine(DelayedGrowStage(1, timer1Duration));
                break;
            case 2:
                ActivateTimer(timer2UI, timer2UI?.GetComponent<Timer>());
                if (PhotonNetwork.IsMasterClient)
                    StartCoroutine(DelayedGrowStage(2, timer2Duration));
                break;
            case 3:
                ActivateTimer(timer3UI, timer3UI?.GetComponent<Timer>());
                if (PhotonNetwork.IsMasterClient)
                    StartCoroutine(DelayedGrowStage(3, timer3Duration));
                break;
        }
    }

    void ActivateTimer(GameObject panel, Timer timer)
    {
        panel?.SetActive(true);
        timer?.StartTimer();
    }

    IEnumerator DelayedGrowStage(int stage, float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC(nameof(RPC_GrowStage), RpcTarget.All, stage);
    }

    [PunRPC]
    public void RPC_GrowStage(int stage)
    {
        switch (stage)
        {
            case 1:
                timer1UI?.GetComponent<Timer>()?.StopTimer();
                timer1UI?.SetActive(false);
                smallPlantObject?.SetActive(true);
                DestroySeedInSocket();
                if (soilRenderer != null && wetSoilMaterial != null)
                    soilRenderer.material = wetSoilMaterial;
                break;
            case 2:
                timer2UI?.GetComponent<Timer>()?.StopTimer();
                timer2UI?.SetActive(false);
                mediumPlantObject?.SetActive(true);
                if (soilRenderer != null && wetSoilMaterial != null)
                    soilRenderer.material = wetSoilMaterial;
                break;
            case 3:
                timer3UI?.GetComponent<Timer>()?.StopTimer();
                timer3UI?.SetActive(false);
                mediumPlantObject?.SetActive(false);
                if (soilRenderer != null && drySoilMaterial != null)
                    soilRenderer.material = drySoilMaterial;
                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Instantiate(finalPlantPrefabName, transform.position, transform.rotation);
                photonView.RPC(nameof(RPC_ResetGrowth), RpcTarget.All);
                break;
        }
        growthStage = stage;
        if (stage < 3)
            photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.All);
    }

    void DestroySeedInSocket()
    {
        if (seedSocket == null) return;
        foreach (var child in seedSocket.transform.GetComponentsInChildren<Transform>(true))
        {
            if (!child.CompareTag(seedTag)) continue;
            var go = child.gameObject;
            var pv = go.GetComponent<PhotonView>();
            if (pv != null && PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(go);
            else if (pv == null)
                Destroy(go);
        }
    }

    [PunRPC]
    public void RPC_ResetGrowth()
    {
        // reset visuals
        growthStage = 0;
        isWatering  = false;
        waterTimer  = 0f;
        waterUI?.SetActive(false);
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(false);
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(false);
        if (soilRenderer != null && drySoilMaterial != null)
            soilRenderer.material = drySoilMaterial;

        // flat and mixed dirt back to initial
        if (potManager != null)
        {
            potManager.flatDirt?.SetActive(false);
            potManager.enabled = true;
        }
        if (dirtScript != null)
            dirtScript.ResetToInitial();
    }
}