
Shader "Unlit/Liquid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LiquidColor ("Liquid Color", Color) = (0, 0.5, 1, 1)
        _Smoothness ("Smoothness", Range(0.01, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _LiquidColor;
            float _Smoothness;

            // 粒子数据
            int _ParticlesCount;
            float4 _Particles[64];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float totalInfluence = 0.0;

                for (int j = 0; j < _ParticlesCount; j++)
                {
                    // 将粒子世界坐标转换为屏幕坐标
                    float4 particleScreenPos = ComputeScreenPos(UnityObjectToClipPos(float4(_Particles[j].xyz, 1.0)));
                    float2 particleUV = particleScreenPos.xy / particleScreenPos.w;
                    
                    // 计算当前片元与粒子的距离
                    float dist = distance(screenUV, particleUV);
                    
                    // 使用反比函数计算影响力，距离越近，影响力越大
                    totalInfluence += 1.0 / (dist * dist + 0.01);
                }

                // 使用 smoothstep 创建平滑的液体边缘
                float liquid = smoothstep(1.0 - _Smoothness, 1.0, totalInfluence);

                // 应用颜色和透明度
                fixed4 col = _LiquidColor;
                col.a *= liquid;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
