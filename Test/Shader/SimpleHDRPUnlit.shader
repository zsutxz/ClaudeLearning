Shader "Custom/SimpleHDRPUnlit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRP" "RenderType"="Opaque" }

        HLSLINCLUDE
            #pragma target 4.5
            #pragma multi_compile _ DEBUG_DISPLAY

            #include "Packages/com.unity.render-pipelines.high-definition/ShaderLibrary/HDRPUnlit.hlsl"

            // 片元着色器输入结构
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 顶点着色器输出到片元着色器的结构
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Shader 属性
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            /**
             * @brief 顶点着色器
             * @param input 顶点输入属性
             * @return 顶点输出属性
             */
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv; // 简单传递UV
                return output;
            }

            /**
             * @brief 片元着色器
             * @param input 片元输入属性 (由顶点着色器输出)
             * @return 片元颜色
             */
            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 直接使用基准颜色
                return _BaseColor;
            }

        ENDHLSL

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="Unlit" } // HDRP中Unlit Pass的LightMode

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
    FallBack "Hidden/ShaderGraph/Unlit" // 当Shader不兼容时，使用Fallback
} 