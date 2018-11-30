using UnityEngine;
using System.Collections;

/// <summary>
/// Common math functions
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class MathCommon {

	/// <summary>
	/// linspace
	/// </summary>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	/// <param name="n">N.</param>
	public static float [] linspace(float min, float max, int n) {
		float[] rv = new float[n];
		rv [0] = min;
		rv [n - 1] = max;
		float step = (max - min) / (float)(n - 1);
		for (int i = 0; i < n - 1; i++)
			rv [i] = min + (float)i * step;
		return rv;
	}

	/// <summary>
	/// linspace
	/// </summary>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	/// <param name="n">N.</param>
	public static double [] linspace(double min, double max, int n) {
		double [] retval = new double[n];
		retval [0] = min;
		double space = (max - min) / (double)(n - 1);
		for (int i = 1; i < n - 1; i++) {
			retval [i] = min + i * space;
		}
		retval [n - 1] = max;
		return retval;
	}
}
