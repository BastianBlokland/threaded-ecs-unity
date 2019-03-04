Shader "Custom/Instanced/TurretShader"
{
    Properties
    {
        colorMap ("colorMap", 2D) = "white" {}
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
                float4 color : COLOR;
            };

            struct v2f
            {
                fixed4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D colorMap;

            StructuredBuffer<float3x4> matrixBuffer;

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                float3x4 transMatrix = matrixBuffer[instanceID];
                float3 transPos = float3(transMatrix[0][3], transMatrix[1][3], transMatrix[2][3]); //Take the position part of the matrix

                //R-channel of the vertex color drives a lerp between a rotated and a non rotated position, this way the base of the turret
                //can ignore the rotation and always stay upright while the barrel will follow the rotation
                float4 worldPosIncRotation = float4(mul(transMatrix, float4(v.vertex, 1)), 1);
                float4 worldPosExclRotation = float4(v.vertex + transPos, 1);
                float4 worldPos = lerp(worldPosExclRotation, worldPosIncRotation, v.color.r);

                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, worldPos);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(colorMap, i.uv);
            }
            ENDCG
        }
    }
}
