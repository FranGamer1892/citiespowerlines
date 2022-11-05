using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonVisible : MonoBehaviour
{
    public GameObject button;
    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            button.SetActive(false);
        }
    }
}
