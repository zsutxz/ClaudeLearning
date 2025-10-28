Shader "Universal Render Pipeline/URP_GrassWindShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Grass Color", Color) = (1,1,1,1)
        _WindFrequency ("Wind Frequency", Range(0, 5)) = 1.0
        _WindAmplitude ("Wind Amplitude", Range(0, 0.5)) = 0.1
        
        // URP 需要这个属性来做透明裁剪
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }

        Pass
        {
            // HLSL 代码块开始
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 关键：为 URP 开启 Instancing
            #pragma multi_compile_instancing

            // 包含 URP 的核心库文件
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                // 关键：获取实例ID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                // 关键：传递实例ID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 属性的 CBUFFER
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _WindFrequency;
                float _WindAmplitude;
                float _Cutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // 关键：设置实例ID
                UNITY_SETUP_INSTANCE_ID(IN);
                // 关键：传递实例ID
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                // --- 风吹效果 ---
                float isTopVertex = saturate(IN.uv.y * 10.0 - 1.0);
                float windPhase = _Time.y * _WindFrequency + UNITY_GET_INSTANCE_ID(IN) * 0.3;
                float wind = sin(windPhase) * _WindAmplitude * isTopVertex;
                positionWS.x += wind;
                // --- 结束 ---

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 关键：传递实例ID
                UNITY_SETUP_INSTANCE_ID(IN);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                
                // 透明裁剪
                clip(col.a - _Cutoff);

                return col;
            }
            HLSLEND
        }
    }
}
