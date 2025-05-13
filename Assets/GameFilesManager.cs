using System.Globalization;
using System.IO;
using UnityEngine;

public class GameFilesManager : MonoBehaviour {
	[SerializeField] private string dataFolder;
	[SerializeField] private string companyDataFolder;

	private void Start() {
		string path = GetDataFolder();
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		path = GetCompanyDataFolder();
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
	}

	public string GetDataFolder() {
		return Application.dataPath + '/' + dataFolder;
	}

	public string GetCompanyDataFolder() {
		return GetDataFolder() + '/' + companyDataFolder;
	}
}