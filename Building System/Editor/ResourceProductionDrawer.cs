#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace BuildingSystem.Editor
{

    [CustomPropertyDrawer(typeof(ResourceProduction))]
    public class ResourceProductionDrawer : PropertyDrawer
    {
        private const float LINE_HEIGHT = 24f;
        private const float SPACING = 8f;
        private const float PADDING = 8f;
        private const float LABEL_WIDTH = 140f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var useDailyProp = property.FindPropertyRelative("UseDailyProduction");
            bool usesDaily = useDailyProp != null && useDailyProp.boolValue;

            float height = PADDING + (LINE_HEIGHT + SPACING) * 4 + SPACING * 2;

            if (usesDaily)
            {
                height += LINE_HEIGHT + SPACING;
            }

            height += PADDING;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("Type");
            var amountProp = property.FindPropertyRelative("Amount");
            var useDailyProp = property.FindPropertyRelative("UseDailyProduction");
            var amountPerDayProp = property.FindPropertyRelative("AmountPerDay");

            float yPos = position.y + PADDING;
            float fullWidth = position.width;

            DrawField(position.x, yPos, fullWidth, "Resource Type", typeProp);
            yPos += LINE_HEIGHT + SPACING;

            bool isDisabled = useDailyProp.boolValue;
            GUI.enabled = !isDisabled;
            DrawField(position.x, yPos, fullWidth, "Amount Per Cycle", amountProp);
            GUI.enabled = true;
            yPos += LINE_HEIGHT + SPACING;

            yPos += SPACING;

            Rect headerRect = new Rect(position.x, yPos, fullWidth, LINE_HEIGHT);
            EditorGUI.LabelField(headerRect, "Daily Production (Optional)", EditorStyles.boldLabel);
            yPos += LINE_HEIGHT + SPACING;

            Rect labelRect = new Rect(position.x, yPos, LABEL_WIDTH, LINE_HEIGHT);
            Rect toggleRect = new Rect(position.x + LABEL_WIDTH, yPos, 20f, LINE_HEIGHT);

            EditorGUI.LabelField(labelRect, "Use Daily Production");
            useDailyProp.boolValue = EditorGUI.Toggle(toggleRect, useDailyProp.boolValue);
            yPos += LINE_HEIGHT + SPACING;

            if (useDailyProp.boolValue)
            {
                Color oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f, 1f);

                float indent = 20f;
                DrawField(position.x + indent, yPos, fullWidth - indent, "Amount Per Day (24h)", amountPerDayProp);

                GUI.backgroundColor = oldBg;
            }

            EditorGUI.EndProperty();
        }

        private void DrawField(float x, float y, float width, string labelText, SerializedProperty prop)
        {
            Rect labelRect = new Rect(x, y, LABEL_WIDTH, LINE_HEIGHT);
            Rect fieldRect = new Rect(x + LABEL_WIDTH, y, width - LABEL_WIDTH, LINE_HEIGHT);

            EditorGUI.LabelField(labelRect, labelText);
            EditorGUI.PropertyField(fieldRect, prop, GUIContent.none);
        }
    }
}
#endif

