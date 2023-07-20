using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    private AudioSource audioSource;
    private float[] bufferDecrease = new float[8];
    private float[] freqBandHighest = new float[8];
    private float[] samplesLeft = new float[512];
    private float[] samplesRight = new float[512];
    private float[] freqBands = new float[8];
    private float[] bandBuffers = new float[8];
    private float amplitudeHighest;

    public float[] AudioBands = new float[8];
    public float[] AudioBufferBands = new float[8];
    public float Amplitude, AmplitudeBuffer;
    public float AudioProfile;


    // Start is called before the first frame update
    private void Start()
    {
        this.audioSource = this.GetComponent<AudioSource>();
        this.SetAudioProfile(this.AudioProfile);
    }

    // Update is called once per frame
    private void Update()
    {
        this.GetSpectrumAudioSource();
        this.MakeFrequencyBands();
        this.SetBandBuffer();
        this.CreateAudioBands();
        this.GetAmplitude();
    }

    private void GetSpectrumAudioSource()
    {
        this.audioSource.GetSpectrumData(samplesLeft, 0, FFTWindow.Blackman);
        this.audioSource.GetSpectrumData(samplesRight, 1, FFTWindow.Blackman);

    }

    private void MakeFrequencyBands()
    {
        var count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            var sampleCount = (int)Mathf.Pow(2, i) * 2;
            
            if(i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += this.samplesLeft[count] + this.samplesRight[count]* (count + 1);
                count++;
            }

            average /= count;
            this.freqBands[i] = average * 10;
        }
    }

    private void SetBandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if(this.freqBands[i] > this.bandBuffers[i])
            {
                this.bandBuffers[i] = this.freqBands[i];
                this.bufferDecrease[i] = 0.003f;
            }
            else if (this.freqBands[i] < this.bandBuffers[i])
            {
                this.bandBuffers[i] -= this.bufferDecrease[i];
                this.bufferDecrease[i] *= 1.1f;
            }
        }
    }

    private void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if(this.freqBands[i] > this.freqBandHighest[i])
            {
                this.freqBandHighest[i] = this.freqBands[i];
            }
            this.AudioBands[i] = (this.freqBands[i] / this.freqBandHighest[i]);
            this.AudioBufferBands[i] = (this.bandBuffers[i] / this.freqBandHighest[i]);
        }
    }

    private void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer =0;
        
        for (int i = 0; i < 8; i++)
        {
            currentAmplitude += this.AudioBands[i];
            currentAmplitudeBuffer += this.AudioBufferBands[i];
        }
        
        if(currentAmplitude > this.amplitudeHighest)
        {
            this.amplitudeHighest = currentAmplitude;
        }
        
        this.Amplitude = currentAmplitude / this.amplitudeHighest;
        this.AmplitudeBuffer = currentAmplitudeBuffer / this.amplitudeHighest;
    }

    private void SetAudioProfile(float audioProfile)
    {
        for (int i = 0; i < 8; i++)
        {
            this.freqBandHighest[i] = audioProfile;
        }
    }
}
