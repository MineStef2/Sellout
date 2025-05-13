using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameSceneManager : MonoBehaviour {
	private ObjectTransform[] initialTransforms;

	private void Start() {
		GameObject[] objects = GetSceneGameObjects(typeof(GameObject)) as GameObject[];
		initialTransforms = new ObjectTransform[objects.Length];
		for (int i = 0; i < objects.Length; i++) {
			initialTransforms[i] = new ObjectTransform(objects[i].GetComponent<Transform>(), objects[i].GetInstanceID());
		}
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.F3)) {
			Debug.Log("Reloading!");

			GameObject[] sceneObjects = GetSceneGameObjects(typeof(GameObject)) as GameObject[];
			foreach (ObjectTransform objectTransform in initialTransforms) {
				GameObject gameObject = EditorUtility.InstanceIDToObject(objectTransform.instanceID) as GameObject;
				(EditorUtility.InstanceIDToObject(objectTransform.instanceID) as GameObject).transform.SetTransform(objectTransform.transform);
			}

			foreach (ReloadableMonoBehaviour reloadable in GetSceneGameObjects(typeof(ReloadableMonoBehaviour)) as ReloadableMonoBehaviour[]) {
				reloadable.OnSceneReload();
			}
		}
	}

	private Object[] GetSceneGameObjects(Type type) {
		return Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
	}

	public struct ObjectTransform {
		public Transform transform { get; set; }
		public int instanceID { get; }

		public ObjectTransform(Transform transform, int instanceID) {
			this.transform = transform;
			this.instanceID = instanceID;
		}
	}
}

public static class TransformSetter {

	public static void SetTransform(this Transform target, Transform transform) {
		target.SetPositionAndRotation(transform.position, transform.rotation);
		target.localScale = transform.localScale;
	}
}