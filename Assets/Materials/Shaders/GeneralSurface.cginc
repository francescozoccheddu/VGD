#ifndef _INC_GENSURF
#define _INC_GENSURF

#include "GeneralVertex.cginc"

inline void _gensurf_calcMaterial (in fixed _in, out fixed _smoothness, out fixed _metallic)
{
	_smoothness = lerp (0.0, 0.8, _in);
	_metallic = lerp (0.0, 0.85, _in);
}

inline void surf (in Input _in, inout SurfaceOutputStandard _out)
{
	_gensurf_calcMaterial (_in.material, _out.Smoothness, _out.Metallic);
	_out.Albedo = _in.albedo;
	_out.Alpha = _Alpha;
	_out.Emission = _in.emission;
	_out.Occlusion = 1.0;
}

#endif