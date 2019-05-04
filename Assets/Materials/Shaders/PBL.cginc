#ifndef _INC_PBL
#define _INC_PBL

#include "Input.cginc"

inline void calcMaterial (in fixed _in, out fixed _smoothness, out fixed _metallic)
{
	_smoothness = lerp (0.2, 0.8, _in);
	_metallic = lerp (0.9, 0.1, _in);
}

inline void pbl (in Input _in, in fixed3 _paintColor, in fixed _paintMaterial, in fixed _emissiveMaterial, in fixed _alpha, inout SurfaceOutputStandard _out)
{
	bool emissive = isEmissive (_in);
	bool paint = isPaint (_in);
	fixed material = emissive ? _emissiveMaterial : (paint ? _paintMaterial : getMaterial(_in));
	calcMaterial (material, _out.Smoothness, _out.Metallic);
	_out.Albedo = paint ? _paintColor : getAlbedo(_in);
	_out.Alpha = _alpha;
	_out.Emission = _out.Albedo * emissive;
	_out.Occlusion = 1.0;
}

#endif