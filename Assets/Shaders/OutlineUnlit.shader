Shader "Custom/OutlineUnlit"
{
    Properties
    {
        _Color ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Cull Front        // draw backfaces only
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // Extrude along world normal by the outline width (in world units)
                worldPos += worldNormal * _OutlineWidth;
                o.pos = UnityWorldToClipPos(worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}