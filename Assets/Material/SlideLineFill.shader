Shader "Custom/SlideLineFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (0,1,1,1)
        _FillColor ("Fill Color", Color) = (1,0,1,1)
        _HitLineY ("Hit Line Y", Float) = -3.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float worldY : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _LineColor;
            float4 _FillColor;
            float _HitLineY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldY = worldPos.y;

                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (i.worldY < _HitLineY)
                    return _FillColor;
                else
                    return _LineColor;
            }
            ENDCG
        }
    }
}