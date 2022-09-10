using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Impressum : MonoBehaviour {

	private const string text = "QU5HQUJFTiBHRU3DhFNTIMKnIDUgVE1HOgpLZWl3YW4gRG9ueWFnYXJkIFZhamVkCgpLb250YWt0CmtlaXdhbi5kb255YWdhcmRAZ21haWwuY29tCkZheDogKzQ5IDIzMSA5ODE5NDgzMQpTY2jDvHR6ZW5zdHJhw59lIDk3CjQ0MTQ3IERvcnRtdW5k";

	[SerializeField]
	private Text label;

	void Start () {
		byte[] decodedBytes = System.Convert.FromBase64String(text);
		string decodedText = System.Text.Encoding.UTF8.GetString (decodedBytes);
		label.text = decodedText;
	}
}