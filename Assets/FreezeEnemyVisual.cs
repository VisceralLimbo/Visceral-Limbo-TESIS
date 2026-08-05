using UnityEngine;

public class FreezeEnemyVisual : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;

    private static readonly int EffectActiveID = Shader.PropertyToID("_EffectActive");

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetFrozen(bool frozen)
    {
        float value = frozen ? 1f : 0f;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(EffectActiveID, value);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}