Shader "Custom/BlinkingWaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Range(0.1, 1.0)) = 0.2
        _WaveStrength ("Wave Strength", Range(0.01, 0.1)) = 0.03
        _WaveFrequency ("Wave Frequency", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            float4 _MainTex_ST;

            float _WaveSpeed;
            float _WaveStrength;
            float _WaveFrequency;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            float wave(float2 uv, float time)
            {
                // Calcula un ruido pseudoaleatorio basado en las coordenadas UV
                float2 randomOffset = float2(rand(uv * 10.0), rand(uv * 20.0));
                float wave = sin((uv.x + randomOffset.x + time) * _WaveFrequency) +
                             cos((uv.y + randomOffset.y + time) * _WaveFrequency);
                return wave * 0.5 + 0.5; // Normalizar el rango entre 0 y 1
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _WaveSpeed;
                float waveEffect = wave(i.uv, time);
                float2 distortedUV = i.uv + waveEffect * _WaveStrength * float2(sin(time), cos(time));
                return tex2D(_MainTex, distortedUV);
            }
            ENDCG
        }
    }
}