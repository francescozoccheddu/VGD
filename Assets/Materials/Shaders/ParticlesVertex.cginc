#ifndef _INC_PARTVERT
#define _INC_PARTVERT

#include "Material.cginc"

struct Input
{
	fixed3 albedo;
	fixed3 emission;
};

void vert(inout appdata_full _in, out Input _out)
{
	UNITY_INITIALIZE_OUTPUT(Input, _out);
	_out.albedo = _in.color.rgb;
	_out.emission = calcEmission(_out.albedo, _in.color.a);
}
#endif