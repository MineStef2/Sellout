using System.Globalization;
using System.IO;
using UnityEngine;

public class GameFilesManager : MonoBehaviour {
	[SerializeField] private string dataFolder;
	[SerializeField] private string companyDataFolder;

	private void Start() {
		string path = GetDataFolderPath();
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		path = GetCompanyDataFolderPath();
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
	}

	public string GetDataFolderPath() {
		return Application.dataPath + '/' + dataFolder;
	}

	public string GetCompanyDataFolderPath() {
		return GetDataFolderPath() + '/' + companyDataFolder;
	}
}