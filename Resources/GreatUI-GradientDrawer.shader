Shader "Hidden/GreatUI/GradientDrawer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Rect;

            v2f vert (appdata v)
            {
                v2f o;
                float4 vertex = UnityObjectToClipPos(v.vertex);
                float4 rect = _Rect;
#if UNITY_UV_STARTS_AT_TOP
                rect.y = 1 - rect.y - rect.w;
#endif
                float2 xy = rect.xy + rect.zw * (vertex.xy / vertex.w + 1) * 0.5;
                o.vertex = float4((xy * 2 - 1) * vertex.w, vertex.z, vertex.w);
                o.uv = v.uv;

                return o;
            }

            sampler2D _MainTex;
            int _ColorKeyCount;
            float4 _ColorKeys[16];
            int _AlphaKeyCount;
            float4 _AlphaKeys[16];

            fixed4 frag (v2f i) : SV_Target
            {
                float x = i.uv.x;
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
                return color * tex2D(_MainTex, float2(x, x));
            }
            ENDCG
        }
    }
}
