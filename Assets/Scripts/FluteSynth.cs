using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluteSynth : MonoBehaviour
{
    [Range(200f, 2000f)]
    public float frequency = 440f;
    [Range(0f, 1f)]
    public float pressureAmount = 0.1f;
    [Range(0f, 1f)]
    public float noiseAmount = 0.2f;

    public float inputDelayFraction = 0.1f;
    [Range(0f, 1f)]
    public float outputGain = 0.5f;
    public float reflectionGain = 0.8f;
    public float jetFeedbackGain = 0.5f;
    public int dampingOrder = 4;
    [Range(0f, 1f)]
    public float jetBias = 0.1f;

    [SerializeField]
    bool isPlaying = false;
    int sampleRate;
    [SerializeField]
    float boreFrontValue = 0f;
    DelayLine boreDelayLine;
    DelayLine inputDelayLine;
    AdjustableDampingFilter dampingFilter;
    SpringMassResonator DCBlcoker = new(60f, 0.7f, SpringMassResonator.FilterType.HighPass);
    System.Random random = new();
    PinkNoiseGenerator noiseGenerator = new(true, 80f);

    // Start is called before the first frame update
    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        boreDelayLine = new DelayLine
        {
            length = (int)(sampleRate / frequency)
        };
        inputDelayLine = new DelayLine
        {
            length = (int)(sampleRate / frequency * inputDelayFraction)
        };
        dampingFilter = new AdjustableDampingFilter(dampingOrder);
    }

    // Update is called once per frame
    void Update()
    {
        isPlaying = Input.GetKey(KeyCode.U);

        boreDelayLine.length = (int)(sampleRate / frequency);
        inputDelayLine.length = (int)(sampleRate / frequency * inputDelayFraction);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            float input = 0f;
            if (isPlaying)
            {
                float unitNoise = (float)(random.NextDouble() * 2.0 - 1.0);
                //float unitNoise = noiseGenerator.Sample();
                input = pressureAmount + unitNoise * noiseAmount;
            }

            input += boreFrontValue * jetFeedbackGain + jetBias;
            input = inputDelayLine.Process(input);

            //float x = Mathf.Clamp(input, -1f, 1f); // Prevent it to go outside the stable region
            //input = x - x * x * x;
            input = System.MathF.Sin(input * 3f) * 0.4f;

            float feedbackValue = dampingFilter.Process(boreFrontValue * reflectionGain + input);

            float output = DCBlcoker.Process(feedbackValue);
            for (int c = 0; c < channels; c++)
            {
                data[i + c] += Mathf.Clamp(output * outputGain, -1f, 1f);
            }

            boreFrontValue = boreDelayLine.Process(feedbackValue);
        }
    }
}
