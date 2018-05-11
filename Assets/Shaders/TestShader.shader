Shader "Test/Instanced/Simple"
{
	Properties
	{
		youngColor ("Young Color", Color) = (1, 1, 1, 1)
		oldColor ("Old Color", Color) = (0, 0, 0, 1)
		oldAge ("Old Age", float) = 5
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
				float4 vertex : POSITION;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			fixed4 youngColor;
			fixed4 oldColor;
			float oldAge;

			StructuredBuffer<float3x4> matrixBuffer;
			StructuredBuffer<float> ageBuffer;

			v2f vert(appdata v, uint instanceID : SV_InstanceID)
			{
				float4 modelPos = float4(mul(matrixBuffer[instanceID], v.vertex), 1);
				float age = ageBuffer[instanceID];

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, modelPos);
				o.color = lerp(youngColor, oldColor, saturate(age / oldAge));
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