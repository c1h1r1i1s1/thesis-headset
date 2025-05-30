Shader "Custom/URP/BBoxOcclusionMetaDepth"
{
    Properties
    {
        _Color("Box Color", Color)             = (1,1,1,0.3921)
        _SectionHeight("Fade Section Height", Float) = 0.3
        _EdgeThickness("Edge Thickness", Float)     = 0.005
        _XScale("X Scale", Float)                   = 1
        _YScale("Y Scale", Float)                   = 1
        _ZScale("Z Scale", Float)                   = 1
        _FloorHeight("Floor Height", Float)         = 0.2
    }
    SubShader
    {
        Tags { "RequireDepthTexture"="true" "RenderType"="Opaque" "Queue"="Transparent+5" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            Name "MetaDepthOcclusion"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #pragma multi_compile_instancing
                #pragma multi_compile _ STEREO_INSTANCING_ON
                #pragma multi_compile _ HARD_OCCLUSION SOFT_OCCLUSION

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
                #include "Assets/Shaders/EnvironmentOcclusionURP_NoCore.hlsl"

                struct Attributes
                {
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    float4 vertex : POSITION;
                    float2 uv     : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float2 uv         : TEXCOORD0;
                    META_DEPTH_VERTEX_OUTPUT(1) // May need to put later with higher value of TEXCOOORD +1
                    float4 reluv      : TEXCOORD2;
                    float  height     : TEXCOORD3;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                CBUFFER_START(UnityPerMaterial)
                    half4 _Color;
                    float  _SectionHeight;
                    float  _EdgeThickness;
                    float  _XScale, _YScale, _ZScale;
                    float  _FloorHeight;
                CBUFFER_END

                Varyings vert(Attributes v)
                {
                    Varyings o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    o.positionCS = TransformObjectToHClip(v.vertex.xyz);

                    o.uv = v.uv;
                    META_DEPTH_INITIALIZE_VERTEX_OUTPUT(o, v.vertex.xyz);

                    float3 wn = TransformObjectToWorldNormal(v.normal);
                    float hd = abs(dot(wn, float3(1,0,0)));
                    float vd = abs(dot(wn, float3(0,1,0)));
                    float hs = hd * _ZScale + (1 - hd) * _XScale;
                    float vs = vd * _ZScale + (1 - vd) * _YScale;
                    o.reluv = float4(v.uv.x * hs, v.uv.y * vs, hs, vs);

                    o.height = mul(unity_ObjectToWorld, v.vertex).y - _FloorHeight;

                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    return o;
                }

                half4 frag(Varyings i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i); //Maybe not?
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                    half4 col = _Color;
                    float fadeB = saturate((_SectionHeight - i.height) / _SectionHeight);
                    float fadeT = saturate((i.height - (_YScale - _SectionHeight)) / _SectionHeight);
                    col.a *= (fadeB + fadeT);

                    float2 euv = i.reluv.xy;
                    float2 suv = i.reluv.zw;
                    if (euv.x <= _EdgeThickness || euv.x >= suv.x - _EdgeThickness ||
                        euv.y <= _EdgeThickness || euv.y >= suv.y - _EdgeThickness)
                    {
                        if (col.a > 0) col.a = 1;
                    }

                    META_DEPTH_OCCLUDE_OUTPUT_PREMULTIPLY(i, col, 0);
                    return col;
                }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
