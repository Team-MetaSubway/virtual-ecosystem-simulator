using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBarController : MonoBehaviour
{
    Transform cam;
    public Polyperfect.Common.Common_WanderScript animal;
    public Image hpForeground;
    public Image hpBackground;
    private float lengthFactor = 1/30f;
    private float borderLiftFactor;
    float maxHpWidth;
    float maxHpHeight;
    float maxHungerWidth;
    float maxHungerHeight;


    private void Start()
    {
        cam = GameObject.Find("Free Camera").transform;
        InitStatBar();        
    }

    private void Update()
    {
        transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
        UpdateStatBar();
    }

    void InitStatBar()
    {
        maxHpWidth = animal.MaxToughness * lengthFactor;
        maxHpHeight = hpBackground.rectTransform.sizeDelta.y;
        borderLiftFactor = 2.5f - maxHpWidth * 0.5f;
        hpForeground.rectTransform.Translate(new Vector3(-borderLiftFactor, 0, 0));
        hpBackground.rectTransform.Translate(new Vector3(-borderLiftFactor, 0, 0));
        hpBackground.rectTransform.sizeDelta = new Vector2(maxHpWidth, maxHpHeight);

        UpdateStatBar();
    }
    void UpdateStatBar()
    {
        hpForeground.rectTransform.sizeDelta = new Vector2(animal.Toughness * lengthFactor, maxHpHeight);
    }
}
