Shader "EasyRoads3D/edit-mode-surfaces" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}

	[Header(Terrain Z Fighting Offset)]
	_OffsetFactor ("Offset Factor", Range(0.0,-10.0)) = -1
    _OffsetUnit ("Offset Unit", Range(0.0,-10.0)) = -1
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200
	Offset [_OffsetFactor],[_OffsetUnit]

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "VertexLit"
}
