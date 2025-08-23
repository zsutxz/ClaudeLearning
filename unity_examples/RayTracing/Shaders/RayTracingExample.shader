Shader "RayTracing/RayTracingExample"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/RayTracing/RayTracingHelpers.hlsl"
        
        // Ray payload structure
        struct RayPayload
        {
            float3 color : SV_Target0;
        };
        
        // Constant buffer for shader parameters
        ConstantBuffer<RaytracingShaderConstants> _RaytracingShaderConstants;
        
        // Ray generation shader - entry point for rays
        [shader("raygeneration")]
        void RayGeneration()
        {
            // Get ray from camera
            uint2 dispatchIdx = DispatchRaysIndex().xy;
            float2 normalizedPixel = (float2)dispatchIdx + 0.5f;
            normalizedPixel /= (float2)DispatchRaysDimensions().xy;
            
            // Convert to clip space
            float2 clipXY = normalizedPixel * 2.0f - 1.0f;
            
            // Get ray origin and direction from camera
            RayDesc ray;
            ray.Origin = _RaytracingShaderConstants._WorldSpaceCameraPos;
            ray.Direction = normalize(mul(_RaytracingShaderConstants._InvViewProjectionMatrix, float4(clipXY, 0.0f, 1.0f)).xyz);
            ray.TMin = 0.0f;
            ray.TMax = _RaytracingShaderConstants._CameraFarDistance;
            
            // Trace ray
            RayPayload payload;
            payload.color = float3(0.0f, 0.0f, 0.0f);
            TraceRay(_RaytracingShaderConstants._AccelerationStructure, RAY_FLAG_NONE, 0xFF, 0, 1, 0, ray, payload);
            
            // Write result to render target
            uint2 pixelCoord = dispatchIdx;
            RenderTarget[pixelCoord] = float4(payload.color, 1.0f);
        }
        
        // Closest hit shader - called when ray hits geometry
        [shader("closesthit")]
        void ClosestHit(inout RayPayload payload, AttributeData attrib)
        {
            // Get hit data
            float3 hitPosition = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
            float3 hitNormal = HitWorldToObjectNormal(attrib.normalOS);
            
            // Simple lighting calculation
            float3 lightDir = normalize(_RaytracingShaderConstants._DirectionalLightDirection);
            float NdotL = saturate(dot(hitNormal, lightDir));
            
            // Simple color based on normal and lighting
            payload.color = NdotL * float3(0.8f, 0.7f, 0.6f) + 0.2f;
        }
        
        // Miss shader - called when ray doesn't hit anything
        [shader("miss")]
        void Miss(inout RayPayload payload)
        {
            // Simple sky color for miss case
            payload.color = float3(0.3f, 0.5f, 0.8f);
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "RayTracing"
            Tags { "LightMode" = "RayTracing" }
            
            HLSLPROGRAM
            #pragma raytracing surface
            ENDHLSL
        }
    }
    
    // Fallback for non-raytracing rendering
    SubShader
    {
        Pass
        {
            Name "Standard"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float3 lightDir = normalize(float3(1, 1, 1));
                float NdotL = saturate(dot(i.normal, lightDir));
                return float4(NdotL * float3(0.8f, 0.7f, 0.6f) + 0.2f, 1.0f);
            }
            ENDHLSL
        }
    }
}