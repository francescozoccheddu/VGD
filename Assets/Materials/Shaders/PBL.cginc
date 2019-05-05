#ifndef _INC_PBL
#define _INC_PBL

#include "Input.cginc"

fixed _pbl_easeMaterial (fixed _x)
{
	half t = _x; half b = 0; half c = 1; half d = 1;
	return c * (t /= d) * t + b;
}

inline void _pbl_calcMaterial (in fixed _in, out fixed _smoothness, out fixed _metallic)
{
	fixed emat = _pbl_easeMaterial (_in);
	_smoothness = lerp (0.0, 0.8, emat);
	_metallic = lerp (0.0, 0.85, emat);
}

inline void surf (in Input _in, inout SurfaceOutputStandard _out)
{
	_pbl_calcMaterial (_in.material, _out.Smoothness, _out.Metallic);
	_out.Albedo = _in.albedo;
	_out.Alpha = _Alpha;
	_out.Emission = _in.emission;
	_out.Occlusion = 1.0;
}

#endif