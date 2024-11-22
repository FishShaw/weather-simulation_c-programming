Shader "Custom/WeatherVisualization"
{
    Properties
    {
        _MainTex ("Weather Data", 2D) = "white" {}
        _ColorRamp ("Color Ramp", 2D) = "white" {}
        _Alpha ("Alpha", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

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

            sampler2D _MainTex;
            sampler2D _ColorRamp;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 获取数据值（已经在生成纹理时归一化）
                float value = tex2D(_MainTex, i.uv).r;
                
                // 使用颜色渐变图获取颜色
                fixed4 color = tex2D(_ColorRamp, float2(value, 0.5));
                color.a *= _Alpha;
                
                // 如果值接近0，则完全透明（用于降水数据）
                if (value < 0.001) 
                {
                    color.a = 0;
                }
                
                return color;
            }
            ENDCG
        }
    }
}