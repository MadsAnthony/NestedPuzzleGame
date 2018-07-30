// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CustomCameraShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HideCameraTwo("_HideCameraTwo", Range(0,1)) = 1
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		//Blend One One
		ZWrite On Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha
		//Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		//LOD 100

		Pass {
		    CGPROGRAM

		    #pragma vertex vert             
		    #pragma fragment frag
		    #include "UnityCG.cginc"

		    sampler2D _MainTex;
		    sampler2D  _CameraOne;
		    sampler2D  _CameraTwo;
		    float4 _MainTex_TexelSize;
		    float _HideCameraTwo;

		    struct vertInput {
		        float4 pos : POSITION;
		        float2 uv : TEXCOORD0;
		    };

		    struct vertOutput {
		        float4 pos : SV_POSITION;
		        float2 uv : TEXCOORD0;
		    };

		    vertOutput vert(vertInput input) {
		        vertOutput o;
		        o.pos = UnityObjectToClipPos(input.pos);
		        o.uv = input.uv;
		        return o;
		    }

		    half4 frag(vertOutput output) : COLOR {
		    	float4 col = tex2D(_CameraOne, output.uv);
		    	float4 col2 = tex2D(_CameraTwo, output.uv);
		    	float4 newCol = float4(0,0,0,0);
		    	float threshold = 0.1f;
		    	if (_HideCameraTwo<1 || (col2[0]<=threshold && col2[1]<=threshold && col2[2]<=threshold && col2[3]>=0.99f)) {
		    	    newCol = float4(col.x,col.y,col.z,1);
		    	} else {
		    	    newCol = (1-col2[3])*float4(col.x,col.y,col.z,1)+col2[3]*float4(col2.x,col2.y,col2.z,1);
		    	}
		    	return newCol;
		    }
		    ENDCG
		}
	}
}
