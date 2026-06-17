using UnityEngine;

/// <summary>
/// Comportamento dell'ostacolo Ascia sincronizzato: parte IN BASSO 
/// e mantiene la rotazione perpendicolare corretta per ogni quadrante.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Axe : MonoBehaviour
{
    //TODO:Need to be fixed. Instead try to make the rotation follow the position to rotate correctly the gameobject with collider.
    [Header("Configurazione Orbita Matematica")]
    [SerializeField] private Transform _centerPoint; // Trascina qui "AxePhysics"
    [SerializeField] private float _radius = 3.0f;     // Raggio della circonferenza
    [SerializeField] private float _speed = 90f;       // Velocità dell'orbita

    [Header("Configurazione Rotazione Fluida Perpendicolare")]
    [SerializeField] private float _angleStep = 90f;       // Allineamento sui quadranti (90°)
    [SerializeField] private float _rotationSpeed = 180f;  // Velocità con cui si gira 🔄
    [SerializeField] private float _rotationOffset = 0f;    // Modifica se vuoi cambiare l'orientamento della lama

    private Rigidbody2D _rb;
    private float _currentAngle;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = true;
    }

    private void Start()
    {
        // Forziamo l'angolo di partenza a -90 gradi (ore 6, la parte bassa del cerchio).
        // Questo sincronizza istantaneamente la matematica di posizione e rotazione.
        _currentAngle = -90f;
    }

    private void FixedUpdate()
    {
        if (_centerPoint == null) return;

        // 1. Avanzamento dell'angolo nel tempo
        _currentAngle += _speed * Time.fixedDeltaTime;
        float radians = _currentAngle * Mathf.Deg2Rad;

        // 2. Calcolo della posizione sulla circonferenza (ora è sincronizzato)
        float x = _centerPoint.position.x + Mathf.Cos(radians) * _radius;
        float y = _centerPoint.position.y + Mathf.Sin(radians) * _radius;
        Vector2 targetPosition = new Vector2(x, y);

        // 3. Spostiamo il Rigidbody nella posizione corretta
        _rb.MovePosition(targetPosition);

        // 4. Calcolo del quadrante speculare all'angolo corrente
        float stepCalculation = _currentAngle / _angleStep;
        float stepCount = (_speed >= 0) ? Mathf.Floor(stepCalculation) : Mathf.Ceil(stepCalculation);

        // 5. Calcolo della rotazione target perpendicolare che guarda fuori
        float targetStepRotation = (stepCount * _angleStep) + _rotationOffset;

        // 6. Rotazione fluida verso il quadrante senza scatti orari/antiorari pazzi
        float smoothRotation = Mathf.MoveTowardsAngle(
            _rb.rotation,
            targetStepRotation,
            _rotationSpeed * Time.fixedDeltaTime
        );

        _rb.MoveRotation(smoothRotation);
    }
}