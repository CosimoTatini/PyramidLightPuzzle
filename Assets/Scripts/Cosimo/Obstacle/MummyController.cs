using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MummyVerticalAnimationController : MonoBehaviour
{
    private IVelocityProvider _velocityProvider;
    private Animator _animator;

    [Header("Impostazioni Movimento")]
    [SerializeField] private float _speedThreshold = 0.05f; // Soglia di tolleranza antirumore fisica

    // Caching dell'hash per massimizzare le prestazioni della CPU
    private static readonly int VelocityYHash = Animator.StringToHash("VelocityY");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _velocityProvider = GetComponent<IVelocityProvider>();

        if (_velocityProvider == null)
        {
            Debug.LogError($"[{gameObject.name}] Manca un componente che implementa IVelocityProvider!", this);
        }
    }

    private void Start()
    {
        // Pulizia frame zero
        _animator.SetFloat(VelocityYHash, 0f);
    }

    private void Update()
    {
        if (_velocityProvider == null) return;

        // Otteniamo il vettore velocità reale dalla piattaforma (es. Vector2(0, 2) o Vector2(0, -2))
        Vector2 currentVelocity = _velocityProvider.Velocity;

        // Isoliamo la componente verticale reale
        float verticalVelocity = currentVelocity.y;

        // Se la velocità verticale assoluta supera la soglia, passiamo la direzione normalizzata
        if (Mathf.Abs(verticalVelocity) > _speedThreshold)
        {
            // Mathf.Sign restituisce 1f se positivo (sale), -1f se negativo (scende)
            float targetY = Mathf.Sign(verticalVelocity);

            _animator.SetFloat(VelocityYHash, targetY);
        }
        else
        {
            // Idle quando è ferma
            _animator.SetFloat(VelocityYHash, 0f);
        }
    }
}