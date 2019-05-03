
Shader "GeneralPurpose" {
	Properties {
		_Color ("Paint color", Color) = (1, 1, 1, 1)
	}
		SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 400

		CGPROGRAM
#pragma surface surf StandardSpecular 

		fixed3 _Color;

	struct Input
	{
		half4 color : COLOR0;
	};

	void surf (Input IN, inout SurfaceOutputStandardSpecular o)
	{
		o.Albedo = IN.color.rgb;
		o.Alpha = 1.0;
		o.Specular = fixed3 (0.5, 0.5, 0.5);
		o.Emission = fixed3 (0.0, 0.0, 0.0);
		o.Smoothness = 0.5;
		o.Occlusion = 1.0;
	}
	ENDCG
	}

		FallBack "Specular"
}
