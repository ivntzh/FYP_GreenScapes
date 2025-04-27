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
    public GameObject[] npc;        // your NPC prefabs
    public float startDelay = 5f;   // initial delay before first spawn
    public int maxNpcCount = 10;
    public bool SpawnTime = true;

    [Header("UI & Audio")]
    public GameObject canvas;       // your canvas to hide
    public AudioClip timesUp;       // sound to play on time up

    [Header("Other References")]
    public GameObject lightManager;
    public GameObject player;       // for audio position

    void Start()
    {
        // Only the host (MasterClient) kicks things off
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(Begin), startDelay);
        }
    }

    /// <summary>
    /// Starts the zeppelin fly-in. Called by MasterClient.
    /// </summary>
    public void Begin()
    {
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
            float curved = flyInCurve.Evaluate(t);
            zeppelin.transform.position = Vector3.Lerp(startPoint, endPoint, curved);
            yield return null;
        }

        zeppelin.transform.position = endPoint;

        // After fly-in, start the game
        StartGame();
    }

    /// <summary>
    /// Hides the canvas, enables lights, and begins NPC spawning.
    /// </summary>
    public void StartGame()
    {
        SpawnTime = true;
        Hide();
        lightManager?.SetActive(true);

        // MasterClient schedules the first spawn
        if (PhotonNetwork.IsMasterClient)
            Invoke(nameof(Spawn), 0f);
    }

    void Hide()
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
    }

    /// <summary>
    /// MasterClient-only: spawn NPCs up to max, with random delays.
    /// </summary>
    public void Spawn()
    {
        if (!PhotonNetwork.IsMasterClient || !SpawnTime) return;

        int currentNpc = GameObject.FindGameObjectsWithTag("NPC").Length;
        if (currentNpc < maxNpcCount)
        {
            int idx = Random.Range(0, npc.Length);
            Vector3 spawnPos = new Vector3(91, 1, -7);

            // networked instantiate
            PhotonNetwork.Instantiate(
                npc[idx].name,
                spawnPos,
                npc[idx].transform.rotation
            );
        }

        // schedule next attempt
        float delay = Random.Range(7f, 12f);
        Invoke(nameof(Spawn), delay);
    }
}
