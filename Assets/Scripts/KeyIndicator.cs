using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyIndicator : MonoBehaviour
{
    public KeyCode key;
    public TextMeshProUGUI text;
    public Image backgroundImage;
    public Color baseColor;
    public Color pressedColor;

    // Start is called before the first frame update
    void Start()
    {
        backgroundImage.color = baseColor;
        text.color = pressedColor;
        text.text = key.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            backgroundImage.color = pressedColor;
            text.color = baseColor;
        }
        else if (Input.GetKeyUp(key))
        {
            backgroundImage.color = baseColor;
            text.color = pressedColor;
        }
    }
}
