using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPostProcessing : MonoBehaviour {
    
    public Material GravityMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, GravityMaterial);
    }
}
