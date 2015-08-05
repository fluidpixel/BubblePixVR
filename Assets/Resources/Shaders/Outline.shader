Shader "Custom/Outline" {
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_OutLineSpread("Outline Spread", Range(0, 0.02)) = 0.001
		_Color("Outline Color", Color) = (1,1,1,1)
	}

	SubShader
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			ZWrite On
			Blend One OneMinusSrcAlpha
			Cull Off
			LOD 110

			Pass
			{
				CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color	: COLOR;
					half2 texcoord  : TEXCOORD0;
				};

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color;

					return OUT;
				}

				float4 _Color;
				float _OutLineSpread;
				sampler2D _MainTex;

				float4 frag(v2f IN) : SV_Target
				{
					float4 tex = tex2D(_MainTex, IN.texcoord);
					float alpha = 4 * tex.a;
					alpha -= tex2D(_MainTex, IN.texcoord + float2(_OutLineSpread, 0.0)).a;
					alpha -= tex2D(_MainTex, IN.texcoord + float2(0.0, _OutLineSpread)).a;
					alpha -= tex2D(_MainTex, IN.texcoord + float2(-_OutLineSpread, 0.0)).a;
					alpha -= tex2D(_MainTex, IN.texcoord + float2(0.0, -_OutLineSpread)).a;
					float4 res;

					if (alpha > 0.5) {
						res = _Color;
						res.a = alpha;
					}
					else {
						res = (0,0,0,0);
					}
					return res;
				}
					ENDCG
			}
		}

}