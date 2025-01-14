Shader "Weather/DataProcessor"
{
    Properties
    {
        _RainfallTex ("Rainfall Texture", 2D) = "white" {}
        _WindUTex ("Wind U Component", 2D) = "white" {}
        _WindVTex ("Wind V Component", 2D) = "white" {}
        
        // Scale parameters
        _RainfallScale ("Rainfall Scale", Float) = 100.0
        _WindScale ("Wind Scale", Float) = 327.67
        _WindOffset ("Wind Offset", Float) = 100.0
        
        _Opacity ("Opacity", Range(0,1)) = 0
        
        // UV mapping for terrain tiles
        _WeatherUV_BL ("Bottom Left UV", Vector) = (0,0,0,0)
        _WeatherUV_BR ("Bottom Right UV", Vector) = (1,0,0,0)
        _WeatherUV_TL ("Top Left UV", Vector) = (0,1,0,0)
        _WeatherUV_TR ("Top Right UV", Vector) = (1,1,0,0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
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
            };

            sampler2D _RainfallTex;
            sampler2D _WindUTex;
            sampler2D _WindVTex;
            float _RainfallScale;
            float _WindScale;
            float _WindOffset;
            float _Opacity;
            float4 _WeatherUV_BL;
            float4 _WeatherUV_BR;
            float4 _WeatherUV_TL;
            float4 _WeatherUV_TR;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Bilinear interpolation for weather data UV mapping
                float2 uv = v.uv;
                float2 weatherUV = lerp(
                    lerp(_WeatherUV_BL.xy, _WeatherUV_BR.xy, uv.x),
                    lerp(_WeatherUV_TL.xy, _WeatherUV_TR.xy, uv.x),
                    uv.y
                );
                o.uv = weatherUV;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample and scale rainfall data (stored in red channel)
                float rainfall = tex2D(_RainfallTex, i.uv).r;
                rainfall = rainfall / _RainfallScale;
                
                // Sample and scale wind data
                float windU = tex2D(_WindUTex, i.uv).r;
                float windV = tex2D(_WindVTex, i.uv).r;
                
                // Apply wind scaling and offset
                windU = (windU / _WindScale) - _WindOffset;
                windV = (windV / _WindScale) - _WindOffset;
                
                return float4(rainfall, windU, windV, _Opacity);
            }
            ENDCG
        }
    }
}
