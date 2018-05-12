Shader "Custom/Instanced/Explosion"
{
	Properties
	{
		colorMap ("colorMap", 2D) = "white" {}
		animationMap ("animationMap", 2D) = "white" {}
		animationMapHeight ("animationMapHeight", int) = 64
		clipThreshold ("clipThreshold", float) = 0.05
	}
	SubShader
	{
		Cull Off
		Pass 
		{
			CGPROGRAM
			#pragma exclude_renderers gles //Because non-square matrices
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 boneIndex : TEXCOORD1;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D colorMap;
			sampler2D animationMap;
			int animationMapHeight;
			float clipThreshold;
			
			StructuredBuffer<float3x4> matrixBuffer;
			StructuredBuffer<float> ageBuffer;

			v2f vert(appdata v, uint instanceID : SV_InstanceID)
			{
				//Find out where to sample in the animationMap
				fixed clampedAge = saturate(ageBuffer[instanceID]);
				fixed boneStartOffset = v.boneIndex.x * 2; //because 2 elements per bone: color and (position, scale)

				//Sample 'color, position, scale' from the animationMap
				float4 colorSample = tex2Dlod(animationMap, float4(clampedAge, boneStartOffset / animationMapHeight, 0, 0));
				float4 positionScaleSample = tex2Dlod(animationMap, float4(clampedAge, (boneStartOffset + 1) / animationMapHeight, 0, 0));
				float3 positionSample = positionScaleSample.xyz;
				float scaleSample = positionScaleSample.a;

				//Calculate position
				float3 modelPos = (v.vertex + positionSample) * scaleSample;
				float4 worldPos = float4(mul(matrixBuffer[instanceID], float4(modelPos, 1)), 1);

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, worldPos);
				o.uv = v.uv;
				o.color = colorSample;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = tex2D(colorMap, i.uv) * i.color;
				clip(color.a - clipThreshold);
				return color;
			}
			ENDCG
		}
	}
}