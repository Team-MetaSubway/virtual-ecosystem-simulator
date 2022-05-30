using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public BTNType currentType;
	public Transform buttonScale;
	Vector3 defaultScale;
	public CanvasGroup currGroup;
	public CanvasGroup nextGroup;

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
				CanvasGroupOn(nextGroup);
				CanvasGroupOff(currGroup);
				break;
			case BTNType.Start:
				//Contents라는 오브젝트의 AnimalContents 컴포넌트 찾아서 동물 데이터(개체수)업로드.
				GameObject.Find("Contents").GetComponent<AnimalContents>().UploadAnimalData();
				SceneLoader.LoadSceneHandle("Play");
				break;
			case BTNType.Animal:
				CanvasGroupOn(nextGroup);
				CanvasGroupOff(currGroup);
				break;
			case BTNType.Weather:
				CanvasGroupOn(nextGroup);
				CanvasGroupOff(currGroup);
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
			case BTNType.AnimalBack:
				CanvasGroupOn(nextGroup);
				CanvasGroupOff(currGroup);
				break;
			case BTNType.Quit:
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
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
