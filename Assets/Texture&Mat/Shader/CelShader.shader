Shader "CelShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Cel Shading Settings)]
        [Toggle] _EnableCelShading ("Enable Cel Shading", Float) = 1.0
        _Threshold ("Shadow Threshold", Range(-1, 1)) = 0.0 // Where the light breaks
        _ShadowColor ("Shadow Tint Color", Color) = (0.3, 0.3, 0.4, 1)

        [Header(Rim Lighting Settings)]
        [Toggle] _EnableRimLight ("Enable Rim Lighting", Float) = 1.0
        _RimColor ("Rim Highlight Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power (Edge Tightness)", Range(0.5, 8.0)) = 3.0 // Higher = thinner edge glow
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
                float4 color : COLOR; // Object color
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION; // Clip space position
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;        // World space normal
                float3 positionWS : TEXCOORD1;   // World space position 
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _BaseColor;
            float _EnableCelShading;
            float _Threshold;
            half4 _ShadowColor;
            float _EnableRimLight;
            half4 _RimColor;
            float _RimPower;

            Varyings vert(Attributes v)
            {
                Varyings o;
                // Transform vertices from object space to clip space
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                // Transform vertices from object space to world space for rim light view calculation
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Transform normals from object space to world space
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                half3 surfaceColor = texColor.rgb * _BaseColor.rgb * i.color.rgb;

                // Normalize the world space normal so its length is exactly 1
                float3 normal = normalize(i.normalWS);

                // Get the main directional light from URP
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                // Dot Product
                // Calculates the cosine of the angle between the surface normal and the incoming light direction
                float NdotL = (normal.x * lightDir.x) + (normal.y * lightDir.y) + (normal.z * lightDir.z);

                half3 finalColor;

                // Toggle Celshading
                if (_EnableCelShading > 0.5)
                {
                    // We split the light into solid bands using an 'if' threshold instead of a smooth gradient
                    if (NdotL > _Threshold)
                    {
                        // Surface faces the light: fully lit
                        finalColor = surfaceColor * mainLight.color;
                    }
                    else
                    {
                        // Surface faces away from the light: apply flat shadow tint
                        finalColor = surfaceColor * _ShadowColor.rgb;
                    }
                }
                else
                {
                    // Defualt Lighting (when toggled off)
                    // Uses max(0, NdotL) to create a smooth gradient that never goes negative
                    float smoothLight = max(0, NdotL);
                    finalColor = surfaceColor * (smoothLight * mainLight.color + 0.25); // Added 0.25 fake ambient
                }

                // Toggle Rim Lighting
                if (_EnableRimLight > 0.5)
                {
                    // Unit vector pointing from the surface point toward the camera
                    float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);

                    // Dot product (n, v): 1.0 when facing the camera directly, 0.0 at grazing silhouette edges
                    float NdotV = (normal.x * viewDir.x) + (normal.y * viewDir.y) + (normal.z * viewDir.z);
                    NdotV = clamp(NdotV, 0.0, 1.0); // Clamp to [0, 1] to prevent negative values

                    // Invert (1 - (n . v)) so outer edges become 1.0 and center becomes 0.0
                    float rimFactor = 1.0 - NdotV;

                    // Raise to _RimPower to narrow the glow strictly to the outer silhouette
                    float rimIntensity = pow(rimFactor, _RimPower);

                    // Add the rim highlight on top of the cel-shaded color
                    finalColor += _RimColor.rgb * rimIntensity;
                }
                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }
        // Using built-in fuction to cast shadow
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
        