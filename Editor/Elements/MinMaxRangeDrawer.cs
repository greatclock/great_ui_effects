using System;
using UnityEditor;
using UnityEngine;

namespace GreatClock.Common.UIEffect {

	[CustomPropertyDrawer(typeof(MinMaxRange))]
	public class MinMaxRangeDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 18f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			MinMaxLimitAttribute attr = Attribute.GetCustomAttribute(fieldInfo, typeof(MinMaxLimitAttribute)) as MinMaxLimitAttribute;
			SerializedProperty pMin = property.FindPropertyRelative("m_Min");
			SerializedProperty pMax = property.FindPropertyRelative("m_Max");
			float min = pMin.floatValue;
			float max = pMax.floatValue;
			float width = Mathf.Clamp(position.width * 0.25f, 60f, 150f);
			EditorGUI.BeginChangeCheck();
			Rect rect;
			rect = new Rect(position.x, position.y, position.width - width - 2f, position.height);
			float minLimit = attr == null ? 0f : attr.min;
			float maxLimit = attr == null ? 1f : attr.max;
			EditorGUI.MinMaxSlider(rect, label, ref min, ref max, minLimit, maxLimit);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			float xfrom = position.xMax - width;
			float tw = width * 0.5f - 4f;
			rect = new Rect(xfrom, position.y, tw, position.height);
			min = EditorGUI.FloatField(rect, min);
			// EditorGUI.PropertyField(rect, pMin, GUIContent.none);
			rect = new Rect(xfrom + tw, position.y, 8f, position.height);
			EditorGUI.LabelField(rect, "-");
			rect = new Rect(position.xMax - tw, position.y, tw, position.height);
			max = EditorGUI.FloatField(rect, max);
			// EditorGUI.PropertyField(rect, pMax, GUIContent.none);
			EditorGUI.indentLevel = indent;
			if (EditorGUI.EndChangeCheck()) {
				pMin.floatValue = min;
				pMax.floatValue = max;
			}
		}

	}

}