using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CompaniesPanelControl : MonoBehaviour {
	public readonly string SAVE_FILE = "companies";
	public readonly string INDEX_FILE = "index.txt";
	private GameObject newCompanyPanel;
	private GameObject availableCompanies;
	private Dictionary<string, List<CompanyCodex>> existingCompanies = new();
	private int companyCount = 0;

	private void Start() {
		newCompanyPanel = GameObject.Find("ObjectPool").GetComponent<Variables>().declarations.Get<GameObject>("newCompanyPanel");
		newCompanyPanel.SetActive(false);
		availableCompanies = this.GetComponent<Variables>().declarations.Get("availableCompanies", typeof(GameObject)) as GameObject;

		string indexPath = this.GetIndexPath();
		if (!File.Exists(indexPath)) {
			File.Create(indexPath).Close();
			return;
		}
		using (StreamReader reader = new StreamReader(new FileStream(indexPath, FileMode.Open, FileAccess.Read))) {
			while (!reader.EndOfStream) {
				string companyIndex = reader.ReadLine();
				Debug.Log($"Found company codex: {companyIndex}");
				CompanyCodex companyCodex = JsonUtility.FromJson(companyIndex, typeof(CompanyCodex)) as CompanyCodex;
				if (existingCompanies.ContainsKey(companyCodex.ceoName)) {
					existingCompanies[companyCodex.ceoName].Add(companyCodex);
				} else {
					existingCompanies.Add(companyCodex.ceoName, new List<CompanyCodex> { companyCodex });
				}
				companyCount++;
			}
		}
		this.QueryAvailableCompanies();
	}

	private void Update() {
		if (availableCompanies.activeSelf) {
			Scrollbar scrollbar = availableCompanies.GetComponentInChildren(typeof(Scrollbar)) as Scrollbar;
			if (scrollbar.value >= 0 && scrollbar.value <= 1) {
				scrollbar.value -= Input.mouseScrollDelta.y / 10;
			} else if (scrollbar.value < 0) {
				scrollbar.value = 0;
			} else if (scrollbar.value > 1) {
				scrollbar.value = 1;
			}
		}
	}

	private void UpdateIndex() {
		string indexPath = this.GetIndexPath();
		File.Delete(indexPath);
		using (StreamWriter streamWriter = new StreamWriter(indexPath, false)) {
			IEnumerable<CompanyCodex> allCodexes = existingCompanies.AsEnumerable().SelectMany(pair => pair.Value);
			foreach (CompanyCodex codex in allCodexes) {
				string json = JsonUtility.ToJson(codex);
				streamWriter.WriteLine(json);
			}
		}
	}

	public void QueryAvailableCompanies() {
		VariableDeclarations referencedObjects = this.GetComponent<Variables>().declarations;
		if (existingCompanies.Count == 0) {
			referencedObjects.Get<GameObject>("noCompanies").SetActive(true);
		} else {
			IEnumerable<CompanyCodex> allCodexes = existingCompanies.AsEnumerable().SelectMany(pair => pair.Value);
			GameObject companyEntryInstance = availableCompanies.GetComponent<Variables>().declarations.Get("companyEntryInstance", typeof(GameObject)) as GameObject;
			foreach (CompanyCodex codex in allCodexes) {
				GameObject instance = Instantiate(companyEntryInstance, availableCompanies.transform);
				instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = codex.internalName;
				instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = codex.ceoName;
			}
			referencedObjects.Get<GameObject>("availableCompanies").SetActive(true);
			referencedObjects.Get<GameObject>("noCompanies").SetActive(false);
		}
	}

	public void ImportCompany() {
	}

	public void CheckValues() {
		TMP_InputField marketingNameField = GameObject.Find("MarketingNameField").GetComponent(typeof(TMP_InputField)) as TMP_InputField;

		string marketingName = GameObject.Find("MarketingNameField").GetComponent<TMP_InputField>().text;
		UnityEngine.UI.Button createButton = GameObject.Find("Create").GetComponent(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button;
		GameObject emptyCompanyWarning = newCompanyPanel.GetComponent<Variables>().declarations.Get("emptyCompanyWarning") as GameObject;

		if (!string.IsNullOrEmpty(marketingName)) {
			string internalName = marketingName;
			List<string> companies = Directory.EnumerateDirectories(GameObject.Find("GameManager").GetComponent<GameFilesManager>().GetCompanyDataFolder())
				.Select(text => text.Split('\\').Last()).ToList();          // could break if Path.PathSeparator changes
			GameObject duplicateCompanyWarning = newCompanyPanel.GetComponent<Variables>().declarations.Get("duplicateCompanyWarning") as GameObject;

			if (companies.Contains(internalName)) {
				marketingNameField.textComponent.color = Color.red;
				duplicateCompanyWarning.SetActive(true);
				createButton.interactable = false;
			} else {
				marketingNameField.textComponent.color = Color.black;
				duplicateCompanyWarning.SetActive(false);
				createButton.interactable = true;
			}
			emptyCompanyWarning.SetActive(false);
		} else {
			marketingNameField.textComponent.color = Color.red;
			emptyCompanyWarning.SetActive(true);
			createButton.interactable = false;
		}

		string ceoName = GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text;
		TMP_InputField ceoNameField = GameObject.Find("CEONameField").GetComponent(typeof(TMP_InputField)) as TMP_InputField;
		GameObject existingCeoInfo = newCompanyPanel.GetComponent<Variables>().declarations.Get("existingCeoInfo") as GameObject;
		GameObject emptyCeoWarning = newCompanyPanel.GetComponent<Variables>().declarations.Get("emptyCeoWarning") as GameObject;

		if (!string.IsNullOrEmpty(ceoName)) {
			if (existingCompanies.ContainsKey(ceoName)) {
				ceoNameField.textComponent.color = Color.blue;
				existingCeoInfo.SetActive(true);
			} else {
				ceoNameField.textComponent.color = Color.black;
				existingCeoInfo.SetActive(false);
			}
			emptyCeoWarning.SetActive(false);
		} else {
			existingCeoInfo.SetActive(false);
			ceoNameField.textComponent.color = Color.red;
			emptyCeoWarning.SetActive(true);
		}
	}

	public void CreateNewCompany() {
		string marketingName = GameObject.Find("MarketingNameField").GetComponent<TMP_InputField>().text;
		string ceoName = GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text;
		string internalName = marketingName;

		CompanyCodex codex = new CompanyCodex(companyCount++, ceoName, internalName);
		if (existingCompanies.ContainsKey(ceoName)) {
			existingCompanies[ceoName].Add(codex);
		} else {
			existingCompanies.Add(ceoName, new List<CompanyCodex> { codex });
		}
		this.UpdateIndex();

		CompanyData companyData = new CompanyData(marketingName, ceoName, internalName);
		Company company = new Company(companyData);
		company.Save();
		Debug.Log($"Created new company \"{companyData.internalName}\" to ceo \"{companyData.ceoName}\" with codex {JsonUtility.ToJson(codex)}");
		newCompanyPanel.GetComponent<ReturnProtocol>().BackButtonPressed();
	}

	public void NewCompany() {
		gameObject.SetActive(false);
		newCompanyPanel.SetActive(true);
		this.CheckValues();
	}

	private string GetIndexPath() {
		return GameObject.Find("GameManager").GetComponent<GameFilesManager>().GetCompanyDataFolder() + "/" + INDEX_FILE;
	}

	private void OnEnable() {
		this.QueryAvailableCompanies();
	}
}

public class CompanyCodex {
	public int index;
	public string ceoName;
	public string internalName;

	public CompanyCodex() { }
	public CompanyCodex(int index, string ceoName, string internalName) {
		this.index = index;
		this.ceoName = ceoName;
		this.internalName = internalName;
	}
}