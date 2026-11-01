Shader "Gameplay/BoardMaskedBlock"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0
        _BoardMask ("Board Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        sampler2D _MainTex;
        sampler2D _BoardMask;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        float4x4 _BoardWorldToLocal;
        float4 _BoardSize;
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 position = mul(_BoardWorldToLocal, float4(IN.worldPos, 1)).xy;
            float2 uv = position / _BoardSize.xy + 0.5;
            clip(uv);
            clip(1 - uv);
            clip(tex2D(_BoardMask, uv).a - 0.5);
            fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = color.a;
        }
        ENDCG
    }
    Fallback Off
}
