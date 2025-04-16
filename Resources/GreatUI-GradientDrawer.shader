Shader "Hidden/GreatUI/GradientDrawer"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Rect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.uv;
                o.uv.zw = (v.uv - _Rect.xy) / _Rect.zw;
                return o;
            }

            // sampler2D _MainTex;
            int _ColorKeyCount;
            float4 _ColorKeys[16];
            int _AlphaKeyCount;
            float4 _AlphaKeys[16];

            fixed4 frag (v2f i) : SV_Target
            {
                clip(float4(i.uv.zw, 1 - i.uv.zw));
                float x = i.uv.z;
                fixed4 color = fixed4(0, 0, 0, 0);
                float4 cfrom = _ColorKeys[0];
                for (int k = 1; k < _ColorKeyCount; k++)
                {
                    float4 key = _ColorKeys[k];
                    if (x <= key.w)
                    {
                        color.rgb = lerp(cfrom.rgb, key.rgb, (x - cfrom.w) / (key.w - cfrom.w));
                        break;
                    }
                    cfrom = key;
                }
                float4 afrom = _AlphaKeys[0];
                for (int l = 1; l < _AlphaKeyCount; l++)
                {
                    float4 key = _AlphaKeys[l];
                    if (x <= key.w)
                    {
                        color.a = lerp(afrom.r, key.r, (x - afrom.w) / (key.w - afrom.w));
                        break;
                    }
                    afrom = key;
                }
                return color;
            }
            ENDCG
        }
    }
}
