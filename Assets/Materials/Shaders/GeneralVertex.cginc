#ifndef _INC_GENVERT
#define _INC_GENVERT

inline fixed3 _genvert_calcEmission (in fixed3 _albedo, in int _intensity)
{
	if (_intensity == 0)
	{
		// None
		return fixed3 (0.0, 0.0, 0.0);
	}
	else if (_intensity == 1)
	{
		// Half
		return _albedo / 2.0;
	}
	else if (_intensity == 2)
	{
		// Full
		return _albedo;
	}
	else
	{
		// Overshoot
		return _albedo * 2.0;
	}
}

struct Input
{
	fixed3 albedo;
	fixed material;
	fixed3 emission;
};

void vert (inout appdata_full _in, out Input _out)
{
	UNITY_INITIALIZE_OUTPUT (Input, _out);
	int met = min (int (_in.color.a * (1 << 6)), (1 << 6) - 1);
	fixed m = (met & ((1 << 3) - 1)) / fixed ((1 << 3) - 1);
	int e = (met >> 3) & ((1 << 2) - 1);
	bool t = (met >> 5) & ((1 << 1) - 1);
	_out.albedo = t ? _PaintColor : _in.color.rgb;
	_out.material = m;
	_out.emission = _genvert_calcEmission (_out.albedo, e);
}

#endif