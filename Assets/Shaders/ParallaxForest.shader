Shader "Custom/ParallaxForest"
{
    Properties
    {
        _Layer1 ("Background", 2D) = "white" {}
        _Layer2 ("Midground", 2D) = "white" {}
        _Layer3 ("Foreground", 2D) = "white" {}

        _TileScale ("Tile Scale", Float) = 1
        _ParallaxStrength ("Parallax Strength", Float) = 0.1
        _YOffset ("Y Offset", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _Layer1;
            sampler2D _Layer2;
            sampler2D _Layer3;

            float4 _Layer1_ST;
            float4 _Layer2_ST;
            float4 _Layer3_ST;

            float _TileScale;
            float _ParallaxStrength;
            float _YOffset;

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                // Camera X/Z for horizontal parallax
                float3 camPos = _WorldSpaceCameraPos;
                float2 viewDirXZ = normalize(i.worldPos.xz - camPos.xz);

                // Base UV from world position and tiling
                float2 baseUV = i.worldPos.xz * _TileScale;

                // --- Horizontal parallax offsets ---
                float2 offsetBG = float2(viewDirXZ.x, 0) * _ParallaxStrength * 0.2;
                float2 offsetMG = float2(viewDirXZ.x, 0) * _ParallaxStrength * 0.5;
                float2 offsetFG = float2(viewDirXZ.x, 0) * _ParallaxStrength * 1.0;

                // --- Apply texture tiling (_ST) and Y offset ---
                float2 uvBG = baseUV * _Layer1_ST.xy + _Layer1_ST.zw + float2(-offsetBG.x, -_YOffset * _Layer1_ST.y);
                float2 uvMG = baseUV * _Layer2_ST.xy + _Layer2_ST.zw + float2(-offsetMG.x, -_YOffset * _Layer2_ST.y);
                float2 uvFG = baseUV * _Layer3_ST.xy + _Layer3_ST.zw + float2(-offsetFG.x, -_YOffset * _Layer3_ST.y);

                // --- Sample textures ---
                float4 bg = tex2D(_Layer1, uvBG);
                float4 mg = tex2D(_Layer2, uvMG);
                float4 fg = tex2D(_Layer3, uvFG);

                // --- Blend layers ---
                float4 color = lerp(bg, mg, 0.5);
                color = lerp(color, fg, 0.5);

                return color;
            }
            ENDHLSL
        }
    }
}
