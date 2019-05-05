
Shader "Wheeled/Opaque" 
{

	Properties 
	{
		_PaintColor ( "Paint color", Color ) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader 
	{

		Tags 
		{
			"RenderType" = "Opaque"
		}
		
		CGPROGRAM

			#pragma surface surf Standard vertex:vert

			fixed3 _PaintColor;
			static const fixed _Alpha = 1.0;

			#include "PBL.cginc"

		ENDCG

	}

	SubShader 
	{

		Tags 
		{
			"RenderType" = "Opaque"
		}

		CGPROGRAM

			#pragma surface surf BlinnPhong vertex:vert

			fixed3 _PaintColor;
			static const fixed _Alpha = 1.0;

			#include "Fallback.cginc"

		ENDCG

	}

	Fallback "VertexLit"
	
}
