Shader "Custom/Outline" 
{

    Properties 
    {
        _MainTex ("Albedo", 2D) = "white" {}
        [HDR] _Tint ("Tint", Color) = (1, 1, 1, 1)

        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0
        [HDR] _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 20)) = 0.03
    }

    SubShader 
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

        // physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input 
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        half4 _Tint;
        half _Glossiness;
        half _Metallic;

        // surface mapping
        void surf(Input IN, inout SurfaceOutputStandard o) 
        {
            fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
            fixed3 albedo = tex.rgb * _Tint.rgb;


            o.Albedo = albedo;
            o.Smoothness = _Glossiness;
            o.Metallic = _Metallic;
            o.Alpha = tex.a * _Tint.a;
        }

        ENDCG

        // outline pass
        Pass 
        {
            // render only faces that are facing away from the camera
            Cull Front

            CGPROGRAM

            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            half _OutlineWidth;

            float4 VertexProgram(float4 position: POSITION, float3 normal: NORMAL): SV_POSITION 
            {
                //calculate clip position ansd normal
                float4 clipPosition = UnityObjectToClipPos(position);
                float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, normal));
                
                //push vertex out by a width of the normal, scaled by clip position width and screen size
                float2 offset = normalize(clipNormal.xy) / _ScreenParams.xy * _OutlineWidth;
                clipPosition.xy += offset;

                return clipPosition;
            }

            half4 _OutlineColor;

            half4 FragmentProgram(): SV_TARGET 
            {
                return _OutlineColor;
            }

            ENDCG

        }

    }

    FallBack "Diffuse"
}