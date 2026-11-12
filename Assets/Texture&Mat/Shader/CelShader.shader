Shader "CelShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        
        [Header(Cel Shading Settings)]
        [Toggle] _EnableCelShading ("Enable Cel Shading", Float) = 1.0
        _Threshold ("Shadow Threshold", Range(-1, 1)) = 0.0 // Where the light breaks
        _ShadowColor ("Shadow Tint Color", Color) = (0.3, 0.3, 0.4, 1)
    }
    SubShader
    {
        // URP tags to ensure the shader renders correctly in the pipeline
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        // Lighting and shadow
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION; // Object space position
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;     // Object space normal
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION; // Clip space position
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;        // World space normal
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _EnableCelShading;
            float _Threshold;
            half4 _ShadowColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                // Transform vertices from object space to clip space
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Transform normals from object space to world space
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);

                // Normalize the world space normal so its length is exactly 1
                float3 normal = normalize(i.normalWS);

                // Get the main directional light from URP
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                // Dot Product
                // Calculates the cosine of the angle between the surface normal and the incoming light direction
                float NdotL = (normal.x * lightDir.x) + (normal.y * lightDir.y) + (normal.z * lightDir.z);

                half3 finalColor;

                // Toggle logic block
                if (_EnableCelShading > 0.5)
                {
                    // We split the light into solid bands using an 'if' threshold instead of a smooth gradient
                    if (NdotL > _Threshold)
                    {
                        // Surface faces the light: fully lit
                        finalColor = texColor.rgb; 
                    }
                    else
                    {
                        // Surface faces away from the light: apply flat shadow tint
                        finalColor = texColor.rgb * _ShadowColor.rgb; 
                    }
                }
                else
                {
                    // Defualt Lighting (when toggled off)
                    // Uses max(0, NdotL) to create a smooth gradient that never goes negative
                    float smoothLight = max(0, NdotL);
                    finalColor = texColor.rgb * (smoothLight * mainLight.color + 0.25); // Added 0.25 fake ambient
                }

                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
}
        