Shader "TestShader"
{
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
				float4 vertex : POSITION;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
			};

			StructuredBuffer<float3x4> matrixBuffer;

			v2f vert(appdata v, uint instanceID : SV_InstanceID)
			{
				float4 modelPos = float4(mul(matrixBuffer[instanceID], v.vertex), 1);

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, modelPos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(1, 1, 1, 1);
			}
			ENDCG
		}
	}
}