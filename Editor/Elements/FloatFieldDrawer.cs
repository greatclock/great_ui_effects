using UnityEngine;
using UnityEditor;

namespace GreatClock.Common.UIEffect {

	[CustomPropertyDrawer(typeof(FloatField))]
	public class FloatFieldDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 18f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty pConstValue = property.FindPropertyRelative("m_ConstValue");
			SerializedProperty pCurve = property.FindPropertyRelative("m_Curve");
			SerializedProperty pUseCurve = property.FindPropertyRelative("m_UseCurve");
			float width = 60f;
			Rect r1 = new Rect(position.x, position.y, position.width - width - 2f, position.height);
			Rect r2 = new Rect(position.width - width + 20f, position.y, width + 20f, position.height);
			EditorGUI.PropertyField(r1, pUseCurve.boolValue ? pCurve : pConstValue, label);
			// int indent = EditorGUI.indentLevel;
			// EditorGUI.indentLevel = 0;
			bool useCurve = EditorGUI.ToggleLeft(r2, "Curve", pUseCurve.boolValue);
			if (useCurve != pUseCurve.boolValue) { pUseCurve.boolValue = useCurve; }
			if (useCurve && pCurve.animationCurveValue.length <= 0) {
				AnimationCurve curve = pCurve.animationCurveValue;
				curve.AddKey(new Keyframe(0f, pConstValue.floatValue));
				curve.AddKey(new Keyframe(1f, pConstValue.floatValue));
				pCurve.animationCurveValue = curve;
			}
			// EditorGUI.indentLevel = indent;
		}

	}

}