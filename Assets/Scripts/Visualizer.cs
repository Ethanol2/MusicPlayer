using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    [Header("References")]
    public Player musicPlayer;
    public Transform visualisedSongFolder;
    public GameObject lineTemplate;
    public BoxCollider2D boxCollider;

    [Header("Settings")]
    public float sequenceWidth = 500f;
    public float squareSpacing = 0.1f;
    public float maxHeight = 40f;
    [Space]
    public int numberOfSamples = 500;
    public int averageSpread = 5;
    public bool positiveAverage = true;

    float[] condensedSamples;
    public float largestSample;

    // Start is called before the first frame update
    void Start()
    {
        float[] samples = new float[musicPlayer.music.channels * musicPlayer.music.samples];
        if (!musicPlayer.music.GetData(samples, 0)) { Debug.Log("Failed to load music samples", this); return; }

        if (positiveAverage)
        {
            condensedSamples = NthSampleSimple_PostiveAverage(samples, numberOfSamples, averageSpread);
        }
        else
        {
            condensedSamples = NthSampleSimple_PostiveSum(samples, numberOfSamples, averageSpread);
        }
        VisualizeSamples();

        boxCollider.offset = new Vector2(sequenceWidth / 2f, 0f);
        boxCollider.size = new Vector2(sequenceWidth, maxHeight);
    }

    float[] NthSampleSimple_PostiveAverage(float[] rawSamples, int sampleNum, int spread)
    {
        List<float> simpleSamples = new List<float>();
        int iSize = rawSamples.Length / sampleNum;
        float offset = 0f;
        largestSample = 0f;

        if (rawSamples.Length % (float)spread > 0f)
        {
            offset = iSize;
        }

        for (int k = 0; k < rawSamples.Length - offset; k += iSize)
        {
            float a = 0f;
            for (int l = 0; l < spread; l++)
            {
                a += Mathf.Abs(rawSamples[k + l]);
            }

            a /= (float)spread;
            largestSample = a > largestSample ? a : largestSample;
            simpleSamples.Add(a);
        }

        if (simpleSamples.Count < sampleNum)
        {
            for (int k = 0; k < sampleNum - simpleSamples.Count; k++)
            {
                simpleSamples.Add(0f);
            }
        }

        return simpleSamples.ToArray();
    }
    float[] NthSampleSimple_PostiveSum(float[] rawSamples, int sampleNum, int spread)
    {
        List<float> simpleSamples = new List<float>();
        int iSize = rawSamples.Length / sampleNum;
        float offset = 0f;

        if (rawSamples.Length % (float)spread > 0f)
        {
            offset = iSize;
        }

        for (int k = 0; k < rawSamples.Length - offset; k += iSize)
        {
            float a = 0f;
            for (int l = 0; l < spread; l++)
            {
                a += rawSamples[k + l];
            }

            a /= (float)spread;
            largestSample = a > largestSample ? a : largestSample;
            simpleSamples.Add(Mathf.Abs(a));
        }

        if (simpleSamples.Count < sampleNum)
        {
            for (int k = 0; k < sampleNum - simpleSamples.Count; k++)
            {
                simpleSamples.Add(0f);
            }
        }

        //Debug.Log(simpleSamples.IndexOf(largestSample));

        return simpleSamples.ToArray();
    }
    void VisualizeSamples()
    {
        // Desmos Formula \frac{h\cdot\sqrt{x\left(\frac{h}{w}\right)^{1.95}}}{\sqrt{w\left(\frac{h}{w}\right)^{1.95}}}
        // Plain Text Formula y = (h * sqrt(x*(height limit / largest sample)^1.95)) / (sqrt(w * (hl / ls)^1.95)

        float xSpacing = sequenceWidth / numberOfSamples;
        float xScale = xSpacing * (1f - squareSpacing);

        float yAdjust1 = Mathf.Pow((maxHeight) / largestSample, 1.95f);
        float yAdjust2 = (maxHeight) / Mathf.Sqrt(yAdjust1 * largestSample);

        float i = 0;

        lineTemplate.SetActive(true);

        foreach (float s in condensedSamples)
        {
            GameObject sample = Instantiate(lineTemplate, visualisedSongFolder);
            sample.name = $"Sample Line ({i})";
            i++;
            sample.transform.localPosition = new Vector3(xSpacing * i, 0f, 0f);
            sample.transform.localScale = new Vector3(
                xScale,
                yAdjust2 * Mathf.Sqrt(yAdjust1 * s) + xScale,
                1f
                );
        }

        lineTemplate.SetActive(false);
        return;
    }
}