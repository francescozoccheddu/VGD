#ifndef _INC_MATERIAL
#define _INC_MATERIAL

inline fixed3 calcEmission (in fixed3 _albedo, in int _intensity)
{
	if (_intensity == 0)
	{
		return fixed3 (0.0, 0.0, 0.0);
	}
	else if (_intensity == 1)
	{
		return _albedo / 2.0;
	}
	else if (_intensity == 2)
	{
		return _albedo;
	}
	else
	{
		return _albedo + fixed3 (0.5, 0.5, 0.5);
	}
}

inline void calcMaterial (in fixed _in, out fixed _smoothness, out fixed _metallic)
{
	_smoothness = lerp (0.0, 0.8, _in);
	_metallic = lerp (0.0, 0.85, _in);
}

#endif