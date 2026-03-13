using System.Collections.Generic;
using UnityEngine;

public class Highlightable : MonoBehaviour
{
    [Header("URP Outline (Shader Graph)")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Color outlineColor = new Color(1f, 1f, 0f, 1f);
    [SerializeField, Range(0.001f, 0.05f)] private float outlineWidth = 0.01f;

    private bool isHighlighted;
    private readonly List<Renderer> outlineRenderers = new();

    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int OutlineColorID_SG = Shader.PropertyToID("OutlineColor");
    private static readonly int OutlineWidthID_SG = Shader.PropertyToID("OutlineWidth");

    private void Awake()
    {
        BuildOutlineObjects();
        isHighlighted = false;
        ForceOutlineEnabled(false);
    }

    private void BuildOutlineObjects()
    {
        outlineRenderers.Clear();

        if (outlineMaterial == null)
        {
            Debug.LogWarning("[Highlightable] outlineMaterial is not assigned.");
            return;
        }

        var matInstance = new Material(outlineMaterial);

        if (matInstance.HasProperty(OutlineColorID_SG))
            matInstance.SetColor(OutlineColorID_SG, outlineColor);
        else if (matInstance.HasProperty(OutlineColorID))
            matInstance.SetColor(OutlineColorID, outlineColor);

        if (matInstance.HasProperty(OutlineWidthID_SG))
            matInstance.SetFloat(OutlineWidthID_SG, outlineWidth);
        else if (matInstance.HasProperty(OutlineWidthID))
            matInstance.SetFloat(OutlineWidthID, outlineWidth);

        matInstance.renderQueue = 4000;

        var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            var mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var go = new GameObject(mr.gameObject.name + "_Outline");
            go.transform.SetParent(mr.transform, false);

            var mf2 = go.AddComponent<MeshFilter>();
            mf2.sharedMesh = mf.sharedMesh;

            var r2 = go.AddComponent<MeshRenderer>();

            int count = (mr.sharedMaterials != null && mr.sharedMaterials.Length > 0) ? mr.sharedMaterials.Length : 1;
            var mats = new Material[count];
            for (int i = 0; i < count; i++) mats[i] = matInstance;
            r2.sharedMaterials = mats;

            r2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r2.receiveShadows = false;
            r2.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r2.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            r2.enabled = false;
            outlineRenderers.Add(r2);
        }

        var skinned = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinned)
        {
            if (smr.sharedMesh == null) continue;

            var go = new GameObject(smr.gameObject.name + "_Outline");
            go.transform.SetParent(smr.transform, false);

            var r2 = go.AddComponent<SkinnedMeshRenderer>();
            r2.sharedMesh = smr.sharedMesh;
            r2.bones = smr.bones;
            r2.rootBone = smr.rootBone;
            r2.updateWhenOffscreen = smr.updateWhenOffscreen;

            int count = (smr.sharedMaterials != null && smr.sharedMaterials.Length > 0) ? smr.sharedMaterials.Length : 1;
            var mats = new Material[count];
            for (int i = 0; i < count; i++) mats[i] = matInstance;
            r2.sharedMaterials = mats;

            r2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r2.receiveShadows = false;
            r2.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r2.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            r2.enabled = false;
            outlineRenderers.Add(r2);
        }
    }

    private void ForceOutlineEnabled(bool on)
    {
        foreach (var r in outlineRenderers)
            if (r != null) r.enabled = on;
    }

    public void SetHighlighted(bool on)
    {
        if (isHighlighted == on) return;
        isHighlighted = on;
        ForceOutlineEnabled(on);
    }

    private void OnDisable()
    {
        isHighlighted = false;
        ForceOutlineEnabled(false);
    }
}