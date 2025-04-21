using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSpawner : MonoBehaviour
{
    public GameObject[] npc;

    public float startDelay = 5.0f;
    public int maxNpcCount = 10;

    public bool SpawnTime = true;

    public float duration = 2f; // Duration of the animation in seconds

    private bool isOpen = false; // Track whether the door is open

    public GameObject canvas; // Assign your Canvas GameObject in the Inspector
    public AudioClip timesUp;

    public GameObject lightManager;
    public GameObject player;

    /*public void facingPlayer()
    {
        if (canvas.activeSelf)
        {
            canvas.transform.LookAt(new Vector3(player.transform.forward.x, 0, player.transform.forward.z));
        }
        else
        {
            Debug.Log("No Canvas");
        }
    }*/

    public void Hide()
    {
        if (canvas != null)
        {
            canvas.SetActive(false); // Disable the Canvas GameObject
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Spawn()
    {
        // Count the number of NPCs currently in the scene
        int currentNpcCount = GameObject.FindGameObjectsWithTag("NPC").Length;

        float delay = Random.Range(7f, 12f);

        // Check if the count is below the maximum limit
        if (currentNpcCount < maxNpcCount && SpawnTime)
        {
            int npcIndex = Random.Range(0, npc.Length);
            Vector3 spawnPos = new Vector3(91, 1, -7);

            // Instantiate the NPC
            Instantiate(npc[npcIndex], spawnPos, npc[npcIndex].transform.rotation);
            Invoke("Spawn", delay);
        }
        else if (currentNpcCount > maxNpcCount && SpawnTime)
        {
            Debug.Log("Maximum NPC count reached. No more NPCs will be spawned.");
            Invoke("Spawn", 1f);
        }
        else if (!SpawnTime)
        {
            stopSpawn();
        }
    }

    public void PlaySound2()
    {
        // Create a temporary audio source to play the sound
        AudioSource.PlayClipAtPoint(timesUp, player.transform.position, 5.0f);
    }

    public void stopSpawn()
    {
        SpawnTime = false;
    }

    public void StartGame()
    {
        SpawnTime = true;
        Spawn();
        Hide();
        lightManager.SetActive(true);
    }
}
