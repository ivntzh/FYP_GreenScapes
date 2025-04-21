using UnityEngine;
using UnityEngine.AI;

public class NpcWander : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField] LayerMask groundLayer;

    Animator m_Animator;

    //patrol
    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float range;

    public GameObject[] bottlePrefabs;
    private float spawnPosY = 1f;
    private float startDelay = 1.0f;

    public AudioClip npcSound;

    public float liveTime = 40f;

    public bool spawnTrash = false;

    public bool dropping = false;

    public float dropTimer = 5f, time;

    public int maxTrashCount = 25;

    GameObject endPoint;

    void SpawnRandomBottle()
    {
        int ballIndex = Random.Range(0, bottlePrefabs.Length);
        Vector3 spawnPos = new Vector3(transform.position.x, spawnPosY, transform.position.z);

        if (spawnTrash == true)
        {
            Instantiate(bottlePrefabs[ballIndex], spawnPos, bottlePrefabs[ballIndex].transform.rotation);

            PlaySound();
            m_Animator.SetTrigger("Drop");
            float delay = Random.Range(15f, 26f);
            Invoke("SpawnRandomBottle", delay);
            dropping = true;
            m_Animator.SetTrigger("Walk");
        }
        else
        {
            Invoke("SpawnRandomBottle", 1f);
        }
    }

    private void PlaySound()
    {
        // Create a temporary audio source to play the sound
        AudioSource.PlayClipAtPoint(npcSound, transform.position);
    }


    // Start is called before the first frame update
    void Start()
    {
        m_Animator = gameObject.GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        Invoke("SpawnRandomBottle", startDelay);
        endPoint = GameObject.Find("EndPoint");
    }

    // Update is called once per frame
    void Update()
    {

        if (!dropping)
        {
            Patrol();
        }
        else if (dropping && liveTime > 7)
        {
            agent.isStopped = true;

            time += Time.deltaTime;
            if (time >= dropTimer)
            {
                time = 0;
                dropping = false;
            }
        }

        if (liveTime < 7)
        {
            spawnTrash = false;
            dropping = false;
        }
    }

    void Patrol()
    {
        liveTime -= Time.deltaTime;
        agent.isStopped = false;
        int currentTrashCount = GameObject.FindGameObjectsWithTag("Trash").Length;


        if (!walkPointSet)
        {
            if (liveTime < 5)
            {
                GoHome();
            }
            else
            {
                SearchForDest();
            }
        }

        else if (walkPointSet)
        {
            agent.SetDestination(destPoint);
        }

        if (Vector3.Distance(transform.position, destPoint) < 3 || dropping == true)
        {
            walkPointSet = false;
        }

        if (transform.position.x > 90 && liveTime < 0)
        {
            Destroy(gameObject);
        }

        if (liveTime < -35)
        {
            Destroy(gameObject);
        }

        if (transform.position.x < 75 && liveTime > 7 && currentTrashCount < maxTrashCount)
        {
            spawnTrash = true;
        }
        else
        {
            spawnTrash = false;
            dropping = false;
        }
    }

    void SearchForDest()
    {

        float z = Random.Range(-22.0f, 15.5f);
        float x = Random.Range(34.0f, 75.0f);

        destPoint = new Vector3(x, transform.position.y, z);

        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
        {
            walkPointSet = true;
        }
    }

    void GoHome()
    {
        destPoint = new Vector3(91, transform.position.y, -7);

        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
        {
            walkPointSet = true;
        }

    }
}
