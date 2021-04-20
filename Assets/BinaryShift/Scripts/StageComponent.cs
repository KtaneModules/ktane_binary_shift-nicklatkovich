using UnityEngine;

public class StageComponent : MonoBehaviour {
	public TextMesh Text;
	public KMSelectable Selectable;

	private int _stage = 0;
	public int stage {
		get { return _stage; }
		set {
			if (_stage == value) return;
			_stage = value;
			UpdateText();
		}
	}

	private int _stagesCount;
	public int stagesCount {
		get { return _stagesCount; }
		set {
			if (_stagesCount == value) return;
			_stagesCount = value;
			UpdateText();
		}
	}

	private bool _highlighted;
	public bool highlighted {
		get { return _highlighted; }
		private set {
			if (_highlighted == value) return;
			_highlighted = value;
			UpdateColors();
		}
	}

	private void Start() {
		Selectable.OnHighlight += () => highlighted = true;
		Selectable.OnHighlightEnded += () => highlighted = false;
	}

	private void UpdateText() {
		Text.text = string.Format("{0}/{1}", stage, stagesCount);
	}

	private void UpdateColors() {
		Text.color = highlighted ? Color.red : Color.white;
	}
}
