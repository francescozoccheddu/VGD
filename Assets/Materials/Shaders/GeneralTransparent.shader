
Shader "Wheeled/General/Transparent"
{

	Properties
	{
		_PaintColor ("Paint color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Alpha ("Alpha", Range (0.0, 1.0)) = 0.5
	}

	SubShader
	{

		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

			#pragma surface surf Standard vertex:vert alpha:fade

			fixed3 _PaintColor;
			fixed _Alpha;

			#include "GeneralSurface.cginc"

		ENDCG

	}

}
