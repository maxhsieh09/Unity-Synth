using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StringSynth), typeof(LineRenderer))]
public class StringVisualizer : MonoBehaviour
{
    public float length = 10f;
    public float width = 0.05f;
    public float vibrationAmount = 0.1f;
    public float vibrationFrequency = 10f;
    public Vector3 stopPosition = Vector3.zero;

    float time = 0f;

    StringSynth stringSynth;
    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        stringSynth = GetComponent<StringSynth>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = width;
        lineRenderer.positionCount = 4;
        /*
        lineRenderer.SetPosition(0, transform.position - transform.right * length / 2);
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.SetPosition(2, transform.position);
        lineRenderer.SetPosition(3, transform.position + transform.right * length / 2);
        */
        stopPosition = transform.position - transform.right * length / 2;
    }

    // Update is called once per frame
    void Update()
    {
        float peakY = 0f;
        if (!stringSynth.isMuted)
        {
            peakY = Mathf.Sin(time * vibrationFrequency * 2 * Mathf.PI) * stringSynth.Coefficients[0] * vibrationAmount;
        }
        Vector3 rightPoint = transform.position + transform.right * length / 2;

        lineRenderer.SetPosition(0, transform.position - transform.right * length / 2);
        lineRenderer.SetPosition(1, stopPosition);
        lineRenderer.SetPosition(2, (stopPosition + rightPoint) / 2 + Vector3.up * peakY); // Midpoint between stopPosition and rightPoint
        lineRenderer.SetPosition(3, rightPoint);

        time += Time.deltaTime;
    }
}
