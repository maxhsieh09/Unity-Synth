using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFilter
{
    float Process(float input);
}

public class DelayLine : IFilter
{
    float[] buffer;
    int index = 0;

    public DelayLine(int length)
    {
        buffer = new float[length];
    }

    public float Process(float input)
    {
        float output = buffer[index];
        buffer[index] = input;
        index = (index + 1) % buffer.Length;
        return output;
    }
}

public class SampleAverageFilter : IFilter
{
    float prevSample = 0f;
    float weight;

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
