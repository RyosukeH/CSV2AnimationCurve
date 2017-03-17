using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurveCreator : MonoBehaviour {

	[SerializeField]
	CSVReader_sample csvReader;

	public AnimationCurve curve;

	// Use this for initialization
	void Start () {
		csvReader.ReadCSVFile ();
		curve = csvReader.CreateLinearAnimationCurve ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
