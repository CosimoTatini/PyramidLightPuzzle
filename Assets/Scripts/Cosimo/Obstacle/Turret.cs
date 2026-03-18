using System.Runtime.CompilerServices;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectionRadius = 1.5f;
    [SerializeField] private float _viewAngle = 90f;
    [SerializeField] private LayerMask _playerMask;

    [Header("Shoot")]
    [SerializeField] private float _fireRate;
    [SerializeField] private float fireTimer;

    [Header("Ref")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Projectile _projectilePrefab;

    private ObjectPooler<Projectile> _pool;
    private Transform _target;

    private void Awake()
    {
        _pool = new ObjectPooler<Projectile>(_projectilePrefab);
    }

    private void Update()
    {
        //DetectPlayer();

        //if (_target != null)
        //{

        //    fireTimer += Time.deltaTime;


        //    if (fireTimer >= 1f / _fireRate)
        //    {
        //        Shoot();
        //        fireTimer = 0f;
        //    }
        //}

        DetectPlayer();

        if (!IsValidTarget())
        {
            _target = null;
            return;
        }

        fireTimer += Time.deltaTime;

        if (fireTimer >= 1f / _fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }

    }

    private void Shoot()
    {
        Projectile newProjectile = _pool.Get();
        newProjectile.transform.position = firePoint.position;
        newProjectile.gameObject.SetActive(true);

        Vector2 direction = (_target.position - firePoint.position).normalized;

        newProjectile.Initialize(direction, _pool);
    }

    private void DetectPlayer()
    {
        
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerMask);

        if (hit == null || !hit.TryGetComponent<Player>(out Player player))
        {
            _target = null;
            return;
        }

        Vector2 dirToPlayer = (hit.transform.position - transform.position).normalized;
        Vector2 forward = -transform.right;

        float angle = Vector2.Angle(forward, dirToPlayer);

        if (angle < _viewAngle / 2f)
            _target = hit.transform;
        else
            _target = null;

        //Collider2D hit = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerMask);

        //if (hit != null && hit.GetComponent<Player>() != null)
        //{
        //    _target = hit.transform;
        //    return;
        //}

        //if (hit == null)
        //{
        //    return;
        //}

        //Transform player = hit.transform;

        //Vector2 dirToPlayer = (player.position - transform.position).normalized;


        //Vector2 forward = -transform.right;

        //float angle = Vector2.Angle(forward, dirToPlayer);

        //if (angle < _viewAngle / 2f)
        //{
        //    _target = player;
        //}
        //else
        //{
        //    _target = null;
        //}
    }
       

   

}
    private bool IsValidTarget()
    {
        if (_target == null) return false;

        float distance = Vector2.Distance(transform.position, _target.position);
        if (distance > _detectionRadius) return false;

        Vector2 dir = (_target.position - transform.position).normalized;
        float angle = Vector2.Angle(-transform.right, dir);

        return angle < _viewAngle / 2f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

    }
#endif
}
