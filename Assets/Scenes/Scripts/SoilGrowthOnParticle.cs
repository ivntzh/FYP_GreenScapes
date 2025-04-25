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

    void Start()
    {
        // hide everything at launch
        waterUI?.SetActive(false);
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(false);
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(false);
    }

    void Update()
    {
        // only the host drives the watering countdown
        if (!PhotonNetwork.IsMasterClient || !isWatering) return;

        waterTimer += Time.deltaTime;
        if (waterTimer >= waterTimeRequired)
        {
            // done watering for this stage
            isWatering = false;
            waterTimer = 0f;

            // hide the prompt & start the appropriate timer
            photonView.RPC(nameof(RPC_HideWaterUI), RpcTarget.AllBuffered);
            photonView.RPC(
                nameof(RPC_StartTimerForStage),
                RpcTarget.AllBuffered,
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
            photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.AllBuffered);
        }
        else
        {
            photonView.RPC(nameof(RequestBeginWatering), RpcTarget.MasterClient);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water")) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // cancel watering if they pull out early
            isWatering = false;
            waterTimer = 0f;
        }
    }

    // client→host RPCs
    [PunRPC]
    public void RequestBeginWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        BeginWatering();
        photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.AllBuffered);
    }

    void BeginWatering()
    {
        isWatering       = true;
        waterTimer       = 0f;
    }

    // === UI RPCs ===

    [PunRPC]
    public void RPC_ShowWaterUI()
    {
        waterUI?.SetActive(true);
    }

    [PunRPC]
    public void RPC_HideWaterUI()
    {
        waterUI?.SetActive(false);
    }

    // Called with 1,2,3 to start each wait-timer
    [PunRPC]
    public void RPC_StartTimerForStage(int stage)
    {
        switch (stage)
        {
            case 1:
                ActivateTimer(timer1UI, timer1UI.GetComponent<Timer>());
                if (PhotonNetwork.IsMasterClient)
                    StartCoroutine(DelayedGrowStage(1, timer1Duration));
                break;
            case 2:
                ActivateTimer(timer2UI, timer2UI.GetComponent<Timer>());
                if (PhotonNetwork.IsMasterClient)
                    StartCoroutine(DelayedGrowStage(2, timer2Duration));
                break;
            case 3:
                ActivateTimer(timer3UI, timer3UI.GetComponent<Timer>());
                if (PhotonNetwork.IsMasterClient)
                    StartCoroutine(DelayedGrowStage(3, timer3Duration));
                break;
        }
    }

    void ActivateTimer(GameObject panel, Timer timer)
    {
        panel?.SetActive(true);
        if (timer != null) timer.StartTimer();
    }

    // Wait for the given duration, then grow
    IEnumerator DelayedGrowStage(int stage, float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC(nameof(RPC_GrowStage), RpcTarget.AllBuffered, stage);
    }

    // All-clients RPC to show the plant & reset UI for the next cycle
    [PunRPC]
    public void RPC_GrowStage(int stage)
    {
        // hide _only_ that stage’s timer
        switch (stage)
        {
            case 1:
                timer1UI?.GetComponent<Timer>()?.StopTimer();
                timer1UI?.SetActive(false);
                // destroy any seed under the pot
                if (PhotonNetwork.IsMasterClient)
                    DestroySeedInPot();
                // show small plant
                smallPlantObject?.SetActive(true);
                soilRenderer.material = wetSoilMaterial;
                break;

            case 2:
                timer2UI?.GetComponent<Timer>()?.StopTimer();
                timer2UI?.SetActive(false);
                mediumPlantObject?.SetActive(true);
                soilRenderer.material = wetSoilMaterial;
                break;

            case 3:
                timer3UI?.GetComponent<Timer>()?.StopTimer();
                timer3UI?.SetActive(false);
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

        // after growing each stage except final, prompt with water UI again
        if (stage < 3)
            photonView.RPC(nameof(RPC_ShowWaterUI), RpcTarget.AllBuffered);
    }

    void DestroySeedInPot()
    {
        // Look for any seed child under this transform
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
}
