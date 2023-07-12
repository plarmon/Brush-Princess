using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintObj : MonoBehaviour
{
    [SerializeField] private Shader textureBlendShader;
    [SerializeField] private Texture2D noColorTex;
    [SerializeField] private Texture2D colorTex;
    private Material mat;
    private float blendTex;

    // Start is called before the first frame update
    void Start()
    {
        blendTex = 0.0f;
        mat = new Material(textureBlendShader);
        mat.SetTexture("TextureNoColor", noColorTex);
        mat.SetTexture("TextureColor", colorTex);
        mat.SetFloat("BlendTransition", blendTex);
        mat.enableInstancing = true;
        gameObject.GetComponent<Renderer>().material = mat;
    }

    public void StartTransition()
    {
        StartCoroutine(TextureTransition());
    }

    private IEnumerator TextureTransition()
    {
        while (blendTex < 1.0f) {
            blendTex += 1.5f * Time.deltaTime;
            mat.SetFloat("BlendTransition", blendTex);
            yield return null;
        }
        blendTex = 1.0f;
        mat.SetFloat("BlendTransition", blendTex);
    }
}
