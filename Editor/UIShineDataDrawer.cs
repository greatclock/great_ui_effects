using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace GreatClock.Common.UIEffect {

	[CustomPropertyDrawer(typeof(UIEffectShine.ShineData))]
	public class UIShineDataDrawer : PropertyDrawer {

		private struct PropertySize {
			public string property;
			public float size;
			public PropertySize(string property, float size) {
				this.property = property;
				this.size = size;
			}
		}

		private static Regex property_path_regex = new Regex(@"(\S+)\.Array\.data\[(\d+)\]$");

		private static bool style_inited = false;
		private static GUIStyle style_normal_label;
		private static GUIStyle style_bold_label;

		private static HashSet<string> folded_items = null;

		private List<PropertySize> mProps = new List<PropertySize>();

		private string mPropertyArrayPath = null;
		private int mPropertyArrayIndex = -1;

		private static void SaveFolded() {
			string[] folded = new string[folded_items.Count];
			folded_items.CopyTo(folded);
			EditorPrefs.SetString("ShineData_FoldedItems", string.Join("/", folded));
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if (folded_items == null) {
				folded_items = new HashSet<string>();
				string saved = EditorPrefs.GetString("ShineData_FoldedItems", null);
				if (!string.IsNullOrEmpty(saved)) {
					foreach (string split in saved.Split('/')) {
						folded_items.Add(split);
					}
				}
			}
			UIEffectShine.eShineType type = (UIEffectShine.eShineType)property.FindPropertyRelative("m_ShineType").enumValueIndex;
			mProps.Clear();
			float lineheight = EditorGUIUtility.singleLineHeight;
			float height = lineheight;
			float spacing = EditorGUIUtility.standardVerticalSpacing;
			mPropertyArrayPath = null;
			mPropertyArrayIndex = -1;
			Match match = property_path_regex.Match(property.propertyPath);
			if (match.Success) {
				mPropertyArrayPath = match.Groups[1].Value;
				mPropertyArrayIndex = int.Parse(match.Groups[2].Value);
				string foldKey = property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;
				if (folded_items.Contains(foldKey)) { return height; }
			}
			GetPropertyList(type, mProps);
			for (int i = mProps.Count - 1; i >= 0; i--) {
				height += mProps[i].size + spacing;
			}
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (!style_inited) {
				style_inited = true;
				style_normal_label = GUI.skin.label;
				style_bold_label = new GUIStyle(style_normal_label);
				style_bold_label.fontStyle = FontStyle.Bold;
			}
			Rect rect = new Rect(position.x + 14f, position.y, position.width - 14f, position.height);
			string foldKey = null;
			bool folded = false;
			GUIStyle labelStyle = style_normal_label;
			if (!string.IsNullOrEmpty(mPropertyArrayPath)) {
				foldKey = property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;
				folded = folded_items.Contains(foldKey);
				if (!folded) { labelStyle = style_bold_label; }
			}
			float lineheight = EditorGUIUtility.singleLineHeight;
			float spacing = EditorGUIUtility.standardVerticalSpacing;
			// Debug.LogWarning($"{label.text}, {position}");
			if (GUI.Button(new Rect(rect.x, rect.y, rect.width, lineheight), label, labelStyle)) {
				if (!string.IsNullOrEmpty(mPropertyArrayPath)) {
					SerializedObject so = property.serializedObject;
					switch (Event.current.button) {
						case 0:
							if (foldKey != null) {
								if (!folded_items.Remove(foldKey)) { folded_items.Add(foldKey); }
								SaveFolded();
							}
							break;
						case 1:
							GenericMenu menu = new GenericMenu();
							menu.AddItem(new GUIContent("Insert"), false, () => {
								Debug.LogWarning("Insert");
								so.FindProperty(mPropertyArrayPath).InsertArrayElementAtIndex(mPropertyArrayIndex);
								so.ApplyModifiedProperties();
							});
							menu.AddItem(new GUIContent("Delete"), false, () => {
								Debug.LogWarning("Delete");
								so.FindProperty(mPropertyArrayPath).DeleteArrayElementAtIndex(mPropertyArrayIndex);
								so.ApplyModifiedProperties();
							});
							menu.ShowAsContext();
							break;
					}
				}
			}
			SerializedProperty pInited = property.FindPropertyRelative("m_Inited");
			if (!pInited.boolValue) {
				InitGradient(property.FindPropertyRelative("m_Gradient"),
					new GradientColorKey[] {
						new GradientColorKey(Color.black, 0f),
						new GradientColorKey(Color.gray, 0.4f),
						new GradientColorKey(Color.gray, 0.6f),
						new GradientColorKey(Color.black, 1f)
					},
					new GradientAlphaKey[] {
						new GradientAlphaKey(0.8f, 0f),
						new GradientAlphaKey(0.8f, 1f)
					}
				);
				InitGradient(property.FindPropertyRelative("m_ColorOverLifetime"),
					new GradientColorKey[] {
						new GradientColorKey(Color.white, 0f),
						new GradientColorKey(Color.white, 1f)
					},
					new GradientAlphaKey[] {
						new GradientAlphaKey(1f, 0f),
						new GradientAlphaKey(1f, 1f)
					}
				);
				SerializedProperty pmt = property.FindPropertyRelative("m_MoveTweenEase");
				AnimationCurve cmt = pmt.animationCurveValue;
				if (cmt != null && cmt.length <= 0) {
					cmt.AddKey(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f));
					cmt.AddKey(new Keyframe(3f, 1f, 0f, 0f, 0f, 0f));
					pmt.animationCurveValue = cmt;
				}
				property.FindPropertyRelative("m_LuminanceFactor").floatValue = 1f;
				property.FindPropertyRelative("m_LightWidth").FindPropertyRelative("m_ConstValue").floatValue = 100f;
				property.FindPropertyRelative("m_EnvelopeSize").FindPropertyRelative("m_ConstValue").floatValue = 100f;
				property.FindPropertyRelative("m_EnvelopeAspectRatio").FindPropertyRelative("m_ConstValue").floatValue = 1f;
				property.FindPropertyRelative("m_Pivot").vector2Value = new Vector2(0.5f, 0.5f);
				property.FindPropertyRelative("m_Loop").boolValue = true;
				pInited.boolValue = true;
			}
			if (folded) { return; }
			int shineTypeIndex = property.FindPropertyRelative("m_ShineType").enumValueIndex;
			UIEffectShine.eShineType type = (UIEffectShine.eShineType)typeof(UIEffectShine.eShineType).GetEnumValues().GetValue(shineTypeIndex);
			mProps.Clear();
			GetPropertyList(type, mProps);
			float y = rect.y + lineheight + spacing;
			float buttonwidth = 48f;
			Rect rbtn = new Rect(rect.x + rect.width - buttonwidth, y, buttonwidth, lineheight);
			UIEffectShine fx = property.serializedObject.targetObject as UIEffectShine;
			if (!fx.IsPlaying(mPropertyArrayIndex) && GUI.Button(rbtn, "Play")) {
				fx.Play(mPropertyArrayIndex);
			}
			if (fx.IsPlaying(mPropertyArrayIndex) && GUI.Button(rbtn, "Stop")) {
				fx.Stop(mPropertyArrayIndex);
			}
			int pn = mProps.Count - 1;
			for (int i = 0; i <= pn; i++) {
				PropertySize ps = mProps[i];
				SerializedProperty prop = property.FindPropertyRelative(ps.property);
				float w = i == 0 ? rect.width - buttonwidth - 8f : rect.width;
				EditorGUI.PropertyField(new Rect(rect.x, y, w, ps.size), prop);
				if (i < pn) { y += lineheight + spacing; }
			}
		}

		private void GetPropertyList(UIEffectShine.eShineType type, List<PropertySize> props) {
			float lineheight = EditorGUIUtility.singleLineHeight;
			props.Add(new PropertySize("m_Name", lineheight));
			props.Add(new PropertySize("m_ShineType", lineheight));
			switch (type) {
				case UIEffectShine.eShineType.Linear:
					props.Add(new PropertySize("m_Gradient", lineheight));
					props.Add(new PropertySize("m_GradientLUT", lineheight));
					props.Add(new PropertySize("m_LightWidth", lineheight));
					break;
				case UIEffectShine.eShineType.Ring:
					props.Add(new PropertySize("m_Gradient", lineheight));
					props.Add(new PropertySize("m_GradientLUT", lineheight));
					props.Add(new PropertySize("m_Pivot", lineheight));
					props.Add(new PropertySize("m_LightWidth", lineheight));
					props.Add(new PropertySize("m_EnvelopeSize", lineheight));
					props.Add(new PropertySize("m_EnvelopeAspectRatio", lineheight));
					break;
				case UIEffectShine.eShineType.Texture:
					props.Add(new PropertySize("m_Pivot", lineheight));
					props.Add(new PropertySize("m_Rotation", lineheight));
					props.Add(new PropertySize("m_Texture", lineheight));
					props.Add(new PropertySize("m_EnvelopeSize", lineheight));
					props.Add(new PropertySize("m_EnvelopeAspectRatio", lineheight));
					break;
			}
			props.Add(new PropertySize("m_ColorOverLifetime", lineheight));
			props.Add(new PropertySize("m_LuminanceFactor", lineheight));
			props.Add(new PropertySize("m_Direction", lineheight));
			props.Add(new PropertySize("m_MoveTweenEase", lineheight));
			props.Add(new PropertySize("m_Delay", lineheight));
			props.Add(new PropertySize("m_Interval", lineheight));
			props.Add(new PropertySize("m_Loop", lineheight));
			props.Add(new PropertySize("m_AutoPlay", lineheight));
		}

		private void InitGradient(SerializedProperty property, GradientColorKey[] colors, GradientAlphaKey[] alphas) {
			if (property.propertyType != SerializedPropertyType.Gradient) { return; }
			int nc = colors.Length;
			int na = alphas.Length;
			for (int i = 0; i < 8; i++) {
				SerializedProperty pkey = property.FindPropertyRelative("key" + i);
				SerializedProperty pct = property.FindPropertyRelative("ctime" + i);
				SerializedProperty pat = property.FindPropertyRelative("atime" + i);
				Color color = Color.clear;
				int ct = 0;
				int at = 0;
				if (i < nc) {
					GradientColorKey ck = colors[i];
					color.r = ck.color.r; color.g = ck.color.g; color.b = ck.color.b;
					ct = Mathf.RoundToInt(65535f * ck.time);
				}
				if (i < na) {
					GradientAlphaKey ak = alphas[i];
					color.a = ak.alpha;
					at = Mathf.RoundToInt(65535f * ak.time);
				}
				pkey.colorValue = color;
				pct.intValue = ct;
				pat.intValue = at;
			}
			property.FindPropertyRelative("m_NumColorKeys").intValue = nc;
			property.FindPropertyRelative("m_NumAlphaKeys").intValue = na;
		}

	}

}