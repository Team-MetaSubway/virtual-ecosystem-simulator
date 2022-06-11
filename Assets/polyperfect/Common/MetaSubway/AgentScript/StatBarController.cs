using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBarController : MonoBehaviour
{
    private Polyperfect.Common.Common_WanderScript animal;

    private Image hpForeground;
    private Image hpBackground;
    private Image hungerForeground;
    private Image hungerBackground;
    private Image staminaForeground;
    private Image staminaBackground;

    public float hpLengthFactor = 1/20f;
    public float hungerLengthFactor = 1/20f;
    public float staminaLengthFactor = 1/20f;

    private float borderLiftFactor;


    float maxHpWidth;
    float maxHpHeight;
    float maxHungerWidth;
    float maxHungerHeight;
    float maxStaminaWidth;
    float maxStaminaHeight;

    private void Start()
    {
        //cam = GameObject.Find("Free Camera").transform;

        hpForeground = transform.Find("hp-foreground").GetComponent<Image>();
        hpBackground = transform.Find("hp-background").GetComponent<Image>();
        hungerForeground = transform.Find("hunger-foreground").GetComponent<Image>();
        hungerBackground = transform.Find("hunger-background").GetComponent<Image>();
        staminaForeground = transform.Find("stamina-foreground").GetComponent<Image>();
        staminaBackground = transform.Find("stamina-background").GetComponent<Image>();

        animal = GetComponentInParent<Polyperfect.Common.Common_WanderScript>();
        InitStatBar();        
    }

    private void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        UpdateStatBar();
    }

    void InitStatBar()
    {
        maxHpWidth = animal.MaxToughness * hpLengthFactor;
        maxHpHeight = hpBackground.rectTransform.sizeDelta.y;
        borderLiftFactor = 5f - maxHpWidth * 0.5f; //캔버스 가로 크기의 반 - 체력 바 크기의 반만큼 왼쪽에서 lift.

        maxHungerWidth = animal.MaxHunger * hungerLengthFactor;
        maxHungerHeight = hungerBackground.rectTransform.sizeDelta.y;

        maxStaminaWidth = animal.MaxStamina * staminaLengthFactor;
        maxStaminaHeight = staminaBackground.rectTransform.sizeDelta.y;


        hpForeground.rectTransform.Translate(new Vector3(borderLiftFactor, 0, 0));
        hpBackground.rectTransform.Translate(new Vector3(borderLiftFactor, 0, 0));
        hungerForeground.rectTransform.Translate(new Vector3(borderLiftFactor, 0, 0));
        hungerBackground.rectTransform.Translate(new Vector3(borderLiftFactor, 0, 0));
        staminaForeground.rectTransform.Translate(new Vector3(borderLiftFactor, 0, 0));
        staminaBackground.rectTransform.Translate(new Vector3(borderLiftFactor, 0, 0));

        hpBackground.rectTransform.sizeDelta = new Vector2(maxHpWidth, maxHpHeight);
        hungerBackground.rectTransform.sizeDelta = new Vector2(maxHungerWidth, maxHungerHeight);
        staminaBackground.rectTransform.sizeDelta = new Vector2(maxStaminaWidth, maxStaminaHeight);

        UpdateStatBar();
    }
    void UpdateStatBar()
    {
        hpForeground.rectTransform.sizeDelta = new Vector2(animal.Toughness * hpLengthFactor, maxHpHeight);
        hungerForeground.rectTransform.sizeDelta = new Vector2(animal.Hunger * hungerLengthFactor, maxHungerHeight);
        staminaForeground.rectTransform.sizeDelta = new Vector2(animal.Stamina * staminaLengthFactor, maxStaminaHeight);
    }
}
