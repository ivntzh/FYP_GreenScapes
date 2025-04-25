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
    public float waterTimeRequired = 5f; // how long to hold under water

    // internal state (only MasterClient drives growth)
    private bool isWatering        = false;
    private bool hasGrowthStarted = false;
    private int  growthStage      = 0; // 0=pre-small,1=pre-medium,2=pre-final,3=done
    private float waterTimer      = 0f;
    private bool hasFinalSpawned  = false;  // track if final plant already spawned

    void Start()
    {
        // turn all off
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
            switch (growthStage)
            {
                case 0:
                    photonView.RPC(nameof(RPC_ShowStage1), RpcTarget.AllBuffered);
                    break;
                case 1:
                    photonView.RPC(nameof(RPC_ShowStage2), RpcTarget.AllBuffered);
                    break;
                case 2:
                    photonView.RPC(nameof(RPC_ShowFinal), RpcTarget.AllBuffered);
                    break;
            }
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
        // keep hasGrowthStarted so stage stays
    }

    [PunRPC]
    public void RPC_ShowBeginWatering()
    {
        // show water icon & start wait indicator
        waterUI?.SetActive(true);
        if (growthStage == 0 && timer1UI != null)
        {
            timer1UI.SetActive(true);
            var t = timer1UI.GetComponent<Timer>(); if (t != null) t.StartTimer();
        }
        else if (growthStage == 1 && timer2UI != null)
        {
            timer2UI.SetActive(true);
            var t = timer2UI.GetComponent<Timer>(); if (t != null) t.StartTimer();
        }
        else if (growthStage == 2 && timer3UI != null)
        {
            timer3UI.SetActive(true);
            var t = timer3UI.GetComponent<Timer>(); if (t != null) t.StartTimer();
        }
    }

    [PunRPC]
    public void RPC_ShowStopWatering()
    {
        // hide water icon & stop current wait indicator
        waterUI?.SetActive(false);
        if (growthStage == 0 && timer1UI != null)
        {
            var t = timer1UI.GetComponent<Timer>(); if (t != null) t.StopTimer();
            timer1UI.SetActive(false);
        }
        else if (growthStage == 1 && timer2UI != null)
        {
            var t = timer2UI.GetComponent<Timer>(); if (t != null) t.StopTimer();
            timer2UI.SetActive(false);
        }
        else if (growthStage == 2 && timer3UI != null)
        {
            var t = timer3UI.GetComponent<Timer>(); if (t != null) t.StopTimer();
            timer3UI.SetActive(false);
        }
    }

    [PunRPC]
    public void RPC_ShowStage1()
    {
        // complete stage1: small plant appears
        growthStage = 1;
        smallPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;
        // hide timer1
        if (timer1UI != null) { timer1UI.SetActive(false); }
        // prompt next water
        waterUI?.SetActive(true);
    }

    [PunRPC]
    public void RPC_ShowStage2()
    {
        // complete stage2: medium plant appears
        growthStage = 2;
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;
        // hide timer2
        if (timer2UI != null) { timer2UI.SetActive(false); }
        // prompt next water
        waterUI?.SetActive(true);
    }

    [PunRPC]
    public void RPC_ShowFinal()
    {
        // complete final: spawn final prefab
        growthStage = 3;
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
        // hide timer3
        if (timer3UI != null) { timer3UI.SetActive(false); }
    }
}
