using UnityEngine;

public class ReturnProtocol : MonoBehaviour {
	[SerializeField]
	protected GameObject referencedObject;

	public void BackButtonPressed() {
		gameObject.SetActive(false);
		referencedObject.SetActive(true);
	}
}