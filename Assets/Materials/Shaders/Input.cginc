#ifndef _INC_INPUT
#define _INC_INPUT

struct Input
{
	fixed3 albedo;
	fixed material;
	fixed3 emission;
};

inline fixed3 _input_calcEmission (in fixed3 _albedo, in fixed _intensity)
{
	fixed3 a = _intensity > 0.5 ? _albedo : fixed3 (0.0, 0.0, 0.0);
	fixed3 b = _intensity > 0.5 ? fixed3 (1.0, 1.0, 1.0) : _albedo;
	fixed s = frac (_intensity * 2.0);
	return lerp (a, b, s);
}

inline fixed3 _input_calcAlbedo (in fixed3 _color, in fixed3 _paintColor, in fixed _tint)
{
	return lerp (_color, _paintColor, _tint);
}

void vert (inout appdata_full _in, out Input _out)
{
	UNITY_INITIALIZE_OUTPUT (Input, _out);
	_out.albedo = _input_calcAlbedo (_in.color.rgb, _PaintColor, _in.texcoord.y);
	_out.material = _in.color.a;
	_out.emission = _input_calcEmission (_out.albedo, _in.texcoord.x);
}

#endif