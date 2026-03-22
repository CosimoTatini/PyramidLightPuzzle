using UnityEngine;

public class RevealingPlatform : MonoBehaviour
{
    [SerializeField] private Collider2D _platformCollider;
    [SerializeField] private SpriteRenderer _platformSprite;
    [SerializeField] private LightSensor _lightSensor;

    private bool _isActive = false;

    private void OnEnable()
    {
        _lightSensor.OnLightChanged.AddListener(OnLightChanged);
    }

    private void OnDisable()
    {
        _lightSensor.OnLightChanged.RemoveListener(OnLightChanged);
    }

    private void Start()
    {
        SetInactive();
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

    private void OnLightChanged()
    {
        bool shouldBeActive = _lightSensor.AreAmountsRight();
        if (shouldBeActive && _isActive) return;

        // set alpha
        int redDifference = Mathf.Abs(_lightSensor.NeededRedAmount - _lightSensor.CurrentRedAmount);
        int greenDifference = Mathf.Abs(_lightSensor.NeededGreenAmount - _lightSensor.CurrentGreenAmount);
        int blueDifference = Mathf.Abs(_lightSensor.NeededBlueAmount - _lightSensor.CurrentBlueAmount);

        int totalNeededAmount = _lightSensor.NeededRedAmount + _lightSensor.NeededGreenAmount + _lightSensor.NeededBlueAmount;
        int difference = redDifference + greenDifference + blueDifference;
        if (difference == 0)
        {
            SetAlpha(1f);
            SetActive();
        }
        else
        {
            SetAlpha((totalNeededAmount - difference) / (float)totalNeededAmount);
            if (_isActive)
            {
                SetInactive();
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        Color color = _platformSprite.color;
        color.a = alpha;
        _platformSprite.color = color;
    }

}
