Shader "Unlit/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            // Pass 0: Vertical Blur
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
                float2 uv[5] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // 纹理的像素大小 (1/width, 1/height, width, height)
            float _BlurSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 uv = v.uv;
                
                // 在顶点着色器中计算采样坐标，效率更高
                o.uv[0] = uv;
                o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0 * _BlurSize);
                o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0 * _BlurSize);
                o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0 * _BlurSize);
                o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0 * _BlurSize);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float weights[3] = { 0.4026, 0.2442, 0.0545 };
                fixed3 sum = tex2D(_MainTex, i.uv[0]).rgb * weights[0];
                
                sum += tex2D(_MainTex, i.uv[1]).rgb * weights[1];
                sum += tex2D(_MainTex, i.uv[2]).rgb * weights[1];
                sum += tex2D(_MainTex, i.uv[3]).rgb * weights[2];
                sum += tex2D(_MainTex, i.uv[4]).rgb * weights[2];
                
                return fixed4(sum, 1.0);
            }
            ENDCG
        }

        Pass
        {
            // Pass 1: Horizontal Blur
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
                float2 uv[5] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 uv = v.uv;

                o.uv[0] = uv;
                o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0 * _BlurSize, 0.0);
                o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0 * _BlurSize, 0.0);
                o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0 * _BlurSize, 0.0);
                o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0 * _BlurSize, 0.0);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float weights[3] = { 0.4026, 0.2442, 0.0545 };
                fixed3 sum = tex2D(_MainTex, i.uv[0]).rgb * weights[0];
                
                sum += tex2D(_MainTex, i.uv[1]).rgb * weights[1];
                sum += tex2D(_MainTex, i.uv[2]).rgb * weights[1];
                sum += tex2D(_MainTex, i.uv[3]).rgb * weights[2];
                sum += tex2D(_MainTex, i.uv[4]).rgb * weights[2];
                
                return fixed4(sum, 1.0);
            }
            ENDCG
        }
    }
}
