Shader "Unlit/VoxelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Vector) = (1,1,1,1)
        _DeformScaleNormal("Deform Scale Along Normal", float) = 0.05
        _DeformScaleLateral("Deform Scale Along XY", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "simplex.hlsl"

            //sample the simplex noise 3 times at offset length along that colors axis
            float3 tri_color_sample(float3 position)
            {
                return normalize(float3(
                    openSimplex2SDerivatives_ImproveXY(position + float3(1,0,0)).w,
                    openSimplex2SDerivatives_ImproveXY(position + float3(0,1,0)).w,
                    openSimplex2SDerivatives_ImproveXY(position + float3(0,0,1)).w
                ));
            }



            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2g 
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
                float3 normal : NORMAL;
			};

            struct g2f
            {
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(1)
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 position_ws: TEXCOORD1;
            };

            v2g midpoint(v2g p1, v2g p2) 
			{
                v2g mp;
                mp.vertex = (p1.vertex + p2.vertex) / 2;
                mp.uv = (p1.uv + p2.uv) / 2;
                mp.normal = normalize(p1.normal + p2.normal);
                return mp;
			};

            void subdivide(triangle v2g input[3], out v2g output[12]) 
            {
				v2g vert0 = input[0];
				v2g vert1 = input[1];
				v2g vert2 = input[2];
                v2g vert05 = midpoint(vert0, vert1);
                v2g vert15 = midpoint(vert1, vert2);
                v2g vert25 = midpoint(vert2, vert0);

                output[0] = vert0;
                output[1] = vert05;
                output[2] = vert25;
                //
                output[3] = vert05;
                output[4] = vert15;
                output[5] = vert25;
                //
                output[6] = vert25;
                output[7] = vert15;
                output[8] = vert2;
                //
                output[9]  = vert05;
                output[10] = vert1;
                output[11] = vert15;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _NoiseScale;
            float _DeformScaleNormal;
            float _DeformScaleLateral;

            v2g vert (appdata v)
            {
                v2g o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
			
			[maxvertexcount(12)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
			{
                uint i;
				v2g v;
                v2g tris[12] = {v,v,v,v,v,v,v,v,v,v,v,v} ;

                subdivide(input, tris);
                for (i = 0; i < 12; i++)
                {
                    float3 noise = tri_color_sample(tris[i].vertex.xyz * _NoiseScale);
                    noise *= float3(_DeformScaleLateral, _DeformScaleNormal, _DeformScaleLateral);
                    // Transform from tangent space to object space
                    tris[i].vertex = tris[i].vertex + float4(noise ,1);
				}
                for (i = 0; i < 12; i++) 
                {
                    g2f output;
                    output.vertex = UnityObjectToClipPos(tris[i].vertex);
                    output.uv = tris[i].uv;
                    output.position_ws = tris[i].vertex.xyz;
                    output.normal = tris[i].normal;
                    triStream.Append(output);
                    if (i % 3 == 2) {
                        triStream.RestartStrip();
                    }
                };
			}

            fixed4 frag (g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
