using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class CompanyImporter : MonoBehaviour {

	private void OnEnable() => GetDropdown().AddOptions(CompanySerialization.UniqueOwners());

	public void UpdateCEONameField() {
		TMP_Dropdown dropdown = GetDropdown();
		GetOwnerField().text = dropdown.options[dropdown.value].text;
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
		if (!Directory.Exists(path)) {
			this.gameObject.GetComponent<ReturnProtocol>().BackButtonPressed();
			return;
		}
		// check other data.....

		string internalReference = CompanySerialization.ToInternalReference(path.Split('/').Last());
		string name = CompanySerialization.FromInternalReference(internalReference);
		Company company = new Company(new CompanyData(name, owner));
		// other data....
		CompanyCodex codex = new CompanyCodex(CompanySerialization.FlattenedCompanyRegistries().Count(), owner, internalReference);
		CompanySerialization.RegisterCodex(codex);
		CompanySerialization.Save(company, codex, indexPath);
		this.gameObject.GetComponent<ReturnProtocol>().BackButtonPressed();
	}

	private TMP_Dropdown GetDropdown() => ObjectPool.GetFromVariables<GameObject>(this.gameObject, "ceoNameField").GetComponentInChildren<TMP_Dropdown>();

	private TMP_InputField GetOwnerField() => ObjectPool.GetFromVariables<GameObject>(this.gameObject, "ceoNameField").GetComponent<TMP_InputField>();

	private TMP_InputField GetPathField() => ObjectPool.GetFromVariables<GameObject>(this.gameObject, "pathField").GetComponent<TMP_InputField>();
}