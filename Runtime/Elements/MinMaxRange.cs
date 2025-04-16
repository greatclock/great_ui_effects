using System;
using UnityEngine;

namespace GreatClock.Common.UIEffect {

	[Serializable]
	public class MinMaxRange {

		[SerializeField]
		private float m_Min = 0f;
		[SerializeField]
		private float m_Max = 1f;

		public float Min { get { return m_Min; } }
		public float Max { get { return m_Max; } }

	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class MinMaxLimitAttribute : Attribute {
		public readonly float min;
		public readonly float max;
		public MinMaxLimitAttribute(float min, float max) {
			this.min = min;
			this.max = max;
		}
	}

}