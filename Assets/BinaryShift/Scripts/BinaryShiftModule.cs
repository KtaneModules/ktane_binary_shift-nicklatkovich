using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BinaryShiftModule : MonoBehaviour {
	public Transform NumberScreensContainer;
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMSelectable SelfSelectable;
	public NumberComponent NumberComponentPrefab;

	private NumberComponent[] numbers;

	void Start() {
		List<NumberComponent> numbers = new List<NumberComponent>();
		for (int i = -1; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
				NumberComponent number = Instantiate(NumberComponentPrefab);
				number.transform.parent = NumberScreensContainer;
				number.transform.localPosition = new Vector3(0.05f * i, 0f, -0.03f * j);
				number.transform.localScale = Vector3.one;
				number.transform.localRotation = Quaternion.identity;
				number.Selectable.Parent = SelfSelectable;
				number.target = Random.Range(0, 100);
				numbers.Add(number);
			}
		}
		this.numbers = numbers.ToArray();
		SelfSelectable.Children = numbers.Select((n) => n.Selectable).ToArray();
		SelfSelectable.UpdateChildren();
	}
}
