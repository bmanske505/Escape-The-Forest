Shader "Custom/3DWallParallax2D"
{
    Properties
    {
        _Layer0Tex ("Layer 0 (Front)", 2D) = "white" {}
        _Layer1Tex ("Layer 1 (Mid)", 2D) = "white" {}
        _Layer2Tex ("Layer 2 (Back)", 2D) = "white" {}

        _Layer0Speed ("Layer 0 Speed", Vector) = (1,1,0,0)
        _Layer1Speed ("Layer 1 Speed", Vector) = (0.5,0.5,0,0)
        _Layer2Speed ("Layer 2 Speed", Vector) = (0.2,0.2,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Layer0Tex;
            sampler2D _Layer1Tex;
            sampler2D _Layer2Tex;

            float4 _Layer0Speed;
            float4 _Layer1Speed;
            float4 _Layer2Speed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Get world position of vertex
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Calculate parallax offset in XZ plane only
                float2 camOffset = _WorldSpaceCameraPos.xz;

                o.uv0 = v.uv + camOffset * _Layer0Speed.xy;
                o.uv1 = v.uv + camOffset * _Layer1Speed.xy;
                o.uv2 = v.uv + camOffset * _Layer2Speed.xy;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col0 = tex2D(_Layer0Tex, i.uv0);
                fixed4 col1 = tex2D(_Layer1Tex, i.uv1);
                fixed4 col2 = tex2D(_Layer2Tex, i.uv2);

                // Simple overlay: front layer replaces transparent pixels of back layers
                fixed4 color = col2;
                color = lerp(color, col1, col1.a);
                color = lerp(color, col0, col0.a);

                return color;
            }
            ENDCG
        }
    }
}
