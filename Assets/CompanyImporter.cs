using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CompanyImporter : MonoBehaviour {

	private void OnEnable() {
		GetDropdown().AddOptions(CompanySerialization.UniqueOwners());
		CheckValues();
	}

	public void UpdateCEONameField() {
		TMP_Dropdown dropdown = GetDropdown();
		GetOwnerField().text = dropdown.options[dropdown.value].text;
	}

	public void CheckValues() {
		VariableDeclarations indicators = this.GetComponent<Variables>().declarations;
		void SetActive(string name, bool active) => indicators.Get<GameObject>(name).SetActive(active);

		TMP_InputField pathField = GetPathField();
		bool CheckPath() {
			string path = pathField.text;
			bool empty = string.IsNullOrEmpty(path);
			bool exists = Directory.Exists(path);
			bool isValid = true && exists;                    // 'true' will be changed in future company data impl

			SetActive("emptyPathWarning", empty && !exists);
			SetActive("invalidPathWarning", !isValid && !empty);
			return isValid && !empty;
		}

		TMP_InputField ownerField = GetOwnerField();
		bool CheckOwner() {
			bool empty = string.IsNullOrEmpty(ownerField.text);

			SetActive("emptyOwnerWarning", empty);
			return !empty;
		}

		Button importButton = GameObject.Find("Import").GetComponent(typeof(Button)) as Button;
		bool validPath = CheckPath();
		importButton.interactable = CheckOwner() && validPath;
	}

	public void ImportFromFolder() {
		TMP_InputField pathField = GetPathField();
		string path = EditorUtility.OpenFolderPanel("Import Company", "..", pathField.text);
		pathField.text = path;
	}

	public void Import() {
		string path = GetPathField().text;
		string owner = GetOwnerField().text;
		string indexPath = CompanySerialization.GetDefaultIndexPath();
		string internalReference = CompanySerialization.ToInternalReference(path.Split('/').Last());
		string name = CompanySerialization.FromInternalReference(internalReference);
		Company company = new Company(new CompanyData(name, owner));
		// other data....
		CompanyCodex codex = new CompanyCodex(CompanySerialization.FlattenedCompanyRegistries().Count(), owner, internalReference);
		CompanySerialization.RegisterCodex(codex);
		CompanySerialization.Save(company, codex, indexPath);
		GetOwnerField().text = "";
		GetPathField().text = "";
		this.gameObject.GetComponent<ReturnProtocol>().BackButtonPressed();
	}

	private TMP_Dropdown GetDropdown() => ObjectPool.GetFromVariables<GameObject>(this.gameObject, "ceoNameField").GetComponentInChildren<TMP_Dropdown>();

	private TMP_InputField GetOwnerField() => ObjectPool.GetFromVariables<GameObject>(this.gameObject, "ceoNameField").GetComponent<TMP_InputField>();

	private TMP_InputField GetPathField() => ObjectPool.GetFromVariables<GameObject>(this.gameObject, "pathField").GetComponent<TMP_InputField>();
}