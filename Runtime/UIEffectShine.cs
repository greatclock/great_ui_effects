using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GreatClock.Common.UIEffect {

	[RequireComponent(typeof(Graphic), typeof(RectTransform)), ExecuteInEditMode]
	public class UIEffectShine : MonoBehaviour, IMaterialModifier, IMeshModifier {

		[SerializeField]
		private bool m_SourceVisible = true;
		[SerializeField]
		private ShineData[] m_EffectShines = new ShineData[0];

		public void Play() {
			if (!mMaterialOK) { return; }
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				ShineData data = m_EffectShines[i];
				if (data.AutoPlay) {
					data.Start();
				}
			}
		}

		public bool Play(int index) {
			if (index < 0 || index >= m_EffectShines.Length) { return false; }
			if (!mMaterialOK) { return false; }
			m_EffectShines[index].Start();
			return true;
		}

		public bool Play(string name) {
			if (string.IsNullOrEmpty(name)) { return false; }
			if (!mMaterialOK) { return false; }
			int n = 0;
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				ShineData data = m_EffectShines[i];
				if (data.Name == name) {
					n++;
					data.Start();
				}
			}
			return n > 0;
		}

		public bool IsPlaying(int index) {
			if (index < 0 || index >= m_EffectShines.Length) { return false; }
			return m_EffectShines[index].Playing;
		}

		public bool IsPlaying(string name) {
			if (string.IsNullOrEmpty(name)) { return false; }
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				ShineData data = m_EffectShines[i];
				if (data.Name == name && data.Playing) {
					return true;
				}
			}
			return false;
		}

		public bool ResetPivot(int index, Vector2 pivot) {
			if (index < 0 || index >= m_EffectShines.Length) { return false; }
			ShineData data = m_EffectShines[index];
			data.ResetPivot(pivot);
			data.Init(GetGraphicSize());
			return true;
		}

		public bool ResetPivot(string name, Vector2 pivot) {
			if (string.IsNullOrEmpty(name)) { return false; }
			int n = 0;
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				ShineData data = m_EffectShines[i];
				if (data.Name == name) {
					data.ResetPivot(pivot);
					data.Init(GetGraphicSize());
					n++;
				}
			}
			return n > 0;
		}

		public void Stop() {
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				m_EffectShines[i].Stop();
			}
		}

		public bool Stop(int index) {
			if (index < 0 || index >= m_EffectShines.Length) { return false; }
			m_EffectShines[index].Stop();
			return true;
		}

		public bool Stop(string name) {
			if (string.IsNullOrEmpty(name)) { return false; }
			int n = 0;
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				ShineData data = m_EffectShines[i];
				if (data.Name == name) {
					n++;
					data.Stop();
				}
			}
			return n > 0;
		}

		public void ReInit() {
			Stop();
			Init();
		}

		private RectTransform mTrans;
		private Graphic mGraphic;
		private Image mImage;
		// private RawImage mRawImage;

		private Material mRenderMaterial;
		private bool mMaterialOK = true;

		#region lifecycle
		void Awake() {
			mTrans = GetComponent<RectTransform>();
			mGraphic = GetComponent<Graphic>();
			mImage = mGraphic as Image;
			// mRawImage = mGraphic as RawImage;
			if (mRenderMaterial == null) {
				mRenderMaterial = new Material(Shader.Find("Hidden/GreatUI/EffectShine"));
			}
		}

		void OnEnable() {
			Init();
#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				if (mMaterialOK) { Play(); }
#if UNITY_EDITOR
			}
#endif
		}

		void OnDisable() {
#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				Stop();
#if UNITY_EDITOR
			}
