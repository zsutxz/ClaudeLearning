Shader "Unlit/GrassWindShader_Final_Fixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WindFrequency ("Wind Frequency", Range(0, 5)) = 1.0
        _WindAmplitude ("Wind Amplitude", Range(0, 0.5)) = 0.1
        _Color ("Grass Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WindFrequency;
            float _WindAmplitude;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);

                float isTopVertex = saturate(v.uv.y * 10.0 - 1.0);
                float windPhase = 0;

                // 【【【最终的、决定性的修复】】】
                // 使用预处理指令，确保只在实例化开启时才编译这段代码
                #if defined(UNITY_INSTANCING_ENABLED)
                    windPhase = _Time.y * _WindFrequency + UNITY_INSTANCE_ID * 0.3;
                #else
                    // 如果不实例化，就使用一个基础的、没有随机相位的风
                    windPhase = _Time.y * _WindFrequency;
                #endif
                // 【【【结束修复】】】
                
                float wind = sin(windPhase) * _WindAmplitude * isTopVertex;
                v.vertex.x += wind;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(_MainTex, v.uv);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - 0.5);
                return col;
            }
            ENDCG
        }
    }
}
