using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Adaptive float array.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class AdaptiveFloatArray  {

	public float[] x;
	int nbuffer;
	int n;

	public AdaptiveFloatArray() {
		nbuffer = 16;
		x = new float[nbuffer];
		n = 0;
	}

	public void growArray() {
		if (n < nbuffer)
			return;
		nbuffer *= 2;
		float [] temp = new float[nbuffer];
		for (int i = 0; i < n; i++)
			temp [i] = x [i];
		x = temp;
	}

	public void push(float value) {
		growArray ();
		x [n++] = value;
	}

	public float pop() {
		n--;
		return x [n];
	}

	public float pop(int j) {
		float value = x [j];
		n--;
		for (int i = j; i < n; i++)
			x [i] = x [i + 1];
		return value;
	}

	public void push(int j, float value) {
		growArray ();
		for (int i = n; i > j; j--) {
			x [i] = x [i - 1];
		}
		x [j] = value;
	}

}
