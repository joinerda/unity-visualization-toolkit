using UnityEngine;
using System.Collections;

/// <summary>
/// Color map class stores colors by a floating point range and
/// interpolates the map to a given value.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class ColorMap {

	Gradient gradient;
	GradientColorKey[] gck;
	GradientAlphaKey[] gak;
	float minGradientScale = 0.0f;
	float maxGradientScale = 5.0f;
	float alpha = 1.0f;

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorMap"/> class.
	/// </summary>
	public ColorMap() {
		gradient = new Gradient ();
		gck = new GradientColorKey[3];
		gak = new GradientAlphaKey[2];
		gck [0].color = Color.red;
		gck [0].time = 0.0f;
		gck [1].color = Color.green;
		gck [1].time = 0.5f;
		gck [2].color = Color.blue;
		gck [2].time = 1.0f;
		gak [0].alpha =1.0f;
		gak [0].time = 0.0f;
		gak [1].alpha = 1.0f;
		gak[1].time=1.0f;
		gradient.SetKeys(gck,gak);
	}

	/// <summary>
	/// Set a list of colors, alpha values, and free variable values.
	/// </summary>
	/// <param name="colors">Colors.</param>
	/// <param name="alpha">Alpha.</param>
	/// <param name="times">Times.</param>
	public void init(Color [] colors, float alpha, float [] times) {
		if(colors.Length != times.Length) {
			Debug.LogAssertion ("ColorMap:ColorMap(colors, times)    array lengths do not match");
		}
		minGradientScale = Mathf.Min (times);
		maxGradientScale = Mathf.Max (times);
		gradient = new Gradient ();
		gck = new GradientColorKey[colors.Length];
		gak = new GradientAlphaKey[colors.Length];
		for (int i = 0; i < colors.Length; i++) {
			gck [i].color = colors [i];
			gck [i].color.a = alpha;
			gak [i].alpha = alpha;
			gck [i].time = (times [i]-minGradientScale)/(maxGradientScale-minGradientScale);
			gak[i].time=(times [i]-minGradientScale)/(maxGradientScale-minGradientScale);
		}
		gradient.mode = GradientMode.Blend;
		gradient.SetKeys(gck,gak);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorMap"/> class.
	/// </summary>
	/// <param name="colors">Colors.</param>
	/// <param name="alpha">Alpha.</param>
	/// <param name="times">Times.</param>
	public ColorMap(Color [] colors, float alpha, float [] times) {
		init (colors, alpha, times);
		
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorMap"/> class.
	/// </summary>
	/// <param name="colors">Colors.</param>
	/// <param name="times">Times.</param>
	public ColorMap(Color [] colors, float [] times) {
		init(colors, 1.0f, times);
	}

	/// <summary>
	/// Eval the specified value.
	/// </summary>
	/// <param name="value">Value.</param>
	public  Color eval(float value) {
		return eval (value, minGradientScale, maxGradientScale);
	}

	/// <summary>
	/// Eval the specified value on user provided limits
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="minGradientScale">Minimum gradient scale.</param>
	/// <param name="maxGradientScale">Max gradient scale.</param>
	public  Color eval(float value, float minGradientScale, float maxGradientScale) {
		float tval = (value - minGradientScale) / (maxGradientScale - minGradientScale);

		//tval = Mathf.Max (tval, 0.00001f);
		//tval = Mathf.Min (tval, 0.99999f);
		return gradient.Evaluate ((float)tval);
	}
}
