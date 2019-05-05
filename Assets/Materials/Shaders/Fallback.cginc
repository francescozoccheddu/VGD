#ifndef _INC_FALLBACK
#define _INC_FALLBACK

#include "Input.cginc"

inline void _fallback_calcMaterial (in fixed _in, out fixed _specular, out fixed _gloss)
{
	_specular = lerp (0.03, 1.0, _in);
	_gloss = 1.0;
}

inline void surf (in Input _in, inout SurfaceOutput _out)
{
	_fallback_calcMaterial (_in.material, _out.Specular, _out.Gloss);
	_out.Albedo = _in.albedo;
	_out.Alpha = _Alpha;
	_out.Emission = _in.emission;
}

#endif