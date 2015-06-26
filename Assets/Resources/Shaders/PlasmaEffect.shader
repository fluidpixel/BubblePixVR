Shader "Custom/PlasmaEffect" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BorderTex("Border Texture", 2D) = "white" {}
		_isActive("is Active", Int) = 0
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BorderTex;
		fixed4 _Color;


		struct Input {
			float2 uv_MainTex;
			float2 uv_BorderTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
		//This probably needs to go into a fragment shader. something to work on next week.
			float pi = 3.1415926535897932384626433832795f;
			float time = _Time.g;
			float v = 0.0f;
			float2 u_k = (2, 2);
			float2 c = IN.uv_MainTex * u_k - u_k * 0.5f;

			v += sin(c.x + time);
			v += sin((c.y + time) * 0.5f);
			v += sin((c.x + c.y + time) * 0.5f);
			c += u_k * 0.5f * float2(sin(time / 3.0f), cos(time * 0.5f ));
			v += sin(sqrt(c.x * c.x + c.y * c.y +1.0) + time);
			v = v * 0.5f;

			float rgb = sin( v * 5 * pi);

			float4 col = (rgb, rgb, rgb, 1);

			half4 a = tex2D (_MainTex, IN.uv_MainTex) * ((_Color + col) * 0.5f);
			half4 b = tex2D (_BorderTex, IN.uv_BorderTex);
			o.Albedo = a.rgb;
			if (b.r < 0.5f) {
				o.Alpha = 0.0f;
			}
			else {
				o.Alpha = a.a;
			}
		}
		ENDCG
	} 
	Fallback "Transparent/VertexLit"
}
