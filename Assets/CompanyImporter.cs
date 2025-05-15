using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CompanyImporter : MonoBehaviour {
	public GameObject pathField;
	public GameObject ceoNameField;
	public GameObject dropdown;

	public void QueryAvailableCEOs() {
		TMP_Dropdown dropdown = this.dropdown.GetComponentInChildren(typeof(TMP_Dropdown)) as TMP_Dropdown;
		dropdown.AddOptions(GameManager.GetObject("companiesPanel").GetComponent<CompaniesPanelControl>().existingCompanies.Keys.ToListPooled());
	}

	public void UpdateCEONameField() {
		TMP_Dropdown dropdown = this.dropdown.GetComponentInChildren(typeof(TMP_Dropdown)) as TMP_Dropdown;
		ceoNameField.GetComponent<TMP_InputField>().text = dropdown.options[dropdown.value].text;
	}

	public void ImportFromFolder() {
		TMP_InputField pathField = this.pathField.GetComponent<TMP_InputField>();
		string path = EditorUtility.OpenFolderPanel("Import Company", "..", pathField.text);
		pathField.text = path;
	}

	public void Import() {
		string path = this.pathField.GetComponent<TMP_InputField>().text;
		string ceoName = this.ceoNameField.GetComponent<TMP_InputField>().text;
		if (!Directory.Exists(path)) {
			this.gameObject.GetComponent<ReturnProtocol>().BackButtonPressed();
			return;
		}

		string marketingName = path.Split('/').Last();
		new Company(new CompanyData(marketingName, ceoName, marketingName)).Save();
		CompaniesPanelControl companiesPanelControl = GameManager.GetObject("companiesPanel").GetComponent<CompaniesPanelControl>();
		Dictionary<string, List<CompanyCodex>> companies = companiesPanelControl.existingCompanies;
		CompanyCodex companyCodex = new CompanyCodex(companiesPanelControl.companyCount++, ceoName, marketingName);
		if (companies.ContainsKey(ceoName) && !string.IsNullOrEmpty(ceoName)) {
			companies[ceoName].Add(companyCodex);
		} else {
			companies.Add(ceoName, new List<CompanyCodex>() { companyCodex });
		}
		companiesPanelControl.UpdateIndex();
	}
}