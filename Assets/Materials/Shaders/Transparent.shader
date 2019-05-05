
Shader "Wheeled/Transparent" 
{

	Properties 
	{
		[Header (Paint mode)]
		_PaintMaterial ("Material", Range (0.0, 1.0)) = 0.5
		_PaintColor ("Albedo", Color) = (1.0, 1.0, 1.0, 1.0)
		[Header (Emissive mode)]
		_EmissiveMaterial ("Material", Range (0.0, 1.0)) = 0.5
		_EmissiveIntensity ("Intensity", Range (0.0, 1.0)) = 0.5
		[Header(Transparency)]
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

			#pragma surface main Standard alpha:premul

			fixed3 _PaintColor;
			fixed _PaintMaterial;
			fixed _EmissiveMaterial;
			fixed _EmissiveIntensity;
			fixed _Alpha;

			#include "PBL.cginc"

			void main (in Input _in, inout SurfaceOutputStandard _out)
			{
				pbl (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, _EmissiveIntensity, _Alpha, _out);
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

			#pragma surface main BlinnPhong alpha:blend

			fixed3 _PaintColor;
			fixed _PaintMaterial;
			fixed _EmissiveMaterial;
			fixed _EmissiveIntensity;
			fixed _Alpha;

			#include "Fallback.cginc"

			void main (in Input _in, inout SurfaceOutput _out)
			{
				fallback (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, _EmissiveIntensity, _Alpha, _out);
			}

		ENDCG
	
	}

}
