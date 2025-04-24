using System.Collections;
using UnityEngine;

using Photon.Pun;

public class SoilGrowthOnParticle : MonoBehaviourPunCallbacks
{
    [Header("Growth Prefabs (must live in Resources/)")]
    public string smallPlantPrefabName;
    public string mediumPlantPrefabName;
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

    // internal state (only truly driven on host)
    private bool   isWatering        = false;
    private bool   hasGrowthStarted = false;
    private float  waterTimer       = 0f;

    void Update()
    {
        // Only MasterClient should drive the growth timer & trigger
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
        {
            BeginWateringLocal();
        }
        else
        {
            // client asks host to begin watering
            photonView.RPC(nameof(RequestBeginWatering), RpcTarget.MasterClient);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water")) return;

        if (PhotonNetwork.IsMasterClient)
        {
            StopWateringLocal();
        }
        else
        {
            // client asks host to stop watering
            photonView.RPC(nameof(RequestStopWatering), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestBeginWatering(PhotonMessageInfo info)
    {
        // Only host should handle
        if (!PhotonNetwork.IsMasterClient) return;
        BeginWateringLocal();
    }

    [PunRPC]
    void RequestStopWatering(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StopWateringLocal();
    }

    // Host‐only helpers
    void BeginWateringLocal()
    {
        isWatering        = true;
        waterTimer        = 0f;
        hasGrowthStarted  = false;
        if (waterUI != null) waterUI.SetActive(true);
    }

    void StopWateringLocal()
    {
        isWatering = false;
        waterTimer = 0f;
        if (waterUI != null) waterUI.SetActive(false);
    }

    [PunRPC]
    void RPC_GrowSmall()
    {
        // 1) Destroy socketed seed
        if (seedSocket.hasSelection && seedSocket.firstInteractableSelected != null)
        {
            var seedGO = seedSocket.firstInteractableSelected.transform.gameObject;
            if (seedGO.CompareTag(seedTag))
            {
                var pv = seedGO.GetComponent<PhotonView>();
                if (pv != null)
                    PhotonNetwork.Destroy(seedGO);
                else
                    Destroy(seedGO);
            }
        }

        // 2) Spawn small plant
        var prefab = Resources.Load<GameObject>(smallPlantPrefabName);
        if (prefab != null)
            Instantiate(prefab,
                        seedSocket.transform.position,
                        Quaternion.identity,
                        transform);
        else
            Debug.LogError($"Missing Resources/{smallPlantPrefabName}");

        // 3) Soil UI updates
        if (soilRenderer != null) soilRenderer.material = wetSoilMaterial;
        if (waterUI   != null)    waterUI.SetActive(false);
        if (timer1UI  != null)    timer1UI.SetActive(true);

        // 4) Next stage
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DelayedMedium());
    }

    private IEnumerator DelayedMedium()
    {
        yield return new WaitForSeconds(timeToMediumPlant);
        photonView.RPC(nameof(RPC_GrowMedium), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_GrowMedium()
    {
        // Remove small, spawn medium
        foreach (Transform t in transform)
            if (t.name.Contains("SmallPlant")) Destroy(t.gameObject);

        var prefab = Resources.Load<GameObject>(mediumPlantPrefabName);
        if (prefab != null)
            Instantiate(prefab,
                        seedSocket.transform.position,
                        Quaternion.identity,
                        transform);

        if (soilRenderer != null) soilRenderer.material = wetSoilMaterial;
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
    void RPC_GrowFinal()
    {
        // Destroy any intermediate
        foreach (Transform t in transform)
            Destroy(t.gameObject);

        var prefab = Resources.Load<GameObject>(finalPlantPrefabName);
        if (prefab != null)
            Instantiate(prefab,
                        transform.position,
                        transform.rotation);

        if (soilRenderer != null) soilRenderer.material = drySoilMaterial;
        timer2UI?.SetActive(false);
        timer3UI?.SetActive(true);
    }
}
