using UnityEngine;

public class TargetComponent : MonoBehaviour {
	public TextMesh Text;

	private int _value = 0;
	public int value {
		get { return _value; }
		set {
			if (_value == value) return;
			_value = value;
			UpdateText();
		}
	}

	private void UpdateText() {
		Text.text = value.ToString();
	}
}
