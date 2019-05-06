
Shader "Wheeled/Particles/Opaque"
{

	Properties
	{
		_Material ("Material", Range (0.0, 1.0)) = 0.5
		_Emission ("Emission", Range (0.0, 1.0)) = 0.5
	}

	SubShader
	{

		Tags
		{
			"RenderType" = "Opaque"
		}

		CGPROGRAM

			#pragma surface surf Standard

			fixed _Material;
			fixed _Emission;
			static const fixed _Alpha = 1.0;

			#pragma surface surf Standard
			#include "ParticlesSurface.cginc"

		ENDCG

	}

	Fallback "VertexLit"

}
