
Shader "Wheeled/Opaque" 
{

	Properties 
	{
		[Header(Paint mode)]
		_PaintMaterial ( "Material", Range (0.0, 1.0) ) = 0.5
		_PaintColor ( "Albedo", Color ) = (1.0, 1.0, 1.0, 1.0)
		[Header(Emissive mode)]
		_EmissiveMaterial ( "Material", Range (0.0, 1.0) ) = 0.5
		_EmissiveIntensity ( "Intensity", Range (0.0, 1.0) ) = 0.5
	}

	SubShader 
	{

		Tags 
		{
			"RenderType" = "Opaque"
		}
		
		CGPROGRAM

			#pragma surface main Standard

			fixed3 _PaintColor;
			fixed _PaintMaterial;
			fixed _EmissiveMaterial;
			fixed _EmissiveIntensity;

			#include "PBL.cginc"

			void main (in Input _in, inout SurfaceOutputStandard _out)
			{
				pbl (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, _EmissiveIntensity, 1.0, _out);
			}

		ENDCG

	}
	
	SubShader 
	{

		Tags 
		{
			"RenderType" = "Opaque"
		}

		CGPROGRAM

			#pragma surface main BlinnPhong

			fixed3 _PaintColor;
			fixed _PaintMaterial;
			fixed _EmissiveMaterial;
			fixed _EmissiveIntensity;

			#include "Fallback.cginc"

			void main (in Input _in, inout SurfaceOutput _out)
			{
				fallback (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, _EmissiveIntensity, 1.0, _out);
			}

		ENDCG

	}

}
