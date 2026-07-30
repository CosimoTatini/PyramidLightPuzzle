using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))] 
public class MummyAnimationController : MonoBehaviour
{
    [SerializeField] private PlatformHandler _platformHandler;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    [Header("Impostazioni Movimento")]
    [SerializeField] private float _speedThreshold = 0.05f;

    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private static readonly int VelocityYHash = Animator.StringToHash("VelocityY");

    private Vector2 _currentActiveDirection = Vector2.right;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_platformHandler == null)
        {
            Debug.LogError($"[{gameObject.name}] Manca il componente IVelocityProvider!", this);
        }
    }

    private void Start()
    {
        ApplyDirection(_currentActiveDirection);
    }

    private void Update()
    {
        if (_platformHandler == null) return;

        Vector2 velocity = _platformHandler.Velocity;
        float absX = Mathf.Abs(velocity.x);
        float absY = Mathf.Abs(velocity.y);

  Debug.Log($"[{gameObject.name}] Velocità: {velocity}");

        // 1. Antirumore per i waypoint: se siamo quasi fermi, congeliamo lo stato attuale
        if (absX <= _speedThreshold && absY <= _speedThreshold)
        {
            return;
        }

        Vector2 newDirection = _currentActiveDirection;

        // 2. Rilevamento dell'asse dominante
        if (absX > absY)
        {
            // Movimento orizzontale dominato: forziamo X a 1f per attivare WalkRight nel Blend Tree
            newDirection = new Vector2(1f, 0f);

            // Gestiamo il lato visivo (FlipX) basandoci sul segno reale della velocità 🔄
            // Se la velocità è negativa stiamo andando a sinistra, quindi flippiamo lo sprite
            bool shouldFlip = velocity.x < 0f; 
            Debug.Log($"[{gameObject.name}] FlipX: {shouldFlip} (Velocità X: {velocity.x})");

            // Ottimizzazione: cambiamo il flip solo se è diverso dallo stato attuale
            if (_spriteRenderer.flipX != shouldFlip)
            {
                _spriteRenderer.flipX = shouldFlip;
            }
        }
        else
        {
            // Movimento verticale dominato: azzeriamo X
            newDirection = new Vector2(0f, Mathf.Sign(velocity.y));
        }

        // 3. Aggiorniamo i parametri dell'Animator solo se c'è un cambio effettivo
        if (newDirection != _currentActiveDirection)
        {
            _currentActiveDirection = newDirection;
            ApplyDirection(_currentActiveDirection);
        }
    }

    private void ApplyDirection(Vector2 dir)
    {
        _animator.SetFloat(VelocityXHash, dir.x);
        _animator.SetFloat(VelocityYHash, dir.y);
    }
}