using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCanvasOnStart : MonoBehaviour
{
    public Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        canvas.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
