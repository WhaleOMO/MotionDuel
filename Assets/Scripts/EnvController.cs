using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class EnvController : MonoBehaviour
{
    public Volume globalVolume;
    public GameObject mainLight;
    public Camera specCubeCam;
    public Camera overlayCubeCam;
    public Material skyboxMaterial;
    public Material fogMaterial;
    
    [Range(0,1)]public float relativeTimer = 0;
    
    [Header("Sky Control")]
    [GradientUsage(true)]
    public Gradient skyZenithColor;
    [GradientUsage(true)]
    public Gradient skyMidColor;
    [GradientUsage(true)]
    public Gradient skyNightZenithColor;
    [GradientUsage(true)]
    public Gradient skyNightMidColor;

    [Header("Fog Control")] 
    public Gradient fogColor;
    public AnimationCurve fogDensity;
    public AnimationCurve fogFalloff;
    public AnimationCurve fogSunPower;
    public AnimationCurve envSpecCubeIntensity;

    [Header("Surface Control")] 
    public AnimationCurve surfaceSaturation;
    public Material iceMaterial;
    public Material fullScreenIceMaterial;
    
    [Header("Thunder Weather")] 
    [ColorUsage(false,true)]
    public Color thunderSkyMidColor;
    [ColorUsage(false,true)] 
    public Color thunderSkyZenithColor;
    public Color thunderFogColor;
    public Color thunderFogSunColor;
    public float thunderFogFalloff;
    public float thunderFogDensity;
    
    [Header("Reflection")] 
    public bool manualUpdate = false;
    public Cubemap skyCube;
    
    private float angle;
    private float lastUpdate;
    private bool isThunder;

    private static int DayZenithColorID = Shader.PropertyToID("_DayZenithColor");
    private static int DayMidColorID = Shader.PropertyToID("_DayMidColor");
    private static int NightZenithColorID = Shader.PropertyToID("_NightZenithColor");
    private static int NightMidColorID = Shader.PropertyToID("_NightMidColor");
    private static int FogColorID = Shader.PropertyToID("_FogColor");
    private static int FogFalloffID = Shader.PropertyToID("_FogFalloff");
    private static int FogDensityID = Shader.PropertyToID("_FogGlobalDensity");

    private Sequence iceSequence;
    private Sequence fullIceSequence;
    
    private void Start()
    {
        relativeTimer = 0;
        angle = 60f;
        if (manualUpdate)
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = skyCube;
            UpdateSkyReflection();
        }
        else
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }
        //RenderSettings.ambientMode = AmbientMode.Skybox;
        // RenderSettings.ambientProbe = skySH;
        iceSequence = DOTween.Sequence();
        fullIceSequence = DOTween.Sequence();
        iceMaterial.SetFloat(Shader.PropertyToID("_IceAmount"), 0);
        fullScreenIceMaterial.SetFloat(Shader.PropertyToID("_Bright"), 0);
    }

    private void UpdateSkyReflection()
    {
        // return;
        specCubeCam.cullingMask = 0;
        // Terrible Performance, only update when necessary, todo: screen space reflection
        {
            specCubeCam.RenderToCubemap(skyCube);
            skyCube.Apply();
            RenderSettings.customReflectionTexture = skyCube;
        }
        specCubeCam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "Water", "UI");
    }
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            FadeInThunderWeather(1.0f,2);
        }
        
        if (Application.isPlaying)
        {
            relativeTimer += 0.02f * Time.deltaTime;
            if (manualUpdate)
            {
                if (relativeTimer - lastUpdate >= 0.1)
                {
                    UpdateSkyReflection();
                    lastUpdate = relativeTimer;
                }
            }
        }

        angle = 60.0f - Mathf.Lerp(0, 80.0f, relativeTimer);
        mainLight.transform.rotation = Quaternion.Euler(angle, 180.0f, 0f);
        
        if (!isThunder)
        {
            skyboxMaterial.SetColor(DayZenithColorID, skyZenithColor.Evaluate(relativeTimer));
            skyboxMaterial.SetColor(DayMidColorID, skyMidColor.Evaluate(relativeTimer));
            skyboxMaterial.SetColor(NightZenithColorID, skyNightZenithColor.Evaluate(relativeTimer));
            skyboxMaterial.SetColor(NightMidColorID, skyNightMidColor.Evaluate(relativeTimer));
            fogMaterial.SetColor(FogColorID, fogColor.Evaluate(relativeTimer));
            fogMaterial.SetFloat(FogDensityID, fogDensity.Evaluate(relativeTimer));
            fogMaterial.SetFloat(FogFalloffID, fogFalloff.Evaluate(relativeTimer));
            fogMaterial.SetFloat(Shader.PropertyToID("_SunFogPower"), fogSunPower.Evaluate(relativeTimer));
            Shader.SetGlobalFloat(Shader.PropertyToID("_Saturation"), surfaceSaturation.Evaluate(relativeTimer));
        }

        RenderSettings.reflectionIntensity = envSpecCubeIntensity.Evaluate(relativeTimer);
    }

    public void FadeInThunderWeather(float fade, float dur)
    {
        isThunder = true;
        float multiplier = relativeTimer > 0.75f ? 0.2f : 1.0f;
        float nightFog = relativeTimer > 0.75f ? 1.2f : 1f;
        DOTween.Sequence().Append(skyboxMaterial.DOColor(thunderSkyMidColor * multiplier, DayMidColorID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(skyboxMaterial.DOColor(thunderSkyMidColor * multiplier, NightMidColorID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(skyboxMaterial.DOColor(thunderSkyZenithColor * multiplier, DayZenithColorID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(skyboxMaterial.DOColor(thunderSkyZenithColor * multiplier, NightZenithColorID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(fogMaterial.DOColor(thunderFogColor, FogColorID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(fogMaterial.DOColor(thunderFogSunColor, Shader.PropertyToID("_LightColor"), fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(fogMaterial.DOFloat(thunderFogDensity * nightFog, FogDensityID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).Play();
        DOTween.Sequence().Append(fogMaterial.DOFloat(thunderFogFalloff * nightFog, FogFalloffID, fade))
            .AppendInterval(dur).SetLoops(2, LoopType.Yoyo).OnComplete((() => { isThunder = false;})).Play();
        Shader.SetGlobalFloat(Shader.PropertyToID("_Saturation"), 0);
    }

    public void FadeInIce(float dur)
    {
        iceMaterial.DOFloat(1.0f, Shader.PropertyToID("_IceAmount"), dur/2).SetLoops(2, LoopType.Yoyo);
        fullScreenIceMaterial.DOFloat(1.0f, Shader.PropertyToID("_Bright"), 0.3f)
            .OnComplete((() =>
            {
                fullScreenIceMaterial.DOFloat(0f, Shader.PropertyToID("_Bright"), 0.3f).SetDelay(dur-0.6f);
            }));
        //     .AppendInterval(dur - 0.6f).SetLoops(2, LoopType.Yoyo)
        // iceSequence.Append(iceMaterial.DOFloat(1.0f, Shader.PropertyToID("_IceAmount"), dur)).SetLoops(2, LoopType.Yoyo);
        // fullIceSequence.Append(fullScreenIceMaterial.DOFloat(1.0f, Shader.PropertyToID("_Bright"), 0.3f))
        //     .AppendInterval(dur - 0.6f).SetLoops(2, LoopType.Yoyo);
    }

    public void MainCameraShake(float dur)
    {
        if(globalVolume.profile.TryGet<PaniniProjection>(out var panini))
        {
           panini.distance.overrideState = true;
           DOTween.To(() => panini.distance.value, x => panini.distance.value = x, 0.5f, 0.2f).SetEase(Ease.InBounce);
           //panini.distance.value = 0.6f;
        }
        if(globalVolume.profile.TryGet<ChromaticAberration>(out var chromatic))
        {
            chromatic.intensity.overrideState = true;
            DOTween.To(() => chromatic.intensity.value, x => chromatic.intensity.value = x, 1f, 0.2f);
            //chromatic.intensity.value = 1.0f;
        }
        
        specCubeCam.DOShakePosition(dur, 3f, 5, 45f, true, ShakeRandomnessMode.Harmonic);
        overlayCubeCam.DOShakePosition(dur, 0.2f, 20, 80f, true, ShakeRandomnessMode.Harmonic).OnComplete(
            (() =>
            {
                DOTween.To(() => panini.distance.value, x => panini.distance.value = x, 0f, 0.2f).OnComplete(
                    (() =>
                    {
                        DOTween.To(() => chromatic.intensity.value, x => chromatic.intensity.value = x, 0f, 0.2f);
                    })
                    );
            })
            );
    }
}
