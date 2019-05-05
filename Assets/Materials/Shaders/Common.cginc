#ifndef _INC_COMMON
#define _INC_COMMON

inline fixed3 getEmission (in fixed3 _albedo, in fixed _intensity)
{
	fixed3 a = _intensity > 0.5 ? _albedo : fixed3 (0.0, 0.0, 0.0);
	fixed3 b = _intensity > 0.5 ? fixed3 (1.0, 1.0, 1.0) : _albedo;
	fixed s = frac (_intensity * 2.0);
	return lerp (a, b, s);
}

#endif