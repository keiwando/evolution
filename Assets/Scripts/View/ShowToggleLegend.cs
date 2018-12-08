using UnityEngine;
using UnityEngine.UI;

public class ShowToggleLegend : MonoBehaviour {

	[SerializeField] private GameObject label;

	void Start () {
	
		var toggle = GetComponent<Toggle>();
		if (toggle != null) {

			label.SetActive(toggle.isOn);

			toggle.onValueChanged.AddListener(delegate(bool arg0) {
				label.SetActive(arg0);
			});
		}
	}
	
	public void SetLabelVisibility(bool visible) {
		label.SetActive(visible);
	}
}
