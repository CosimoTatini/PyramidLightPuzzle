using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField] private float _fallSpeed = 5f;
    [SerializeField] private bool _isFalling;
    private Transform _player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Player player))
        {
            _player= player.transform;
            _isFalling= true;
        }
        
    }

    private void Update()
    {
        if(_isFalling && _player!=null)
        {
            _player.position += Vector3.down * _fallSpeed * Time.deltaTime;
            
        }
    }


}
