using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CompaniesPanelControl : MonoBehaviour {
	private GameObject newCompanyPanel;
	private GameObject availableCompaniesPanel;
	private List<CompanyCodex> registeredCompanies;
	private List<GameObject> companyEntries = new();
	private float totalOverEntryHeightRatio;                // this should never be 0

	private void Start() {
		availableCompaniesPanel = ObjectPool.GetFromVariables<GameObject>(this.gameObject, "availableCompaniesPanel");
		newCompanyPanel = ObjectPool.Get("newCompanyPanel");
		newCompanyPanel.SetActive(false);
	}

	private void OnEnable() {
		registeredCompanies = CompanySerialization.LoadFromIndex(CompanySerialization.GetDefaultIndexPath());
		this.QueryAvailableCompanies();
	}

	private void Update() {
		if (availableCompaniesPanel.activeSelf) {
			Scrollbar scrollbar = availableCompaniesPanel.GetComponentInChildren(typeof(Scrollbar)) as Scrollbar;
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

	public void QueryAvailableCompanies() {
		companyEntries.ForEach(Destroy);
		companyEntries.Clear();
		if (registeredCompanies == null || registeredCompanies.Count == 0) {
			ObjectPool.GetFromVariables<GameObject>(this.gameObject, "noCompanies").SetActive(true);
			return;
		}

		List<CompanyCodex> allCodexes = CompanySerialization.FlattenedCompanyRegistries();
		GameObject companyEntryInstance = ObjectPool.GetFromVariables<GameObject>(this.gameObject, "companyEntryInstance");
		float yoffset = 0;
		float companyEntryHeight = 0;
		foreach (CompanyCodex codex in allCodexes) {
			if (availableCompaniesPanel == null) {
				availableCompaniesPanel = ObjectPool.GetFromVariables<GameObject>(this.gameObject, "availableCompaniesPanel");
			}
			GameObject entry = Instantiate(companyEntryInstance, availableCompaniesPanel.transform);
			entry.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = CompanySerialization.FromInternalReference(codex.internalReference);
			entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Owned by {codex.owner}";
			entry.transform.position -= new Vector3(0, yoffset, 0);
			RectTransform rectTransform = entry.GetComponent(typeof(RectTransform)) as RectTransform;
			if (companyEntryHeight == 0) {
				// will probably break on other ratios
				companyEntryHeight = rectTransform.rect.size.y * rectTransform.GetComponentInParent<Canvas>().scaleFactor;
			}
			yoffset += companyEntryHeight;
			companyEntries.Add(entry);
		}
		ObjectPool.GetFromVariables<GameObject>(this.gameObject, "availableCompaniesPanel").SetActive(true);
		ObjectPool.GetFromVariables<GameObject>(this.gameObject, "noCompanies").SetActive(false);

		Scrollbar scrollbar = availableCompaniesPanel.GetComponentInChildren(typeof(Scrollbar)) as Scrollbar;
		float totalHeight = yoffset;
		if (totalHeight > 0) {
			totalOverEntryHeightRatio = totalHeight / companyEntryHeight;
			scrollbar.size = companyEntryHeight / totalHeight;
		}
		scrollbar.value = 0;
	}

	public void CheckValues() {
		VariableDeclarations indicators = newCompanyPanel.GetComponent<Variables>().declarations;
		void SetActive(string name, bool active) => indicators.Get<GameObject>(name).SetActive(active);

		TMP_InputField nameField = GameObject.Find("MarketingNameField").GetComponent(typeof(TMP_InputField)) as TMP_InputField;
		bool CheckName() {
			string name = nameField.text;
			string internalName = CompanySerialization.ToInternalReference(name);
			List<string> companyNames = registeredCompanies.Select(codex => codex.internalReference).ToList();
			bool empty = string.IsNullOrEmpty(name);
			bool duplicate = companyNames.Contains(internalName) && !empty;

			nameField.textComponent.color = empty || duplicate ? Color.red : Color.black;
			SetActive("duplicateCompanyWarning", duplicate);
			SetActive("emptyCompanyWarning", empty);
			return !duplicate && !empty;
		}

		TMP_InputField ownerField = GameObject.Find("CEONameField").GetComponent(typeof(TMP_InputField)) as TMP_InputField;
		bool CheckOwner() {
			string owner = GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text;
			List<string> owners = registeredCompanies.AsEnumerable().Select(codex => codex.owner).ToList();
			bool empty = string.IsNullOrEmpty(owner);
			bool exists = owners.Contains(owner) && !empty;

			ownerField.textComponent.color = empty ? Color.red : (exists ? Color.blue : Color.black);
			SetActive("ceoNameInfo", !empty && !exists);
			SetActive("existingCeoInfo", exists);
			SetActive("emptyCeoWarning", empty);
			return exists;
		}

		UnityEngine.UI.Button createButton = GameObject.Find("Create").GetComponent(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button;
		createButton.interactable = CheckOwner() && CheckName();
	}

	public void CreateNewCompany() {
		string name = GameObject.Find("MarketingNameField").GetComponent<TMP_InputField>().text;
		string owner = GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text;
		string indexPath = CompanySerialization.GetDefaultIndexPath();
		(Company company, CompanyCodex codex) = CompanySerialization.CreateNewCompany(name, owner, CompanySerialization.FlattenedCompanyRegistries().Count(), indexPath);

		Debug.Log($"Created new company \"{codex.internalReference}\" owned by \"{codex.owner}\" with codex {JsonUtility.ToJson(codex)}");
		GameObject.Find("MarketingNameField").GetComponent<TMP_InputField>().text = "";
		GameObject.Find("CEONameField").GetComponent<TMP_InputField>().text = "";
		newCompanyPanel.GetComponent<ReturnProtocol>().BackButtonPressed();
	}

	public void ImportCompany() {
		ObjectPool.Get("importCompanyPanel").SetActive(true);
		gameObject.SetActive(false);
	}

	public void NewCompany() {
		gameObject.SetActive(false);
		newCompanyPanel.SetActive(true);
		this.CheckValues();
	}
}