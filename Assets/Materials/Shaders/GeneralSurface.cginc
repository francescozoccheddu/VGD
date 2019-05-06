#ifndef _INC_GENSURF
#define _INC_GENSURF

#include "GeneralVertex.cginc"
#include "Material.cginc"

inline void surf (in Input _in, inout SurfaceOutputStandard _out)
{
	calcMaterial (_in.material, _out.Smoothness, _out.Metallic);
	_out.Albedo = _in.albedo;
	_out.Alpha = _Alpha;
	_out.Emission = _in.emission;
	_out.Occlusion = 1.0;
}

#endif