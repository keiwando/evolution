Shader "Unlit/BackgroundGrid"
{
    Properties
    {
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
        }
        LOD 100
        ZTest Less
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 gridPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BackgroundColor;
            float _GridVisibility;

            static const fixed4 backgroundColor = fixed4(0.93, 0.93, 0.93, 1.0);
            static const fixed4 gridColor = fixed4(0.7, 0.7, 0.7, 1.0);
            static const float GRID_SCALE = 5.0;
            static const float GRID_LINE_PIXEL_WIDTH = 4.0;
            // static const float GRID_LINE_WORLD_WIDTH = 0.02;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.gridPos = mul(unity_ObjectToWorld, v.vertex) / GRID_SCALE;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float2 distFromGridLineVec = abs(i.gridPos - round(i.gridPos)) / 0.5;
                float distFromGridLine = min(distFromGridLineVec.x, distFromGridLineVec.y);
                float pixelWidth = ddx(i.gridPos);
                
                float lerpT = 1.0 - smoothstep(0, GRID_LINE_PIXEL_WIDTH * pixelWidth, distFromGridLine);
                lerpT *= _GridVisibility;

                fixed4 col = lerp(backgroundColor, gridColor, lerpT);
                return col;
            }
            ENDCG
        }
    }
}
