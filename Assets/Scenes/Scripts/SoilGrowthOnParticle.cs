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
    public GameObject waterUI;      // water icon panel
    public GameObject timer1UI;    // panel with Timer script for stage 1
    public GameObject timer2UI;    // panel with Timer script for stage 2
    public GameObject timer3UI;    // panel with Timer script for stage 3

    [Header("Timing Settings")]
    public float waterTimeRequired = 2f;
    public float timeToMediumPlant = 7f;
    public float timeToFinalReplace = 10f;

    // internal state (only MasterClient drives growth)
    private bool isWatering = false;
    private bool hasGrowthStarted = false;
    private float waterTimer = 0f;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isWatering) return;
        waterTimer += Time.deltaTime;
        if (waterTimer >= waterTimeRequired && !hasGrowthStarted)
        {
            hasGrowthStarted = true;
            waterTimer = 0f;
            photonView.RPC(nameof(RPC_GrowSmall), RpcTarget.AllBuffered);
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
        Debug.Log("RPC_BeginWatering called");
        waterUI?.SetActive(true);
        if (timer1UI != null)
        {
            timer1UI.SetActive(true);
            var t = timer1UI.GetComponent<Timer>();
            if (t != null)
            {
                t.StartTimer();
                Debug.Log("Timer 1 started");
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            isWatering = true;
            hasGrowthStarted = false;
            waterTimer = 0f;
        }
    }

    [PunRPC]
    public void RPC_StopWatering(PhotonMessageInfo info)
    {
        Debug.Log("RPC_StopWatering called");
        waterUI?.SetActive(false);
        if (timer1UI != null)
        {
            var t = timer1UI.GetComponent<Timer>();
            if (t != null)
            {
                t.StopTimer();
                Debug.Log("Timer 1 stopped");
            }
            timer1UI.SetActive(false);
        }
        if (PhotonNetwork.IsMasterClient)
            isWatering = false;
    }

    [PunRPC]
    public void RPC_GrowSmall()
    {
        Debug.Log("RPC_GrowSmall called");
        if (PhotonNetwork.IsMasterClient && seedSocket.hasSelection)
        {
            var seedGO = seedSocket.firstInteractableSelected?.transform.gameObject;
            if (seedGO != null && seedGO.CompareTag(seedTag))
                PhotonNetwork.Destroy(seedGO);
        }

        smallPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;

        if (timer1UI != null)
        {
            var t1 = timer1UI.GetComponent<Timer>();
            if (t1 != null)
            {
                t1.StopTimer();
                Debug.Log("Timer 1 stopped");
            }
            timer1UI.SetActive(false);
        }
        if (timer2UI != null)
        {
            timer2UI.SetActive(true);
            var t2 = timer2UI.GetComponent<Timer>();
            if (t2 != null)
            {
                t2.StartTimer();
                Debug.Log("Timer 2 started");
            }
        }

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
        Debug.Log("RPC_GrowMedium called");
        smallPlantObject?.SetActive(false);
        mediumPlantObject?.SetActive(true);
        soilRenderer.material = wetSoilMaterial;

        if (timer2UI != null)
        {
            var t2 = timer2UI.GetComponent<Timer>();
            if (t2 != null)
            {
                t2.StopTimer();
                Debug.Log("Timer 2 stopped");
            }
            timer2UI.SetActive(false);
        }
        if (timer3UI != null)
        {
            timer3UI.SetActive(true);
            var t3 = timer3UI.GetComponent<Timer>();
            if (t3 != null)
            {
                t3.StartTimer();
                Debug.Log("Timer 3 started");
            }
        }

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
        Debug.Log("RPC_GrowFinal called");
        mediumPlantObject?.SetActive(false);
        smallPlantObject?.SetActive(false);

        PhotonNetwork.Instantiate(
            finalPlantPrefabName,
            transform.position,
            transform.rotation
        );

        soilRenderer.material = drySoilMaterial;

        if (timer3UI != null)
        {
            var t3 = timer3UI.GetComponent<Timer>();
            if (t3 != null)
            {
                t3.StopTimer();
                Debug.Log("Timer 3 stopped");
            }
            timer3UI.SetActive(false);
        }
    }
}