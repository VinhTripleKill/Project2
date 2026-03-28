Shader "Custom/SlideLineFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        slide_BaseS ("Base Color (Above HitLine)", Color) = (0,1,1,1)
        slide_FillS ("Fill Color (Below HitLine)", Color) = (1,0,1,1)
        _HitLineY ("Hit Line Y", Float) = -3.5
        _CanFill ("Can Fill", Float) = 0
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
            float4 slide_BaseS;
            float4 slide_FillS;
            float _HitLineY;
            float _CanFill;

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
    if (_CanFill < 0.5)
    {
        return slide_BaseS;
    }

    // 👉 mask hitline
    float mask = step(i.worldY, _HitLineY);

    // 👉 fill giả: dùng UV.y (từ đầu -> cuối line)
    float fill = i.uv.y;

    // 👉 chỉ fill phần dưới hitline + theo hướng line
    if (mask > 0.5)
    {
        return lerp(slide_BaseS, slide_FillS, fill);
    }
    else
    {
        return slide_BaseS;
    }
}

            ENDCG
        }
    }
}