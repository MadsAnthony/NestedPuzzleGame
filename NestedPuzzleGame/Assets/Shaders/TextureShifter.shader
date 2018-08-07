// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TextureShifter"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTex2 ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Shifter ("Shifter", Range (0, 1)) = 0
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		//ZWrite On Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		//LOD 100

		Pass {
		    CGPROGRAM

		    #pragma vertex vert             
		    #pragma fragment frag
		    #include "UnityCG.cginc"

		    sampler2D _MainTex;
		    sampler2D _MainTex2;
		    fixed4 _Color;
		    float4 _MainTex_TexelSize;
		    float _Shifter;

		    struct vertInput {
		        float4 pos : POSITION;
		        float2 uv : TEXCOORD0;
		    };

		    struct vertOutput {
		        float4 pos : SV_POSITION;
		        float2 uv : TEXCOORD0;
		    };

		    float4 _MainTex_ST;

		    vertOutput vert(vertInput input) {
		        vertOutput o;
		        o.pos = UnityObjectToClipPos(input.pos);
		        o.uv = TRANSFORM_TEX(input.uv,_MainTex);

		        return o;
		    }

		    half4 frag(vertOutput output) : COLOR {
		    	float4 col1 = tex2D(_MainTex, output.uv);
		    	float4 col2 = tex2D(_MainTex2, output.uv);

		    	return lerp(col1,col2,_Shifter);
		    }
		    ENDCG
		}
	}
}
