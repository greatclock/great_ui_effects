using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace GreatClock.Common.UIEffect {

	[CustomEditor(typeof(UIEffectShine))]
	public class UIEffectShineInspector : Editor {

		private MethodInfo mGameViewRepaint;

		private MethodInfo mMethodTick;
		private object[] mParams = new object[1];

		void OnEnable() {
			Type tpm = Type.GetType("UnityEditor.PlayModeView,UnityEditor");
			mGameViewRepaint = tpm.GetMethod("RepaintAll", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			mMethodTick = typeof(UIEffectShine).GetMethod("Tick", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			EditorApplication.update += OnEditorUpdate;
		}

		private void OnDisable() {
			EditorApplication.update -= OnEditorUpdate;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			UIEffectShine comp = target as UIEffectShine;
			GUILayout.Space(8f);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Play All")) { comp.Play(); }
			if (GUILayout.Button("Stop All")) { comp.Stop(); }
			Canvas canvas = comp.GetComponentInParent<Canvas>();
			EditorGUILayout.EndHorizontal();
			if (canvas != null && (canvas.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0) {
				EditorGUILayout.HelpBox("'TexCoord1' is required in 'Canvas' !", MessageType.Error);
			}
		}

		private DateTime mPrevTick;
		private void OnEditorUpdate() {
			if (!Application.isPlaying) {
				DateTime now = DateTime.UtcNow;
				float deltaTime = (float)(now - mPrevTick).TotalSeconds;
				if (deltaTime < 1f) {
					mParams[0] = deltaTime;
					if ((bool)mMethodTick.Invoke(target, mParams)) {
						//UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
						SceneView.RepaintAll();
						mGameViewRepaint.Invoke(null, null);
					}
				}
				mPrevTick = now;
			}
		}

	}

}