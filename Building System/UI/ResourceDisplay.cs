using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuildingSystem.UI
{

    public class ResourceDisplay : MonoBehaviour
    {
        [Header("Resource Type")]
        [SerializeField] private ResourceType _resourceType;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _resourceIcon;

        private void Start()
        {
            if (_iconImage != null && _resourceIcon != null)
                _iconImage.sprite = _resourceIcon;

            UpdateDisplay();

            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
        }

        private void OnResourceChanged(ResourceType type, int amount)
        {
            if (type == _resourceType)
                UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_amountText == null) return;

            int amount = ResourceManager.Instance?.GetResource(_resourceType) ?? 0;
            _amountText.text = amount.ToString();
        }
    }
}
