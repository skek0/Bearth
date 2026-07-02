Shader "Custom/ModuleGlow"
{
    Properties
    {
        _MainTex      ("Sprite Texture", 2D)    = "white" {}
        _GlowColor    ("Glow Color [HDR]", Color) = (1, 1, 1, 1)
        _GlowMin      ("Glow Min Brightness", Float) = 1
        _GlowMax      ("Glow Max Brightness", Float) = 1.5
        _GlowSpeed    ("Glow Pulse Speed",    Float) = 1.5

        // 피해 깜박임 (C#에서 제어)
        _HitFlicker   ("Hit Flicker Intensity", Float) = 0.0
        _FlickerSpeed ("Flicker Speed",         Float) = 20.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _GlowColor;
                float  _GlowMin;
                float  _GlowMax;
                float  _GlowSpeed;
                float  _HitFlicker;
                float  _FlickerSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;       // SpriteRenderer 버텍스 색상
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;   // ← 그냥 그대로 전달
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // 스프라이트 알파가 0인 부분(투명)은 버림
                clip(texColor.a - 0.01);

                // ── 1) 은은한 형광 글로우 ──────────────────────────
                // _Time.y는 Unity 내장 변수: 게임 시작 후 경과 시간(초)
                // sin()은 -1~1 사이를 오가는 파도. 여기서 0~1 범위로 변환
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                float brightness = lerp(_GlowMin, _GlowMax, pulse);

                // HDR: 밝기가 1.0을 넘으면 Bloom이 번짐을 만들어 줌
                half3 glow = _GlowColor.rgb * brightness;

                // ── 2) 피해 깜박임 ────────────────────────────────
                // _HitFlicker는 C# 스크립트가 0(평소) ↔ 1(피해) 로 설정
                // frac( _Time.y * speed )는 0→1→0→1 을 빠르게 반복
                float flicker = frac(_Time.y * _FlickerSpeed);
                // 깜박일 때 밝게 튀어오르다가 꺼지는 패턴
                float hitBurst = step(0.5, flicker) * _HitFlicker;
                glow += hitBurst * 2.0;

                half4 finalColor;
                finalColor.rgb = texColor.rgb * glow * IN.color.rgb;
                finalColor.a   = texColor.a   * IN.color.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}