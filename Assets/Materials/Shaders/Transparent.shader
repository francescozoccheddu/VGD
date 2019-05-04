
Shader "Wheeled/Transparent" 
{

	Properties 
	{
		_PaintColor ("Paint color", Color) = (1.0, 1.0, 1.0, 1.0)
		_PaintMaterial ("Paint material", Range (0.0, 1.0)) = 0.5
		_EmissiveMaterial ("Emissive material", Range (0.0, 1.0)) = 0.5
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

			#pragma surface main Standard fullforwardshadows alpha:premul

			fixed3 _PaintColor;
			fixed _PaintMaterial;
			fixed _EmissiveMaterial;
			fixed _Alpha;

			#include "PBL.cginc"

			void main (in Input _in, inout SurfaceOutputStandard _out)
			{
				pbl (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, _Alpha, _out);
			}

		ENDCG

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

			#pragma surface main BlinnPhong alpha

			fixed3 _PaintColor;
			fixed _PaintMaterial;
			fixed _EmissiveMaterial;
			fixed _Alpha;

			#include "Fallback.cginc"

			void main (in Input _in, inout SurfaceOutput _out)
			{
				fallback (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, _Alpha, _out);
			}

		ENDCG
	
	}

}
