using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkedPositionReset : MonoBehaviourPun
{
    [Header("Settings")]
    [SerializeField] private float _returnDistance = 10f;
    [SerializeField] private float _checkInterval = 0.5f;
    
    private Vector3 _initialPosition;
    private Rigidbody _rb;
    private float _lastCheckTime;
    private bool _wasKinematic;

    void Start()
    {
        _initialPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
        
        if (PhotonNetwork.IsMasterClient)
        {
            _lastCheckTime = Time.time;
        }
        else
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || Time.time - _lastCheckTime < _checkInterval)
            return;

        _lastCheckTime = Time.time;
        
        // Only check objects that aren't currently owned
        if (photonView.Owner == null || photonView.Owner.IsMasterClient)
        {
            CheckPosition();
        }
    }

    void CheckPosition()
    {
        float currentDistance = Vector3.Distance(transform.position, _initialPosition);
        
        if (currentDistance > _returnDistance)
        {
            photonView.RequestOwnership();
            ResetObject();
        }
    }

    void ResetObject()
    {
        photonView.RPC("RPC_ResetObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ResetObject()
    {
        // Store original kinematic state
        _wasKinematic = _rb.isKinematic;
        
        // Temporarily make kinematic for reset
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = _initialPosition;
        
        // Restore kinematic state after reset
        StartCoroutine(RestorePhysics());
    }

    System.Collections.IEnumerator RestorePhysics()
    {
        yield return new WaitForSeconds(0.1f);
        _rb.isKinematic = _wasKinematic;
        
        // Return ownership to master if needed
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? _initialPosition : transform.position, _returnDistance);
    }
}