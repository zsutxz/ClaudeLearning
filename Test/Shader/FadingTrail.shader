Shader "Custom/FadingTrail"
{
    Properties
    {
        _TintColor ("混合颜色 (Tint Color)", Color) = (1,1,1,0.5)
    }
    SubShader
    {
        // 设置标签为透明，这样 Unity 的渲染管线才知道如何处理它
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            // 混合模式：标准的 Alpha 混合
            Blend SrcAlpha OneMinusSrcAlpha
            // 关闭深度写入，避免透明物体遮挡问题
            ZWrite Off
            // 关闭剔除，渲染拖尾的两个面，这样从任何角度看都不会消失
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            fixed4 _TintColor;

            // 顶点着色器：将顶点从模型空间转换到裁剪空间
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            // 片元着色器：计算最终像素的颜色
            fixed4 frag (v2f i) : SV_Target
            {
                // UV 的 U 坐标存储了顶点的“年龄”（0=新, 1=旧）。
                // 我们用它来计算 Alpha 透明度。
                float alpha = 1.0 - i.uv.x;

                fixed4 finalColor = _TintColor;
                finalColor.a *= alpha; // 基于“年龄”进行渐隐

                return finalColor;
            }
            ENDCG
        }
    }
}
