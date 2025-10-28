Shader "UI/LoadingBar"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 0.5)
        _Progress ("Progress", Range(0, 1)) = 0.5
        _ScanWidth ("Scan Width (for indeterminate)", Range(0, 1)) = 0.2
        _Speed ("Scan Speed (for indeterminate)", Float) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Add a shader feature to toggle between determinate and indeterminate modes
            #pragma shader_feature __ INDETERMINATE_MODE

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

            fixed4 _Color;
            fixed4 _BackgroundColor;
            float _Progress;
            float _ScanWidth;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 finalColor;

                #if INDETERMINATE_MODE
                    // --- Indeterminate (Scanning) Mode ---
                    // Use time to create a back-and-forth motion (ping-pong)
                    float scanPosition = frac(_Time.y * _Speed) * 2.0 - 1.0; // Ranges from -1 to 1
                    scanPosition = abs(scanPosition); // Now ranges from 0 to 1 to 0

                    // Define the start and end of the scanning band
                    float scanStart = scanPosition - _ScanWidth / 2.0;
                    float scanEnd = scanPosition + _ScanWidth / 2.0;

                    // Use smoothstep for anti-aliasing
                    float band = smoothstep(scanStart, scanStart + 0.02, i.uv.x) - smoothstep(scanEnd - 0.02, scanEnd, i.uv.x);
                    
                    finalColor = lerp(_BackgroundColor, _Color, band);

                #else
                    // --- Determinate (Progress Bar) Mode ---
                    // Check if the pixel's x-coordinate is less than the progress
                    float fill = smoothstep(_Progress - 0.01, _Progress, i.uv.x);
                    
                    finalColor = lerp(_Color, _BackgroundColor, fill);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
