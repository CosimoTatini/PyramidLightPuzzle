using UnityEngine;

namespace Core.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class Teleporter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Teleporter _destination;

        [Header("Settings")]
        [Tooltip("Se true, permette l'uso continuo. Se false, disattiva il collegamento da entrambi i lati dopo il primo utilizzo.")]
        [SerializeField] private bool _isReusable = true;

        [Tooltip("Tempo di cooldown per evitare il ciclo infinito (Ping-Pong) quando isReusable è true.")]
        [SerializeField] private float _cooldownTime = 0.5f;

        private Collider2D _collider;
        private bool _isCoolingDown;
        private bool _isDisabled;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Early exit se disabilitato o in cooldown
            if (_isDisabled || _isCoolingDown) return;

            if (collision.TryGetComponent<PlayerController>(out var player))
            {
                PerformTeleport(player);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // Reset del cooldown solo quando il player si allontana fisicamente ed è riutilizzabile
            if (_isReusable && collision.CompareTag("Player"))
            {
                _isCoolingDown = false;
            }
        }

        private void PerformTeleport(PlayerController player)
        {
            if (_destination == null)
            {
                Debug.LogWarning($"[Teleporter] Nape di destinazione non assegnata su {gameObject.name}", this);
                return;
            }

            // 1. Applica il movimento al Player
            player.ResetMoveDirection();
            player.transform.position = _destination.GetDestinationPosition();

            // 2. Gestione Logica di Riuso / Disattivazione
            if (!_isReusable)
            {
                // Disattiva il punto di partenza (A)
                DisableTeleporter();

                // Disattiva istantaneamente anche il punto di arrivo (B) per evitare il ritorno
                _destination.DisableTeleporter();
            }
            else
            {
                // Se riutilizzabile, applica il cooldown sulla destinazione per prevenire il Ping-Pong
                _destination.ActivateCooldown(_cooldownTime);
            }
        }

        public Vector3 GetDestinationPosition()
        {
            return transform.position;
        }

        public void ActivateCooldown(float duration)
        {
            _isCoolingDown = true;
            CancelInvoke(nameof(ResetCooldown));
            Invoke(nameof(ResetCooldown), duration);
        }

        private void ResetCooldown()
        {
            _isCoolingDown = false;
        }

        /// <summary>
        /// Disattiva in modo permanente il teletrasporto e il suo collider.
        /// </summary>
        public void DisableTeleporter()
        {
            _isDisabled = true;
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }
    }
}