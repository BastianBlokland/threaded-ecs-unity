Shader "Custom/Instanced/Impact"
{
	Properties
	{
		colorMap ("colorMap", 2D) = "white" {}
		[HDR] multiplyColor ("multiplyColor", Color) = (0, 0, 1, 1)
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
				float3 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D colorMap;
			float4 multiplyColor;
			float clipThreshold;
			
			StructuredBuffer<float3x4> matrixBuffer;
			StructuredBuffer<float> ageBuffer;

			v2f vert(appdata v, uint instanceID : SV_InstanceID)
			{
				fixed clampedAge = saturate(ageBuffer[instanceID]);
				
				float3x4 transMatrix = matrixBuffer[instanceID];
				float4 worldPos = float4(mul(transMatrix, float4(v.vertex, 1)), 1);

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, worldPos);
				o.uv = v.uv;
				o.color = multiplyColor * (1 - clampedAge);
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