﻿
Shader "Wheeled/Particles/Transparent"
{

	Properties
	{
		_Material ("Material", Range (0.0, 1.0)) = 0.5
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

			#pragma surface surf Standard vertex:vert alpha:premul

			fixed _Material;
			fixed _Alpha;

			#include "ParticlesSurface.cginc"

		ENDCG

	}

}