using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVisual : MonoBehaviour {

    private const int SAMPLE_SIZE = 1024;

    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    public float colorIntensity;
    public Material colorMaterial;
    public Color maxColor;
    public Color minColor;
    public float colorSmoothSpeed = 10.0f;
    public float colorSensitivity = 0.6f;
    public bool colorRandomizer = false;

    private float colorChangeTime = 0.0f;

    public float cubeMargin = 0.0f;
    public float maxVisualScale = 25.0f;
    public float visualModifier = 50.0f;
    public float smoothSpeed = 10.0f;
    public float keepPercentage = 0.5f; //lowest 0.07

    [SerializeField]
    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float sampleRate;

    private Transform[] visualList;
    private float[] visualScale;
    private int amnVisual = 64;


    private void Start()
    {
        if(source == null)
        {
            source = GetComponent<AudioSource>();
        }
        samples = new float[SAMPLE_SIZE];
        spectrum = new float[SAMPLE_SIZE];
        sampleRate = AudioSettings.outputSampleRate;

        SpawnLine();
    }
    private void SpawnLine()
    {
        visualScale = new float[amnVisual];
        visualList = new Transform[amnVisual];

        for(int i = 0; i < amnVisual; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            go.transform.parent = this.transform;
            go.GetComponent<Renderer>().material = colorMaterial;
            visualList[i] = go.transform;
            visualList[i].localPosition = Vector3.right * (i * cubeMargin);
        }
    }

    private void Update()
    {
        AnalyzeSound();
        UpdateVisual();
        UpdateColor();
    }
    private void UpdateVisual()
    {
        int visualIndex = 0;
        int spectrumIndex = 0;
        int averageSize = (int)((SAMPLE_SIZE * keepPercentage) / amnVisual);

        while(visualIndex < amnVisual)
        {
            int j = 0;
            float sum = 0;
            while(j < averageSize)
            {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }

            float scaleY = sum / averageSize * visualModifier;
            visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;
            if (visualScale[visualIndex] < scaleY)
            {
                visualScale[visualIndex] = scaleY;
            }

            if(visualScale[visualIndex] > maxVisualScale)
            {
                visualScale[visualIndex] = maxVisualScale;
            }

            visualList[visualIndex].localScale = Vector3.one + Vector3.up * visualScale[visualIndex];
            visualIndex++;
        }
    }
    private void UpdateColor()
    {
        colorIntensity -= Time.deltaTime * colorSmoothSpeed;
        if(colorIntensity < dbValue / 50)
        {
            colorIntensity = dbValue / 50;
        }

        colorMaterial.color = Color.Lerp(maxColor, minColor, -colorIntensity + colorSensitivity);

        if (colorRandomizer)
        {
            colorChangeTime += Time.deltaTime * 0.1f;
            if (colorChangeTime >= 0 && colorChangeTime <= 0.3f)
            {
                maxColor = Color.Lerp(Color.red, Color.blue, colorChangeTime + (-colorIntensity + colorSensitivity));
                minColor = Color.Lerp(Color.blue, Color.green, colorChangeTime + (-colorIntensity + colorSensitivity));
                Debug.Log("top red to blue, bottom blue to green, PASS 1");
            }
            if (colorChangeTime > 0.3 && colorChangeTime <= 0.6)
            {
                maxColor = Color.Lerp(Color.blue, Color.green, colorChangeTime - (-colorIntensity + colorSensitivity));
                minColor = Color.Lerp(Color.green, Color.red, colorChangeTime - (-colorIntensity + colorSensitivity));
                Debug.Log("top blue to green, bottom green to red, PASS 2");
            }
            if (colorChangeTime > 0.6 && colorChangeTime <= 0.9)
            {
                maxColor = Color.Lerp(Color.green, Color.red, colorChangeTime - (-colorIntensity + colorSensitivity));
                minColor = Color.Lerp(Color.red, Color.blue, colorChangeTime - (-colorIntensity + colorSensitivity));
                Debug.Log("top green to red, bottom red to blue, PASS 3");
            }
            if (colorChangeTime > 0.9)
            {
                colorChangeTime = 0;
            }
        }
    }
    private void AnalyzeSound()
    {
        source.GetOutputData(samples, 0);

        int i = 0;
        float sum = 0;

        for (i = 0; i < SAMPLE_SIZE; i++)
        {
            sum += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

        source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);


    }
}
