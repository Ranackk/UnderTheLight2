Shader "Hidden/GravityPP"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Header(Mask)]
		_MaskTex("Mask", 2D) = "white" {}
		_MaskColor("Color", Color) = (1,1,1,1)
		_InverseMaskTransaprency("Transparency", Range(0,1)) = 0.1
		_MaskFlowTex("Flow Texture", 2D) = "white" {}
		_MaskFlowScale("Flow Scale", Range(0,1)) = 0.1
	}


	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;
			sampler2D _MaskFlowTex;

			float4 _MaskColor;
			float _InverseMaskTransaprency;
			float _MaskFlowScale;

			float4 frag (v2f i) : SV_Target
			{
				float4 baseColor = tex2D(_MainTex, i.uv);

				if (i.uv.y > 0.5) return baseColor;


				// Masking
				float2 maskOffset = tex2D(_MaskFlowTex, i.uv ).rg;
				float2 maskWarp = tex2D(_MaskFlowTex, i.uv + maskOffset +_Time.x).ba * _MaskFlowScale;

				float maskColor = tex2D(_MaskTex, i.uv + maskWarp).r;

				float transparency = 1 - _InverseMaskTransaprency;
				float maskFactor = saturate(maskColor * transparency);

				// Overlay Color

				float4 overlayColor = _MaskColor + (maskWarp.xyxy) * 10;
				// Apply

				float4 finalColor = baseColor + overlayColor * maskFactor;

				return finalColor;
			}
			ENDCG
		}
	}
}
