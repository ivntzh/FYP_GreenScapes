// SoilGrowthOnParticle.cs
using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SoilGrowthOnParticle : MonoBehaviourPunCallbacks
{
    [Header("Growth Objects")]
    public GameObject smallPlantObject;    // baked-in child
    public GameObject mediumPlantObject;   // baked-in child
    [Header("Final Prefab (in Resources/PhotonPrefabs)")]
    public string finalPlantPrefabName;

    [Header("Soil Visuals")]
    public MeshRenderer soilRenderer;
    public Material wetSoilMaterial;
    public Material drySoilMaterial;

    [Header("Seed Setup")]
    public XRSocketInteractor seedSocket;  // where the seed sits
    public string seedTag = "Seed";

    [Header("UI & Timers")]
    public GameObject waterUI;
    public GameObject timer1UI;  // with Timer component
    public GameObject timer2UI;
    public GameObject timer3UI;

    [Header("Timing Settings")]
    public float waterTimeRequired = 5f;
    public float timer1Duration     = 7f;
    public float timer2Duration     = 7f;
    public float timer3Duration     = 7f;

    // internal state (host-authoritative)
    int   growthStage = 0;     // 0=none,1=small,2=medium,3=final
    bool  isWatering  = false;
    float waterTimer  = 0f;

    private bool hasFinalSpawned = false;
    private bool hasGrowthStarted = false;

    void Start()
    {
        // hide everything at launch
        waterUI?.SetActive(false);
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(false);
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(false);

        // Validate critical inspector references
        if (soilRenderer   == null) Debug.LogError("SoilGrowthOnParticle: soilRenderer not assigned");
        if (wetSoilMaterial== null) Debug.LogError("SoilGrowthOnParticle: wetSoilMaterial not assigned");
        if (drySoilMaterial== null) Debug.LogError("SoilGrowthOnParticle: drySoilMaterial not assigned");
        if (waterUI        == null) Debug.LogError("SoilGrowthOnParticle: waterUI not assigned");
    }

    void Update()
    {
        // only the host drives the watering countdown
        if (!PhotonNetwork.IsMasterClient || !isWatering) return;

        waterTimer += Time.deltaTime;
        if (waterTimer >= waterTimeRequired)
        {
            isWatering = false;
            waterTimer = 0f;

            // hide the prompt & start the next timer (no buffering)
            photonView.RPC(nameof(RPC_HideWaterUI), RpcTarget.All);
            photonView.RPC(
                nameof(RPC_StartTimerForStage),
                RpcTarget.All,
                growthStage + 1
            );
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
        {
            photonView.RPC(nameof(RequestBeginWatering), RpcTarget.MasterClient);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (!PhotonNetwork.IsMasterClient) return;

        // cancel watering & hide UI
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

    // === UI RPCs ===
    [PunRPC]
    public void RPC_ShowWaterUI() => waterUI?.SetActive(true);

    [PunRPC]
    public void RPC_HideWaterUI() => waterUI?.SetActive(false);

    // start the stage-delay timer on all clients (no buffering)
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
                if (PhotonNetwork.IsMasterClient) DestroySeedInPot();
                smallPlantObject?.SetActive(true);
                if (soilRenderer!=null && wetSoilMaterial!=null)
                    soilRenderer.material = wetSoilMaterial;
                break;

            case 2:
                timer2UI?.GetComponent<Timer>()?.StopTimer();
                timer2UI?.SetActive(false);
                mediumPlantObject?.SetActive(true);
                if (soilRenderer!=null && wetSoilMaterial!=null)
                    soilRenderer.material = wetSoilMaterial;
                break;

            case 3:
                timer3UI?.GetComponent<Timer>()?.StopTimer();
                timer3UI?.SetActive(false);
                // — point 3: avoid final overlap —
                mediumPlantObject?.SetActive(false);
                if (soilRenderer!=null && drySoilMaterial!=null)
                    soilRenderer.material = drySoilMaterial;
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Instantiate(
                        finalPlantPrefabName,
                        transform.position,
                        transform.rotation
                    );
                }
                break;
        }

        growthStage = stage;

        // after each non-final growth, show water UI again
        if (stage < 3)
            photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.All);
    }

    void DestroySeedInPot()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag(seedTag))
            {
                var pv = child.GetComponent<PhotonView>();
                if (pv != null) PhotonNetwork.Destroy(child.gameObject);
                else           Destroy(child.gameObject);
            }
        }
    }

    [PunRPC]
    public void RPC_ResetGrowth()
    {
        // Reset host state
        growthStage       = 0;
        isWatering        = false;
        hasGrowthStarted  = false;
        waterTimer        = 0f;
        hasFinalSpawned   = false;

        // Hide everything client‐side
        waterUI?.SetActive(false);
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(false);
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(false);

        // Reset soil back to initial (dry) look
        if (soilRenderer!=null && drySoilMaterial!=null)
            soilRenderer.material = drySoilMaterial;
    }
}