#endif
		}

		private void Init() {
			if (!s_shader_prop_inited) {
				s_shader_prop_inited = true;
				s_id_src_mode = Shader.PropertyToID("_SrcMode");
				s_id_src_blend = Shader.PropertyToID("_SrcBlend");
				s_id_dst_blend = Shader.PropertyToID("_DstBlend");
				s_id_linear_count = Shader.PropertyToID("_LinearCount");
				s_id_ring_count = Shader.PropertyToID("_RingCount");
				s_id_texture_count = Shader.PropertyToID("_TextureCount");
				s_id_linear_datas = Shader.PropertyToID("_LinearDatas");
				s_id_ring_datas = Shader.PropertyToID("_RingDatas");
				s_id_texture_datas = Shader.PropertyToID("_TextureDatas");
				s_id_rect = Shader.PropertyToID("_Rect");
				s_id_color_key_count = Shader.PropertyToID("_ColorKeyCount");
				s_id_color_keys = Shader.PropertyToID("_ColorKeys");
				s_id_alpha_key_count = Shader.PropertyToID("_AlphaKeyCount");
				s_id_alpha_keys = Shader.PropertyToID("_AlphaKeys");
				s_id_gradient = Shader.PropertyToID("_Gradient");
				s_id_stencil_comp = Shader.PropertyToID("_StencilComp");
				s_id_stencil = Shader.PropertyToID("_Stencil");
				s_id_stencil_op = Shader.PropertyToID("_StencilOp");
				s_id_stencil_write_mask = Shader.PropertyToID("_StencilWriteMask");
				s_id_stencil_read_mask = Shader.PropertyToID("_StencilReadMask");
				s_id_atlas = Shader.PropertyToID("_Atlas");
			}
			ResetGradients();
			GenerateAtlas();
			Vector2 graphicSize = GetGraphicSize();
			foreach (ShineData ef in m_EffectShines) {
				ef.Init(graphicSize);
			}
			if (m_SourceVisible) {
				mRenderMaterial.SetFloat(s_id_src_mode, 0f);
				mRenderMaterial.SetInt(s_id_src_blend, (int)BlendMode.SrcAlpha);
				mRenderMaterial.SetInt(s_id_dst_blend, (int)BlendMode.OneMinusSrcAlpha);
			} else {
				mRenderMaterial.SetFloat(s_id_src_mode, 1f);
				mRenderMaterial.SetInt(s_id_src_blend, (int)BlendMode.One);
				mRenderMaterial.SetInt(s_id_dst_blend, (int)BlendMode.One);
			}
		}

		void Update() {
			if (mGradientsTexture != null && !mGradientsTexture.IsCreated()) {
				ResetGradients();
			}
			RenderTexture atlas = mAtlas as RenderTexture;
			if (atlas != null && !atlas.IsCreated()) {
				GenerateAtlas();
			}
#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				Tick(Time.deltaTime);
#if UNITY_EDITOR
			}
#endif
		}

		private Vector4[] mLinearData = new Vector4[64];
		private Vector4[] mRingData = new Vector4[64];
		private Vector4[] mTextureData = new Vector4[80];

		private bool Tick(float deltaTime) {
			bool ret = false;
			int linearCount = 0;
			int ringCount = 0;
			int textureCount = 0;
			for (int i = m_EffectShines.Length - 1; i >= 0; i--) {
				ShineData data = m_EffectShines[i];
				Vector4 p0 = Vector4.zero;
				Vector4 p1 = Vector4.zero;
				Vector4 p2 = Vector4.zero;
				Vector4 p3 = Vector4.zero;
				Vector4 p4 = Vector4.zero;
				if (data.Tick(deltaTime, ref p0, ref p1, ref p2, ref p3, ref p4)) {
					// Debug.LogWarning($"p0:{p0}, p1:{p1}, p2:{p2}");
					ret = true;
					int index;
					switch (data.ShineType) {
						case eShineType.Linear:
							index = linearCount * 4;
							mLinearData[index] = p0;
							mLinearData[index + 1] = p1;
							mLinearData[index + 2] = p2;
							mLinearData[index + 3] = p3;
							linearCount++;
							break;
						case eShineType.Ring:
							index = ringCount * 4;
							mRingData[index] = p0;
							mRingData[index + 1] = p1;
							mRingData[index + 2] = p2;
							mRingData[index + 3] = p3;
							ringCount++;
							break;
						case eShineType.Texture:
							index = textureCount * 5;
							mTextureData[index] = p0;
							mTextureData[index + 1] = p1;
							mTextureData[index + 2] = p2;
							mTextureData[index + 3] = p3;
							mTextureData[index + 4] = p4;
							textureCount++;
							break;
					}
				}
			}
			mRenderMaterial.SetInt(s_id_linear_count, linearCount);
			mRenderMaterial.SetInt(s_id_ring_count, ringCount);
			mRenderMaterial.SetInt(s_id_texture_count, textureCount);
			if (linearCount > 0) { mRenderMaterial.SetVectorArray(s_id_linear_datas, mLinearData); }
			if (ringCount > 0) { mRenderMaterial.SetVectorArray(s_id_ring_datas, mRingData); }
			if (textureCount > 0) { mRenderMaterial.SetVectorArray(s_id_texture_datas, mTextureData); }
			return ret;
		}

