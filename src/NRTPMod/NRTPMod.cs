using MelonLoader;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

[assembly: MelonInfo(typeof(NRTPMod), "No Rest for the Potato", "0.3.5", "YourName")]
[assembly: MelonGame("Moon Studios", "NoRestForTheWicked")]

public class NRTPMod : MelonMod
{
    private static readonly string ConfigPath = Path.Combine(
        Path.GetDirectoryName(typeof(NRTPMod).Assembly.Location) ?? Directory.GetCurrentDirectory(),
        "potato_graphics.cfg"
    );

    private static MelonPreferences_Category _cfg;
    private static MelonPreferences_Entry<bool> _shadowsEnabled;
    private static MelonPreferences_Entry<float> _shadowDistance;
    private static MelonPreferences_Entry<int> _shadowCascades;
    private static MelonPreferences_Entry<int> _shadowResolution;
    private static MelonPreferences_Entry<int> _textureQuality;
    private static MelonPreferences_Entry<int> _antiAliasing;
    private static MelonPreferences_Entry<bool> _anisotropic;
    private static MelonPreferences_Entry<bool> _motionBlur;
    private static MelonPreferences_Entry<bool> _fogEnabled;
    private static MelonPreferences_Entry<bool> _reflectionsEnabled;
    private static MelonPreferences_Entry<bool> _postProcessingEnabled;
    private static MelonPreferences_Entry<bool> _volumetricLightingEnabled;
    private static MelonPreferences_Entry<bool> _cloudShadowsEnabled;
    private static MelonPreferences_Entry<bool> _ambientOcclusionEnabled;
    private static MelonPreferences_Entry<bool> _bloomEnabled;
    private static MelonPreferences_Entry<bool> _depthOfFieldEnabled;
    private static MelonPreferences_Entry<bool> _hdrEnabled;

    private static MelonPreferences_Entry<bool> _occlusionCullingEnabled;
    private static MelonPreferences_Entry<bool> _forwardRenderingEnabled;
    private static MelonPreferences_Entry<int> _pixelLightCount;
    private static MelonPreferences_Entry<int> _targetFrameRate;
    private static MelonPreferences_Entry<bool> _particlesEnabled;
    private static MelonPreferences_Entry<float> _particleMultiplier;

    private static MelonPreferences_Entry<bool> _streamingMipmapsEnabled;
    private static MelonPreferences_Entry<int> _streamingMipmapsBudget;
    private static MelonPreferences_Entry<int> _streamingMipmapsMaxReduction;
    private static MelonPreferences_Entry<int> _asyncUploadTimeSlice;
    private static MelonPreferences_Entry<float> _lodBias;

    private static MelonPreferences_Entry<bool> _logMemoryEnabled;

    private static Camera[] _cachedCameras = null;
    private static bool _settingsApplied = false;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("No Rest for the Potato v0.3.5 loaded!");

        LoadExternalConfig();

        _cfg = MelonPreferences.CreateCategory("PotatoGraphics");
        _shadowsEnabled = _cfg.CreateEntry("ShadowsEnabled", true);
        _shadowDistance = _cfg.CreateEntry("ShadowDistance", 50f);
        _shadowCascades = _cfg.CreateEntry("ShadowCascades", 2);
        _shadowResolution = _cfg.CreateEntry("ShadowResolution", 2);
        _textureQuality = _cfg.CreateEntry("TextureQuality", 0);
        _antiAliasing = _cfg.CreateEntry("AntiAliasing", 2);
        _anisotropic = _cfg.CreateEntry("Anisotropic", true);
        _motionBlur = _cfg.CreateEntry("MotionBlur", true);
        _fogEnabled = _cfg.CreateEntry("FogEnabled", true);
        _reflectionsEnabled = _cfg.CreateEntry("ReflectionsEnabled", true);
        _postProcessingEnabled = _cfg.CreateEntry("PostProcessingEnabled", true);
        _volumetricLightingEnabled = _cfg.CreateEntry("VolumetricLightingEnabled", true);
        _cloudShadowsEnabled = _cfg.CreateEntry("CloudShadowsEnabled", true);
        _ambientOcclusionEnabled = _cfg.CreateEntry("AmbientOcclusionEnabled", true);
        _bloomEnabled = _cfg.CreateEntry("BloomEnabled", true);
        _depthOfFieldEnabled = _cfg.CreateEntry("DepthOfFieldEnabled", true);
        _hdrEnabled = _cfg.CreateEntry("HdrEnabled", true);

