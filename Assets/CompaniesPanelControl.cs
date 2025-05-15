using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CompaniesPanelControl : MonoBehaviour {
	public readonly string SAVE_FILE = "companies";
	public readonly string INDEX_FILE = "index.orgx";
	public Dictionary<string, List<CompanyCodex>> existingCompanies = new();
	public int companyCount = 0;
	private GameObject newCompanyPanel;
	private GameObject availableCompanies;
	private List<GameObject> companyEntries = new();
	private float totalOverEntryHeightRatio;                // this should never be 0

	private void Start() {
		availableCompanies = GameManager.GetFromVariables<GameObject>(this.gameObject, "availableCompanies");
		newCompanyPanel = GameObject.Find("ObjectPool").GetComponent<Variables>().declarations.Get<GameObject>("newCompanyPanel");
		newCompanyPanel.SetActive(false);

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
			if (scrollbar.value >= 0 && scrollbar.value <= 1) {         // will make the scroll animation happen even if not focused on panel
				scrollbar.value -= Input.mouseScrollDelta.y / totalOverEntryHeightRatio / companyEntries.Count();
			} else if (scrollbar.value < 0) {
				scrollbar.value = 0;
			} else if (scrollbar.value > 1) {
				scrollbar.value = 1;
			}
			if (scrollbar.value > 0 && scrollbar.value < 1) {
				companyEntries.ForEach(entry => entry.transform.position += new Vector3(0, -Input.mouseScrollDelta.y * 10, 0));
			}
		}
	}

	public void UpdateIndex() {
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
		companyEntries.ForEach(Destroy);
		companyEntries.Clear();
		VariableDeclarations referencedObjects = this.GetComponent<Variables>().declarations;
		if (existingCompanies.Count == 0) {
			referencedObjects.Get<GameObject>("noCompanies").SetActive(true);
			return;
		}

		IEnumerable<CompanyCodex> allCodexes = existingCompanies.AsEnumerable().SelectMany(pair => pair.Value);
		GameObject companyEntryInstance = GameManager.GetFromVariables(availableCompanies, "companyEntryInstance", typeof(GameObject)) as GameObject;
		float yoffset = 0;
		float companyEntryHeight = 0;
		foreach (CompanyCodex codex in allCodexes) {
			GameObject instance = Instantiate(companyEntryInstance, availableCompanies.transform);
			instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = codex.internalName;
			instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Guided by {codex.ceoName}, CEO";
			instance.transform.position -= new Vector3(0, yoffset, 0);
			RectTransform rectTransform = instance.GetComponent(typeof(RectTransform)) as RectTransform;
			if (companyEntryHeight == 0) {
				// will probably break on other ratios
				companyEntryHeight = rectTransform.rect.size.y * rectTransform.GetComponentInParent<Canvas>().scaleFactor;
			}
			yoffset += companyEntryHeight;
			companyEntries.Add(instance);
		}
		referencedObjects.Get<GameObject>("availableCompanies").SetActive(true);
		referencedObjects.Get<GameObject>("noCompanies").SetActive(false);

		Scrollbar scrollbar = availableCompanies.GetComponentInChildren(typeof(Scrollbar)) as Scrollbar;
		float totalHeight = yoffset;
		if (totalHeight > 0) {
			totalOverEntryHeightRatio = totalHeight / companyEntryHeight;
			scrollbar.size = companyEntryHeight / totalHeight;
		}
		scrollbar.value = 0;
	}

	public void CheckValues() {
		VariableDeclarations indicators = newCompanyPanel.GetComponent<Variables>().declarations;

		TMP_InputField marketingNameField = GameObject.Find("MarketingNameField").GetComponent(typeof(TMP_InputField)) as TMP_InputField;
		string marketingName = marketingNameField.text;
		UnityEngine.UI.Button createButton = GameObject.Find("Create").GetComponent(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button;
		GameObject emptyCompanyWarning = indicators.Get("emptyCompanyWarning") as GameObject;
		GameObject duplicateCompanyWarning = indicators.Get("duplicateCompanyWarning") as GameObject;

		if (!string.IsNullOrEmpty(marketingName)) {
			string internalName = marketingName;
			List<string> companies = Directory.EnumerateDirectories(GameManager.GetGameFilesManager().GetCompanyDataFolder())
				.Select(text => text.Split('\\').Last()).ToList();          // could break if Path.PathSeparator changes

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
			duplicateCompanyWarning.SetActive(false);
			createButton.interactable = false;
		}

		string ceoName = GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text;
		TMP_InputField ceoNameField = GameObject.Find("CEONameField").GetComponent(typeof(TMP_InputField)) as TMP_InputField;
		GameObject existingCeoInfo = indicators.Get("existingCeoInfo") as GameObject;
		GameObject emptyCeoWarning = indicators.Get("emptyCeoWarning") as GameObject;

		bool clear = true;
		if (!string.IsNullOrEmpty(ceoName)) {
			if (existingCompanies.ContainsKey(ceoName)) {
				ceoNameField.textComponent.color = Color.blue;
				existingCeoInfo.SetActive(true);
				clear = false;
			} else {
				ceoNameField.textComponent.color = Color.black;
				existingCeoInfo.SetActive(false);
			}
			emptyCeoWarning.SetActive(false);
		} else {
			existingCeoInfo.SetActive(false);
			ceoNameField.textComponent.color = Color.red;
			emptyCeoWarning.SetActive(true);
			clear = false;
		}
		indicators.Get<GameObject>("ceoNameInfo").SetActive(clear);
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
		GameObject.Find("MarketingNameField").GetComponent<TMP_InputField>().text = "";
		GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text = "";
		newCompanyPanel.GetComponent<ReturnProtocol>().BackButtonPressed();
	}

	public void ImportCompany() {
		GameObject importCompanyPanel = GameManager.GetObject("importCompanyPanel");
		importCompanyPanel.SetActive(true);
		importCompanyPanel.GetComponent<CompanyImporter>().QueryAvailableCEOs();
		gameObject.SetActive(false);
	}

	public void NewCompany() {
		gameObject.SetActive(false);
		newCompanyPanel.SetActive(true);
		this.CheckValues();
	}

	private string GetIndexPath() {
		return GameManager.GetGameFilesManager().GetCompanyDataFolder() + "/" + INDEX_FILE;
	}

	private void OnEnable() {
		this.QueryAvailableCompanies();
	}
}

public class CompanyCodex {
	public int index;
	public string ceoName;
	public string internalName;

	public CompanyCodex() {
	}

	public CompanyCodex(int index, string ceoName, string internalName) {
		this.index = index;
		this.ceoName = ceoName;
		this.internalName = internalName;
	}
}