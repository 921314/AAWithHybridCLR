using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetToGray : MonoBehaviour
{
    public ComputeShader cs;
    public Texture inTex;
    public RawImage img;

    private void Start()
    {
        var w = inTex.width;
        var h = inTex.height;
        RenderTexture rt = new RenderTexture(w, h, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        img.texture = rt;
        img.SetNativeSize();

        int kernel = cs.FindKernel("TestMain"); //获得compute shader 句柄
        cs.SetTexture(kernel, "inTex", inTex);
        cs.SetTexture(kernel, "outTex", rt);
        cs.Dispatch(kernel, w / 8, h / 8, 1); //后面3个参数，对应宽，高，深度的
    }

}