        _occlusionCullingEnabled = _cfg.CreateEntry("OcclusionCullingEnabled", true);
        _forwardRenderingEnabled = _cfg.CreateEntry("ForwardRenderingEnabled", false);
        _pixelLightCount = _cfg.CreateEntry("PixelLightCount", 1);
        _targetFrameRate = _cfg.CreateEntry("TargetFrameRate", 60);
        _particlesEnabled = _cfg.CreateEntry("ParticlesEnabled", true);
        _particleMultiplier = _cfg.CreateEntry("ParticleMultiplier", 0.25f);

        _streamingMipmapsEnabled = _cfg.CreateEntry("StreamingMipmapsEnabled", false);
        _streamingMipmapsBudget = _cfg.CreateEntry("StreamingMipmapsBudget", 512);
        _streamingMipmapsMaxReduction = _cfg.CreateEntry("StreamingMipmapsMaxReduction", 2);
        _asyncUploadTimeSlice = _cfg.CreateEntry("AsyncUploadTimeSlice", 2);
        _lodBias = _cfg.CreateEntry("LodBias", 1.0f);

        _logMemoryEnabled = _cfg.CreateEntry("LogMemoryEnabled", false);

        try
        {
            ApplySettings();
            MelonLogger.Msg("Settings loaded and applied (once).");
        }
        catch (Exception e)
        {
            MelonLogger.Error($"ApplySettings failed: {e.Message}");
            MelonLogger.Error($"StackTrace: {e.StackTrace}");
        }