#if UNITY_EDITOR

		void OnValidate() {
			Awake();
			Init();
		}

#endif

		void OnDestroy() {
			if (mGradientsTexture != null) { DestroyImmediate(mGradientsTexture); }
			RenderTexture atlas = mAtlas as RenderTexture;
			if (atlas != null) { DestroyImmediate(atlas); }
		}

		#endregion

		#region properties

		private Vector2 GetGraphicSize() {
			Rect rect = mTrans.rect;
			Vector2 size = rect.size;
			if (mImage != null && mImage.preserveAspect) {
				Sprite sprite = mImage.sprite;
				if (sprite != null) {
					Vector2 s = sprite.rect.size;
					float r = size.x / size.y;
					if (size.x / size.y > r) {
						size.x = size.y * r;
					} else {
						size.y = size.x / r;
					}
				}
			}
			return size;
		}

		#endregion

		#region gradients

		private RenderTexture mGradientsTexture;

		private void ResetGradients() {
			int n = 0;
			foreach (ShineData ef in m_EffectShines) {
				if (ef.ShineType == eShineType.Linear || ef.ShineType == eShineType.Ring) {
					n++;
				}
			}
			int height = n * 4;
			if (mGradientsTexture != null && mGradientsTexture.height != height) {
				DestroyImmediate(mGradientsTexture);
				mGradientsTexture = null;
			}
			float n_ = 0f;
			if (n > 0) {
				if (mGradientsTexture == null) {
					mGradientsTexture = new RenderTexture(256, height, 0, RenderTextureFormat.ARGB32, 0);
					mGradientsTexture.name = "UIShineGradient";
					mGradientsTexture.filterMode = FilterMode.Bilinear;
				}
				if (!mGradientsTexture.IsCreated()) { mGradientsTexture.Create(); }
				Material mat = new Material(Shader.Find("Hidden/GreatUI/GradientDrawer"));
				Vector4[] cbuffer = new Vector4[16];
				Vector4[] abuffer = new Vector4[16];
				n_ = 1f / n;
				for (int i = 0; i < n; i++) {
					mat.SetVector(s_id_rect, new Vector4(0f, i * n_, 1f, n_));
					ShineData data = m_EffectShines[i];
					GradientColorKey[] ckeys = data.Gradient.colorKeys;
					int cn = ckeys.Length;
					for (int j = 0; j < cn; j++) {
						GradientColorKey key = ckeys[j];
						cbuffer[j] = new Vector4(key.color.r, key.color.g, key.color.b, key.time);
					}
					GradientAlphaKey[] akeys = data.Gradient.alphaKeys;
					int an = akeys.Length;
					for (int j = 0; j < an; j++) {
						GradientAlphaKey key = akeys[j];
						abuffer[j] = new Vector4(key.alpha, key.alpha, key.alpha, key.time);
					}
					mat.SetInt(s_id_color_key_count, cn);
					mat.SetVectorArray(s_id_color_keys, cbuffer);
					mat.SetInt(s_id_alpha_key_count, an);
					mat.SetVectorArray(s_id_alpha_keys, abuffer);
					Graphics.Blit(null, mGradientsTexture, mat);
				}
				mRenderMaterial.SetTexture(s_id_gradient, mGradientsTexture);
			}
			float uvy = n_ * 0.5f;
			foreach (ShineData ef in m_EffectShines) {
				if (ef.ShineType == eShineType.Linear || ef.ShineType == eShineType.Ring) {
					ef.UVy = uvy;
					uvy += n_;
				}
			}
		}

		Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial) {
			if (mGraphic.material != mGraphic.defaultMaterial) {
				mMaterialOK = false;
				Stop();
				return baseMaterial;
			}
			bool flag = !mMaterialOK;
			mMaterialOK = true;
			if (mRenderMaterial == null) {
				Debug.LogError("mRenderMaterial is null");
				return baseMaterial;
			}
			if (baseMaterial != null) {
				mRenderMaterial.SetFloat(s_id_stencil_comp, baseMaterial.GetFloat(s_id_stencil_comp));
				mRenderMaterial.SetFloat(s_id_stencil, baseMaterial.GetFloat(s_id_stencil));
				mRenderMaterial.SetFloat(s_id_stencil_op, baseMaterial.GetFloat(s_id_stencil_op));
				mRenderMaterial.SetFloat(s_id_stencil_write_mask, baseMaterial.GetFloat(s_id_stencil_write_mask));
				mRenderMaterial.SetFloat(s_id_stencil_read_mask, baseMaterial.GetFloat(s_id_stencil_read_mask));
			}
#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				if (flag) { Play(); }
#if UNITY_EDITOR
			}
