using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameFilesManager GameFiles() {
		return GameObject.Find("GameManager").GetComponent<GameFilesManager>();
	}
}