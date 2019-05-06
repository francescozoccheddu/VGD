#ifndef _INC_MATERIAL
#define _INC_MATERIAL

inline fixed3 calcEmission (in fixed3 _albedo, in fixed _intensity)
{
	fixed3 a = _intensity > 0.5 ? _albedo : fixed3 (0.0, 0.0, 0.0);
	fixed3 b = _intensity > 0.5 ? fixed3 (1.0, 1.0, 1.0) : _albedo;
	fixed s = frac (_intensity * 2.0);
	return lerp (a, b, s);
}

fixed _material_easeMaterial (fixed _x)
{
	half t = _x; half b = 0; half c = 1; half d = 1;
	return c * (t /= d) * t + b;
}

inline void calcMaterial (in fixed _in, out fixed _smoothness, out fixed _metallic)
{
	fixed emat = _material_easeMaterial (_in);
	_smoothness = lerp (0.0, 0.8, emat);
	_metallic = lerp (0.0, 0.85, emat);
}

#endif