using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectionRadius = 1.5f;
    [SerializeField] private float _viewAngle = 90f;
    [SerializeField] private float _detectionAngle = 45f;
    [SerializeField] private float _angleOffset = 0f;
    [SerializeField] private LayerMask _playerMask;
    private float _currentAngle;
    [SerializeField] private float _speedRotation = 10f;
    [SerializeField] private PolygonCollider2D _detectionPolygon;

    [Header("Shoot")]
    [SerializeField] private float _fireRate;
    [SerializeField] private float _fireTimer;

    [Header("Ref")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _barrelPivot;
    [SerializeField] private Projectile _projectilePrefab;

    private ObjectPooler<Projectile> _pool;
    private Transform _target;

    private void Awake()
    {
        _pool = new ObjectPooler<Projectile>(_projectilePrefab);
    }
    private void Start()
    {
        _barrelPivot.eulerAngles = new Vector3(_barrelPivot.eulerAngles.x, _barrelPivot.eulerAngles.y, _angleOffset);
        SetDetectionTriggerPoints();

        Debug.Log("Converted Angle" + ConvertAngle(-1075));
    }

    private void SetDetectionTriggerPoints()
    {
        List<Vector3> detectionPoints = new List<Vector3>();
        Vector2 forward = transform.right;
        Vector2 origin = _barrelPivot.position;
        float barrelRot = _barrelPivot.eulerAngles.z;
        Vector2 detectionEdgeA = GetPointOnCircle(origin, _detectionRadius, _detectionAngle / 2f + barrelRot + _angleOffset);
        Vector2 detectionEdgeB = GetPointOnCircle(origin, _detectionRadius, -_detectionAngle / 2f + barrelRot + _angleOffset);
        forward = transform.TransformPoint(forward);
        detectionPoints.Add(origin);
        detectionPoints.Add(detectionEdgeA);
        int detectionCount = 20;
        float detectionDistance = _detectionAngle / detectionCount;
        for (int i = 1; i < detectionCount - 1; i++)
        {
            Vector2 arch = GetPointOnCircle(origin, _detectionRadius, _detectionAngle / 2f - detectionDistance * i + barrelRot + _angleOffset);
            detectionPoints.Add(arch);
        }
        detectionPoints.Add(detectionEdgeB);
        Vector2[] detectionPoints2 = new Vector2[detectionPoints.Count];
        int k = 0;
        foreach (var item in detectionPoints)
        {
            detectionPoints2[k++] = _detectionPolygon.transform.InverseTransformPoint(item);
        }
        _detectionPolygon.points = detectionPoints2;
    }

    private void Update()
    {
        DetectPlayer();

        if (_target == null)
        {

            return;
        }
        Vector2 dirToPlayer = (_target.transform.position - _barrelPivot.position).normalized;
        Vector2 forward = _barrelPivot.right;


        float angle = Vector2.SignedAngle(forward, dirToPlayer);
        angle = ConvertAngle(angle);

        _currentAngle = Mathf.MoveTowardsAngle(_barrelPivot.eulerAngles.z, _barrelPivot.eulerAngles.z + angle, _speedRotation * Time.deltaTime);
        _barrelPivot.eulerAngles = new Vector3(_barrelPivot.eulerAngles.x, _barrelPivot.eulerAngles.y, _currentAngle);

        float detectionAngleA = ConvertAngle(-_detectionAngle / 2f + _angleOffset);
        float detectionAngleB = ConvertAngle(_detectionAngle / 2f + _angleOffset);
        Debug.Log($"Angle:{angle} ViewAngleA:{detectionAngleA} ViewAngleB:{detectionAngleB}");

        _fireTimer += Time.deltaTime;
        if (!IsAngleInRange(angle, detectionAngleA, detectionAngleB))
            return;

        if (_fireTimer >= 1f / _fireRate)
        {
            Shoot();
            _fireTimer = 0f;
        }



    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            _target = player.transform;
            Debug.Log("Detected");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Player player))
        {
            _target = null;
        }
    }

    private void Shoot()
    {
        Projectile newProjectile = _pool.Get();
        newProjectile.transform.position = _firePoint.position;
        newProjectile.gameObject.SetActive(true);

        Vector2 direction = (_target.position - _firePoint.position).normalized;

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
            Debug.Log("Player Trovato:" + hit);




        }




    }

    Vector2 GetPointOnCircle(Vector2 center, float radius, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float x = center.x + radius * Mathf.Cos(rad);
        float y = center.y + radius * Mathf.Sin(rad);

        return new Vector2(x, y);
    }

    private float ConvertAngle(float angle)
    {
        if (angle < 0)
        {
            angle *= -1;
            angle %= 360;
            angle = 360 - angle;
        }

        else
        {
            angle %= 360;
        }
        return angle;


    }

    private bool IsAngleInRange(float angle, float angleVertexA, float angleVertexB)
    {
        // se angolo si trova tra a e b , ma non viceversa
        if (angleVertexA < angleVertexB)
        {
            if (angle >= angleVertexA && angle <= angleVertexB)
            {
                return true;
            }
            return false;
        }
        else if (angleVertexB < angleVertexA)
        {

            if (angle > angleVertexB && angle < angleVertexA)
            {
                return false;
            }
            return true;
        }
        else
        {
            if (angle == angleVertexA)
            {
                return true;
            }
        }

        return false;

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.rebeccaPurple;
        Handles.DrawWireDisc(_barrelPivot.position, _barrelPivot.forward, _detectionRadius);
        Gizmos.color = Color.red;
        List<Vector3> detectionPoints = new List<Vector3>();
        Vector2 forward = transform.right;
        Vector2 origin = _barrelPivot.position;
        float barrelRot = _barrelPivot.eulerAngles.z;
        Vector2 detectionEdgeA = GetPointOnCircle(origin, _detectionRadius, _detectionAngle / 2f + barrelRot + _angleOffset);
        Vector2 detectionEdgeB = GetPointOnCircle(origin, _detectionRadius, -_detectionAngle / 2f + barrelRot + _angleOffset);
        Gizmos.DrawLine(origin, detectionEdgeA);
        Gizmos.DrawLine(origin, detectionEdgeB);
        forward = transform.TransformPoint(forward);
        detectionPoints.Add(detectionEdgeA);
        int detectionCount = 20;
        float detectionDistance = _detectionAngle / detectionCount;
        for (int i = 1; i < detectionCount - 1; i++)
        {
            Vector2 arch = GetPointOnCircle(origin, _detectionRadius, _detectionAngle / 2f - detectionDistance * i + barrelRot + _angleOffset);
            detectionPoints.Add(arch);
        }
        detectionPoints.Add(detectionEdgeB);
        Gizmos.DrawLineStrip((ReadOnlySpan<Vector3>)detectionPoints.ToArray().AsSpan(), false);
        Gizmos.DrawCube(forward, new Vector2(2, 2));
        //----------------------------------//
        Gizmos.color = Color.lightBlue;
        List<Vector3> viewPoints = new List<Vector3>();
        Vector2 viewEdgeA = GetPointOnCircle(origin, _detectionRadius, _viewAngle / 2f + barrelRot + _angleOffset);
        Vector2 vieweEdgeB = GetPointOnCircle(origin, _detectionRadius, -_viewAngle / 2f + barrelRot + _angleOffset);
        Gizmos.DrawLine(origin, viewEdgeA);
        Gizmos.DrawLine(origin, vieweEdgeB);
        forward = transform.TransformPoint(forward);
        viewPoints.Add(viewEdgeA);
        int viewCount = 20;
        float viewDistance = _viewAngle / detectionCount;
        for (int i = 1; i < viewCount - 1; i++)
        {
            Vector2 arch = GetPointOnCircle(origin, _detectionRadius, _viewAngle / 2f - viewDistance * i + barrelRot + _angleOffset);
            viewPoints.Add(arch);
        }
        viewPoints.Add(vieweEdgeB);
        Gizmos.DrawLineStrip((ReadOnlySpan<Vector3>)viewPoints.ToArray().AsSpan(), false);


    }
#endif
}
