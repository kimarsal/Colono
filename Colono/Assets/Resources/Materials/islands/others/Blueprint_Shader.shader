// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Blueprint"
{
    Properties
    {
        _LineColor("Line Color", Color) = (1,1,1,1)
        _GridColor("Grid Color", Color) = (1,1,1,0)
        _LineWidth("Line Width", float) = 0.2
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Blend SrcAlpha OneMinusSrcAlpha
        AlphaTest Greater 0.5

        Pass
        {
            CGPROGRAM

            #pragma target 3.0
            #pragma fragmentoption ARB_precision_$$anonymous$$nt_fastest

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            float4 _LineColor;
            float4 _GridColor;
            float4 _LightColor0;
            float _LineWidth;

            struct v2f
            {
                float4 pos : POSITION;
                float4 texcoord : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 uv : TEXCOORD2;

                LIGHTING_COORDS(3,4)
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.normal = normalize(v.normal).xyz;
                o.texcoord = v.texcoord;

                TRANSFER_VERTEX_TO_FRAGMENT(o);

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                float3 L = normalize(normalize(ObjSpaceLightDir(i.pos)));
                float3 N = normalize(i.normal);

                float attenuation = LIGHT_ATTENUATION(i) * 2;
                float4 ambient = UNITY_LIGHTMODEL_AMBIENT * 4;

                float NdotL = saturate(dot(N, L));
                float4 diffuseTerm = NdotL * _LightColor0 * attenuation;

                float lx = step(_LineWidth, i.texcoord.x);
                float ly = step(_LineWidth, i.texcoord.y);
                float hx = step(i.texcoord.x, 1.0 - _LineWidth);
                float hy = step(i.texcoord.y, 1.0 - _LineWidth);

                return lerp(_LineColor, _GridColor, lx * ly * hx * hy) * (ambient + diffuseTerm);

            }
            ENDCG
        }
    }
        Fallback "Diffuse"
}