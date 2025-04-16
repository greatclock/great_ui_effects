Shader "Hidden/GreatUI/EffectShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        _SrcBlend ("Src Blend", Int) = 1
        _DstBlend ("Dst Blend", Int) = 10

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float4 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.texcoord.zw = v.texcoord2;

                OUT.color = v.color * _Color;
                return OUT;
            }

            int _LinearCount;
            float4 _LinearDatas[48];
            int _RingCount;
            float4 _RingDatas[48];
            int _TextureCount;
            float4 _TextureDatas[64];
            sampler2D _Gradient;
            sampler2D _Atlas;
            float _SrcMode;

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord.xy) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                int i;
                float luminance = Luminance(color.rgb);
                half4 shine = half4(0, 0, 0, 0);
                for (i = 0; i < _LinearCount; i++) {
                    int index = i * 4;
                    float4 p0 = _LinearDatas[index];
                    float4 p1 = _LinearDatas[index + 1];
                    float4 p2 = _LinearDatas[index + 2];
                    float4 p3 = _LinearDatas[index + 3];
                    float dis = 0.5 + 0.5 * dot(p0.xy - IN.texcoord.zw, p0.zw) / p3.x;
                    float flag = step(dis, 1) * step(0, dis);
                    half4 s = tex2D(_Gradient, float2(dis, p3.y)) * p1 * flag;
                    float lf = p2.x;
                    s.a *= lerp(1, luminance * max(1, lf), saturate(lf));
                    shine.rgb += s.rgb * s.a;
                    shine.a = 1 - (1 - shine.a) * (1 - saturate(s.a));
                }
                for (i = 0; i < _RingCount; i++) {
                    int index = i * 4;
                    float4 p0 = _RingDatas[index];
                    float4 p1 = _RingDatas[index + 1];
                    float4 p2 = _RingDatas[index + 2];
                    float4 p3 = _RingDatas[index + 3];
                    float2 dir = IN.texcoord.zw - p0.xy;
                    float2 dn = normalize(dir);
                    dn.x += 1 - abs(sign(dn.x * dn.y));
                    float aa = p0.z * p0.z;
                    float bb = p0.w * p0.w;
                    float t = sqrt(aa * bb / (bb * dn.x * dn.x + aa * dn.y * dn.y));
                    float dis = (length(dir) - (t - p3.x)) / (p3.x + p3.x);
                    float flag = step(dis, 1) * step(0, dis);
                    half4 s = tex2D(_Gradient, float2(dis, p3.y)) * p1 * flag;
                    float lf = p2.x;
                    s.a *= lerp(1, luminance * max(1, lf), saturate(lf));
                    shine.rgb += s.rgb * s.a;
                    shine.a = 1 - (1 - shine.a) * (1 - saturate(s.a));
                }
                for (i = 0; i < _TextureCount; i++) {
                    int index = i * 5;
                    float4 p0 = _TextureDatas[index];
                    float4 p1 = _TextureDatas[index + 1];
                    float4 p2 = _TextureDatas[index + 2];
                    float4 p3 = _TextureDatas[index + 3];
                    float4 p4 = _TextureDatas[index + 4];
                    float2 d = IN.texcoord.zw - p0.xy;
                    float2 forward = float2(p0.z * p3.z - p0.w * p3.w, p0.w * p3.z + p0.z * p3.w);
                    float2 fn = float2(-forward.y, forward.x);
                    float2 uv = float2(dot(d, forward), dot(d, fn)) * 0.5 / p3.xy + 0.5;
                    float flag = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
                    half4 s = tex2D(_Atlas, p4.xy + uv * p4.zw) * p1 * flag;
                    float lf = p2.x;
                    s.a *= lerp(1, luminance * max(1, lf), saturate(lf));
                    shine.rgb += s.rgb * s.a;
                    shine.a = 1 - (1 - shine.a) * (1 - saturate(s.a));
                }
                float modeAdd = 1 - abs(sign(_SrcMode));
                float modeMask = 1 - abs(sign(1 - _SrcMode));
                return half4(color.rgb + shine.rgb, color.a) * modeAdd + half4(color.rgb * shine.rgb * color.a * shine.a, 0) * modeMask;
            }
        ENDCG
        }
    }
}
