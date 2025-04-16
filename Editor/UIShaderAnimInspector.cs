using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GreatClock.Common.UIEffect {

	[CustomEditor(typeof(UIShaderAnim))]
	public class UIShaderAnimInspector : Editor {

		//private EditorWindow mGameView;
		private MethodInfo mMethodTick;
		private object[] mParams = new object[1];

		private List<string> mTempStrings = new List<string>();

		void OnEnable() {
			mMethodTick = typeof(UIShaderAnim).GetMethod("Tick", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			EditorApplication.update += OnEditorUpdate;
		}

		void OnDisable() {
			EditorApplication.update -= OnEditorUpdate;
		}

		public override void OnInspectorGUI() {
			UIShaderAnim ins = target as UIShaderAnim;
			Graphic g = ins.GetComponent<Graphic>();
			if (g.material == g.defaultMaterial) {
				EditorGUILayout.HelpBox("[UIEffectShine] only works for CUSTOM materials !", MessageType.Warning);
				return;
			}
			mTempStrings.Clear();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AutoPlay"));
			Color cachedGUIColor;
			SerializedProperty pAnims = serializedObject.FindProperty("m_Anims");
			int an = pAnims.arraySize;
			int removeAt = -1;
			for (int i = 0; i < an; i++) {
				cachedGUIColor = GUI.backgroundColor;
				if ((i & 1) == 0) { GUI.backgroundColor = Color.gray; }
				EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MinHeight(10f));
				GUI.backgroundColor = cachedGUIColor;
				SerializedProperty pAnim = pAnims.GetArrayElementAtIndex(i);
				string pName = pAnim.FindPropertyRelative("m_PropertyName").stringValue;
				//EditorGUILayout.PropertyField(pAnim);
				EditorGUILayout.BeginVertical(GUILayout.MinHeight(10f));
				EditorGUILayout.LabelField(pAnim.FindPropertyRelative("m_PropertyDesc").stringValue);
				EditorGUI.indentLevel++;
				EditorGUI.BeginDisabledGroup(true);
				SerializedProperty pType = pAnim.FindPropertyRelative("m_PropertyType");
				EditorGUILayout.PropertyField(pType);
				EditorGUI.EndDisabledGroup();
				SerializedProperty pUniformedDuration = pAnim.FindPropertyRelative("m_UniformedDuration");
				SerializedProperty pDuration = pAnim.FindPropertyRelative("m_Duration");
				SerializedProperty pCycles = pAnim.FindPropertyRelative("m_Cycles");

				switch ((UIShaderAnim.ePropertyType)pType.enumValueIndex) {
					case UIShaderAnim.ePropertyType.Float:
						EditorGUILayout.PropertyField(pUniformedDuration);
						if (pUniformedDuration.boolValue) {
							EditorGUI.indentLevel++;
							EditorGUILayout.PropertyField(pDuration);
							EditorGUI.indentLevel--;
						}
						EditorGUILayout.PropertyField(pCycles);
						EditorGUILayout.PropertyField(pAnim.FindPropertyRelative("m_CurveX"), new GUIContent(pAnim.FindPropertyRelative("m_CurveNameX").stringValue));
						break;
					case UIShaderAnim.ePropertyType.Color:
						EditorGUILayout.PropertyField(pDuration);
						EditorGUILayout.PropertyField(pCycles);
						EditorGUILayout.PropertyField(pAnim.FindPropertyRelative("m_Gradient"));
						break;
					case UIShaderAnim.ePropertyType.Vector:
						EditorGUILayout.PropertyField(pUniformedDuration);
						if (pUniformedDuration.boolValue) {
							EditorGUI.indentLevel++;
							EditorGUILayout.PropertyField(pDuration);
							EditorGUI.indentLevel--;
						}
						EditorGUILayout.PropertyField(pCycles);
						EditorGUILayout.PropertyField(pAnim.FindPropertyRelative("m_CurveX"), new GUIContent(pAnim.FindPropertyRelative("m_CurveNameX").stringValue));
						EditorGUILayout.PropertyField(pAnim.FindPropertyRelative("m_CurveY"), new GUIContent(pAnim.FindPropertyRelative("m_CurveNameY").stringValue));
						EditorGUILayout.PropertyField(pAnim.FindPropertyRelative("m_CurveZ"), new GUIContent(pAnim.FindPropertyRelative("m_CurveNameZ").stringValue));
						EditorGUILayout.PropertyField(pAnim.FindPropertyRelative("m_CurveW"), new GUIContent(pAnim.FindPropertyRelative("m_CurveNameW").stringValue));
						break;
				}
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
				mTempStrings.Add(pName);
				cachedGUIColor = GUI.backgroundColor;
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button("X", GUILayout.Width(20f))) {
					removeAt = i;
				}
				GUI.backgroundColor = cachedGUIColor;
				EditorGUILayout.EndHorizontal();
			}
			if (removeAt >= 0) {
				pAnims.DeleteArrayElementAtIndex(removeAt);
			}
			if (GUILayout.Button("Add Shader Property Animation")) {
				Shader shader = g.materialForRendering.shader;
				GenericMenu menu = new GenericMenu();
				int n = ShaderUtil.GetPropertyCount(shader);
				for (int i = 0; i < n; i++) {
					string pName = ShaderUtil.GetPropertyName(shader, i);
					string pDesc = ShaderUtil.GetPropertyDescription(shader, i);
					bool invalid = false;
					switch (ShaderUtil.GetPropertyType(shader, i)) {
						case ShaderUtil.ShaderPropertyType.Color: break;
						case ShaderUtil.ShaderPropertyType.Vector: break;
						case ShaderUtil.ShaderPropertyType.Float: break;
						case ShaderUtil.ShaderPropertyType.TexEnv:
							pName = pName + "_ST";
							if (!g.materialForRendering.HasProperty(pName)) { pName = null; }
							break;
						default: invalid = true; break;
					}
					if (pName == null) { continue; }
					GUIContent item = new GUIContent($"{pDesc} ({pName})");
					bool used = mTempStrings.Contains(pName);
					if (used || invalid) {
						menu.AddDisabledItem(item, used);
					} else {
						menu.AddItem(item, false, AddNewAnim, i);
					}

				}
				menu.ShowAsContext();
			}
			if (serializedObject.ApplyModifiedProperties()) {
				AssetDatabase.SaveAssets();
			}
			GUILayout.Space(8f);
			EditorGUILayout.BeginHorizontal(GUILayout.Height(24f));
			if (GUILayout.Button("Play", GUILayout.ExpandHeight(true))) {
				ins.Play();
			}
			GUILayout.Space(16f);
			if (GUILayout.Button("Stop", GUILayout.ExpandHeight(true))) {
				ins.Stop();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void AddNewAnim(object data) {
			int i = (int)data;
			UIShaderAnim ins = target as UIShaderAnim;
			Graphic g = ins.GetComponent<Graphic>();
			Material material = g.materialForRendering;
			Shader shader = material.shader;
			SerializedProperty pAnims = serializedObject.FindProperty("m_Anims");
			int index = pAnims.arraySize;
			pAnims.InsertArrayElementAtIndex(index);
			SerializedProperty pAnim = pAnims.GetArrayElementAtIndex(index);
			SerializedProperty pType = pAnim.FindPropertyRelative("m_PropertyType");
			string spName = ShaderUtil.GetPropertyName(shader, i);
			string nameSuffix = "";
			switch (ShaderUtil.GetPropertyType(shader, i)) {
				case ShaderUtil.ShaderPropertyType.Color:
					pType.enumValueIndex = (int)UIShaderAnim.ePropertyType.Color;
					Color color = material.GetColor(spName);
					SerializedProperty pGradient = pAnim.FindPropertyRelative("m_Gradient");
					pGradient.FindPropertyRelative("m_NumColorKeys").intValue = 2;
					pGradient.FindPropertyRelative("m_NumAlphaKeys").intValue = 2;
					pGradient.FindPropertyRelative("key0").colorValue = color;
					pGradient.FindPropertyRelative("key1").colorValue = color;
					pGradient.FindPropertyRelative("ctime0").intValue = 0;
					pGradient.FindPropertyRelative("ctime1").intValue = 65535;
					pGradient.FindPropertyRelative("atime0").intValue = 0;
					pGradient.FindPropertyRelative("atime1").intValue = 65535;
					pGradient.FindPropertyRelative("m_Mode").intValue = 0;
					break;
				case ShaderUtil.ShaderPropertyType.Vector:
					pType.enumValueIndex = (int)UIShaderAnim.ePropertyType.Vector;
					pAnim.FindPropertyRelative("m_CurveNameX").stringValue = "x";
					pAnim.FindPropertyRelative("m_CurveNameY").stringValue = "y";
					pAnim.FindPropertyRelative("m_CurveNameZ").stringValue = "z";
					pAnim.FindPropertyRelative("m_CurveNameW").stringValue = "w";
					Vector4 vector = material.GetVector(spName);
					SerializedProperty pCurveX = pAnim.FindPropertyRelative("m_CurveX");
					pCurveX.animationCurveValue = new AnimationCurve(new Keyframe(0f, vector.x), new Keyframe(1f, vector.x));
					SerializedProperty pCurveY = pAnim.FindPropertyRelative("m_CurveY");
					pCurveY.animationCurveValue = new AnimationCurve(new Keyframe(0f, vector.y), new Keyframe(1f, vector.y));
					SerializedProperty pCurveZ = pAnim.FindPropertyRelative("m_CurveZ");
					pCurveZ.animationCurveValue = new AnimationCurve(new Keyframe(0f, vector.z), new Keyframe(1f, vector.z));
					SerializedProperty pCurveW = pAnim.FindPropertyRelative("m_CurveW");
					pCurveW.animationCurveValue = new AnimationCurve(new Keyframe(0f, vector.w), new Keyframe(1f, vector.w));
					break;
				case ShaderUtil.ShaderPropertyType.Float:
					pType.enumValueIndex = (int)UIShaderAnim.ePropertyType.Float;
					pAnim.FindPropertyRelative("m_CurveNameX").stringValue = "Value";
					float value = material.GetFloat(spName);
					SerializedProperty pCurve = pAnim.FindPropertyRelative("m_CurveX");
					pCurve.animationCurveValue = new AnimationCurve(new Keyframe(0f, value), new Keyframe(1f, value));
					break;
				case ShaderUtil.ShaderPropertyType.TexEnv:
					pType.enumValueIndex = (int)UIShaderAnim.ePropertyType.Vector;
					nameSuffix = "_ST";
					pAnim.FindPropertyRelative("m_CurveNameX").stringValue = "Tilling.x";
					pAnim.FindPropertyRelative("m_CurveNameY").stringValue = "Tilling.y";
					pAnim.FindPropertyRelative("m_CurveNameZ").stringValue = "Offset.x";
					pAnim.FindPropertyRelative("m_CurveNameW").stringValue = "Offset.y";
					Vector4 st = material.GetVector(spName + nameSuffix);
					SerializedProperty pCurveTX = pAnim.FindPropertyRelative("m_CurveX");
					pCurveTX.animationCurveValue = new AnimationCurve(new Keyframe(0f, st.x), new Keyframe(1f, st.x));
					SerializedProperty pCurveTY = pAnim.FindPropertyRelative("m_CurveY");
					pCurveTY.animationCurveValue = new AnimationCurve(new Keyframe(0f, st.y), new Keyframe(1f, st.y));
					SerializedProperty pCurveOX = pAnim.FindPropertyRelative("m_CurveZ");
					pCurveOX.animationCurveValue = new AnimationCurve(new Keyframe(0f, st.z), new Keyframe(1f, st.z));
					SerializedProperty pCurveOY = pAnim.FindPropertyRelative("m_CurveW");
					pCurveOY.animationCurveValue = new AnimationCurve(new Keyframe(0f, st.w), new Keyframe(1f, st.w));
					break;
				default:
					pAnims.DeleteArrayElementAtIndex(index);
					return;
			}
			SerializedProperty pDesc = pAnim.FindPropertyRelative("m_PropertyDesc");
			SerializedProperty pName = pAnim.FindPropertyRelative("m_PropertyName");
			pName.stringValue = spName + nameSuffix;
			pDesc.stringValue = ShaderUtil.GetPropertyDescription(shader, i);
			pAnim.FindPropertyRelative("m_UniformedDuration").boolValue = true;
			pAnim.FindPropertyRelative("m_Duration").floatValue = 1f;
			pAnim.FindPropertyRelative("m_Cycles").intValue = -1;
			if (serializedObject.ApplyModifiedProperties()) {
				AssetDatabase.SaveAssets();
			}
			Repaint();
		}

		private DateTime mPrevTick;
		private void OnEditorUpdate() {
			if (!Application.isPlaying) {
				DateTime now = DateTime.UtcNow;
				float deltaTime = (float)(now - mPrevTick).TotalSeconds;
				if (deltaTime < 1f) {
					mParams[0] = deltaTime;
					mMethodTick.Invoke(target, mParams);
					UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				}
				mPrevTick = now;
			}
		}

		/*
        private static string ReadPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return property.intValue.ToString();
                case SerializedPropertyType.Boolean: return property.boolValue.ToString();
                case SerializedPropertyType.Float: return property.floatValue.ToString();
                case SerializedPropertyType.String: return property.stringValue;
                case SerializedPropertyType.Color: return property.colorValue.ToString();
                case SerializedPropertyType.ObjectReference: return $"{property.objectReferenceValue}";
                case SerializedPropertyType.LayerMask: return property.intValue.ToString();
                case SerializedPropertyType.Enum: return property.enumNames[property.enumValueIndex];
                case SerializedPropertyType.Vector2: return property.vector2Value.ToString();
                case SerializedPropertyType.Vector3: return property.vector3Value.ToString();
                case SerializedPropertyType.Vector4: return property.vector4Value.ToString();
                case SerializedPropertyType.Rect: return property.rectValue.ToString();
                case SerializedPropertyType.ArraySize: return property.arraySize.ToString();
                case SerializedPropertyType.Character: break;
                case SerializedPropertyType.AnimationCurve: return property.animationCurveValue.ToString();
                case SerializedPropertyType.Bounds: return property.boundsValue.ToString();
                case SerializedPropertyType.Gradient: break;
                case SerializedPropertyType.Quaternion: return property.quaternionValue.ToString();
                case SerializedPropertyType.ExposedReference: return property.exposedReferenceValue.ToString();
                case SerializedPropertyType.FixedBufferSize: return property.fixedBufferSize.ToString();
                case SerializedPropertyType.Vector2Int: return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int: return property.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt: return property.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt: return property.boundsIntValue.ToString();
                case SerializedPropertyType.ManagedReference: return property.managedReferenceFullTypename;
            }
            return $"Unable to read value of type '{property.propertyType}'";
        }
        */
	}

}