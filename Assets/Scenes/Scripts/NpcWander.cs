using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class NpcWander : MonoBehaviourPun, IPunObservable
{
    NavMeshAgent agent;
    Animator     m_Animator;

    [Header("Patrol Settings")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float range;

    [Header("Trash Spawn Settings")]
    public GameObject[] bottlePrefabs;
    public float        spawnPosY   = 1f;
    public float        startDelay  = 1f;
    public bool         spawnTrash  = false;
    public float        dropTimer   = 5f;
    public int          maxTrashCount = 25;
    public AudioClip    npcSound;

    [Header("Lifetime")]
    public float liveTime = 40f;

    // internal
    bool   dropping = false;
    float  time     = 0f;
    Vector3 destPoint;
    bool   walkPointSet;
    GameObject endPoint;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        agent      = GetComponent<NavMeshAgent>();
        endPoint   = GameObject.Find("EndPoint");

        // Only host schedules drop calls
        if (photonView.IsMine)
            Invoke(nameof(SpawnRandomBottle), startDelay);
    }

    void Update()
    {
        // Only owner/host drives liveTime & patrol decisions
        if (!photonView.IsMine) return;

        liveTime -= Time.deltaTime;

        if (!dropping)
            Patrol();
        else
        {
            agent.isStopped = true;
            time += Time.deltaTime;
            if (time >= dropTimer)
            {
                time     = 0f;
                dropping = false;
                m_Animator.SetTrigger("Walk");
            }
        }

        // Remove NPC after they wander off or time out
        if ((transform.position.x > 90 && liveTime < 0f) ||
            liveTime < -35f)
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(gameObject);
            else
                Destroy(gameObject);
        }
    }

    void Patrol()
    {
        agent.isStopped = false;

        // Drop logic
        int currentTrash = GameObject.FindGameObjectsWithTag("Trash").Length;
        spawnTrash = (transform.position.x < 75f && liveTime > 7f && currentTrash < maxTrashCount);

        if (!walkPointSet)
        {
            if (liveTime < 5f)
                GoHome();
            else
                SearchForDest();
        }
        else
        {
            agent.SetDestination(destPoint);
            if (Vector3.Distance(transform.position, destPoint) < 3f)
                walkPointSet = false;
        }
    }

    void SearchForDest()
    {
        float z = Random.Range(-22f, 15.5f);
        float x = Random.Range(34f, 75f);
        destPoint = new Vector3(x, transform.position.y, z);
        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            walkPointSet = true;
    }

    void GoHome()
    {
        destPoint = new Vector3(91f, transform.position.y, -7f);
        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
            walkPointSet = true;
    }

    void SpawnRandomBottle()
    {
        if (!photonView.IsMine) return;

        if (spawnTrash)
        {
            int idx = Random.Range(0, bottlePrefabs.Length);
            Vector3 spawnPos = new Vector3(transform.position.x, spawnPosY, transform.position.z);

            // networked spawn
            PhotonNetwork.Instantiate(
                bottlePrefabs[idx].name,
                spawnPos,
                bottlePrefabs[idx].transform.rotation
            );

            // play your sound & anim
            AudioSource.PlayClipAtPoint(npcSound, transform.position);
            m_Animator.SetTrigger("Drop");

            // schedule next drop
            float delay = Random.Range(15f, 26f);
            Invoke(nameof(SpawnRandomBottle), delay);
            dropping = true;
        }
        else
        {
            // re-try quickly until they start dropping
            Invoke(nameof(SpawnRandomBottle), 1f);
        }
    }

    // Sync position/rotation so movement looks smooth on all clients
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
