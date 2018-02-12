// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader"Custom/ZMask"{
	Properties{
		_MainColor ("MainColor(RGB)",COLOR) = (1,1,1,1)
		_MainTex ("Base Texture",2D) = "white"{}
		_ZColor ("ZWrite Color(RGBA)",COLOR) = (1,1,1,1)
	}
	SubShader{
		Pass{
			Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest GEqual
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include"UnityCG.cginc"

			float4 _ZColor;

			struct v2f{
				float4 pos:SV_POSITION;
			};

			v2f vert(appdata_base b)
			{
				v2f v;
				v.pos = UnityObjectToClipPos(b.vertex);
				return v;
			}

			half4 frag(v2f i):COLOR
			{
				return _ZColor;
			}
			ENDCG
		}
		Pass{
		Tags{ "RenderType"="Opaque" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include"UnityCG.cginc"

			float4 _MainColor;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct v2f{
				float4 pos:SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			v2f vert(appdata_base b)
			{
				v2f v;
				v.pos = UnityObjectToClipPos(b.vertex);
				v.uv = b.texcoord.xy*_MainTex_ST.xy + _MainTex_ST.zw;
				return v;
			}

			half4 frag(v2f i):COLOR
			{
				half4 c = tex2D(_MainTex,i.uv);
				return c*_MainColor;
			}
			ENDCG
		}
	}
}