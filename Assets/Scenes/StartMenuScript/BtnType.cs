using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public BTNType currentType;
	public Transform buttonScale;
	Vector3 defaultScale;
	public CanvasGroup startGroup;
	public CanvasGroup mainGroup;
	public CanvasGroup optionGroup;
	private void Start()
	{
		defaultScale = buttonScale.localScale;
	}
	bool isSound;
	public void OnBtnClick()
	{
		switch (currentType)
		{
			case BTNType.Press:
				CanvasGroupOn(mainGroup);
				CanvasGroupOff(startGroup);
				break;
			case BTNType.New:
				SceneLoader.LoadSceneHandle("Play", 0);
				break;
			case BTNType.Continue:
				SceneLoader.LoadSceneHandle("Play", 1);
				break;
			case BTNType.Option:
				CanvasGroupOn(optionGroup);
				CanvasGroupOff(mainGroup);
				break;
			case BTNType.Sound:
				if (isSound)
				{
					Debug.Log("사운드 Off");
				}
				else
				{
					Debug.Log("사운드 ON");
				}
				isSound = !isSound;
				break;
			case BTNType.Back:
				CanvasGroupOn(mainGroup);
				CanvasGroupOff(optionGroup);
				break;
			case BTNType.Quit:
				Application.Quit();
				Debug.Log("종료");
				break;
		}
	}
	public void CanvasGroupOn(CanvasGroup cg)
	{
		cg.alpha = 1;
		cg.interactable = true;
		cg.blocksRaycasts = true;
	}
	public void CanvasGroupOff(CanvasGroup cg)
	{
		cg.alpha = 0;
		cg.interactable = false;
		cg.blocksRaycasts = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		buttonScale.localScale = defaultScale * 1.2f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		buttonScale.localScale = defaultScale;
	}
}
