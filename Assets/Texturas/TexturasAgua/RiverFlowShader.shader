Shader "Custom/RiverFlowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Textura del agua
        _WaveSpeed ("Wave Speed", Range(0.01, 1)) = 0.01 // Velocidad de las olas
        _FlowSpeed ("Flow Speed", Range(0.001, 0.5)) = 0.001 // Velocidad del flujo (más lenta)
        _WaveStrength ("Wave Strength", Range(0, 0.1)) = 0.01 // Fuerza de las olas
        _Transparency ("Transparency", Range(0,1)) = 0.8 // Transparencia del agua
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // Transparencia

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _WaveSpeed;
            float _FlowSpeed;
            float _WaveStrength;
            float _Transparency;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float timeFactor = _Time.y * _FlowSpeed; // Movimiento en el eje Z (ahora más lento)
                float waveEffect = sin(i.uv.y * 10 + _Time.y * _WaveSpeed) * _WaveStrength; // Olas en el agua
                float2 flowUV = i.uv + float2(waveEffect, timeFactor); // Flujo en el eje Z

                fixed4 col = tex2D(_MainTex, flowUV);
                col.a *= _Transparency; // Aplicar transparencia
                return col;
            }
            ENDCG
        }
    }
}
