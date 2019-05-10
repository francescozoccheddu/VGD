#ifndef _INC_GENVERT
#define _INC_GENVERT

#include "Material.cginc"

struct Input
{
	fixed3 albedo;
	fixed material;
	fixed3 emission;
};

inline fixed3 _input_calcAlbedo (in fixed3 _color, in fixed3 _paintColor, in fixed _tint)
{
	return lerp (_color, _paintColor, _tint);
}

void vert (inout appdata_full _in, out Input _out)
{
	UNITY_INITIALIZE_OUTPUT (Input, _out);
	_out.albedo = _input_calcAlbedo (_in.color.rgb, _PaintColor, _in.texcoord.x);
	_out.material = _in.color.a;
	_out.emission = calcEmission (_out.albedo, _in.texcoord.y);
}

#endif