#endif
			return mRenderMaterial;
		}

		void IMeshModifier.ModifyMesh(Mesh mesh) { }

		void IMeshModifier.ModifyMesh(VertexHelper verts) {
			int n = verts.currentVertCount;
			Vector2 vertMin = new Vector2(float.MaxValue, float.MaxValue);
			// Vector2 vertMax = new Vector2(float.MinValue, float.MinValue);
			UIVertex vertex = new UIVertex();
			for (int i = 0; i < n; i++) {
				verts.PopulateUIVertex(ref vertex, i);
				vertMin = Vector2.Min(vertMin, vertex.position);
				// vertMax = Vector2.Max(vertMax, vertex.position);
			}
			for (int i = 0; i < n; i++) {
				verts.PopulateUIVertex(ref vertex, i);
				vertex.uv1 = (Vector2)vertex.position - vertMin;
				verts.SetUIVertex(vertex, i);
			}
		}

		#endregion

		#region atlas

		private Texture mAtlas;

		private void GenerateAtlas() {
			List<Texture> textures = new List<Texture>();
			foreach (ShineData ef in m_EffectShines) {
				if (ef.ShineType == eShineType.Texture && ef.Texture != null) {
					textures.Add(ef.Texture);
					ef.UV = new Rect(0f, 0f, 1f, 1f);
				}
			}
			int n = textures.Count;
			RenderTexture rt = mAtlas as RenderTexture;
			if (n <= 1) {
				if (rt != null) {
					DestroyImmediate(rt);
				}
				mAtlas = null;
				if (n == 1) {
					mAtlas = textures[0];
				}
			} else {
				float sqrt = Mathf.Sqrt(n);
				int w = Mathf.CeilToInt(sqrt);
				int h = Mathf.RoundToInt(sqrt);
				int size = 512;
				while (w * size > 2048) {
					size >>= 1;
				}
				int width = w * size;
				int height = h * size;
				if (rt == null || rt.width != width || rt.height != height) {
					if (rt != null) { DestroyImmediate(rt); }
					rt = new RenderTexture(width, height, 0, RenderTextureFormat.BGRA32, 0);
					rt.name = "UIEffectShine.Atlas";
					rt.filterMode = FilterMode.Bilinear;
					mAtlas = rt;
				}
				List<Rect> uvs = new List<Rect>();
				Material mat = new Material(Shader.Find("Hidden/GreatUI/AtlasDrawer"));
				float u = 0f;
				float v = 0f;
				float wu = size / (float)width;
				float hv = size / (float)height;
				for (int i = 0; i < n; i++) {
					Rect uv = new Rect(u, v, wu, hv);
					mat.SetVector(s_id_rect, new Vector4(uv.x, uv.y, uv.width, uv.height));
					Graphics.Blit(textures[i], rt, mat);
					uvs.Add(uv);
					u += wu;
					if (u > 0.99f) {
						u = 0f;
						v += hv;
					}
				}
				DestroyImmediate(mat);
				foreach (ShineData ef in m_EffectShines) {
					if (ef.ShineType == eShineType.Texture && ef.Texture != null) {
						ef.UV = uvs[textures.IndexOf(ef.Texture)];
					}
				}
			}
			mRenderMaterial.SetTexture(s_id_atlas, mAtlas);
		}

		#endregion

		public enum eShineType { Linear, Ring, Texture }

		[Serializable]
		public class ShineData {

			[SerializeField]
			private bool m_Inited;
			[SerializeField]
			private string m_Name;
			[SerializeField]
			private eShineType m_ShineType;
			[SerializeField]
			private Gradient m_Gradient;
			[SerializeField]
			private Gradient m_ColorOverLifetime;
			[SerializeField, Range(0f, 5f)]
			private float m_LuminanceFactor = 1f;
			[SerializeField]
			private Vector2 m_Pivot = new Vector2(0.5f, 0.5f);
			[SerializeField]
			private float m_Direction;
			[SerializeField]
			private AnimationCurve m_MoveTweenEase;
			[SerializeField]
			private FloatField m_Rotation = new FloatField(0f);
			[SerializeField]
			public FloatField m_LightWidth = new FloatField(40f);
			[SerializeField]
			private Texture m_Texture;
			[SerializeField]
			public FloatField m_EnvelopeSize = new FloatField(100f);
			[SerializeField]
			public FloatField m_EnvelopeAspectRatio = new FloatField(1f);
			[SerializeField, Min(0f)]
			public float m_Delay = 2f;
			[SerializeField, Min(0f)]
			public float m_Interval = 4f;
			[SerializeField]
			public bool m_Loop = true;
			[SerializeField]
			private bool m_AutoPlay;

			public string Name { get { return m_Name; } }

			public eShineType ShineType { get { return m_ShineType; } }

			public Gradient Gradient { get { return m_Gradient; } }

			public Texture Texture { get { return m_Texture; } }

			private float mDirection = float.NaN;
			private float mSin, mCos;
			public void GetRotationSinCos(out float sin, out float cos) {
				if (mDirection != m_Direction) {
					mDirection = m_Direction;
					float rad = Mathf.Deg2Rad * m_Direction;
					mSin = Mathf.Sin(rad);
					mCos = Mathf.Cos(rad);
				}
				sin = mSin;
				cos = mCos;
			}

			public bool AutoPlay { get { return m_AutoPlay; } }

			public float UVy { get; set; }
			public Rect UV { get; set; }

			private Vector2 mFrom, mTo;

			public void Init(Vector2 graphicSize) {
				Vector2 center = graphicSize * 0.5f;
				Vector2 pivot = graphicSize * m_Pivot - center;
				Vector2 hs0 = center;
				Vector2 hs1 = center;
				Vector2 lightWidth = Vector2.zero;
				if (m_ShineType == eShineType.Ring || m_ShineType == eShineType.Texture) {
					float envelope0 = m_EnvelopeSize.GetValue(0f);
					float envelope1 = m_EnvelopeSize.GetValue(1f);
					float ratio0 = m_EnvelopeAspectRatio.GetValue(0f);
					float ratio1 = m_EnvelopeAspectRatio.GetValue(1f);
					hs0 += new Vector2(envelope0, envelope0 / ratio0) * 0.5f;
					hs1 += new Vector2(envelope1, envelope1 / ratio1) * 0.5f;
				}
				if (m_ShineType == eShineType.Linear || m_ShineType == eShineType.Ring) {
					lightWidth.x = m_LightWidth.GetValue(0f) * 0.5f;
					lightWidth.y = m_LightWidth.GetValue(1f) * 0.5f;
				}
				Vector2 oa0 = new Vector2(-hs0.x, -hs0.y) - pivot;
				Vector2 ob0 = new Vector2(-hs0.x, hs0.y) - pivot;
				Vector2 oc0 = new Vector2(hs0.x, hs0.y) - pivot;
				Vector2 od0 = new Vector2(hs0.x, -hs0.y) - pivot;
				Vector2 oa1 = new Vector2(-hs1.x, -hs1.y) - pivot;
				Vector2 ob1 = new Vector2(-hs1.x, hs1.y) - pivot;
				Vector2 oc1 = new Vector2(hs1.x, hs1.y) - pivot;
				Vector2 od1 = new Vector2(hs1.x, -hs1.y) - pivot;
				float sin, cos;
				GetRotationSinCos(out sin, out cos);
				Vector2 dir = new Vector2(cos, sin);
				float ta0 = Vector2.Dot(dir, oa0);
				float tb0 = Vector2.Dot(dir, ob0);
				float tc0 = Vector2.Dot(dir, oc0);
				float td0 = Vector2.Dot(dir, od0);
				float ta1 = Vector2.Dot(dir, oa1);
				float tb1 = Vector2.Dot(dir, ob1);
				float tc1 = Vector2.Dot(dir, oc1);
				float td1 = Vector2.Dot(dir, od1);
				float min = Mathf.Min(Mathf.Min(ta0, tb0), Mathf.Min(tc0, td0)) - lightWidth.x;
				float max = Mathf.Max(Mathf.Max(ta1, tb1), Mathf.Max(tc1, td1)) + lightWidth.y;
				mFrom = center + pivot + dir * min;
				mTo = center + pivot + dir * max;
			}

			public void ResetPivot(Vector2 pivot) {
				m_Pivot = pivot;
			}

			public void Start() {
				if (mPlaying) { return; }
				mTimer = -m_Delay;
				mPlaying = true;
			}

			public void Stop() {
				mPlaying = false;
			}

			public bool Playing { get { return mPlaying; } }

			private bool mPlaying = false;
			private float mTimer;
			public bool Tick(float deltaTime, ref Vector4 p0, ref Vector4 p1, ref Vector4 p2, ref Vector4 p3, ref Vector4 p4) {
				if (!mPlaying) { return false; }
				mTimer += deltaTime;
				if (mTimer <= 0f) { return false; }
				float dur = m_MoveTweenEase[m_MoveTweenEase.length - 1].time;
				float t = mTimer / dur;
				if (t >= 1f) {
					if (m_Loop) {
						mTimer -= dur + m_Interval;
					} else {
						mPlaying = false;
					}
					return false;
				}
				Vector2 p = Vector2.LerpUnclamped(mFrom, mTo, m_MoveTweenEase.Evaluate(Mathf.Min(mTimer, dur)));
				float envelopesize, ratio;
				switch (m_ShineType) {
					case eShineType.Linear:
						p0 = new Vector4(p.x, p.y, mCos, mSin);
						p3 = new Vector4(m_LightWidth.GetValue(t) * 0.5f, UVy, 1f, 1f);
						break;
					case eShineType.Ring:
						envelopesize = m_EnvelopeSize.GetValue(t) * 0.5f;
						ratio = m_EnvelopeAspectRatio.GetValue(t);
						p0 = new Vector4(p.x, p.y, envelopesize, envelopesize / ratio);
						p3 = new Vector4(m_LightWidth.GetValue(t) * 0.5f, UVy, 1f, 1f);
						break;
					case eShineType.Texture:
						if (m_Texture == null) { return false; }
						float rotation = m_Rotation.GetValue(t) * Mathf.Deg2Rad;
						envelopesize = m_EnvelopeSize.GetValue(t) * 0.5f;
						ratio = m_EnvelopeAspectRatio.GetValue(t);
						p0 = new Vector4(p.x, p.y, mCos, mSin);
						p3 = new Vector4(envelopesize, envelopesize / ratio, Mathf.Cos(rotation), Mathf.Sin(rotation));
						p4 = new Vector4(UV.x, UV.y, UV.width, UV.height);
						break;
				}
				Color color = m_ColorOverLifetime.Evaluate(t);
				p1 = new Vector4(color.r, color.g, color.b, color.a);
				p2 = new Vector4(m_LuminanceFactor, 0f, 0f, 0f);
				return true;
			}

		}

		private static bool s_shader_prop_inited = false;
		private static int s_id_src_mode;
		private static int s_id_src_blend;
		private static int s_id_dst_blend;
		private static int s_id_linear_count;
		private static int s_id_ring_count;
		private static int s_id_texture_count;
		private static int s_id_linear_datas;
		private static int s_id_ring_datas;
		private static int s_id_texture_datas;
		private static int s_id_rect;
		private static int s_id_color_key_count;
		private static int s_id_color_keys;
		private static int s_id_alpha_key_count;
		private static int s_id_alpha_keys;
		private static int s_id_gradient;
		private static int s_id_stencil_comp;
		private static int s_id_stencil;
		private static int s_id_stencil_op;
		private static int s_id_stencil_write_mask;
		private static int s_id_stencil_read_mask;
		private static int s_id_atlas;

	}

}