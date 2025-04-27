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
    public float startDelay = 5f;   // still used after button press
    public int maxNpcCount = 10;

    // ¡û change default to false!
    public bool SpawnTime = false;

    [Header("UI & Audio")]
    public GameObject canvas;
    public AudioClip timesUp;

    [Header("Other References")]
    public GameObject lightManager;
    public GameObject player;

    void Start()
    {
        // Nothing happens automatically anymore!
        // (We removed the Invoke(Begin) from here.)
    }

    /// <summary>
    /// Call *this* from your Start button OnClick (only on MasterClient).
    /// </summary>
    public void Begin()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (zeppelin != null && zeppelinDestination != null)
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
            zeppelin.transform.position = Vector3.Lerp(startPoint, endPoint, flyInCurve.Evaluate(t));
            yield return null;
        }

        zeppelin.transform.position = endPoint;
        StartGame();
    }

    void StartGame()
    {
        // now we actually start spawning
        SpawnTime = true;
        HideCanvas();
        lightManager?.SetActive(true);

        // schedule the first NPC spawn
        if (PhotonNetwork.IsMasterClient)
            Invoke(nameof(Spawn), startDelay);
    }

    void HideCanvas()
    {
        if (canvas != null)
            canvas.SetActive(false);
    }

    public void PlaySound2()
    {
        AudioSource.PlayClipAtPoint(timesUp, player.transform.position, 5f);
    }

    public void stopSpawn()
    {
        SpawnTime = false;
        CancelInvoke(nameof(Spawn));
    }

    public void Spawn()
    {
        if (!PhotonNetwork.IsMasterClient || !SpawnTime)
            return;

        int count = GameObject.FindGameObjectsWithTag("NPC").Length;
        if (count < maxNpcCount)
        {
            int idx = Random.Range(0, npc.Length);
            Vector3 pos = new Vector3(91, 1, -7);
            PhotonNetwork.Instantiate(
                npc[idx].name,
                pos,
                npc[idx].transform.rotation
            );
        }

        // schedule next
        float delay = Random.Range(7f, 12f);
        Invoke(nameof(Spawn), delay);
    }
}
