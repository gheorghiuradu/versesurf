using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phyllotaxis : MonoBehaviour
{
    public AudioPeer AudioPeer;
    public Color TrailColor;

    public float Degree, Scale;
    public int NumberStart;
    public int StepSize;
    public int MaxIteration;
    public bool Repeat, Invert;

    // Lerping
    public bool UseLerping;
    private bool isLerping;
    private Vector3 startPosition, endPosition;
    private float lerpPositionTimer, lerpPositionSpeed;
    public Vector2 LerpPositionSpeedMinMax;
    public AnimationCurve LerpPosAnimCurve;
    public int LerpPositionBand;

    // Scaling
    public bool UseScaleAnimation, UseScaleCurve;
    public Vector2 ScaleAnimMinMax;
    public AnimationCurve ScaleAnimCurve;
    public float ScaleAnimSpeed;
    public int ScaleBand;
    private float scaleTimer, currentScale;

    private int number;
    private int currentIteration;
    private TrailRenderer trailRenderer;
    private Vector2 phyllotaxisPosition;
    private bool forward;


    // Start is called before the first frame update
    void Awake()
    {
        this.currentScale = this.Scale;
        this.forward = true;
        this.trailRenderer = this.GetComponent<TrailRenderer>();
        //this.trailMaterial = new Material(trailRenderer.material);
        //this.trailMaterial.SetColor("_TintColor", this.TrailColor);
        //this.trailRenderer.material = this.trailMaterial;
        //this.trailRenderer.material.SetColor("_TintColor", this.TrailColor);
        this.trailRenderer.material.color = this.TrailColor;

        this.number = this.NumberStart;
        if (!this.UseLerping)
            this.transform.localPosition = this.CalculatePhyllotaxis(this.Degree, this.currentScale, this.number);

        if (this.UseLerping)
        {
            this.isLerping = true;
            this.SetLerpPositions();
        }
    }

    private void SetLerpPositions()
    {
        this.phyllotaxisPosition = this.CalculatePhyllotaxis(this.Degree, this.currentScale, this.number);
        this.startPosition = this.transform.localPosition;
        this.endPosition = new Vector3(this.phyllotaxisPosition.x, this.phyllotaxisPosition.y, this.transform.localPosition.z);
    }

    private void Update()
    {
        if (this.UseScaleAnimation)
        {
            if (this.UseScaleCurve)
            {
                this.scaleTimer += (this.ScaleAnimSpeed * this.AudioPeer.AudioBands[this.ScaleBand]) * Time.deltaTime;
                if(this.scaleTimer >= 1)
                {
                    this.scaleTimer -= 1;
                }
                this.currentScale = Mathf.Lerp(this.ScaleAnimMinMax.x, this.ScaleAnimMinMax.y, this.ScaleAnimCurve.Evaluate(
                    this.scaleTimer));
            }
            else
            {
                this.currentScale = Mathf.Lerp(this.ScaleAnimMinMax.x, this.ScaleAnimMinMax.y, this.AudioPeer.AudioBands[this.ScaleBand]);
            }
        }

        if (this.UseLerping)
        {
            if (this.isLerping)
            {
                var anim = this.LerpPosAnimCurve.Evaluate(this.AudioPeer.AudioBands[this.LerpPositionBand]);
                this.lerpPositionSpeed = Mathf.Lerp(this.LerpPositionSpeedMinMax.x, this.LerpPositionSpeedMinMax.y, anim);
                this.lerpPositionTimer += Time.deltaTime * this.lerpPositionSpeed;
                this.transform.localPosition = Vector3.Lerp(this.startPosition, this.endPosition, Mathf.Clamp01(this.lerpPositionTimer));
                if (this.lerpPositionTimer >= 1)
                {
                    this.lerpPositionTimer -= 1;
                    if (this.forward)
                    {
                        this.number += this.StepSize;
                        this.currentIteration++;
                    }
                    else
                    {
                        this.number -= this.StepSize;
                        this.currentIteration--;
                    }
                    
                    if(this.currentIteration > 0 && this.currentIteration < this.MaxIteration)
                    {
                        this.SetLerpPositions();
                    }
                    else // current iteration has hit 0 or max iteration
                    {
                        if (this.Repeat)
                        {
                            if (this.Invert)
                            { // Go backwards
                                this.forward = !this.forward;
                                this.SetLerpPositions();
                            }
                            else
                            {
                                // Restart
                                this.number = this.NumberStart;
                                this.currentIteration = 0;
                                this.SetLerpPositions();
                            }
                        }
                        else
                        {
                            this.isLerping = false; // Stop
                        }
                    }
                }
            }
        }
        else
        {
            this.phyllotaxisPosition = this.CalculatePhyllotaxis(this.Degree, this.currentScale, this.number);
            this.transform.localPosition = new Vector3(this.phyllotaxisPosition.x, this.phyllotaxisPosition.y, this.transform.localPosition.z);
            this.number += this.StepSize;
            this.currentIteration++;
        }
    }

    private Vector2 CalculatePhyllotaxis(float degree, float scale, int count)
    {
        double angle = count * (degree * Mathf.Deg2Rad);
        float r = scale * Mathf.Sqrt(count);

        float x = r * (float)System.Math.Cos(angle);
        float y = r * (float)System.Math.Sin(angle);

        return new Vector2(x, y);
    }

}
