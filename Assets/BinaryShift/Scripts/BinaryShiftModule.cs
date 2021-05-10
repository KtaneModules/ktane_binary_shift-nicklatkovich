using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BinaryShiftModule : MonoBehaviour {
	private const int MAX_VALUE = 100;

	public readonly string TwitchHelpMessage = new[] {
		"\"!{0} press 1s23s 4s ss56s7s\" to press numbers by its position in reading order or to press stage-display",
		"Word \"press\" is optional",
		"Spaces between buttons are optional",
	}.Join(". ");

	private static int moduleIdCounter = 1;

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

	public int stagesCount { get { return Stage.stagesCount; } }

	public HashSet<int> possibleInitialNumbers {
		get {
			int limit = 20;
			HashSet<int> result = new HashSet<int>();
			while (limit-- > 0) {
				if (result.Count > 5) return result;
				result.Add(Random.Range(0, target + 1));
			}
			return result;
		}
	}

	private bool _forceSolved = true;
	public bool forceSolved { get { return _forceSolved; } }

	private bool activated = false;
	private bool readyToReset = false;
	private bool solved = false;
	private int moduleId;
	private int target;
	private int[] initialNumbers = new int[9];
	private NumberComponent[] numbers;
	private HashSet<int>[] selectedNumbers;

	private void Start() {
		moduleId = moduleIdCounter++;
		List<NumberComponent> numbers = new List<NumberComponent>();
		int index = 0;
		for (int z = 0; z < 3; z++) {
			for (int x = -1; x < 2; x++) {
				NumberComponent number = Instantiate(NumberComponentPrefab);
				number.transform.parent = NumberScreensContainer;
				number.transform.localPosition = new Vector3(0.05f * x, 0f, -0.03f * z);
				number.transform.localScale = Vector3.one;
				number.transform.localRotation = Quaternion.identity;
				number.Selectable.Parent = SelfSelectable;
				number.OnActualized += OnActualized;
				number.OnSelected += () => Audio.PlaySoundAtTransform("BinaryShiftNumberOn", number.transform);
				number.OnManualyDeselected += () => Audio.PlaySoundAtTransform("BinaryShiftNumberOff", number.transform);
				number.index = index++;
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
		IEnumerable<int> selected = numbers.Select((n, i) => new { n = n, i = i }).Where((s) => s.n.selected).Select((s) => s.i + 1);
		Debug.LogFormat("[Binary Shift #{0}] Stage #{1} selected numbers: {2}", moduleId, Stage.stage, selected.Join(""));
		selectedNumbers[Stage.stage] = new HashSet<int>();
		foreach (NumberComponent number in numbers.Where((n) => n.selected)) {
			selectedNumbers[Stage.stage].Add(number.index);
			int newValue = number.target + (1 << Stage.stage);
			number.target = newValue;
			number.selected = false;
		}
		Stage.stage += 1;
		if (Stage.stage >= Stage.stagesCount) {
			foreach (NumberComponent number in numbers) number.active = false;
			if (numbers.All(n => n.target == target)) _forceSolved = false;
		}
		if (numbers.All((n) => n.actual)) OnActualized();
	}

	private void OnActualized() {
		if (!numbers.All((n) => n.actual)) return;
		if (Stage.stage >= Stage.stagesCount) {
			var invalidNumber = numbers.Select((n, i) => new { n = n, i = i }).FirstOrDefault((s) => s.n.target != target);
			if (invalidNumber == null) {
				Debug.LogFormat("[Binary Shift #{0}] Solved", moduleId);
				solved = true;
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
				BombModule.HandlePass();
				return;
			}
			Debug.LogFormat("[Binary Shift #{0}] Number #{1} equals {2} (target is {3}). Strike", moduleId, invalidNumber.i + 1, invalidNumber.n.target, target);
			Strike();
			return;
		}
	}

	public int GetInitialNumber(int index) {
		return initialNumbers[index];
	}

	public HashSet<int> GetSelectedNumberPositions(int stageIndex) {
		return selectedNumbers[stageIndex];
	}

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.Trim().ToLower();
		if (command.StartsWith("press ")) command = command.Skip(6).Join("").Trim();
		if (!Regex.IsMatch(command, @"^[1-9s ]+$") || (Stage.stage >= Stage.stagesCount && !readyToReset)) yield break;
		yield return null;
		yield return command.ToCharArray().Where(c => c != ' ').Select(c => c == 's' ? Stage.Selectable : numbers[c - '1'].Selectable).ToArray();
		if (Stage.stage >= Stage.stagesCount && !solved && !readyToReset) {
			bool willSolve = numbers.All(n => n.target == target);
			yield return willSolve ? "solve" : "strike";
		}
	}

	public void TwitchHandleForcedSolve() {
		if (solved) return;
		if (Stage.stage >= Stage.stagesCount && !readyToReset && numbers.All(n => n.target == target)) return;
		Stage.stage = Stage.stagesCount;
		readyToReset = false;
		foreach (NumberComponent n in numbers) n.target = target;
		if (numbers.All(n => n.actual)) OnActualized();
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
			initialNumbers[number.index] = number.target;
			if (target < number.target) target = number.target;
		}
		target = Random.Range(target, MAX_VALUE);
		Debug.LogFormat("[Binary Shift #{0}] Target: {1}", moduleId, target);
		Stage.stagesCount = TransformationsCount(numbers.Select((n) => n.target).Min(), target);
		selectedNumbers = new HashSet<int>[Stage.stagesCount];
		Debug.LogFormat("[Binary Shift #{0}] Stages count: {1}", moduleId, Stage.stagesCount);
		LogSolution();
		Stage.stage = 0;
		readyToReset = false;
	}

	private void LogSolution() {
		Debug.LogFormat("[Binary Shift #{0}] Numbers: {1}", moduleId, numbers.Select((n) => n.target).Join(","));
		Debug.LogFormat("[Binary Shift #{0}] Solution:", moduleId);
		int[] temp = numbers.Select((n) => target - n.target).ToArray();
		for (int stage = 0; stage < Stage.stagesCount; stage++) {
			IEnumerable<int> toSelect = temp.Select((n, i) => new { i = i, n = n }).Where((s) => s.n % 2 == 1).Select((s) => s.i + 1);
			Debug.LogFormat("[Binary Shift #{0}] Stage #{1}: {2}", moduleId, stage, toSelect.Join(""));
			for (int i = 0; i < temp.Length; i++) temp[i] /= 2;
		}
	}
}
