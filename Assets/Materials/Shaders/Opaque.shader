
Shader "Wheeled/Opaque" 
{

	Properties 
	{
		[Header(Paint mode)]
		_PaintColor ( "Albedo", Color ) = (1.0, 1.0, 1.0, 1.0)
		_PaintMaterial ( "Material", Range (0.0, 1.0) ) = 0.5
		[Header(Emissive mode)]
		_EmissiveMaterial ( "Material", Range (0.0, 1.0) ) = 0.5
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

			#include "PBL.cginc"

			void main (in Input _in, inout SurfaceOutputStandard _out)
			{
				pbl (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, 1.0, _out);
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

			#include "Fallback.cginc"

			void main (in Input _in, inout SurfaceOutput _out)
			{
				fallback (_in, _PaintColor, _PaintMaterial, _EmissiveMaterial, 1.0, _out);
			}

		ENDCG

	}

}
