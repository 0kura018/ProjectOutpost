using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.BattleControler
{

    public class BattleDebugPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MainBattleManager _battleManager;

        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugUI = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

        private bool _isVisible = false;
        private GUIStyle _boxStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;

        private void Start()
        {
            if (_battleManager == null)
                _battleManager = FindAnyObjectByType<MainBattleManager>();
        }

        private void Update()
        {
            if (!_showDebugUI) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.f1Key.wasPressedThisFrame)
                _isVisible = !_isVisible;

            if (!_isVisible) return;

            if (keyboard.kKey.wasPressedThisFrame)
                KillAllEnemies();

            if (keyboard.lKey.wasPressedThisFrame)
                KillAllAllies();

            if (keyboard.hKey.wasPressedThisFrame)
                HealAllAllies();

            if (keyboard.dKey.wasPressedThisFrame)
                DamageRandomEnemy(50);

            if (keyboard.eKey.wasPressedThisFrame)
                ToggleCanAttack();

            if (keyboard.tKey.wasPressedThisFrame)
                ToggleTimeScale();

            if (keyboard.pKey.wasPressedThisFrame)
                TogglePause();
        }

        private void OnGUI()
        {
            if (!_showDebugUI || !_isVisible) return;

            InitStyles();

            float width = 250;
            float height = 320;
            float x = 10;
            float y = 10;

            GUI.Box(new Rect(x, y, width, height), "?? Battle Debug (F1)", _boxStyle);

            float buttonY = y + 30;
            float buttonHeight = 25;
            float padding = 5;

            int allyCount = _battleManager?.Alies?.Count ?? 0;
            int enemyCount = _battleManager?.Enemies?.Count ?? 0;
            GUI.Label(new Rect(x + 10, buttonY, width - 20, 20),
                $"Allies: {allyCount} | Enemies: {enemyCount} | Time: {Time.timeScale}x", _labelStyle);
            buttonY += 25;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), "[K] Kill All Enemies", _buttonStyle))
                KillAllEnemies();
            buttonY += buttonHeight + padding;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), "[L] Kill All Allies", _buttonStyle))
                KillAllAllies();
            buttonY += buttonHeight + padding;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), "[H] Heal All Allies", _buttonStyle))
                HealAllAllies();
            buttonY += buttonHeight + padding;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), "[D] Damage Random Enemy (50)", _buttonStyle))
                DamageRandomEnemy(50);
            buttonY += buttonHeight + padding;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), "[E] Toggle CanAttack", _buttonStyle))
                ToggleCanAttack();
            buttonY += buttonHeight + padding;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), $"[T] Time Scale: {Time.timeScale}x", _buttonStyle))
                ToggleTimeScale();
            buttonY += buttonHeight + padding;

            if (GUI.Button(new Rect(x + 10, buttonY, width - 20, buttonHeight), Time.timeScale == 0 ? "[P] Resume" : "[P] Pause", _buttonStyle))
                TogglePause();
            buttonY += buttonHeight + padding;

            if (BattleSceneManager.Instance != null && BattleSceneManager.Instance.IsBattleActive)
            {
                buttonY += 10;
                if (GUI.Button(new Rect(x + 10, buttonY, (width - 25) / 2, buttonHeight), "? Victory", _buttonStyle))
                    BattleSceneManager.Instance.EndBattle(new BattleResult(true));

                if (GUI.Button(new Rect(x + 15 + (width - 25) / 2, buttonY, (width - 25) / 2, buttonHeight), "? Defeat", _buttonStyle))
                    BattleSceneManager.Instance.EndBattle(new BattleResult(false));
            }
        }

        private void InitStyles()
        {
            if (_boxStyle != null) return;

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
        }

        private void KillAllEnemies()
        {
            if (_battleManager == null) return;

            var enemies = _battleManager.Enemies.ToArray();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.UnitCurrentStats.IsAlive)
                    enemy.UnitCurrentStats.MainUnitProfile.Die();
            }
            Debug.Log("[Debug] Killed all enemies");
        }

        private void KillAllAllies()
        {
            if (_battleManager == null) return;

            var allies = _battleManager.Alies.ToArray();
            foreach (var ally in allies)
            {
                if (ally != null && ally.UnitCurrentStats.IsAlive)
                    ally.UnitCurrentStats.MainUnitProfile.Die();
            }
            Debug.Log("[Debug] Killed all allies");
        }

        private void HealAllAllies()
        {
            if (_battleManager == null) return;

            foreach (var ally in _battleManager.Alies)
            {
                if (ally != null && ally.UnitCurrentStats.IsAlive)
                    ally.UnitCurrentStats.MainUnitProfile.Heal(9999);
            }
            Debug.Log("[Debug] Healed all allies");
        }

        private void DamageRandomEnemy(int damage)
        {
            if (_battleManager == null || _battleManager.Enemies.Count == 0) return;

            var randomEnemy = _battleManager.Enemies[Random.Range(0, _battleManager.Enemies.Count)];
            if (randomEnemy != null)
            {
                randomEnemy.UnitCurrentStats.MainUnitProfile.TakeDamage(damage);
                Debug.Log($"[Debug] Dealt {damage} damage to {randomEnemy.name}");
            }
        }

        private void ToggleCanAttack()
        {
            if (_battleManager == null) return;

            bool newState = true;

            if (_battleManager.Alies.Count > 0)
                newState = !_battleManager.Alies[0].UnitCurrentStats.CanAttack;

            foreach (var ally in _battleManager.Alies)
                if (ally != null) ally.UnitCurrentStats.CanAttack = newState;

            foreach (var enemy in _battleManager.Enemies)
                if (enemy != null) enemy.UnitCurrentStats.CanAttack = newState;

            Debug.Log($"[Debug] CanAttack set to {newState} for all units");
        }

        private void ToggleTimeScale()
        {
            Time.timeScale = Time.timeScale >= 5f ? 1f : Time.timeScale + 1f;
            Debug.Log($"[Debug] Time scale: {Time.timeScale}x");
        }

        private void TogglePause()
        {
            Time.timeScale = Time.timeScale == 0 ? 1f : 0f;
            Debug.Log($"[Debug] {(Time.timeScale == 0 ? "Paused" : "Resumed")}");
        }
    }
}
