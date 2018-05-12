Shader "Test/Instanced/SpaceshipShader"
{
	Properties
	{
		colorMap ("colorMap", 2D) = "white" {}
		exhaustMap ("exhaustMap", 2D) = "white" {}
		exhaustScale ("exhaustScale", float) = 1
		[HDR] exhaustColor ("exhaustColor", Color) = (0, 0, 1, 1)
	}
	SubShader
	{
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
				float3 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 exhaustUV : TEXCOORD1;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 addColor : COLOR;
			};

			sampler2D colorMap;
			sampler2D exhaustMap;
			float3 exhaustColor;
			float exhaustScale;
			
			StructuredBuffer<float3x4> matrixBuffer;
			StructuredBuffer<float> ageBuffer;

			v2f vert(appdata v, uint instanceID : SV_InstanceID)
			{
				float age = ageBuffer[instanceID];
				float exhaustMult = saturate(age);
				float3 exhaustSample = tex2Dlod(exhaustMap, float4(_Time.y + age, v.exhaustUV.y, 0, 0));
				float heightMap = exhaustSample.r;
				float colorMultiplier = exhaustSample.g;

				//Calculate position (heightmap pushes vertices in the negative z axis for getting a exhaust effect)
				float3 modelPos = v.vertex + float3(0, 0, -heightMap * exhaustScale * exhaustMult);
				float4 worldPos = float4(mul(matrixBuffer[instanceID], float4(modelPos, 1)), 1);

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, worldPos);
				o.uv = v.uv;
				o.addColor = float4(exhaustColor * colorMultiplier * exhaustMult, 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(colorMap, i.uv) + i.addColor;
			}
			ENDCG
		}
	}
}