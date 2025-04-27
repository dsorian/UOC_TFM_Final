Shader "Custom/SectionedWaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveSpeed ("Global Wave Speed", Range(0.1, 1.0)) = 0.2
        _WaveStrength ("Global Wave Strength", Range(0.01, 0.1)) = 0.03
        _Rows ("Number of Rows", Range(1, 50)) = 2
        _Columns ("Number of Columns", Range(1, 50)) = 2
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
            float _Rows;
            float _Columns;

            // Genera un valor pseudoaleatorio
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Calcula las ondas para una sección específica
            float waveEffect(float2 uv, float2 sectionCenter, float time, float strength)
            {
                float distanceFromCenter = length(uv - sectionCenter);
                float wave = sin((distanceFromCenter + time) * 10.0) * strength;
                return wave;
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
                float time = _Time.y * _WaveSpeed;

                // Tamaño de cada sección
                float2 sectionSize = float2(1.0 / _Columns, 1.0 / _Rows);

                // Coordenadas de la sección actual
                float2 sectionCoords = floor(i.uv / sectionSize);

                // Centro de la sección actual
                float2 sectionCenter = (sectionCoords + 0.5) * sectionSize;

                // Generar un valor de fuerza aleatoria para la sección para que sea más uniforme
                float strength = _WaveStrength * (0.5 + rand(sectionCoords) * 0.5);
                

                // Calcular el efecto de la onda para la sección
                float wave = waveEffect(i.uv, sectionCenter, time, strength);

                // Aplicar el desplazamiento UV
                float2 distortedUV = i.uv + wave * float2(0.01, 0.01);

                // Retornar la textura con el desplazamiento aplicado
                return tex2D(_MainTex, distortedUV);
            }
            ENDCG
        }
    }
}
