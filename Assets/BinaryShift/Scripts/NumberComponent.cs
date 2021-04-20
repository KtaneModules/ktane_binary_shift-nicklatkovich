using System.Collections;
using UnityEngine;

public class NumberComponent : MonoBehaviour {
	private const float SPEED = 10f;

	public delegate void OnActualizedHandler();
	public event OnActualizedHandler OnActualized;

	public delegate void OnSelectedHandler();
	public event OnSelectedHandler OnSelected;

	public delegate void OnManualyDeselectedHandler();
	public event OnManualyDeselectedHandler OnManualyDeselected;

	public TextMesh Text;
	public KMSelectable Selectable;

	public bool active = true;
	public int target = 0;

	private bool _highlighted = false;
	public bool highlighted {
		get { return _highlighted; }
		private set {
			if (_highlighted == value) return;
			_highlighted = value;
			UpdateColors();
		}
	}

	private bool _selected = false;
	public bool selected {
		get { return _selected; }
		set {
			if (_selected == value) return;
			_selected = value;
			if (selected && OnSelected != null) OnSelected();
			UpdateColors();
		}
	}

	private int _value;
	public int value {
		get { return _value; }
		private set {
			if (_value == value) return;
			_value = value;
			Text.text = value.ToString();
		}
	}

	private float delta = 0f;

	public bool actual { get { return value == target; } }

	private void Start() {
		Selectable.OnHighlight += () => highlighted = true;
		Selectable.OnHighlightEnded += () => highlighted = false;
		Selectable.OnInteract += () => {
			if (!active) return false;
			selected = !selected;
			if (!selected && OnManualyDeselected != null) OnManualyDeselected();
			return false;
		};
		value = Random.Range(0, 100);
		UpdateColors();
	}

	private void Update() {
		if (actual) return;
		delta += Time.deltaTime;
		int steps = Mathf.FloorToInt(delta * SPEED);
		if (steps == 0) return;
		delta -= steps / SPEED;
		if (Mathf.Abs(target - value) <= steps) value = target;
		else value += steps * (int)Mathf.Sign(target - value);
		if (actual && OnActualized != null) OnActualized();
	}

	private IEnumerator UpdateValue() {
		yield return new WaitForSeconds(Random.Range(0f, .1f));
		while (true) {
			if (target < 0) value = Random.Range(0, 100);
			yield return new WaitForSeconds(.1f);
		}
	}

	private void UpdateColors() {
		Text.color = selected ?
			(highlighted ? Color.red : Color.white) :
			(highlighted ? new Color(.5f, 0, 0) : Color.gray);
	}
}
