using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private TMP_Text txt;
    private Button btn;
    // Start is called before the first frame update
    void Start()
    {
        var canvas = GameObject.Find("Canvas");
        var btn = canvas.GetComponentInChildren<Button>();
        if (null != btn)
        {
            this.btn = btn;
            btn.onClick.AddListener(OnClickBtn);
        }

        var txt = canvas.GetComponentInChildren<TMP_Text>();
        if (null != txt)
        {
            this.txt = txt;
            txt.text = "WDNMD2";
        }
    }

    void OnClickBtn()
    {
        Debug.Log("hot fix code");
        txt.text = "after hot fix!!!!!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
