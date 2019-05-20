
Shader "Wheeled/Gradient Skybox"
{

	Properties
	{
		_Color1 ("Color 1", Color) = (1, 1, 1, 0)
		_Color2 ("Color 2", Color) = (1, 1, 1, 0)
		_Intensity ("Intensity", Float) = 1.0
		_Exponent ("Exponent", Float) = 1.0
	}

	SubShader
	{

		Tags 
		{ 
			"RenderType" = "Background" 
			"Queue" = "Background" 
		}

		ZWrite Off
		Cull Off
		Fog { Mode Off }

		Pass 
		{

			CGPROGRAM

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			fixed4 _Color1;
			fixed4 _Color2;
			fixed _Intensity;
			fixed _Exponent;

			v2f vert (appdata _in)
			{
				v2f o;
				o.position = UnityObjectToClipPos (_in.position);
				o.texcoord = _in.texcoord;
				return o;
			}

			fixed4 frag (v2f _in) : COLOR
			{
				half d = dot (normalize (_in.texcoord), fixed4 (0.0, 1.0, 0.0, 0.0)) * 0.5f + 0.5f;
				return lerp (_Color1, _Color2, pow (d, _Exponent))* _Intensity;
			}

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag

			ENDCG

		}
		
	}

}