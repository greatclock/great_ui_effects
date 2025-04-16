using System;
using UnityEngine;
using UnityEngine.UI;

namespace GreatClock.Common.UIEffect {

	[ExecuteAlways]
	[RequireComponent(typeof(Graphic))]
	public class UIShaderAnim : MonoBehaviour {

		[SerializeField]
		private bool m_AutoPlay = true;
		[SerializeField]
		private PropertyAnim[] m_Anims;

		private Material mMaterial;

		public void Play() {
			if (mMaterial == null) {
				Graphic graphic = GetComponent<Graphic>();
				mMaterial = graphic.materialForRendering;
			}
		}

		public void Pause() {
			mMaterial = null;
		}

		public void Stop() {
			Material mat = mMaterial;
			mMaterial = null;
			if (mat == null) {
				Graphic graphic = GetComponent<Graphic>();
				mat = graphic.materialForRendering;
			}
			for (int i = m_Anims.Length - 1; i >= 0; i--) {
				PropertyAnim anim = m_Anims[i];
				anim.Reset();
				anim.UpdateAndApply(0f, mat);
			}
		}

		void Start() {
			if (Application.isPlaying && m_AutoPlay) {
				Play();
			}
		}

		void Update() {
#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				Tick(Time.deltaTime);
#if UNITY_EDITOR
			}
#endif
		}

		private void Tick(float dt) {
			if (mMaterial == null) { return; }
			for (int i = m_Anims.Length - 1; i >= 0; i--) {
				m_Anims[i].UpdateAndApply(dt, mMaterial);
			}
		}

		public enum ePropertyType { Float, Color, Vector }

		[Serializable]
		public class PropertyAnim {

			[SerializeField]
			private string m_PropertyDesc;
			[SerializeField]
			private string m_PropertyName;
			[SerializeField]
			private ePropertyType m_PropertyType;
			[SerializeField]
			private bool m_UniformedDuration;
			[SerializeField]
			private float m_Duration;
			[SerializeField]
			private int m_Cycles;
			[SerializeField]
			private string m_CurveNameX;
			[SerializeField]
			private string m_CurveNameY;
			[SerializeField]
			private string m_CurveNameZ;
			[SerializeField]
			private string m_CurveNameW;
			[SerializeField]
			private AnimationCurve m_CurveX;
			[SerializeField]
			private AnimationCurve m_CurveY;
			[SerializeField]
			private AnimationCurve m_CurveZ;
			[SerializeField]
			private AnimationCurve m_CurveW;
			[SerializeField]
			private Gradient m_Gradient;

			private bool mInited = false;
			private int mPropertyId;

			private float mTimerX;
			private float mTimerY;
			private float mTimerZ;
			private float mTimerW;
			private float mDurationX;
			private float mDurationY;
			private float mDurationZ;
			private float mDurationW;
			private int mCyclesX;
			private int mCyclesY;
			private int mCyclesZ;
			private int mCyclesW;

			public void UpdateAndApply(float dt, Material mat) {
				if (!mInited) {
					mInited = true;
					mPropertyId = Shader.PropertyToID(m_PropertyName);
					switch (m_PropertyType) {
						case ePropertyType.Float:
							float dur = m_UniformedDuration ? m_Duration : m_CurveX[m_CurveX.length - 1].time;
							mDurationX = dur > 0f ? 1f / dur : 0f;
							break;
						case ePropertyType.Color:
							mDurationX = m_Duration > 0f ? 1f / m_Duration : 0f;
							break;
						case ePropertyType.Vector:
							float durX = m_UniformedDuration ? m_Duration : m_CurveX[m_CurveX.length - 1].time;
							float durY = m_UniformedDuration ? m_Duration : m_CurveY[m_CurveY.length - 1].time;
							float durZ = m_UniformedDuration ? m_Duration : m_CurveZ[m_CurveZ.length - 1].time;
							float durW = m_UniformedDuration ? m_Duration : m_CurveW[m_CurveW.length - 1].time;
							mDurationX = durX > 0f ? 1f / durX : 0f;
							mDurationY = durY > 0f ? 1f / durY : 0f;
							mDurationZ = durZ > 0f ? 1f / durZ : 0f;
							mDurationW = durW > 0f ? 1f / durW : 0f;
							break;
					}
					mTimerX = mTimerY = mTimerZ = mTimerW = 0f;
					mCyclesX = mCyclesY = mCyclesZ = mCyclesW = m_Cycles;
				}
				switch (m_PropertyType) {
					case ePropertyType.Float:
						float tf = Tick(dt, ref mTimerX, mDurationX, ref mCyclesX);
						float val = tf >= 0f ? m_CurveX.Evaluate(m_UniformedDuration ? tf : mTimerX) : m_CurveX[m_CurveX.length - 1].value;
						mat.SetFloat(mPropertyId, val);
						break;
					case ePropertyType.Color:
						float tc = Tick(dt, ref mTimerX, mDurationX, ref mCyclesX);
						mat.SetColor(mPropertyId, m_Gradient.Evaluate(tc >= 0f ? tc : 1f));
						break;
					case ePropertyType.Vector:
						float tx = Tick(dt, ref mTimerX, mDurationX, ref mCyclesX);
						float ty = Tick(dt, ref mTimerY, mDurationY, ref mCyclesY);
						float tz = Tick(dt, ref mTimerZ, mDurationZ, ref mCyclesZ);
						float tw = Tick(dt, ref mTimerW, mDurationW, ref mCyclesW);
						float x = tx >= 0f ? m_CurveX.Evaluate(m_UniformedDuration ? tx : mTimerX) : m_CurveX[m_CurveX.length - 1].value;
						float y = ty >= 0f ? m_CurveY.Evaluate(m_UniformedDuration ? ty : mTimerY) : m_CurveY[m_CurveY.length - 1].value;
						float z = tz >= 0f ? m_CurveZ.Evaluate(m_UniformedDuration ? tz : mTimerZ) : m_CurveZ[m_CurveZ.length - 1].value;
						float w = tw >= 0f ? m_CurveW.Evaluate(m_UniformedDuration ? tw : mTimerW) : m_CurveW[m_CurveW.length - 1].value;
						mat.SetVector(mPropertyId, new Vector4(x, y, z, w));
						break;
				}
			}

			public void Reset() {
				mInited = false;
			}

			private static float Tick(float dt, ref float timer, float inv_dur, ref int cycles) {
				if (inv_dur <= 0f || cycles == 0) { return -1f; }
				timer += dt;
				float t = timer * inv_dur;
				if (t > 1) {
					if (cycles > 0) { cycles--; }
					if (cycles == 0) {
						t = 1f;
						timer = 1f / inv_dur;
					} else {
						t -= 1f;
						timer -= 1f / inv_dur;
					}
				}
				return t;
			}

		}

	}

}