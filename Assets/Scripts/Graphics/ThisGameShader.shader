Shader "Custom/ThisGameShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_NoiseTex("Noise Tex", 2D) = "white" {}
		_FogColor("Fog Color", Color) = (1, 1, 0.85, 1.0)
		_FogDistance("Fog Distance", Float) = 3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float depth : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			fixed4 _FogColor;
			float _FogDistance;;

            UNITY_INSTANCING_BUFFER_START(Props)
              UNITY_DEFINE_INSTANCED_PROP(fixed4, _MainTex_UV)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

				float4 offset; // Modify height of vertices based on their x,z position using those coordinates in the noise texture
				offset = float4(
					0,
					tex2Dlod(_NoiseTex, float4(IN.vertex.x, IN.vertex.y, 0, 0)).r,
					0,
					0);

				OUT.vertex = mul(UNITY_MATRIX_MV, IN.vertex); // + offset
				OUT.depth = OUT.vertex.z; // Get distance from camera, to apply fog
				OUT.vertex = mul(UNITY_MATRIX_P, OUT.vertex);

				// Set UV size to mesh UV * MaterialPropertyBlock x y, and set UV position to MaterialPropertyBlock z w.
				OUT.uv = (IN.uv * UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).xy) + UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).zw;

                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
				fixed4 c = tex2D(_MainTex, IN.uv); // Get color from texture at uv coords
				clip(c.a - 0.5); // Dont render transparent pixels
				// c = lerp(c, _FogColor, saturate(-_FogDistance / 100 * i.depth)); // Apply fog
				return c;
            }
            ENDCG
        }
    }
}