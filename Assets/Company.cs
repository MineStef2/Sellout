using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Company {
	private CompanyData data;

	public Company(CompanyData data) {
		this.data = data;
	}

	public void Save() {
		string folderPath = GameObject.Find("GameManager").GetComponent<GameFilesManager>().GetCompanyDataFolder() + "/" + data.internalName;
		if (!Directory.Exists(folderPath)) {
			Directory.CreateDirectory(folderPath);
		}
	}
}

public class CompanyData {
	public string marketingName { get; set; }
	public string ceoName { get; set; }
	public string internalName { get; set; }

	public CompanyData(string marketingName, string ceoName, string internalName) {
		this.marketingName = marketingName;
		this.ceoName = ceoName;
		this.internalName = internalName;
	}
}