        LogMemory("After ApplySettings");
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            MelonLogger.Msg("F10 pressed (GUI disabled - edit potato_graphics.cfg instead)");
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        // НЕ вызывать ApplySettings() здесь — сцена может перезагружаться бесконечно,
        // и это вызовет фризы и утечки памяти.
        // Настройки применяются только один раз при старте мода.
    }

    private void LoadExternalConfig()
    {
        if (!File.Exists(ConfigPath)) return;

        try
        {
            foreach (var line in File.ReadAllLines(ConfigPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2) continue;
                var key = parts[0].Trim();
                var val = parts[1].Trim();

                switch (key)
                {
                    case "ShadowsEnabled": _shadowsEnabled.Value = ParseBool(val); break;
                    case "ShadowDistance": _shadowDistance.Value = ParseFloat(val); break;
                    case "ShadowCascades": _shadowCascades.Value = ParseInt(val); break;
                    case "ShadowResolution": _shadowResolution.Value = ParseInt(val); break;
                    case "TextureQuality": _textureQuality.Value = ParseInt(val); break;
                    case "AntiAliasing": _antiAliasing.Value = ParseInt(val); break;
                    case "Anisotropic": _anisotropic.Value = ParseBool(val); break;
                    case "MotionBlur": _motionBlur.Value = ParseBool(val); break;
                    case "FogEnabled": _fogEnabled.Value = ParseBool(val); break;
                    case "ReflectionsEnabled": _reflectionsEnabled.Value = ParseBool(val); break;
                    case "PostProcessingEnabled": _postProcessingEnabled.Value = ParseBool(val); break;
                    case "VolumetricLightingEnabled": _volumetricLightingEnabled.Value = ParseBool(val); break;
                    case "CloudShadowsEnabled": _cloudShadowsEnabled.Value = ParseBool(val); break;
                    case "AmbientOcclusionEnabled": _ambientOcclusionEnabled.Value = ParseBool(val); break;
                    case "BloomEnabled": _bloomEnabled.Value = ParseBool(val); break;
                    case "DepthOfFieldEnabled": _depthOfFieldEnabled.Value = ParseBool(val); break;
                    case "HdrEnabled": _hdrEnabled.Value = ParseBool(val); break;
                    case "OcclusionCullingEnabled": _occlusionCullingEnabled.Value = ParseBool(val); break;
                    case "ForwardRenderingEnabled": _forwardRenderingEnabled.Value = ParseBool(val); break;
                    case "PixelLightCount": _pixelLightCount.Value = ParseInt(val); break;
                    case "TargetFrameRate": _targetFrameRate.Value = ParseInt(val); break;
                    case "ParticlesEnabled": _particlesEnabled.Value = ParseBool(val); break;
                    case "ParticleMultiplier": _particleMultiplier.Value = ParseFloat(val); break;
                    case "StreamingMipmapsEnabled": _streamingMipmapsEnabled.Value = ParseBool(val); break;
                    case "StreamingMipmapsBudget": _streamingMipmapsBudget.Value = ParseInt(val); break;
                    case "StreamingMipmapsMaxReduction": _streamingMipmapsMaxReduction.Value = ParseInt(val); break;
                    case "AsyncUploadTimeSlice": _asyncUploadTimeSlice.Value = ParseInt(val); break;
                    case "LodBias": _lodBias.Value = ParseFloat(val); break;
                    case "LogMemoryEnabled": _logMemoryEnabled.Value = ParseBool(val); break;
                }
            }
            MelonLogger.Msg($"Config loaded: {ConfigPath}");
        }
        catch (Exception e)
        {
            MelonLogger.Warning($"Config load failed: {e.Message}");
        }
    }

    public void SaveExternalConfig()
    {
        try
        {
            var lines = new List<string>
            {
                "# No Rest for the Potato - Graphics Settings",
                "# Edit this file to change graphics settings before launching the game",
                "",
                $"ShadowsEnabled={_shadowsEnabled.Value.ToString().ToLower()}",
                $"ShadowDistance={_shadowDistance.Value}",
                $"ShadowCascades={_shadowCascades.Value}",
                $"ShadowResolution={_shadowResolution.Value}",
                $"TextureQuality={_textureQuality.Value}",
                $"AntiAliasing={_antiAliasing.Value}",
                $"Anisotropic={_anisotropic.Value.ToString().ToLower()}",
                $"MotionBlur={_motionBlur.Value.ToString().ToLower()}",
                $"FogEnabled={_fogEnabled.Value.ToString().ToLower()}",
                $"ReflectionsEnabled={_reflectionsEnabled.Value.ToString().ToLower()}",
                $"PostProcessingEnabled={_postProcessingEnabled.Value.ToString().ToLower()}",
                $"VolumetricLightingEnabled={_volumetricLightingEnabled.Value.ToString().ToLower()}",
                $"CloudShadowsEnabled={_cloudShadowsEnabled.Value.ToString().ToLower()}",
                $"AmbientOcclusionEnabled={_ambientOcclusionEnabled.Value.ToString().ToLower()}",
                $"BloomEnabled={_bloomEnabled.Value.ToString().ToLower()}",
                $"DepthOfFieldEnabled={_depthOfFieldEnabled.Value.ToString().ToLower()}",
                $"HdrEnabled={_hdrEnabled.Value.ToString().ToLower()}",
                $"OcclusionCullingEnabled={_occlusionCullingEnabled.Value.ToString().ToLower()}",
                $"ForwardRenderingEnabled={_forwardRenderingEnabled.Value.ToString().ToLower()}",
                $"PixelLightCount={_pixelLightCount.Value}",
                $"TargetFrameRate={_targetFrameRate.Value}",
                $"ParticlesEnabled={_particlesEnabled.Value.ToString().ToLower()}",
                $"ParticleMultiplier={_particleMultiplier.Value}",
                $"StreamingMipmapsEnabled={_streamingMipmapsEnabled.Value.ToString().ToLower()}",
                $"StreamingMipmapsBudget={_streamingMipmapsBudget.Value}",
                $"StreamingMipmapsMaxReduction={_streamingMipmapsMaxReduction.Value}",
                $"AsyncUploadTimeSlice={_asyncUploadTimeSlice.Value}",
                $"LodBias={_lodBias.Value}",
                $"LogMemoryEnabled={_logMemoryEnabled.Value.ToString().ToLower()}"
            };
            File.WriteAllLines(ConfigPath, lines);
            MelonLogger.Msg($"Config saved: {ConfigPath}");
        }
        catch (Exception e)
        {
            MelonLogger.Warning($"Config save failed: {e.Message}");
        }
    }

    private bool ParseBool(string v) => v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1";
    private int ParseInt(string v) => int.TryParse(v, out var r) ? r : 0;
    private float ParseFloat(string v) => float.TryParse(v, out var r) ? r : 0f;

    private void ApplySettings()
    {
        if (_settingsApplied) return;

        LogMemory("Before ApplySettings");

        try
        {
            ApplyShadowSettings();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Shadows failed: {e.Message}");
        }

        try
        {
            ApplyTextureSettings();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Textures failed: {e.Message}");
        }

        try
        {
            ApplyAAAndFiltering();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"AA/Aniso failed: {e.Message}");
        }

        try
        {
            ApplyHDR();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"HDR failed: {e.Message}");
        }

        try
        {
            ApplyRenderingOptimizations();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Rendering optimizations failed: {e.Message}");
        }

        try
        {
            ApplyParticleSettings();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Particles failed: {e.Message}");
        }

        _settingsApplied = true;
        MelonLogger.Msg("Settings applied (once).");

        LogMemory("After ApplySettings");

        try
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            MelonLogger.Msg("GC collect done.");
        }
        catch (Exception e)
        {
            MelonLogger.Warning($"GC failed: {e.Message}");
        }
    }

    private static Camera[] GetCachedCameras()
    {
        if (_cachedCameras == null || _cachedCameras.Length == 0)
        {
            _cachedCameras = Camera.allCameras;
        }
        return _cachedCameras;
    }

    private void ApplyShadowSettings()
    {
        if (_shadowsEnabled.Value)
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = _shadowDistance.Value;
            QualitySettings.shadowCascades = _shadowCascades.Value;
            QualitySettings.shadowResolution = (ShadowResolution)_shadowResolution.Value;
        }
        else
        {
            QualitySettings.shadows = ShadowQuality.Disable;
        }
    }

    private void ApplyTextureSettings()
    {
        QualitySettings.masterTextureLimit = _textureQuality.Value;
    }

    private void ApplyAAAndFiltering()
    {
        QualitySettings.antiAliasing = _antiAliasing.Value;
        QualitySettings.anisotropicFiltering = _anisotropic.Value ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
    }

    private void ApplyHDR()
    {
        var cameras = GetCachedCameras();
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
                cameras[i].allowHDR = _hdrEnabled.Value;
        }
    }

    private void ApplyRenderingOptimizations()
    {
        // Occlusion Culling (CPU-тяжелая операция)
        SetComponentsEnabled("OcclusionCulling", _occlusionCullingEnabled.Value);

        // Forward Rendering для камер
        if (_forwardRenderingEnabled.Value)
        {
            var cams = GetCachedCameras();
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i] != null && cams[i].actualRenderingPath == RenderingPath.DeferredShading)
                    cams[i].renderingPath = RenderingPath.Forward;
            }
            MelonLogger.Msg("Cameras switched to Forward rendering.");
        }

        // Лимит пиксельных источников света
        QualitySettings.pixelLightCount = _pixelLightCount.Value;
        MelonLogger.Msg($"Pixel lights limited to {_pixelLightCount.Value}.");

        // Целевой FPS
        if (_targetFrameRate.Value > 0)
        {
            Application.targetFrameRate = _targetFrameRate.Value;
            MelonLogger.Msg($"Target FPS set to {_targetFrameRate.Value}.");
        }

        // === Потоковые текстуры (v0.3.3) ===
        QualitySettings.streamingMipmapsActive = _streamingMipmapsEnabled.Value;
        QualitySettings.streamingMipmapsMemoryBudget = _streamingMipmapsBudget.Value;
        QualitySettings.streamingMipmapsMaxLevelReduction = _streamingMipmapsMaxReduction.Value;
        MelonLogger.Msg($"Streaming mipmaps: active={_streamingMipmapsEnabled.Value}, budget={_streamingMipmapsBudget.Value}MB, maxReduction={_streamingMipmapsMaxReduction.Value}.");

        // === Асинхронная загрузка и LOD (v0.3.3) ===
        QualitySettings.asyncUploadTimeSlice = _asyncUploadTimeSlice.Value;
        QualitySettings.lodBias = _lodBias.Value;
        MelonLogger.Msg($"Async upload time slice: {_asyncUploadTimeSlice.Value}ms, LOD bias: {_lodBias.Value}.");
    }

    private void ApplyParticleSettings()
    {
        var all = Resources.FindObjectsOfTypeAll<ParticleSystem>();
        foreach (var ps in all)
        {
            if (ps == null) continue;

            if (!_particlesEnabled.Value)
            {
                ps.Stop();
                ps.gameObject.SetActive(false);
                continue;
            }

            ps.gameObject.SetActive(true);
            var main = ps.main;
            main.simulationSpeed = _particleMultiplier.Value;

            var emission = ps.emission;
            var rate = emission.rateOverTime;
            rate = new ParticleSystem.MinMaxCurve(rate.constantMax * _particleMultiplier.Value);
            emission.rateOverTime = rate;

            main.maxParticles = Mathf.Max(1, (int)(main.maxParticles * _particleMultiplier.Value));
        }
    }

    private void SetComponentsEnabled(string nameContains, bool enabled)
    {
        try
        {
            var all = Resources.FindObjectsOfTypeAll<Component>();
            foreach (var comp in all)
            {
                if (comp == null || !(comp is Behaviour b)) continue;
                var name = comp.GetType().Name;
                if (name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    b.enabled = enabled;
                }
            }
        }
        catch (Exception e)
        {
            MelonLogger.Warning($"SetComponentsEnabled({nameContains}) failed: {e.Message}");
        }
    }

    private void LogMemory(string stage)
    {
        if (!_logMemoryEnabled.Value) return;

        try
        {
            long gcMem = GC.GetTotalMemory(false);
            long ws = Process.GetCurrentProcess().WorkingSet64;
            MelonLogger.Msg($"[MEMORY {stage}] GC={gcMem / 1024 / 1024}MB, WorkingSet={ws / 1024 / 1024}MB");
        }
        catch (Exception e)
        {
            MelonLogger.Warning($"Memory log failed: {e.Message}");
        }
    }
}