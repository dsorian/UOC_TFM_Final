Shader "Custom/BlinkingWaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}        // Textura principal
        _WaveSpeed ("Wave Speed", Float) = 1.0      // Velocidad de las olas
        _WaveStrength ("Wave Strength", Float) = 0.1 // Amplitud de las olas
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;             // Textura principal
            float _WaveSpeed;              // Velocidad de las olas
            float _WaveStrength;           // Amplitud de las olas

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

            // Función Vertex Shader
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Ondas en la textura
                float wave = sin((v.uv.y + _Time.y * _WaveSpeed) * 10.0) * _WaveStrength;
                o.uv = v.uv + float2(wave, 0.0);

                return o;
            }

            // Función Fragment Shader
            fixed4 frag (v2f i) : SV_Target
            {
                // Textura base
                fixed4 col = tex2D(_MainTex, i.uv);

                // Devolvemos el color de la textura con las coordenadas modificadas
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}