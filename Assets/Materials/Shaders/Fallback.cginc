#ifndef _INC_FALLBACK
#define _INC_FALLBACK

#include "Input.cginc"

inline void calcMaterial (in fixed _in, out fixed _specular, out fixed _gloss)
{
	_specular = lerp (0.2, 0.8, _in);
	_gloss = lerp (0.2, 0.8, _in);
}

inline void fallback (in Input _in, in fixed3 _paintColor, in fixed _paintMaterial, in fixed _emissiveMaterial, in fixed _alpha, inout SurfaceOutput _out)
{
	bool emissive = isEmissive (_in);
	bool paint = isPaint (_in);
	fixed material = emissive ? _emissiveMaterial : (paint ? _paintMaterial : getMaterial (_in));
	calcMaterial (material, _out.Specular, _out.Gloss);
	_out.Albedo = paint ? _paintColor : getAlbedo (_in);
	_out.Alpha = _alpha;
	_out.Emission = _in.color.rgb * emissive;
}

#endif