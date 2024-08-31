Shader"Unlit/ASCII"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ASCIIKarmaTex ("KarmaASCII", 2D) = "white" {}
        _Contrast ("Contrast", Float) = 1.5
        _Offset ("Offset", Float) = 0.1
        _TexWidth ("ASCIITexWidth", Float) = 16
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

            Pass
                    {
                        Stencil
			            {
                            Ref 1
				            Comp Equal
				            Pass Keep
			            }
                        CGPROGRAM
                   #pragma vertex vert
                   #pragma fragment frag
                   // make fog work
                   #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 scrPos : TEXCOORD1;
                float4 clr : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _ASCIIKarmaTex;
            float4 _MainTex_ST;
            uniform float _TexWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                o.scrPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 finalCol = tex2D(_MainTex, i.uv);
                return finalCol;
            }
            ENDCG
        }

        Pass
        {
            Stencil
			{
                Ref 1
				Comp NotEqual
				Pass Keep
			}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 scrPos : TEXCOORD1;
                float4 clr : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _ASCIIKarmaTex;
            float4 _MainTex_ST;
            uniform float _TexWidth;

            float get_lum(float4 pixcol)
            {
                float lum = ((pixcol.r * 0.2126) + (pixcol.g * 0.7152) + (pixcol.b * 0.0722));
                float contrast = 1.5;
                float offset = 0.4;
                lum = (lum - 0.5 + offset) * contrast + 0.5;
                lum = clamp(lum, 0.0, 1.0);
                return lum;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.scrPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 sampTexSize = float2(_TexWidth, 1);
    
                // Downscale and calculate proper offset
                float2 screenCoords = i.uv * _ScreenParams.xy / i.scrPos.w;
                float2 offset = float2(screenCoords.x % 8, screenCoords.y % 8);
                float2 sampleCoords = float2(screenCoords.x - offset.x, screenCoords.y - offset.y);
                sampleCoords = sampleCoords.xy / _ScreenParams.xy * i.scrPos.w;
    
                // sample the texture and calculate luminance
                fixed4 col = tex2D(_MainTex, sampleCoords);
                float lum = get_lum(col);
    
                // Use luminance to choose ascii character, quantize to 16 luminance values first
                lum = lum - (lum % (1 / sampTexSize.x));
                float charNum = lum / (1 / sampTexSize.x);
                float2 asciiCoords = float2((charNum * 8 + offset.x) / (sampTexSize.x * 8), offset.y / 8 - 8);
                float4 finalCol = tex2D(_ASCIIKarmaTex, asciiCoords);
                
                finalCol.rgb *= col.rgb;
                return finalCol;
            }
            ENDCG
        }
    }
}
