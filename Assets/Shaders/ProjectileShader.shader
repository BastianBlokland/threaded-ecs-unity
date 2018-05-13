Shader "Custom/Instanced/ProjectileShader"
{
	Properties
	{
		colorMultiplier ("colorMultiplier", float) = 1
	}
	SubShader
	{
		Blend One One
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
				float4 color : COLOR;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			float colorMultiplier;

			StructuredBuffer<float3x4> matrixBuffer;

			v2f vert(appdata v, uint instanceID : SV_InstanceID)
			{
				float3x4 transMatrix = matrixBuffer[instanceID];
				float4 worldPos = float4(mul(transMatrix, float4(v.vertex, 1)), 1);

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, worldPos);
				o.color = v.color * colorMultiplier;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}