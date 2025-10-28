Shader "UI/MenuTransition"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _Progress ("Progress", Range(0, 1)) = 0

        // For Sliding
        _SlideDirection ("Slide Direction", Vector) = (-1, 0, 0, 0)

        // Stencil properties for UI Masking
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="true"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            // Shader features to create variants for each effect
            #pragma shader_feature __ FADE_ON
            #pragma shader_feature __ SLIDE_ON
            #pragma shader_feature __ POPUP_ON

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            half _Progress;
            float4 _SlideDirection;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                
                #if SLIDE_ON
                // Move vertices based on progress and direction
                // We use (1.0 - _Progress) so it slides IN as progress goes 0 -> 1
                float slideOffset = (1.0 - _Progress) * 2.0; // Multiplier for effect strength
                v.vertex.xy += _SlideDirection.xy * slideOffset;
                #endif

                #if POPUP_ON
                // Scale vertices from the center based on progress
                v.vertex.xyz *= _Progress;
                #endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                #if FADE_ON
                // Modify alpha for fade effect
                col.a *= _Progress;
                #endif

                #if POPUP_ON
                // Can also fade the alpha during popup for a smoother effect
                col.a *= _Progress;
                #endif
                
                return col;
            }
            ENDCG
        }
    }
}