Shader "Test/ExplosionShader"
{
	Properties
	{
		colorMap ("colorMap", 2D) = "white" {}
		animationMap ("animationMap", 2D) = "white" {}
		animationMapHeight ("animationMapHeight", int) = 64
		clipThreshold ("clipThreshold", float) = 0.05
		maxBoneCount ("maxBoneCount", int) = 10
		age ("age", Range(0, 1)) = 0.5
	}
	SubShader
	{
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D colorMap;
			sampler2D animationMap;
			int animationMapHeight;
			float clipThreshold;
			int maxBoneCount;
			float age;

			v2f vert (appdata v)
			{
				fixed boneIndex = v.uv2.x * maxBoneCount;
				fixed colorUV = boneIndex / animationMapHeight;
				fixed4 colorSample = tex2Dlod(animationMap, float4(saturate(age), colorUV, 0, 0));

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = colorSample;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 color = tex2D(colorMap, i.uv) * i.color;
				clip(color.a - clipThreshold);
				return color;
			}
			ENDCG
		}
	}
}