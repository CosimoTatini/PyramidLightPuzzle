using UnityEngine;

public class DisappearingWall : MonoBehaviour, ILightTriggerReceiver
{
    [SerializeField] private Collider2D _wallCollider;
    [SerializeField] private TriggerArea2D _triggerArea;
    [SerializeField] private SpriteRenderer _wallSprite;

    private bool _isActive = false;

    public LightTrigger LightTrigger { get; private set; }

    public LightSensor LightSensor => LightTrigger.LightSensor;

    [SerializeField] private bool _useRadius;
    private bool _isInsideRadius = false;

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
                    if (!_isInsideRadius)
                    {
                        if (_wallSprite.color.a != 0f)
                        {
                            SetInactive();
                        }

                    }
                }
                // not using radius
                else
                {
                    // if outside we need to check if light is active, if so activate it
                    if (!_isInsideRadius)
                    {
                        if (LightTrigger.IsActive)
                        {
                            LightChanged();
                            LightActivated();
                        }
                        else
                        {
                            LightChanged();
                            LightDeactivated();
                        }
                    }
                }
            }
        }
    }
    /*
     * ✔️ UseRadius:
     *  - Inside => Status can be either Active, Inactive, when value changes we need to change the alpha
     *  - Outside => Status can only be Inactive
     * ==================================================================================================
     * ❌ Don't UseRadius:
     *  Receives updates indipendently from radius
     * ================================================================================================== 
     * ❌ => ✔️ From Don't UseRadius to UseRadius
     *  - Inside => We are already inside and since we weren't using Radius the status is already correct
     *  - Outside => If it's active we need to turn it off
     * ==================================================================================================
     * ✔️ => ❌ From UseRadius to Don't UseRadius
     *  - Inside => We are already inside, UseRadius already handled the status
     *  - Outside => If light is active we need to turn it on since UseRadius couldn't handle it
    */

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
        _triggerArea.OnTriggerEnter += TriggerEnter;
        _triggerArea.OnTriggerExit += TriggerExit;
    }

    private void OnDisable()
    {
        _triggerArea.OnTriggerEnter -= TriggerEnter;
        _triggerArea.OnTriggerExit -= TriggerExit;
    }

    private void Start()
    {
        SetInactive();
    }

    private void TriggerEnter(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger) && trigger == LightTrigger)
        {
            Debug.Log("POOOOOO");
            _isInsideRadius = true;
            if (!_useRadius) return;

            if (LightTrigger.IsActive)
            {
                LightChanged();
                LightActivated();
            }
            else
            {
                LightChanged();
                LightDeactivated();
            }
        }
    }

    private void TriggerExit(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger) && trigger == LightTrigger)
        {
            Debug.Log("SOOOOOOo");
            if (_useRadius)
            {
                SetAlpha(1f);
                LightDeactivated();
            }
            _isInsideRadius = false;
        }
    }

    public void SetActive()
    {
        _wallCollider.enabled = false;
        _isActive = true;
    }
    public void SetInactive()
    {
        _wallCollider.enabled = true;
        _isActive = false;
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        Color color = _wallSprite.color;
        color.a = alpha;
        _wallSprite.color = color;
    }

    public void SetLightTrigger(LightTrigger lightTrigger)
    {
        LightTrigger = lightTrigger;
        // we don't use radius, so we need to check the light status here
        if (!_useRadius)
        {
            if (LightTrigger.IsActive)
            {
                LightChanged();
                LightActivated();
            }
            else
            {
                LightChanged();
                LightDeactivated();
            }
        }
    }

    public void LightActivated()
    {
        if (!_useRadius) goto LightActivatedAction;

        if (!_isInsideRadius)
        {
            return;
        }

    LightActivatedAction:
        SetActive();
    }

    public void LightChanged()
    {
        if (!_useRadius) goto LightChangedAction;

        if (!_isInsideRadius)
        {
            return;
        }

    LightChangedAction:
        //bool shouldBeActive = LightSensor.AreAmountsRight();
        //if (shouldBeActive && _isActive) return;

        // set alpha
        int redDifference = Mathf.Abs(LightSensor.NeededRedAmount - LightSensor.CurrentRedAmount);
        int greenDifference = Mathf.Abs(LightSensor.NeededGreenAmount - LightSensor.CurrentGreenAmount);
        int blueDifference = Mathf.Abs(LightSensor.NeededBlueAmount - LightSensor.CurrentBlueAmount);

        int totalNeededAmount = LightSensor.NeededRedAmount + LightSensor.NeededGreenAmount + LightSensor.NeededBlueAmount;
        int difference = redDifference + greenDifference + blueDifference;
        if (totalNeededAmount == 0)
        {
            SetAlpha(0f);
        }
        else
        {
            SetAlpha(1 - ((totalNeededAmount - difference) / (float)totalNeededAmount));
        }
    }

    public void LightDeactivated()
    {
        if (!_useRadius) goto LightDeactivatedAction;

        if (!_isInsideRadius)
        {
            return;
        }

    LightDeactivatedAction:
        SetInactive();
    }

}
