Shader "Custom/URP/BBoxOcclusionUnlit"
{
    Properties
    {
        [MainColor] _Color("Box Color", Color)      = (1,1,1,0.3921)
        _SectionHeight("Fade Section Height", Float)= 0.3
        _EdgeThickness("Edge Thickness", Float)     = 0.005

        _XScale("X Scale", Float)                   = 1
        _YScale("Y Scale", Float)                   = 1
        _ZScale("Z Scale", Float)                   = 1
        _FloorHeight("Floor Height", Float)         = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+5" }

        // premultiplied-style blend for occlusion output
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha  
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // standard URP + fog
            #pragma multi_compile_fog

            // GPU instancing
            #pragma multi_compile_instancing

            // Single-Pass Instanced stereo: off (_) / on
            #pragma multi_compile _ STEREO_INSTANCING_ON

            // Occlusion modes: none (_), hard, or soft
            #pragma multi_compile _ HARD_OCCLUSION SOFT_OCCLUSION

            // --- bring in Unity’s built-in CG includes ---------------
            // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "UnityInstancing.cginc"

            // URP core + occlusion helpers
            #include "Packages/com.meta.xr.sdk.core/Shaders/EnvironmentDepth/URP/EnvironmentOcclusionURP.hlsl"

            // -- appdata & varyings ----------------------------------------

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                // world-pos for occlusion (TEXCOORD1)
                META_DEPTH_VERTEX_OUTPUT(1)

                // for our fade/edge math
                float4 reluv      : TEXCOORD2; 
                float  height     : TEXCOORD3;

                UNITY_VERTEX_OUTPUT_STEREO
                // UNITY_VERTEX_OUTPUT_INSTANCE_ID
            };

            // -- properties -----------------------------------------------

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float  _SectionHeight;
                float  _EdgeThickness;
                float  _XScale, _YScale, _ZScale;
                float  _FloorHeight;
            CBUFFER_END

            // -- vertex ---------------------------------------------------

            Varyings vert(Attributes vIn)
            {
                Varyings o;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_SETUP_INSTANCE_ID(vIn);
                // UNITY_TRANSFER_INSTANCE_ID(vIn, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // 1) core URP transform + occlusion
                o.positionCS = TransformObjectToHClip(vIn.vertex.xyz);
                o.uv         = vIn.uv;
                META_DEPTH_INITIALIZE_VERTEX_OUTPUT(o, vIn.vertex);

                // 2) compute scaled UVs for edge lines
                float3 worldNormal = TransformObjectToWorldNormal(vIn.normal);
                float horizDot     = abs(dot(worldNormal, float3(1,0,0)));
                float vertDot      = abs(dot(worldNormal, float3(0,1,0)));

                float horizScale = horizDot * _ZScale + (1 - horizDot) * _XScale;
                float vertScale  = vertDot  * _ZScale + (1 - vertDot ) * _YScale;

                o.reluv.x = vIn.uv.x * horizScale;
                o.reluv.y = vIn.uv.y * vertScale;
                o.reluv.z = horizScale;
                o.reluv.w = vertScale;

                // 3) world-space height for fade
                float worldY = mul(unity_ObjectToWorld, vIn.vertex).y;
                o.height    = worldY - _FloorHeight;

                return o;
            }

            // -- fragment -------------------------------------------------

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // 1) base color
                half4 col = _Color;

                // 2) top/bottom fade
                float fadeB = saturate((_SectionHeight - i.height) / _SectionHeight);
                float fadeT = saturate((i.height - (_YScale - _SectionHeight)) / _SectionHeight);
                float fade = fadeB + fadeT;

                col.a *= fade;

                // 3) edges: if within thickness, force alpha = 1
                int isEdge = 0;
                isEdge += step(i.reluv.x,         _EdgeThickness);
                isEdge += step(i.reluv.z - _EdgeThickness, i.reluv.x);
                isEdge += step(i.reluv.y,         _EdgeThickness);
                isEdge += step(i.reluv.w - _EdgeThickness, i.reluv.y);

                if (isEdge > 0 && col.a > 0)
                    col.a = 1;

                // 4) occlusion: multiply+discard if fully hidden
                META_DEPTH_OCCLUDE_OUTPUT_PREMULTIPLY(i, col, 0);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Diffuse"
}
