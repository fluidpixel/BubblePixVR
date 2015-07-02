﻿Shader "Custom/PlasmaEffect" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BorderTex("Border Texture", 2D) = "white" {}
		_isActive("is Active", Int) = 0
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _BorderTex;
			uniform fixed4 _Color;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_base v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			float4 frag (v2f i) : SV_Target {
				float pi = 3.1415926535897932384626433832795f;
				float time = _Time.y;

				if (time > 12.0f) {
					time = (time / 12.0f) * 12.0f;
				}

				float2 u_k = (0.4f, 0.4f);
				float2 c = i.uv * u_k - u_k * 0.5f;
				time *= 0.1f;
				float v = sin(c.x + time);
				v += sin((c.y + time) * 0.5f);
				v += sin((c.x + c.y + time) * 0.5f);
				c += u_k * 0.5f * float2(sin(time / 3.0f), cos(time * 0.5f));
				v += sin(sqrt(c.x * c.x + c.y * c.y +1.0) + time);
				v *= 0.5f;
				
				float rgb = sin( v * 5 * pi);
				
				float4 wave = float4(rgb, rgb, rgb, 1.0f);
				
				float4 fragColor = tex2D(_MainTex, i.uv) * wave;
				
				if (fragColor.r + fragColor.g + fragColor.b < _Color.r + _Color.g + _Color.b) {
					fragColor = _Color;
				}
				else {
					fragColor *= _Color;
				}
				float4 border = tex2D (_BorderTex, i.uv);
				
				if (border.r < 0.5f) {
					fragColor.a = 0.0f;
				}
				
				//Sobel filter to provide edge highlights



				return fragColor ;
			}
			ENDCG
		}
	} 
	Fallback "Transparent/VertexLit"
}
