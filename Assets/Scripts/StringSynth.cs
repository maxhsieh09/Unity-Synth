using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringSynth : MonoBehaviour
{
    [Range(0, 1)]
    public float volume = 0.2f;
    [Range(60, 1000)]
    public float frequency = 330f;

    public int numHarmonics = 60;
    public float inharmonicity = 0.005f;
    public float damping = 0.004f;
    public float lowPassFactor = 0.01f;
    public float pitchShift = 0.005f; // Currently unused
    public float pitchShiftDecay = 18f;
    public float MuteDamping = 200f;
    public float triggerTimeRandomness = 0.005f;
    public float triggerDelay = 0.02f;
    public float pluckPosition = 0.3f;
    public bool isMuted = false;

    int sampleRate;
    float[] coefficients;
    //float[] phases;
    float[] harmonicFreqs;
    float[] sinP, cosP, sinStep, cosStep;
    float pitchFactor = 0f;
    double triggerTime = double.PositiveInfinity;

    // Start is called before the first frame update
    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;

        coefficients = new float[numHarmonics];
        harmonicFreqs = new float[numHarmonics];
        sinP = new float[numHarmonics];
        cosP = new float[numHarmonics];
        sinStep = new float[numHarmonics];
        cosStep = new float[numHarmonics];
        //phases = new float[numHarmonics];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Trigger()
    {
        triggerTime = AudioSettings.dspTime + Random.Range(0, triggerTimeRandomness);
    }

    public void ResetState()
    {
        pitchFactor = pitchShift;

        float m = 1f / Mathf.Clamp(pluckPosition, 0.02f, 0.98f);
        for (int i = 0; i < numHarmonics; i++)
        {
            float currentFreq = frequency * (i + 1);
            float increment = currentFreq * Mathf.PI * 2 / sampleRate;
            harmonicFreqs[i] = currentFreq;
            sinP[i] = 0;
            cosP[i] = 1;
            sinStep[i] = Mathf.Sin(increment);
            cosStep[i] = Mathf.Cos(increment);

            // Calculate Fourier series coefficients of a triangle wave
            coefficients[i] = -(2f * Mathf.Pow(-1f, i + 1) * Mathf.Pow(m, 2f))
                / (Mathf.Pow(i + 1, 2f) * (m - 1) * (Mathf.PI * 2));
            coefficients[i] *= Mathf.Sin((i + 1) * (m - 1) * Mathf.PI / m);
            coefficients[i] *= Mathf.Exp(-Mathf.Pow(currentFreq / lowPassFactor, 2f)); // Gaussian low-pass filter
            // Sound amplitude is proportional to the harmonic number squared (acceleration)
            coefficients[i] *= Mathf.Pow(i + 1, 2f) / 10f;
            coefficients[i] *= Mathf.Sin(pluckPosition * Mathf.PI); // Louder when plucked at the center
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double currentTime = AudioSettings.dspTime;
        float actualDamping = isMuted ? MuteDamping : damping;
        
        for (int i = 0; i < data.Length; i += channels)
        {
            if (currentTime > triggerTime)
            {
                if (currentTime - triggerTime < triggerDelay)
                {
                    actualDamping = MuteDamping;
                }
                else
                {
                    ResetState();
                    triggerTime = double.PositiveInfinity;
                    actualDamping = isMuted ? MuteDamping : damping;
                }
            }

            float value = 0f;
            for (int j = 0; j < numHarmonics; j++)
            {
                value += coefficients[j] * sinP[j];
                //float harmonicFreqs = frequency * (1 + j * (1f + inharmonicity)) * (1 + pitchFactor);
                //phases[j] += harmonicFreqs * Mathf.PI * 2 / sampleRate;
                //phases[j] %= Mathf.PI * 2;
                float newSin = sinP[j] * cosStep[j] + cosP[j] * sinStep[j];
                float newCos = cosP[j] * cosStep[j] - sinP[j] * sinStep[j];
                sinP[j] = newSin;
                cosP[j] = newCos;

                // Damping
                coefficients[j] *= Mathf.Max(1 - actualDamping * harmonicFreqs[j] / sampleRate, 0);
            }
            
            for (int c = 0; c < channels; c++)
            {
                data[i + c] += value * volume;
            }

            pitchFactor *= Mathf.Max(1 - pitchShiftDecay / sampleRate, 0);

            currentTime += 1f / sampleRate;
        }
    }
}
