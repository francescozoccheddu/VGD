
Shader "Wheeled/Particles/Opaque"
{

	Properties
	{
		_Material ("Material", Range (0.0, 1.0)) = 0.5
	}

	SubShader
	{

		Tags
		{
			"RenderType" = "Opaque"
		}

		CGPROGRAM

			#pragma surface surf Standard vertex:vert

			fixed _Material;
			static const fixed _Alpha = 1.0;

			#include "ParticlesSurface.cginc"

		ENDCG

	}

	Fallback "VertexLit"

}
