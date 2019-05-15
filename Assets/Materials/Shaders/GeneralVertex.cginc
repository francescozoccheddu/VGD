#ifndef _INC_GENVERT
#define _INC_GENVERT

#include "Material.cginc"

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
	fixed m = (met & ((1 << 3) - 1)) / fixed (1 << 3);
	int e = (met >> 3) & ((1 << 2) - 1);
	bool t = (met >> 5) & ((1 << 1) - 1);
	_out.albedo = t ? _PaintColor : _in.color.rgb;
	_out.material = m;
	_out.emission = calcEmission (_out.albedo, e);
}

#endif