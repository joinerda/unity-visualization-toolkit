using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adaptive array vector3.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class AdaptiveArrayVector3 {



		public Vector3[] x;
		int nbuffer;
		public int n;

		public int Length() {
			return n;
		}

	public AdaptiveArrayVector3() {
			nbuffer = 16;
			x = new Vector3[nbuffer];
			n = 0;
		}

	public AdaptiveArrayVector3(int nBig) {
			nbuffer = nBig;
			x = new Vector3[nbuffer];
			n = 0;
		}

		public void growArray(int nBig) {
			if (nBig < nbuffer)
				return;
			nbuffer = nBig;
			Vector3 [] temp = new Vector3[nBig];
			for (int i = 0; i < n; i++)
				temp [i] = x [i];
			x = temp;
		}

		public void growArray() {
			if (n < nbuffer)
				return;
			nbuffer *= 2;
			Vector3 [] temp = new Vector3[nbuffer];
			for (int i = 0; i < n; i++)
				temp [i] = x [i];
			x = temp;
		}

		public int push(Vector3 value) {
			growArray ();
			int index = n;
			x [n++] = value;
			return index;
		}

		/*
	public int pushUnique(double value) {
		growArray ();
		int position = contains(value);
		if (position > -1)
			return position;
		int index = n;
		x [n++] = value;
		return index;
	}
	*/

		public Vector3 pop() {
			n--;
			return x [n];
		}

	public Vector3 pop(int j) {
		Vector3 value = x [j];
			n--;
			for (int i = j; i < n; i++)
				x [i] = x [i + 1];
			return value;
		}

		public Vector3[] pop(int j, int length) {
			Vector3[] value = new Vector3[length];
			for (int i = 0; i < length; i++) {
				value [i] = x [j + i];
			}
			n-=length;
			for (int i = j; i < n; i++)
				x [i] = x [i + length];
			return value;
		}

	public Vector3[] pop(int [] j) {
			Vector3[] value = new Vector3[j.Length];
			for (int i = 0; i < j.Length; i++) {
				int stop = --n;
				if (i < j.Length - 1)
					stop = j [i + 1]-(i+1);
				value [i] = x [j [i]-i];
				for (int k = j [i]-i; k < stop; k++) {
					x [k] = x [k + (i + 1)];
				}
			}
		return value;
	}

	public int pushUnique(Vector3 value) {
		//int index = contains (value);
		//if (index > -1)
		//	return index;
		//else
			return push (value);
	}

	public void push(int j, Vector3 value) {
			growArray ();
			for (int i = n; i > j; j--) {
				x [i] = x [i - 1];
			}
			x [j] = value;
		}

		public Vector3 peek(int j) {
			return x [j];
		}

		public Vector3[] toArray() {
			Vector3[] array = new Vector3[n];
			for (int i = 0; i < n; i++) {
				array [i] = x [i];
			}
			return array;
		}

	public void set(int i,Vector3 value) {
			if (i < n)
				x [i] = value;
			else {
				growArray (2 * i);
				n = i + 1;
				set (i, value);
			}
		}


	public int contains(Vector3 value) {
			for (int i = 0; i < n; i++) {
				if (value == x [i]) {
					return i;
				}
			}
		return -1;
	}

}
