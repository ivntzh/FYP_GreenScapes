using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class NetworkedPositionReset : MonoBehaviourPun
{
    [Header("Settings")]
    [SerializeField] private float _returnDistance = 10f;
    [SerializeField] private float _returnSpeed = 5f;
    [SerializeField] private bool _useSmoothReturn = true;

    private Vector3 _initialPosition;
    private Rigidbody _rb;
    private bool _isReturning = false;

    void Start()
    {
        _initialPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
        
        // Only master client handles position reset logic
        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
        }
    }

    void Update()
    {
        // Only check if we have authority or it's unowned
        if (photonView.Owner == null || photonView.IsMine)
        {
            float currentDistance = Vector3.Distance(transform.position, _initialPosition);
            
            if (currentDistance > _returnDistance && !_isReturning)
            {
                StartReturnToOrigin();
            }
        }

        if (_isReturning)
        {
            if (_useSmoothReturn)
            {
                SmoothReturn();
            }
            else
            {
                InstantReturn();
            }
        }
    }

    void StartReturnToOrigin()
    {
        _isReturning = true;
        
        // Request ownership before resetting
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }
    }

    void SmoothReturn()
    {
        transform.position = Vector3.Lerp(transform.position, _initialPosition, _returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _initialPosition) < 0.1f)
        {
            FinishReturn();
        }
    }

    void InstantReturn()
    {
        transform.position = _initialPosition;
        FinishReturn();
    }

    void FinishReturn()
    {
        _isReturning = false;
        
        // Reset physics and release ownership back to master
        if (_rb != null)
        {
            _rb.isKinematic = false;
        }
        
        // Transfer ownership back to server
        if (photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    [PunRPC]
    void SyncResetPosition(Vector3 position)
    {
        transform.position = position;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_isReturning);
            stream.SendNext(_initialPosition);
        }
        else
        {
            _isReturning = (bool)stream.ReceiveNext();
            _initialPosition = (Vector3)stream.ReceiveNext();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_initialPosition, _returnDistance);
    }
}