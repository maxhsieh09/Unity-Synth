using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioFilter
{
    float Process(float input);
}

public class DelayLine : IAudioFilter
{
    float[] buffer;
    int readIndex = 0;
    public int length;

    public DelayLine(int maxLength = 1000)
    {
        buffer = new float[maxLength];
    }

    public float Process(float input)
    {
        float output = buffer[readIndex];
        int writeIndex = (readIndex + length) % buffer.Length;
        buffer[writeIndex] = input;
        readIndex = (readIndex + 1) % buffer.Length;
        return output;
    }
}

public class SampleAverageFilter : IAudioFilter
{
    float prevSample = 0f;
    readonly float weight;

    public SampleAverageFilter(float weight)
    {
        this.weight = weight;
    }

    public float Process(float input)
    {
        float average = (prevSample + input) * 0.5f;
        prevSample = input;
        return average * weight + input * (1 - weight);
    }
}

public class AdjustableDampingFilter : IAudioFilter
{
    readonly int numFilters;
    readonly SampleAverageFilter[] filters;

    public AdjustableDampingFilter(int numFilters)
    {
        this.numFilters = numFilters;
        filters = new SampleAverageFilter[numFilters];
        for (int i = 0; i < numFilters; i++)
        {
            filters[i] = new SampleAverageFilter(1f);
        }
    }

    public float Process(float input)
    {
        float output = input;
        for (int i = 0; i < numFilters; i++)
        {
            output = filters[i].Process(output);
        }
        return output;
    }
}

public class SpringMassResonator : IAudioFilter
{
    readonly float omega;
    readonly float damping;
    readonly float dt;
    float x = 0f;
    float v = 0f;

    public enum FilterType { LowPass, BandPass, HighPass };
    readonly FilterType filterType;

    public SpringMassResonator(float frequency, float damping, FilterType filterType, int sampleRate = 44100)
    {
        omega = frequency * Mathf.PI * 2;
        this.damping = damping;
        this.filterType = filterType;
        dt = 1f / sampleRate;
    }

    public float Process(float input)
    {
        float dv = (input - x) * omega * omega - v * damping * omega * 2f;
        v += dv * dt;
        x += v * dt;
        
        return filterType switch
        {
            FilterType.LowPass => x,
            FilterType.BandPass => v * damping * 2f / omega,
            FilterType.HighPass => dv / (omega * omega),
            _ => 0f
        };
    }
}

public class PinkNoiseGenerator
{
    float[] rows = new float[9];
    int i = 0;
    float sum = 0f;
    System.Random random = new();
    SpringMassResonator highPassFilter;
    bool useHighPass = false;
    static readonly int[] trailingZeros = {8,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,5,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,6,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,5,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,7,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,5,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,6,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,5,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,4,0,1,0,2,0,1,0,3,0,1,0,2,0,1,0,};

    public PinkNoiseGenerator(bool useHighPass, float cutoffFrequency)
    {
        this.useHighPass = useHighPass;
        if (useHighPass)
        {
            highPassFilter = new(cutoffFrequency, 0.7f, SpringMassResonator.FilterType.HighPass);
        }
    }

    public float Sample()
    {
        float noise = (float)(random.NextDouble() * 2 - 1);
        int updateIndex = trailingZeros[i];
        sum -= rows[updateIndex];
        rows[updateIndex] = noise;
        sum += noise;
        i = (i + 1) % 256;

        float output = sum + (float)(random.NextDouble() * 2 - 1);
        if (useHighPass)
        {
            output = highPassFilter.Process(output);
        }
        return output / 9f;
    }
}
