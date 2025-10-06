Shader "Custom/GridHighlightWithMask_Corrected"
{
    Properties
    {
        _FillColor ("Fill Color", Color) = (0,1,0,0.35)
        _BorderColor("Border Color", Color) = (0,0.2,0,0.8)
        _InvalidFillColor("Invalid Fill Color", Color) = (1,0,0,0.35)
        _Origin ("Origin (world XY)", Vector) = (0,0,0,0)
        _CellSize ("Cell Size (XY)", Vector) = (1,1,0,0)
        _MaskTex ("Mask Texture (R channel white=on)", 2D) = "white" {}
        _MaskBounds ("Mask Bounds (xMin,yMin,width,height)", Vector) = (0,0,1,1)
        _BorderThickness ("Border thickness (0..0.5)", Range(0,0.5)) = 0.08
        _Fade ("Fade", Range(0,1)) = 0.0
        _IsValid ("Is Valid (0/1)", Float) = 1
        _MainTex ("Texture (optional)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _FillColor;
            float4 _BorderColor;
            float4 _InvalidFillColor;
            float4 _Origin;
            float4 _CellSize;
            sampler2D _MaskTex;
            float4 _MaskBounds;
            float _BorderThickness;
            float _Fade;
            float _IsValid;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 worldPos : TEXCOORD1; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 worldXY = i.worldPos.xy;
                float2 origin = _Origin.xy;
                float2 cellSize = _CellSize.xy;
                float2 cellFloat = (worldXY - origin) / cellSize;
                int2 cellCoord = int2(floor(cellFloat + 0.00001));

                float maskXMin = _MaskBounds.x;
                float maskYMin = _MaskBounds.y;
                float maskW = max(1.0, _MaskBounds.z);
                float maskH = max(1.0, _MaskBounds.w);

                float u = (cellCoord.x - maskXMin) / (maskW - 1);
                float v = (cellCoord.y - maskYMin) / (maskH - 1);

                if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0)
                    return fixed4(0,0,0,0);

                fixed4 maskSample = tex2D(_MaskTex, float2(u, v));
                float maskVal = maskSample.r;

                if (maskVal < 0.5)
                    return fixed4(0,0,0,0);

                float2 fracPos = frac(cellFloat);
                float2 distToEdge = min(fracPos, 1.0 - fracPos);
                float edgeMin = min(distToEdge.x, distToEdge.y);
                float borderT = smoothstep(_BorderThickness - 0.01, _BorderThickness + 0.01, edgeMin);
                float fillAlpha = 1.0 - _Fade * (1.0 - smoothstep(0.0, 0.5, min(fracPos.x, fracPos.y)));

                float4 colorFill = lerp(_InvalidFillColor, _FillColor, _IsValid);
                float4 col = colorFill;
                col.a *= fillAlpha;

                float4 borderCol = _BorderColor;
                float borderMix = 1.0 - borderT;
                float4 final = lerp(col, borderCol, borderMix);
                return final;
            }
            ENDCG
        }
    }
}
