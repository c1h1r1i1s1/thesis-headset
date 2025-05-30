Shader "Unlit/StereoTopBottomBBox"
{
	Properties
	{
		_Color("Color", Color) = (1,0,0,1)
		_SectionHeight("Section Height (World)", float) = 0.3
		_XScale("X Scale", float) = 1
		_YScale("Y Scale", float) = 1
		_ZScale("Z Scale", float) = 1
		_FloorHeight("Floor Height", float) = 0

		_EdgeThickness("Edge Thickness", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderQueue"="Transparent+5" }

		Blend SrcAlpha OneMinusSrcAlpha
		//Cull off
		//ZTest Always
		ZWrite off

        Pass
        {
			Cull off
            
			CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			// For stereo
			#pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON

            #include "UnityCG.cginc"
			#include "UnityInstancing.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 reluv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float height : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
            };

			float4 _Color;
			float _SectionHeight;

			float _XScale;
			float _YScale;
			float _ZScale;
			float _FloorHeight;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

				//Get UVs relative to their face's scale. 
				//o.uv = v.uv; 
				float3 normal = UnityObjectToWorldNormal(v.normal);
				//float3 normal = float3(0, 0, 1);

				float horizdot = abs(dot(float3(1, 0, 0), normal));
				float horizscale = horizdot * _ZScale + (1 - horizdot) * _XScale;
				o.reluv.x = v.uv.x * horizscale;

				float vertdot = dot(float3(0, 1, 0), normal);
				float vertscale = vertdot * _ZScale + (1 - vertdot) * _YScale;
				o.reluv.y = v.uv.y * vertscale;
				//o.uv = v.uv;

				o.reluv.z = horizscale;
				o.reluv.w = vertscale;


				//Find how the section height fits into the box height. 
				float realheightfrombot = mul(unity_ObjectToWorld, v.vertex).y - _FloorHeight;
				o.height = realheightfrombot;
				return o;
            }

			float _EdgeThickness;

            fixed4 frag (v2f i) : SV_Target
            {
				if (i.height == 0) discard; //Removes the bottom. 

				//Calculate fade based on distance from top and bottoms, which we computed in the vertex shader. 
				float alphaaddbottom = clamp(_SectionHeight - i.height, 0, _SectionHeight) / _SectionHeight;
				//alphaaddbottom *= step(0, alphaaddbottom);
				float alphaaddtop = clamp(i.height - (_YScale - _SectionHeight), 0, _SectionHeight) / _SectionHeight;
				//alphaaddtop *= step(0, alphaaddtop);

				fixed4 col = fixed4(_Color.rgb, _Color.a * (alphaaddbottom + alphaaddtop));

				//Determine if we're on the edge and should draw a line accordingly. 
				int isedge = 0;
				isedge += step(i.reluv.x, _EdgeThickness);
				isedge += step(i.reluv.z - _EdgeThickness, i.reluv.x);

				isedge += step(i.reluv.y, _EdgeThickness);
				isedge += step(i.reluv.w - _EdgeThickness, i.reluv.y);

				col.a += ceil(col.a) * isedge; //We just draw the alpha as 1 if it's greater than zero. 

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return col;
            }
            ENDCG
        }

		
    }
}
