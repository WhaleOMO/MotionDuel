using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class EnvController : MonoBehaviour
{
    public GameObject mainLight;
    public Camera specCubeCam;
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

    [Header("Reflection")] 
    public Cubemap skyCube;
    
    private float angle;
    private float lastUpdate;

    private void Start()
    {
        relativeTimer = 0;
        angle = 60f;
        // RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        // RenderSettings.customReflectionTexture = skyCube;
        // UpdateSkyReflection();
        //RenderSettings.ambientMode = AmbientMode.Skybox;
        // RenderSettings.ambientProbe = skySH;
    }

    private void UpdateSkyReflection()
    {
        return;
        specCubeCam.cullingMask = 0;
        // Terrible Performance, only update when necessary, todo: screen space reflection
        {
            specCubeCam.RenderToCubemap(skyCube);
            skyCube.Apply();
        }
        specCubeCam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "Water", "UI");
    }
    
    private void Update()
    {
        if (Application.isPlaying)
        {
            relativeTimer += 0.05f * Time.deltaTime;
            if (relativeTimer - lastUpdate >= 0.05)
            {
                UpdateSkyReflection();
                lastUpdate = relativeTimer;
            }
        }

        angle = 60.0f - Mathf.Lerp(0, 80.0f, relativeTimer);
        mainLight.transform.rotation = Quaternion.Euler(angle, 180.0f, 0f);
        skyboxMaterial.SetColor(Shader.PropertyToID("_DayZenithColor"), skyZenithColor.Evaluate(relativeTimer));
        skyboxMaterial.SetColor(Shader.PropertyToID("_DayMidColor"), skyMidColor.Evaluate(relativeTimer));
        skyboxMaterial.SetColor(Shader.PropertyToID("_NightZenithColor"), skyNightZenithColor.Evaluate(relativeTimer));
        skyboxMaterial.SetColor(Shader.PropertyToID("_NightMidColor"), skyNightMidColor.Evaluate(relativeTimer));
        fogMaterial.SetColor(Shader.PropertyToID("_FogColor"), fogColor.Evaluate(relativeTimer));
        fogMaterial.SetFloat(Shader.PropertyToID("_FogGlobalDensity"), fogDensity.Evaluate(relativeTimer));
        fogMaterial.SetFloat(Shader.PropertyToID("_FogFalloff"), fogFalloff.Evaluate(relativeTimer));
        fogMaterial.SetFloat(Shader.PropertyToID("_SunFogPower"), fogSunPower.Evaluate(relativeTimer));

        // DynamicGI.UpdateEnvironment();
        specCubeCam.cullingMask = LayerMask.GetMask();
        /* Terrible Performance, not going to use, todo: screen space reflection
        {
            Transform camTransform = specCubeCam.transform;
            Vector3 originPos = camTransform.position;
            Quaternion originRot = camTransform.rotation;

            camTransform.position = Vector3.zero;
            camTransform.rotation = Quaternion.identity;

            specCubeCam.RenderToCubemap(skyCube);
            skyCube.Apply();

            camTransform.position = originPos;
            camTransform.rotation = originRot;
        }
        */
        specCubeCam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "Water", "UI");
    }
}
