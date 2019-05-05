#ifndef _INC_FALLBACK
#define _INC_FALLBACK

#include "Input.cginc"

inline void calcMaterial (in fixed _in, out fixed _specular, out fixed _gloss)
{
	_specular = lerp (0.03, 1.0, _in);
	_gloss = 1.0;
}

inline void fallback (in Input _in, in fixed3 _paintColor, in fixed _paintMaterial, in fixed _emissiveMaterial, in fixed _alpha, inout SurfaceOutput _out)
{
	bool emissive = isEmissive (_in);
	bool paint = isPaint (_in);
	fixed material = emissive ? _emissiveMaterial : (paint ? _paintMaterial : getMaterial (_in));
	calcMaterial (material, _out.Specular, _out.Gloss);
	_out.Albedo = paint ? _paintColor : getAlbedo (_in);
	_out.Alpha = _alpha;
	_out.Emission = _out.Albedo.rgb * emissive;
}

#endif