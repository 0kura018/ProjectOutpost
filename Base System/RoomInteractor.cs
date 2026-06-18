using UnityEngine;
using UnityEngine.InputSystem;
using BuildingSystem;
using UnityEngine.EventSystems;

namespace BaseSystem
{

    public class RoomInteractor : MonoBehaviour
    {
        [SerializeField] private LayerMask _roomLayer;

        private Camera _mainCamera;
        private Mouse _mouse;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _mouse = Mouse.current;
        }

        private void Update()
        {
            if (_mouse == null) return;

            if (_mouse.leftButton.wasPressedThisFrame)
            {

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                TrySelectRoom();
            }
        }

        private void TrySelectRoom()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector2 mousePos = _mouse.position.ReadValue();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            worldPos.z = 0;

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0.1f, _roomLayer);

            if (hit.collider != null)
            {
                Room room = hit.collider.GetComponentInParent<Room>();
                if (room != null && room.State == RoomState.Completed && room.Config != null && room.Config.CanProvideWork)
                {
                    OnRoomSelected(room);
                }
            }
        }

        private void OnRoomSelected(Room room)
        {
            Debug.Log($"[RoomInteractor] Selected room: {room.Config.RoomName}");

            RoomWorkHandler workHandler = room.GetComponent<RoomWorkHandler>();
            if (workHandler == null)
            {
                workHandler = room.gameObject.AddComponent<RoomWorkHandler>();
            }

            RoomWorkUI workUI = Object.FindAnyObjectByType<RoomWorkUI>();
            if (workUI != null)
            {
                workUI.Show(room);
            }
            else
            {
                Debug.LogWarning("[RoomInteractor] RoomWorkUI not found on scene!");
            }
        }
    }
}
