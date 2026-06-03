
using UnityEngine;

/// <summary>
/// Axe Obstacle behaviour.
/// </summary>
public class Axe : MonoBehaviour
{
    [SerializeField] private float _angle = 60f;
    [SerializeField] private float _speed = 2f;

    private void Update()
    {
        float z = Mathf.Sin(_speed * Time.time) * _angle;
        transform.rotation= Quaternion.Euler(0,0,z);
    }


}

