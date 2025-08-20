using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.Camera;
using Mafi.Unity.Entities;
using Mafi.Unity.TexturesGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CoI.ModToolsAutogen;

/// <summary>
/// Render entity icons to be used for the game default assets.
/// </summary>
/// <remarks>
/// Adapted from Mafi.Unity.TexturesGenerators.IconsGenerator
/// </remarks>
[GlobalDependency(RegistrationMode.AsSelf)]
internal class IconGenerator
{
    private readonly ProtosDb m_db;
    private readonly ProtoModelFactory m_modelFactory;
    private readonly ColorizableMaterialsCache m_colorizableMaterialsCache;
    private readonly CameraController m_cameraController;
    private readonly Material m_outlineBgMaterial;

    public IconGenerator(ProtosDb db, AssetsDb assetsDb, ProtoModelFactory modelFactory, ColorizableMaterialsCache colorizableMaterialsCache, CameraController cameraController)
    {
        m_db = db;
        m_modelFactory = modelFactory;
        m_colorizableMaterialsCache = colorizableMaterialsCache;
        m_cameraController = cameraController;
        m_outlineBgMaterial = assetsDb.GetClonedMaterial("Assets/Core/IconOverlay/IconOutline.mat");
    }

    public int GenerateIcons(string? substring, string basePath, AngleDegrees1f pith, AngleDegrees1f yaw, AngleDegrees1f fov)
    {
        var rootGo = GameObject.Find("Game") ?? UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().First();
        var renderer = new GameObjectRenderer(rootGo, 20.0f, m_cameraController.Camera);

        renderer.SetUpRendering();
        try {
            renderer.SetImageSize(new Vector2i(256, 256));
            renderer.SetLight(UnityEngine.Color.white, 40.Degrees(), 110.Degrees());
            m_outlineBgMaterial.SetFloat(OFFSET_PERCENT_SHADER_ID, 4f / (float)256);

            int count = 0;
            foreach (LayoutEntityProto layoutEntityProto in m_db.All<LayoutEntityProto>().OrderBy(x => x.Id)) {
                if (string.IsNullOrEmpty(substring) || layoutEntityProto.Id.Value.Contains(substring, StringComparison.OrdinalIgnoreCase)) {
                    Log.Info($"GenerateIcons: Generating icon for {layoutEntityProto.Id}");

                    renderer.SetCamera(pith, layoutEntityProto.Graphics.YawForGeneratedIcon ?? yaw, fov);

                    GameObject entityGo = m_modelFactory.CreateModelWithPortsFor(layoutEntityProto);

                    // The GameObjectRenderer produces a graphical artifact of unknown origin. The visible artifact gets
                    // smaller when the object gets larger. As such, just scale the object up so the artifact becomes smaller
                    // than a pixel and it becomes invisible. Limit scaling factor to avoid any possible weirdness.
                    var bounds = GetBoundingBox(entityGo);
                    float scaleFactor = bounds.HasValue ? Math.Min(Math.Max(1.0f, 500.0f / bounds.Value.size.x), 100.0f) : 10.0f;
                    entityGo.transform.localScale = entityGo.transform.localScale * scaleFactor;

                    if (layoutEntityProto.Graphics.Color.IsNotEmpty) {
                        m_colorizableMaterialsCache.SetColorOfAllColorizableMaterials(entityGo, layoutEntityProto.Graphics.Color.ToColor());
                    }
                    // Disable animations.
                    var componentsInChildren = entityGo.GetComponentsInChildren<Animator>();
                    foreach (var component in componentsInChildren) {
                        component.speed = 0f;
                    }
                    // Disable emissions.
                    Lyst<Material> emissionMaterials = [];
                    entityGo.InstantiateMaterials((MeshRenderer x) => x.sharedMaterial.IsKeywordEnabled("_EMISSION"), emissionMaterials);
                    foreach (var mat in emissionMaterials) {
                        mat.SetColor(EMISSION_COLOR_SHADER_ID, Color.clear);
                    }

                    string path = Path.Combine(basePath, layoutEntityProto.Graphics.IconPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    foreach (int item in RenderOutlinedModel(renderer, entityGo, path)) {
                        _ = item;
                    }
                    entityGo.DestroyImmediateIfNotNull();

                    count++;
                }
            }
            return count;
        } finally {
            renderer.TearDownRendering();
        }
    }

    private IEnumerable<int> RenderOutlinedModel(GameObjectRenderer renderer, GameObject go, string path, bool disableOutline = false)
    {
        Light[] componentsInChildren = go.GetComponentsInChildren<Light>();
        for (int i = 0; i < componentsInChildren.Length; i++) {
            componentsInChildren[i].enabled = false;
        }
        GameObject? outline = null;
        if (!disableOutline) {
            outline = UnityEngine.Object.Instantiate(go);
            outline.name += "_outline";
            Renderer[] componentsInChildren2 = outline.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < componentsInChildren2.Length; j++) {
                componentsInChildren2[j].sharedMaterial = m_outlineBgMaterial;
            }
            outline.transform.SetParent(go.transform, worldPositionStays: false);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localRotation = Quaternion.identity;
            outline.transform.localScale = Vector3.one;
            outline.SetActive(value: true);
        }
        foreach (int item in renderer.RenderToPngSelfCentering(go, path)) {
            yield return item;
        }
        outline?.DestroyImmediateIfNotNull();
    }

    private static Bounds? GetBoundingBox(GameObject go)
    {
        var componentsInChildren = go.GetComponentsInChildren<Renderer>();
        if (componentsInChildren.Length == 0) return null;

        bool valid = false;
        Bounds result = default;
        foreach (var c in componentsInChildren) {
            var bounds = c.bounds;
            if (bounds.center.IsFinite() && bounds.size.IsFinite()) {
                if (valid) {
                    result.Encapsulate(bounds);
                } else {
                    result = bounds;
                    valid = true;
                }
            }
        }

        return valid ? result : null;
    }

    private static readonly int OFFSET_PERCENT_SHADER_ID = Shader.PropertyToID("_OffsetPercent");
    private static readonly int EMISSION_COLOR_SHADER_ID = Shader.PropertyToID("_EmissionColor");
}
