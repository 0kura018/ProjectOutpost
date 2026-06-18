using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.Visual
{

    public class CanvasPlaceController : MonoBehaviour
    {
        [Header("Stacking")]
        [Tooltip("�������������� ������ (� ������� ��������) ��� ����������� ���������")]
        [SerializeField] private float _stackRadius = 0.75f;

        [Tooltip("���� �� ����������� (���������� �� dx ��� ��������)")]
        [SerializeField] private float _horizontalMultiplier = 1.5f;

        [Tooltip("����������� �������� ����� ������������� (���)")]
        [SerializeField] private float _minUpdateInterval = 0.05f;

        private readonly List<Slider> _sliders = new();

        private readonly Dictionary<Slider, Vector3> _baseLocal = new();
        private readonly Dictionary<Slider, Vector3> _baseWorld = new();

        private readonly Dictionary<Slider, int> _lastGroupSize = new();

        private static int s_lastUpdateFrame = -1;
        private static float s_lastUpdateTime = 0f;

        private bool _allowLowerThisUpdate = false;

        public void SetSliders(List<Slider> sliders)
        {
            ClearAllRegistered();
            AddSliders(sliders);
            _allowLowerThisUpdate = true;
            UpdateStacks();
            _allowLowerThisUpdate = false;
        }

        public void AddSliders(List<Slider> sliders)
        {
            if (sliders == null) return;
            foreach (var s in sliders) RegisterSlider(s, recompute: false);
            UpdateStacks();
        }

        public void RegisterSlider(Slider slider, bool recompute = true)
        {
            if (slider == null) return;
            if (_sliders.Contains(slider)) return;
            _sliders.Add(slider);

            _baseLocal[slider] = slider.transform.localPosition;
            _baseWorld[slider] = slider.transform.position;
            _lastGroupSize[slider] = 0;

            if (recompute) UpdateStacks();
        }

        private void RestoreToBaseNoLower(Slider slider)
        {
            if (slider == null) return;
            if (_baseLocal.TryGetValue(slider, out var baseLocal) && _baseWorld.TryGetValue(slider, out var baseWorld))
            {
                var parentCanvas = slider.GetComponentInParent<Canvas>();
                bool useWorld = parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace;

                if (useWorld)
                {
                    var wp = slider.transform.position;

                    wp.y = Mathf.Max(wp.y, baseWorld.y);
                    slider.transform.position = wp;
                }
                else
                {
                    var parent = slider.transform.parent;
                    if (parent != null)
                    {
                        Vector3 local = parent.InverseTransformPoint(baseWorld);
                        var lp = slider.transform.localPosition;

                        lp.y = Mathf.Max(lp.y, local.y);
                        slider.transform.localPosition = lp;
                    }
                    else
                    {
                        var lp = slider.transform.localPosition;
                        lp.y = Mathf.Max(lp.y, baseLocal.y);
                        slider.transform.localPosition = lp;
                    }
                }
            }
        }

        public void RemoveSlider(Slider slider, bool recompute = true)
        {
            if (slider == null) return;
            if (!_sliders.Contains(slider)) return;

            RestoreToBaseNoLower(slider);

            _sliders.Remove(slider);
            _baseLocal.Remove(slider);
            _baseWorld.Remove(slider);
            _lastGroupSize.Remove(slider);

            if (recompute)
            {
                _allowLowerThisUpdate = true;
                UpdateStacks();
                _allowLowerThisUpdate = false;
            }
        }

        public void ClearAllRegistered()
        {
            var copy = new List<Slider>(_sliders);
            foreach (var s in copy)
            {
                RestoreToBaseNoLower(s);
                _sliders.Remove(s);
                _baseLocal.Remove(s);
                _baseWorld.Remove(s);
                _lastGroupSize.Remove(s);
            }

            UpdateStacks();
        }

        private void LateUpdate()
        {

            if (Time.frameCount == s_lastUpdateFrame) return;
            if (Time.realtimeSinceStartup - s_lastUpdateTime < _minUpdateInterval) return;

            s_lastUpdateFrame = Time.frameCount;
            s_lastUpdateTime = Time.realtimeSinceStartup;

            UpdateStacks();
        }

        private void UpdateStacks()
        {
            if (_sliders.Count == 0) return;

            var list = new List<Slider>(_sliders);
            int n = list.Count;
            var used = new bool[n];

            var newGroupSizes = new Dictionary<Slider, int>();

            for (int i = 0; i < n; i++)
            {
                if (used[i]) continue;
                var root = list[i];
                if (root == null) { used[i] = true; continue; }

                var rootPos = root.transform.position;

                var groupIdx = new List<int>();
                for (int j = 0; j < n; j++)
                {
                    if (used[j]) continue;
                    var other = list[j];
                    if (other == null) continue;

                    var otherPos = other.transform.position;
                    float dx = (otherPos.x - rootPos.x) * _horizontalMultiplier;
                    float dz = otherPos.z - rootPos.z;
                    if (dx * dx + dz * dz <= _stackRadius * _stackRadius)
                        groupIdx.Add(j);
                }

                if (groupIdx.Count == 0)
                {
                    used[i] = true;
                    continue;
                }

                var group = new List<Slider>(groupIdx.Count);
                foreach (var idx in groupIdx) group.Add(list[idx]);

                group.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

                var heights = new float[group.Count];
                for (int k = 0; k < group.Count; k++)
                {
                    var rt = group[k].GetComponent<RectTransform>();
                    heights[k] = rt != null ? rt.rect.height * group[k].transform.lossyScale.y : 0.1f;
                }

                var targetWorldY = new float[group.Count];
                targetWorldY[0] = group[0].transform.position.y;
                for (int k = 1; k < group.Count; k++)
                {
                    float prev = targetWorldY[k - 1];
                    float prevHalf = heights[k - 1] * 0.5f;
                    float currHalf = heights[k] * 0.5f;
                    targetWorldY[k] = prev + prevHalf + currHalf;
                }

                for (int k = 0; k < group.Count; k++)
                {
                    var s = group[k];
                    if (s == null) continue;

                    bool wasInLargerGroup = _lastGroupSize.TryGetValue(s, out var prevSize) && prevSize > group.Count;
                    bool isIsolatedNow = group.Count == 1;

                    float desiredWorldY = targetWorldY[k];
                    bool allowLowerForThisSlider = _allowLowerThisUpdate;

                    if (isIsolatedNow && wasInLargerGroup)
                    {
                        if (_baseWorld.TryGetValue(s, out var baseW))
                        {
                            desiredWorldY = baseW.y;
                            allowLowerForThisSlider = true;
                        }
                    }
                    else
                    {

                        if (!allowLowerForThisSlider)
                        {
                            foreach (var member in group)
                            {
                                if (_lastGroupSize.TryGetValue(member, out var psize) && psize > group.Count)
                                {
                                    allowLowerForThisSlider = true;
                                    break;
                                }
                            }
                        }
                    }

                    var parent = s.transform.parent;
                    float baseLocalY = _baseLocal.TryGetValue(s, out var bl) ? bl.y : float.MinValue;
                    var currentLocal = s.transform.localPosition;
                    float currentLocalY = currentLocal.y;

                    float desiredLocalY;
                    if (parent != null)
                    {

                        var targetWorldPos = s.transform.position;
                        targetWorldPos.y = desiredWorldY;
                        Vector3 local = parent.InverseTransformPoint(targetWorldPos);
                        desiredLocalY = local.y;
                    }
                    else
                    {

                        desiredLocalY = desiredWorldY;
                    }

                    float finalLocalY;
                    if (allowLowerForThisSlider)
                    {

                        finalLocalY = Mathf.Max(desiredLocalY, baseLocalY);
                    }
                    else
                    {

                        finalLocalY = Mathf.Max(currentLocalY, desiredLocalY, baseLocalY);
                    }

                    var lpApply = s.transform.localPosition;
                    lpApply.y = finalLocalY;
                    s.transform.localPosition = lpApply;

                    newGroupSizes[s] = group.Count;
                }

                foreach (var idx in groupIdx) used[idx] = true;
            }

            foreach (var kv in newGroupSizes)
            {
                _lastGroupSize[kv.Key] = kv.Value;
            }

            _allowLowerThisUpdate = false;
        }
    }
}
