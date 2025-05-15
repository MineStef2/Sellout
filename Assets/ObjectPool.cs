using System;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool {

	public static GameObject Get(string name) {
		return GameObject.Find("ObjectPool").GetComponent<Variables>().declarations.Get<GameObject>(name);
	}

	public static T GetFromVariables<T>(GameObject variablesHolder, string variable) {
		return (T)GetFromVariables(variablesHolder, variable, typeof(T));
	}

	private static object GetFromVariables(GameObject variablesHolder, string variable, Type variableType) {
		return variablesHolder.GetComponent<Variables>().declarations.Get(variable, variableType);
	}
}