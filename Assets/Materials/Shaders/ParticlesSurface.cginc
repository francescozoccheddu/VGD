#ifndef _INC_PARTSURF
#define _INC_PARTSURF

#include "ParticlesVertex.cginc"
#include "Material.cginc"

inline void surf (in Input _in, inout SurfaceOutputStandard _out)
{
	calcMaterial (_Material, _out.Smoothness, _out.Metallic);
	_out.Albedo = _in.color;
	_out.Alpha = _Alpha;
	_out.Emission = calcEmission (_out.Albedo, _Emission);
	_out.Occlusion = 1.0;
}

#endif