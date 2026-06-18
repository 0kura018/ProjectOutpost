using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeSystem;

public class RoomBuildTimerUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _timeLeftText;

    private BuildingSystem.Room _room;
    private TimeBasedProgress _progressComponent;

    public void Initialize(BuildingSystem.Room room)
    {
        _room = room;

        _progressComponent = room.GetComponent<TimeBasedProgress>();

        if (_progressComponent != null)
        {
            _progressComponent.OnProgressChanged += UpdateProgress;
            _progressComponent.OnCompleted += OnBuildCompleted;

            UpdateProgress(_progressComponent.ProgressNormalized);
        }
        else
        {
            Debug.LogError($"[RoomBuildTimerUI] TimeBasedProgress не найден на {room.name}");
        }
    }

    private void Update()
    {
        if (_progressComponent != null && _progressComponent.IsActive)
        {
            UpdateTimeLeft();
        }
    }

    private void UpdateProgress(float normalizedProgress)
    {
        if (_progressSlider != null)
        {
            _progressSlider.value = normalizedProgress;
        }

        if (_progressText != null)
        {
            _progressText.text = $"{normalizedProgress * 100f:F0}%";
        }
    }

    private void UpdateTimeLeft()
    {
        if (_timeLeftText == null || _progressComponent == null) return;

        int ticksLeft = _progressComponent.EstimateTicksToComplete();
        float hoursLeft = _progressComponent.RemainingHours;

        string timeText = hoursLeft < 1f
            ? $"{ticksLeft} такт. ({hoursLeft * 60f:F0}м)"
            : $"{ticksLeft} татк. ({hoursLeft:F1}ч)";

        _timeLeftText.text = timeText;
    }

    private void OnBuildCompleted()
    {

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_progressComponent != null)
        {
            _progressComponent.OnProgressChanged -= UpdateProgress;
            _progressComponent.OnCompleted -= OnBuildCompleted;
        }
    }
}
