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

    [Header("UI Panels and Timers")]
    public GameObject waterUI;
    public GameObject timer1UI;
    public GameObject timer2UI;
    public GameObject timer3UI;

    [Header("Timing Settings")]
    public float waterTimeRequired = 2f;
    public float timeToMediumPlant  = 7f;
    public float timeToFinalReplace = 10f;

    // internal state (only MasterClient drives growth)
    private bool isWatering        = false;
    private bool hasGrowthStarted = false;
    private bool hasFinalSpawned  = false;
    private float waterTimer      = 0f;

    void Start()
    {
        // ensure all UI/timers are off at start
        waterUI?.SetActive(false);
        timer1UI?.SetActive(false);
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(false);
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isWatering) return;
        waterTimer += Time.deltaTime;
        if (waterTimer >= waterTimeRequired && !hasGrowthStarted)
        {
            hasGrowthStarted = true;
            waterTimer       = 0f;
            photonView.RPC(nameof(RPC_ShowStage1), RpcTarget.AllBuffered);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (PhotonNetwork.IsMasterClient)
        {
            BeginWateringLocal();
            photonView.RPC(nameof(RPC_ShowBeginWatering), RpcTarget.AllBuffered);
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
            StopWateringLocal();
            photonView.RPC(nameof(RPC_ShowStopWatering), RpcTarget.AllBuffered);
        }
        else
        {
            photonView.RPC(nameof(RequestStopWatering), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    public void RequestBeginWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        BeginWateringLocal();
        photonView.RPC(nameof(RPC_ShowBeginWatering), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RequestStopWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StopWateringLocal();
        photonView.RPC(nameof(RPC_ShowStopWatering), RpcTarget.AllBuffered);
    }

    // MasterClient-only: toggles internal state
    void BeginWateringLocal()
    {
        isWatering        = true;
        hasGrowthStarted  = false;
        waterTimer        = 0f;
    }

    void StopWateringLocal()
    {
        isWatering = false;
        waterTimer = 0f;
        // note: do not reset hasGrowthStarted here to preserve stage 1 once triggered
    }

    // === UI & Timer RPCs ===
    [PunRPC]
    public void RPC_ShowBeginWatering()
    {
        waterUI?.SetActive(true);
        if (timer1UI != null)
        {
            timer1UI.SetActive(true);
            var t = timer1UI.GetComponent<Timer>(); if (t != null) t.StartTimer();
        }
    }

    [PunRPC]
    public void RPC_ShowStopWatering()
    {
        waterUI?.SetActive(false);
        if (timer1UI != null)
        {
            var t = timer1UI.GetComponent<Timer>(); if (t != null) t.StopTimer();
            timer1UI.SetActive(false);
        }
    }

    [PunRPC]
    public void RPC_ShowStage1()
    {
        // Stage 1 complete -> show small plant and timer2
        if (PhotonNetwork.IsMasterClient)
        {
            if (seedSocket.hasSelection)
            {
                var seedGO = seedSocket.firstInteractableSelected?.transform.gameObject;
                if (seedGO != null && seedGO.CompareTag(seedTag))
                    PhotonNetwork.Destroy(seedGO);
            }
        }
        smallPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;

        // switch timers
        if (timer1UI != null)
        {
            var t1 = timer1UI.GetComponent<Timer>(); if (t1 != null) t1.StopTimer();
            timer1UI.SetActive(false);
        }
        if (timer2UI != null)
        {
            timer2UI.SetActive(true);
            var t2 = timer2UI.GetComponent<Timer>(); if (t2 != null) t2.StartTimer();
        }
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayedMedium());
    }

    private IEnumerator DelayedMedium()
    {
        yield return new WaitForSeconds(timeToMediumPlant);
        photonView.RPC(nameof(RPC_ShowStage2), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_ShowStage2()
    {
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;

        // switch timers
        if (timer2UI != null)
        {
            var t2 = timer2UI.GetComponent<Timer>(); if (t2 != null) t2.StopTimer();
            timer2UI.SetActive(false);
        }
        if (timer3UI != null)
        {
            timer3UI.SetActive(true);
            var t3 = timer3UI.GetComponent<Timer>(); if (t3 != null) t3.StartTimer();
        }
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayedFinal());
    }

    private IEnumerator DelayedFinal()
    {
        yield return new WaitForSeconds(timeToFinalReplace);
        photonView.RPC(nameof(RPC_ShowFinal), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_ShowFinal()
    {
        mediumPlantObject?.SetActive(false);
        smallPlantObject?.SetActive(false);

        if (PhotonNetwork.IsMasterClient && !hasFinalSpawned)
        {
            PhotonNetwork.Instantiate(
                finalPlantPrefabName,
                transform.position,
                transform.rotation
            );
            hasFinalSpawned = true;
        }
        soilRenderer.material = drySoilMaterial;

        if (timer3UI != null)
        {
            var t3 = timer3UI.GetComponent<Timer>(); if (t3 != null) t3.StopTimer();
            timer3UI.SetActive(false);
        }
    }
}
