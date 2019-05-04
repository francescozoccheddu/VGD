#ifndef _INC_INPUT
#define _INC_INPUT

struct Input
{
	fixed4 color : COLOR0;
};

static const fixed c_emissiveAlpha = 0.05;
static const fixed c_paintAlpha = 0.95;

inline bool isEmissive (in Input _in)
{
	return false;
	return _in.color.a <= c_emissiveAlpha;
}

inline bool isPaint (in Input _in)
{
	return false;
	return _in.color.a >= c_paintAlpha;
}

inline fixed getMaterial (in Input _in)
{
	return (_in.color.a - c_emissiveAlpha) / (c_paintAlpha - c_emissiveAlpha);
}

inline fixed3 getAlbedo (in Input _in)
{
	return _in.color.rgb;
}

#endif