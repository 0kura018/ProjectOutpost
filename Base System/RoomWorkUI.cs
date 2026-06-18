using UnityEngine;
using UnityEngine.UI;
using BuildingSystem;
using System.Collections.Generic;
using TMPro;

namespace BaseSystem
{

    public class RoomWorkUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private TextMeshProUGUI _titleText;

        private Room _currentRoom;

        private void Start()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Show(Room room)
        {
            _currentRoom = room;
            if (_panel != null) _panel.SetActive(true);
            if (_titleText != null) _titleText.text = $"Assign to {room.Config.RoomName}";

            RefreshButtons();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            _currentRoom = null;
        }

        private void RefreshButtons()
        {
            if (_buttonContainer == null) return;

            foreach (Transform child in _buttonContainer)
            {
                Destroy(child.gameObject);
            }

            CreateButton("None / Free", null);

            if (NonCombatSpawner.Instance != null)
            {
                foreach (var unit in NonCombatSpawner.Instance.SpawnedUnits)
                {
                    if (unit == null) continue;

                    string status = unit.AssignedWorkRoom != null
                        ? $"(At {unit.AssignedWorkRoom.Config.RoomName})"
                        : "(Idle)";

                    CreateButton($"{unit.name} {status}", unit);
                }
            }
        }

        private void CreateButton(string label, NonCombatUnit unit)
        {
            if (_buttonPrefab == null || _buttonContainer == null) return;

            GameObject btnObj = Instantiate(_buttonPrefab, _buttonContainer);

            var text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = label;
            else
            {
                var legacyText = btnObj.GetComponentInChildren<Text>();
                if (legacyText != null) legacyText.text = label;
            }

            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnUnitSelected(unit));
            }
        }

        private void OnUnitSelected(NonCombatUnit unit)
        {
            if (_currentRoom == null) return;

            var workHandler = _currentRoom.GetComponent<RoomWorkHandler>();
            if (workHandler != null)
            {
                workHandler.AssignWorker(unit);
            }

            Hide();
        }
    }
}
