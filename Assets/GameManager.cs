using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {
	private static VariableDeclarations objectPool;

	private void Start() {
		objectPool = GameObject.Find("ObjectPool").GetComponent<Variables>().declarations;
	}

	public static GameObject GetObject(string name) {
		return objectPool.Get(name, typeof(GameObject)) as GameObject;
	}

	public static object GetFromVariables(GameObject variablesHolder, string variable, Type variableType) {
		return variablesHolder.GetComponent<Variables>().declarations.Get(variable, variableType);
	}

	public static T GetFromVariables<T>(GameObject variablesHolder, string variable) {
		return (T)GetFromVariables(variablesHolder, variable, typeof(T));
	}

	public static GameFilesManager GetGameFilesManager() {
		return GameObject.Find("GameManager").GetComponent<GameFilesManager>();
	}
}