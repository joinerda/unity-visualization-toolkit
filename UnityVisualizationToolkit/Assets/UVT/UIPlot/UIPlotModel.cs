using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/// <summary>
/// sample model used in sample UIPlot scene
/// SIR model visualized in 
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class UIPlotModel : MonoBehaviour {
	public GameObject uiPlotGo;
	UIPlot uiPlot;

	List<float> time;
	List<float> S;
	List<float> I;
	List<float> R;
	float ir = 0.002f;
	float rr = 0.5f;
	float dt = 0.1f;
	int maxListSize = 1000;
	float maxTime = 100.0f;
	int SPlotIndex, IPlotIndex, RPlotIndex;



	// Use this for initialization
	void Start () {
		uiPlot = uiPlotGo.GetComponent<UIPlot> ();

		// initial conditions
		S = new List<float> ();
		I = new List<float> ();
		R = new List<float> ();
		time = new List<float> ();

		S.Add (999);
		I.Add (1);
		R.Add (0);
		time.Add (0);

		// get plot space ready
		SPlotIndex = uiPlot.addPlot();
		IPlotIndex = uiPlot.addPlot();
		RPlotIndex = uiPlot.addPlot();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			uiPlotGo.SetActive (!uiPlotGo.activeSelf);
		}
		float t = time[time.Count-1];
		if (t < maxTime) {
			// get last iterations value of states
			float s = S [S.Count - 1];
			float i = I [I.Count - 1];
			float r = R [R.Count - 1];

			// calculate change with Euler's method
			float ds = -ir * i * s * dt;
			float di = ir * i * s * dt - rr * i * dt;
			float dr = +rr * i * dt;

			// update list with new state values;
			S.Add (s + ds);
			I.Add (i + di);
			R.Add (r + dr);
			time.Add (t + dt);

			// if lists grow to long drop oldest values
			while (S.Count > maxListSize) {
				S.RemoveAt (0);
			}
			while (I.Count > maxListSize) {
				I.RemoveAt (0);
			}
			while (R.Count > maxListSize) {
				R.RemoveAt (0);
			}
			while (time.Count > maxListSize) {
				time.RemoveAt (0);
			}

			// update plots
			uiPlot.resetPlot (SPlotIndex, time, S);
			uiPlot.resetPlot (IPlotIndex, time, I);
			uiPlot.resetPlot (RPlotIndex, time, R);
			uiPlot.setXLim (time [0], time [time.Count - 1]);
			uiPlot.setYLim (0, 1000);
		}
	}
}