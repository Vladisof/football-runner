using UnityEngine;

public class HelpRender
{
    public static void SetRendererLayerRecursive(GameObject root, int layer)
    {
        Renderer[] rends = root.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer t in rends)
        {
            t.gameObject.layer = layer;
        }
    }
}
