Shader "Sprites/ReplaceColor"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;

             struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            sampler2D _MainTex;

            #define EPSILON 1e-6

            fixed4 blend(fixed4 src, fixed4 dst) {
                fixed srcA = src.a;
                fixed resA = srcA + dst.a * (1.0 - srcA);
                fixed3 resRGB = (src.rgb * srcA + dst.rgb * dst.a * (1.0 - srcA)) / max(resA, EPSILON);
                return fixed4(resRGB, resA);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = blend(_Color, col).rgb;
                return col;
            }
            ENDCG
        }
    }
}
