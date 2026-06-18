using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildingSystem.UI
{

    public class RoomBuildButton : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RoomConfig _roomConfig;

        [Header("UI Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _button;

        private void Start()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            _button?.onClick.AddListener(OnClick);

            UpdateUI();
        }

        private void Update()
        {

            UpdateButtonState();
        }

        public void SetConfig(RoomConfig config)
        {
            _roomConfig = config;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_roomConfig == null) return;

            if (_iconImage != null)
                _iconImage.sprite = _roomConfig.Icon;

            if (_nameText != null)
                _nameText.text = _roomConfig.RoomName;

            if (_costText != null && _roomConfig.BuildCost != null && _roomConfig.BuildCost.Length > 0)
            {
                string costStr = "";
                foreach (var cost in _roomConfig.BuildCost)
                {
                    if (costStr.Length > 0) costStr += ", ";
                    costStr += $"{cost.Amount} {cost.Type}";
                }
                _costText.text = costStr;
            }
        }

        private void OnClick()
        {
            if (_roomConfig == null) return;
            BuildingSystem.Instance?.StartBuildMode(_roomConfig);
        }

        private void UpdateButtonState()
        {
            if (_button == null || _roomConfig == null) return;

            if (_roomConfig.HasBuildLimit && BuildingManager.Instance != null)
            {
                int currentCount = BuildingManager.Instance.GetRoomCountByType(_roomConfig);
                bool canBuild = currentCount < _roomConfig.MaxCount;
                _button.interactable = canBuild;

                if (_costText != null)
                {
                    string costStr = "";
                    if (_roomConfig.BuildCost != null && _roomConfig.BuildCost.Length > 0)
                    {
                        foreach (var cost in _roomConfig.BuildCost)
                        {
                            if (costStr.Length > 0) costStr += ", ";
                            costStr += $"{cost.Amount} {cost.Type}";
                        }
                    }
                    costStr += $" ({currentCount}/{_roomConfig.MaxCount})";
                    _costText.text = costStr;
                }
            }
            else
            {
                _button.interactable = true;
            }
        }
    }

    public class RoomBuildUI : MonoBehaviour
    {
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private RoomBuildTimerUI _timerPrefab;

        private readonly Dictionary<Room, RoomBuildTimerUI> _timers = new();

        public void ShowTimerForRoom(Room room)
        {
            if (_timers.ContainsKey(room)) return;

            var timer = Instantiate(_timerPrefab, _mainCanvas.transform);
            timer.Initialize(room);
            _timers.Add(room, timer);
        }

        public void HideTimerForRoom(Room room)
        {
            if (_timers.TryGetValue(room, out var timer))
            {
                Destroy(timer.gameObject);
                _timers.Remove(room);
            }
        }

        private void Update()
        {
            foreach (var kvp in _timers)
            {
                var room = kvp.Key;
                var timer = kvp.Value;
                Vector3 worldPos = room.transform.position + Vector3.up * 1.5f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                timer.transform.position = screenPos;
            }
        }
    }
}
