
Shader "Wheeled/General/Opaque"
{

	Properties
	{
		_PaintColor ("Paint color", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader
	{

		Tags
		{
			"RenderType" = "Opaque"
		}

		CGPROGRAM

			#pragma surface surf Standard vertex:vert fullforwardshadows

			fixed3 _PaintColor;
			static const fixed _Alpha = 1.0;

			#include "GeneralSurface.cginc"

		ENDCG

	}

	Fallback "VertexLit"

}
