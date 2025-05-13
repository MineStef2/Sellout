using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuControl : ReloadableMonoBehaviour {
	private Vector3 speed = Vector3.zero;
	private Vector3 companiesSpeed = Vector3.zero;
	private Color fade = new(0, 0, 0, 0);
	private GameObject companiesPanel;
	private bool isDone = false;
	private Vector3 initialpos1;
	private Vector3 initialpos2;

	private void Start() {
		companiesPanel = GameObject.Find("ObjectPool").GetComponent<Variables>().declarations.Get<GameObject>("companiesPanel");
		companiesPanel.SetActive(false);
		//Color color = companiesPanel.GetComponentInChildren<TextMeshProUGUI>().color;
		//color.a = 0;
		//companiesPanel.GetComponentInChildren<TextMeshProUGUI>().color = color;
		//color = companiesPanel.GetComponent<Image>().color;
		//color.a = 0;
		//companiesPanel.GetComponent<Image>().color = color;
		//companiesPanel.SetActive(false);
		//initialpos1 = gameObject.transform.position;
		//initialpos2 = companiesPanel.transform.position;
	}

	private void Update() {
		//GetComponentInChildren<RawImage>().color -= fade;
		//companiesPanel.GetComponentInChildren<TextMeshProUGUI>().color += fade;
		//companiesPanel.GetComponent<Image>().color += fade;
		//gameObject.transform.position -= speed;
		//companiesPanel.transform.position -= companiesSpeed;
		//if (companiesPanel.GetComponent<Image>().color.a >= 1) {
		//	speed.x = 0;
		//	companiesSpeed.x = 0;
		//	fade.a = 0;
		//}
	}

	public void PlayButtonPressed() {
		//speed.x = 5f;
		//companiesSpeed.x = 10;
		//fade.a = 0.025f;
		companiesPanel.GetComponent<CompaniesPanelControl>().QueryAvailableCompanies();
		companiesPanel.SetActive(true);
		gameObject.SetActive(false);
	}

	public override void OnSceneReload() {
		//Color color = GetComponentInChildren<RawImage>().color;
		//color.a = 1;
		//GetComponentInChildren<RawImage>().color = color;
		//color = companiesPanel.GetComponentInChildren<TextMeshProUGUI>().color;
		//color.a = 1;
		//companiesPanel.GetComponentInChildren<TextMeshProUGUI>().color = color;
		//color = companiesPanel.GetComponent<Image>().color;
		//color.a = 1;
		//companiesPanel.GetComponent<Image>().color = color;
		//gameObject.transform.position = initialpos1;
		//companiesPanel.transform.position = initialpos2;
		//companiesPanel.SetActive(false);
		//gameObject.GetComponentInChildren<Transform>(true).gameObject.SetActive(true);
	}
}