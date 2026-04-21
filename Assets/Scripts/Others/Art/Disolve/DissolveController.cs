using System.Linq;
using UnityEngine;
using System.Collections;

public class DissolveController : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private float dissolveDuration = 2f;
    [SerializeField] private float delay = 1f;

    private Material[] materials;

    private void Awake()
    {
        materials = renderers.SelectMany(r => r.materials).ToArray();
    }

    public void StartDissolve()
    {
        renderers = GetComponentsInChildren<Renderer>();
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        yield return new WaitForSeconds(delay);

        float t = 0;

        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float value = t / dissolveDuration;

            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    mat.SetFloat("_DisolveAmount", value);
                }
            }

            yield return null;
        }
    }
}