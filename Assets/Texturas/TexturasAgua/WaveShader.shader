Shader "Custom/BlinkingWaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Range(0.1, 1.0)) = 0.2
        _WaveStrength ("Wave Strength", Range(0.01, 0.1)) = 0.03
        _BaseFrequency ("Base Wave Frequency", Range(0.1, 5.0)) = 1.0
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
            float _BaseFrequency;

            // Genera un valor pseudoaleatorio
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Calcula un valor de frecuencia ajustado dinámicamente cada cierto tiempo
            float dynamicFrequency(float time)
            {
                // Cada intervalo de 1 a 2 segundos, el valor cambiará
                float interval = 1.0 + rand(float2(time, time * 2.0)) * 1.0; 
                float randomOffset = rand(float2(time * 0.1, interval));
                return _BaseFrequency + randomOffset * 0.5; // Ajuste aleatorio en frecuencia
            }

            // Calcula las olas para una zona específica
            float wave(float2 uv, float time, float frequency)
            {
                float2 randomOffset = float2(rand(uv * 10.0), rand(uv * 20.0));
                float wave = sin((uv.x + randomOffset.x + time) * frequency) +
                             cos((uv.y + randomOffset.y + time) * frequency);
                return wave * 0.5 + 0.5; // Normalizar a rango [0, 1]
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Tiempo actual
                float time = _Time.y * _WaveSpeed;

                // Frecuencia dinámica (que cambia cada 1-2 segundos aleatoriamente)
                float frequency = dynamicFrequency(time);

                // Calcula las ondas
                float waveEffect = wave(i.uv, time, frequency);

                // Aplica la distorsión de coordenadas UV con las olas
                float2 distortedUV = i.uv + waveEffect * _WaveStrength * float2(sin(time), cos(time));

                // Muestra la textura distorsionada
                return tex2D(_MainTex, distortedUV);
            }
            ENDCG
        }
    }
}