using System.Collections;
using UnityEngine;
using Photon.Pun;

public class NpcSpawner : MonoBehaviourPun
{
    [Header("Zeppelin Fly-In Settings")]
    public GameObject zeppelin;
    public Transform zeppelinDestination;
    public float flyInDuration = 10f;
    public AnimationCurve flyInCurve;

    [Header("NPC Spawn Settings")]
    public GameObject[] npc;
    public float startDelay = 5f;
    public int   maxNpcCount = 10;

    bool SpawnTime = false;

    [Header("UI & Audio")]
    public GameObject canvas;
    public AudioClip timesUp;

    [Header("Other Refs")]
    public GameObject lightManager;
    public GameObject player;

    // 1) This is what your UI button should call (on ANY client)
    public void OnStartButtonPressed()
    {
        // Broadcast to everyone that we want to begin
        photonView.RPC(nameof(RPC_Begin), RpcTarget.All);
    }

    // 2) Runs on ALL clients: shows the fly-in + hides UI locally
    [PunRPC]
    void RPC_Begin()
    {
        StartCoroutine(FlyInZeppelin());
    }

    private IEnumerator FlyInZeppelin()
    {
        Vector3 startPoint = zeppelin.transform.position;
        Vector3 endPoint   = zeppelinDestination.position;
        float timer        = 0f;

        while (timer < flyInDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / flyInDuration);
            zeppelin.transform.position = Vector3.Lerp(
                startPoint, endPoint, flyInCurve.Evaluate(t)
            );
            yield return null;
        }

        zeppelin.transform.position = endPoint;

        // Now we hide the canvas & turn on lights on every client:
        canvas?.SetActive(false);
        lightManager?.SetActive(true);

        // And *ask* the MasterClient to actually start spawning NPCs:
        if (PhotonNetwork.IsMasterClient)
            StartGame();
    }

    // 3) MasterClient¨Conly: kicks off the spawn loop
    void StartGame()
    {
        SpawnTime = true;
        Invoke(nameof(Spawn), startDelay);
    }

    void Spawn()
    {
        if (!SpawnTime || !PhotonNetwork.IsMasterClient) return;

        int current = GameObject.FindGameObjectsWithTag("NPC").Length;
        if (current < maxNpcCount)
        {
            int idx = Random.Range(0, npc.Length);
            Vector3 pos = new Vector3(91, 1, -7);
            PhotonNetwork.Instantiate(
                npc[idx].name, pos, npc[idx].transform.rotation
            );
        }
        // schedule next
        float delay = Random.Range(7f, 12f);
        Invoke(nameof(Spawn), delay);
    }
}
