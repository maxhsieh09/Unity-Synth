using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StringVisualizer : MonoBehaviour
{
    public float length = 10f;
    public float width = 0.05f;
    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = width;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position - transform.right * length / 2);
        lineRenderer.SetPosition(1, transform.position + transform.right * length / 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
