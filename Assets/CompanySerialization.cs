using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CompanySerialization {
	public static readonly string COMPANIES_FOLDER = "companies";
	private static readonly string DEFAULT_INDEX = "index.orgx";
	private static Dictionary<string, List<CompanyCodex>> existingCompanies = new();

	public static (Company, CompanyCodex) CreateNewCompany(string name, string owner, int index, string indexPath) {
		CompanyCodex codex = new CompanyCodex(index, owner, ToInternalReference(name));
		RegisterCodex(codex);

		Company company = new Company(new CompanyData(name, owner));
		Save(company, codex, indexPath);
		return (company, codex);
	}

	public static void RegisterCodex(CompanyCodex codex) {
		if (existingCompanies.ContainsKey(codex.owner)) {
			existingCompanies[codex.owner].Add(codex);
		} else {
			existingCompanies.Add(codex.owner, new List<CompanyCodex>() { codex });
		}
	}

	public static void UpdateIndex(string indexPath) {
		File.Delete(indexPath);
		using StreamWriter writer = new StreamWriter(new FileStream(indexPath, FileMode.Create, FileAccess.Write));
		List<CompanyCodex> codexes = FlattenedCompanyRegistries();
		codexes.ForEach(codex => writer.WriteLine(JsonUtility.ToJson(codex)));
	}

	public static List<CompanyCodex> LoadFromIndex(string indexPath) {
		if (!File.Exists(indexPath)) {
			File.Create(indexPath);
			Debug.LogError($"Could not find index file '{indexPath}'. Automatically created new file");
			return null;
		} else if (!indexPath.EndsWith(".orgx")) {
			Debug.LogError($"Incorrect extension for index file '{indexPath}', expected .orgx");
			return null;
		}

		List<CompanyCodex> codexes = new();
		existingCompanies.Clear();
		using StreamReader reader = new StreamReader(new FileStream(indexPath, FileMode.Open, FileAccess.Read));
		while (!reader.EndOfStream) {
			string line = reader.ReadLine();
			CompanyCodex codex = JsonUtility.FromJson(line, typeof(CompanyCodex)) as CompanyCodex;
			RegisterCodex(codex);
			codexes.Add(codex);
		}
		return codexes;
	}

	public static void Save(Company company, CompanyCodex codex, string indexData) {
		string folderPath = GameObject.Find("GameManager").GetComponent<GameFilesManager>().GetCompanyDataFolderPath() + "/" + codex.internalReference;
		if (!Directory.Exists(folderPath)) {
			Directory.CreateDirectory(folderPath);
		}
		// to be implemented...

		UpdateIndex(indexData);
	}

	public static List<CompanyCodex> FlattenedCompanyRegistries() {
		return existingCompanies.AsEnumerable().SelectMany(pair => pair.Value).ToList();
	}

	public static List<string> UniqueOwners() {
		if (existingCompanies.Keys.ToList().Count() == 0) {
			LoadFromIndex(GetDefaultIndexPath());
		}
		return existingCompanies.Keys.ToList();
	}

	public static string GetDefaultIndexPath() => GameManager.GameFiles().GetCompanyDataFolderPath() + "/" + DEFAULT_INDEX;

	public static string FromInternalReference(string internalReference) {
		return internalReference;                       // to be implemented
	}

	public static string ToInternalReference(string name) {
		return name;                                    // to be implemented
	}
}

public class CompanyCodex {
	public int index;
	public string owner;
	public string internalReference;

	public CompanyCodex() {
	}

	public CompanyCodex(int index, string owner, string internalReference) {
		this.internalReference = internalReference;
		this.index = index;
		this.owner = owner;
	}
}