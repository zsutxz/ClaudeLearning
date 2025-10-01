Shader "UI/LoadingSpinner"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _ArcWidth ("Arc Width", Range(0.01, 0.5)) = 0.1
        _Speed ("Speed", Float) = 5.0
        // This property can be used to turn it into a progress bar
        _Progress ("Progress", Range(0, 1)) = 1.0 
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
            float _ArcWidth;
            float _Speed;
            float _Progress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Remap UVs to be centered at (0,0)
                float2 centeredUV = i.uv - 0.5;
                
                // 2. Calculate the distance from the center
                float dist = length(centeredUV) * 2.0; // Multiply by 2 to get a 0-1 range

                // 3. Calculate the angle of the pixel
                // atan2 gives an angle in radians from -PI to PI. We add PI and divide by 2*PI to map it to 0-1.
                float angle = (atan2(centeredUV.y, centeredUV.x) + 3.14159265) / (2.0 * 3.14159265);

                // 4. Create the spinning animation using time
                // frac() keeps the value looping between 0 and 1
                float time = frac(_Time.y * _Speed);

                // 5. Define the arc
                // The arc starts at 'time' and has a length of '_Progress'
                float startAngle = time;
                float endAngle = time + _Progress;

                // Check if the pixel's angle is within the arc
                // We use smoothstep to create anti-aliased edges
                float arc = 0;
                if (endAngle > 1.0) {
                    // Handle the case where the arc wraps around the 0-1 boundary
                    arc = (angle > startAngle) || (angle < frac(endAngle));
                } else {
                    arc = (angle > startAngle) && (angle < endAngle);
                }

                // 6. Create the circular band based on distance
                float ring = smoothstep(1.0 - _ArcWidth, 1.0, dist) - smoothstep(1.0, 1.01, dist);

                // 7. Combine the arc and the ring
                float finalAlpha = arc * ring;

                return fixed4(_Color.rgb, finalAlpha * _Color.a);
            }
            ENDCG
        }
    }
}
