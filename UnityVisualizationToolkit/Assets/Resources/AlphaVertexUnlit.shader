Shader "AlphaVertexUnlit" {
	Properties {
	    _Color ("Main Color", Color) = (1,1,1,1)
	   _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"{}
	}

	Category {

		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Cull off
		Lighting Off
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

	 
		SubShader {
		Pass {
			SetTexture [_MainTex] {
				Combine texture * primary DOUBLE
			}
		}
	}
	}
}