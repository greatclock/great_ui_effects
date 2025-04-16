using System;
using UnityEngine;

namespace GreatClock.Common.UIEffect {

	[Serializable]
	public class FloatField {

		[SerializeField]
		private float m_ConstValue;
		[SerializeField]
		private AnimationCurve m_Curve;
		[SerializeField]
		private bool m_UseCurve = false;

		public FloatField() {
			m_ConstValue = 0f;
		}

		public FloatField(float value) {
			m_ConstValue = value;
		}

		public float GetValue(float t) {
			if (m_UseCurve) {
				float dur = m_Curve[m_Curve.length - 1].time;
				return m_Curve.Evaluate(t * dur);
			}
			return m_ConstValue;
		}

	}

}