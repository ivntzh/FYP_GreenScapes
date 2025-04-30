using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkedPositionReset : MonoBehaviourPun
{
    [SerializeField] private float _maxDistance = 10f;
    private Vector3 _startPosition;
    private Rigidbody _rb;

    void Start()
    {
        _startPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
        
        if (!PhotonNetwork.IsMasterClient) enabled = false;
    }

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (Vector3.Distance(transform.position, _startPosition) > _maxDistance)
        {
            // Master takes ownership, resets, then releases
            photonView.RequestOwnership();
            ResetObject();
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    void ResetObject()
    {
        photonView.RPC("RPC_ResetObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ResetObject()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = _startPosition;
        _rb.position = _startPosition; // Double-update for physics
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? _startPosition : transform.position, _maxDistance);
    }
}