using System.Collections;
using UnityEngine;

public class NumberComponent : MonoBehaviour {
	private const float SPEED = 10f;

	public TextMesh Text;
	public Renderer Display;
	public KMSelectable Selectable;

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
		private set {
			if (_selected == value) return;
			_selected = value;
			Display.material.SetColor("_Color", selected ? Color.white : Color.black);
			UpdateColors();
		}
	}

	private int _value = 0;
	public int value {
		get { return _value; }
		private set {
			if (_value == value) return;
			_value = value;
			Text.text = value.ToString();
		}
	}

	private float delta = 0f;

	private void Start() {
		Selectable.OnHighlight += () => highlighted = true;
		Selectable.OnHighlightEnded += () => highlighted = false;
		Selectable.OnInteract += () => { selected = !selected; return false; };
		value = 0;
	}

	private void Update() {
		delta += Time.deltaTime;
		int steps = Mathf.FloorToInt(delta * SPEED);
		if (steps == 0) return;
		delta -= steps / SPEED;
		if (Mathf.Abs(target - value) <= steps) value = target;
		else value += steps * (int)Mathf.Sign(target - value);
	}

	private IEnumerator UpdateValue() {
		yield return new WaitForSeconds(Random.Range(0f, .1f));
		while (true) {
			if (target < 0) value = Random.Range(0, 100);
			yield return new WaitForSeconds(.1f);
		}
	}

	private void UpdateColors() {
		Text.color = highlighted ? Color.red : (selected ? Color.black : Color.white);
	}
}
