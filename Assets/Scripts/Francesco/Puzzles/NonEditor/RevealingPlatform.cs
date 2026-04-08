using UnityEngine;

public class RevealingPlatform : MonoBehaviour, ILightTriggerReceiver
{
    [SerializeField] private Collider2D _platformCollider;
    [SerializeField] private SpriteRenderer _platformSprite;

    private bool _isActive = false;

    public LightTrigger LightTrigger { get; private set; }

    public LightSensor LightSensor => LightTrigger.LightSensor;

    [SerializeField] private bool _useRadius;
    private bool _IsInsideRadius = false;

    public bool UseRadius
    {
        get
        {
            return _useRadius;
        }
        set
        {
#if UNITY_EDITOR
            bool previousValue = _previousUseRadius;
#else
            bool previousValue = _useRadius;
#endif
            _useRadius = value;
            // value changed
            if (previousValue != _useRadius)
            {
                // using radius
                if (_useRadius)
                {
                    // if outside we need to check if the light is active, if so call deactivated
                    if (!_IsInsideRadius && LightTrigger.IsActive)
                    {
                        SetAlpha(0f);
                        SetInactive();
                    }
                }
                // not using radius
                else
                {
                    // if outside we need to check if light is active, if so activate it
                    if (!_IsInsideRadius && LightTrigger.IsActive)
                    {
                        LightChanged();
                        LightActivated();
                    }
                }
            }
        }
    }

#if UNITY_EDITOR

    private bool _previousUseRadius;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Debug.Log(_previousUseRadius + " " + _useRadius);
            if (_previousUseRadius != _useRadius)
            {
                UseRadius = _useRadius;
                _previousUseRadius = _useRadius;
            }
        }
    }

#endif

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Start()
    {
        SetInactive();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger) && trigger == LightTrigger)
        {
            _IsInsideRadius = true;
            if (!_useRadius) return;

            if (LightTrigger.IsActive)
            {
                LightActivated();
            }
            else
            {
                LightDeactivated();
            }
            LightChanged();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger) && trigger == LightTrigger)
        {
            if (_useRadius)
            {
                SetAlpha(0f);
                LightDeactivated();
            }
            _IsInsideRadius = false;
        }
    }

    public void SetActive()
    {
        _platformCollider.enabled = true;
        _isActive = true;
    }

    public void SetInactive()
    {
        _platformCollider.enabled = false;
        _isActive = false;
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        Color color = _platformSprite.color;
        color.a = alpha;
        _platformSprite.color = color;
    }

    public void SetLightTrigger(LightTrigger lightTrigger)
    {
        LightTrigger = lightTrigger;
        if (!_useRadius)
            LightChanged();
    }

    public void LightActivated()
    {
        if (!_useRadius) goto LightActivatedAction;

        if (!_IsInsideRadius)
        {
            return;
        }

    LightActivatedAction:
        SetActive();
    }

    public void LightChanged()
    {
        if (!_useRadius) goto LightChangedAction;

        if (!_IsInsideRadius)
        {
            return;
        }

    LightChangedAction:
        bool shouldBeActive = LightSensor.AreAmountsRight();
        if (shouldBeActive && _isActive) return;

        // set alpha
        int redDifference = Mathf.Abs(LightSensor.NeededRedAmount - LightSensor.CurrentRedAmount);
        int greenDifference = Mathf.Abs(LightSensor.NeededGreenAmount - LightSensor.CurrentGreenAmount);
        int blueDifference = Mathf.Abs(LightSensor.NeededBlueAmount - LightSensor.CurrentBlueAmount);

        int totalNeededAmount = LightSensor.NeededRedAmount + LightSensor.NeededGreenAmount + LightSensor.NeededBlueAmount;
        int difference = redDifference + greenDifference + blueDifference;
        if (difference == 0)
        {
            SetAlpha(1f);
        }
        else
        {
            SetAlpha((totalNeededAmount - difference) / (float)totalNeededAmount);
        }
    }

    public void LightDeactivated()
    {
        if (!_useRadius) goto LightDeactivatedAction;

        if (!_IsInsideRadius)
        {
            return;
        }

    LightDeactivatedAction:
        SetInactive();
    }
}
