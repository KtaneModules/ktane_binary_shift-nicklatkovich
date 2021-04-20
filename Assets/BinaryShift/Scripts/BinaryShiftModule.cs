using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BinaryShiftModule : MonoBehaviour {
	private const int MAX_VALUE = 100;

	private static int TransformationsCount(int from, int to) {
		int diff = to - from;
		int result = 0;
		while (diff > 0) {
			result += 1;
			diff >>= 1;
		}
		return result;
	}

	public Transform NumberScreensContainer;
	public KMAudio Audio;
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMSelectable SelfSelectable;
	public NumberComponent NumberComponentPrefab;
	public TargetComponent Target;
	public StageComponent Stage;

	private bool readyToReset = false;
	private bool activated = false;
	private int target;
	private NumberComponent[] numbers;

	private void Start() {
		List<NumberComponent> numbers = new List<NumberComponent>();
		for (int i = -1; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
				NumberComponent number = Instantiate(NumberComponentPrefab);
				number.transform.parent = NumberScreensContainer;
				number.transform.localPosition = new Vector3(0.05f * i, 0f, -0.03f * j);
				number.transform.localScale = Vector3.one;
				number.transform.localRotation = Quaternion.identity;
				number.Selectable.Parent = SelfSelectable;
				number.OnActualized += OnActualized;
				number.OnSelected += () => Audio.PlaySoundAtTransform("BinaryShiftNumberOn", number.transform);
				number.OnManualyDeselected += () => Audio.PlaySoundAtTransform("BinaryShiftNumberOff", number.transform);
				numbers.Add(number);
			}
		}
		this.numbers = numbers.ToArray();
		List<KMSelectable> children = new List<KMSelectable>(numbers.Select((n) => n.Selectable));
		children.Add(Stage.Selectable);
		Stage.Selectable.OnInteract += () => { OnNextStage(); return false; };
		SelfSelectable.Children = children.ToArray();
		SelfSelectable.UpdateChildren();
		Reset();
		BombModule.OnActivate += () => activated = true;
	}

	private void Update() {
		if (activated) Target.value = Mathf.FloorToInt(BombInfo.GetTime() / 60f) + target;
	}

	private void OnNextStage() {
		if (readyToReset) {
			Audio.PlaySoundAtTransform("BinaryShiftStagePressed", Stage.transform);
			Reset();
			return;
		}
		if (Stage.stage >= Stage.stagesCount) return;
		Audio.PlaySoundAtTransform("BinaryShiftStagePressed", Stage.transform);
		foreach (NumberComponent number in numbers.Where((n) => n.selected)) {
			int newValue = number.target + (1 << Stage.stage);
			if (newValue > MAX_VALUE) newValue = MAX_VALUE;
			number.target = newValue;
			number.selected = false;
		}
		Stage.stage += 1;
		if (Stage.stage >= Stage.stagesCount || numbers.Any((n) => n.target == MAX_VALUE)) foreach (NumberComponent number in numbers) number.active = false;
		if (numbers.All((n) => n.actual)) OnActualized();
	}

	private void OnActualized() {
		if (!numbers.All((n) => n.actual)) return;
		if (Stage.stage >= Stage.stagesCount) {
			if (numbers.All((n) => n.target == target)) BombModule.HandlePass();
			else Strike();
		} else if (numbers.Any((n) => n.target == MAX_VALUE)) Strike();
	}

	private void Strike() {
		foreach (NumberComponent number in numbers) number.selected = false;
		readyToReset = true;
		BombModule.HandleStrike();
	}

	private void Reset() {
		foreach (NumberComponent number in numbers) number.active = true;
		target = 0;
		foreach (NumberComponent number in numbers) {
			number.target = Random.Range(0, MAX_VALUE);
			if (target < number.target) target = number.target;
		}
		target = Random.Range(target, MAX_VALUE);
		Stage.stagesCount = TransformationsCount(numbers.Select((n) => n.target).Min(), target);
		Stage.stage = 0;
		readyToReset = false;
	}
}
