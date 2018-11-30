using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Adaptive array.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class AdaptiveArray<T> where T:IEquatable<T>
 {

	public T[] x;
	int nbuffer;
	public int n;

	public int Length() {
		return n;
	}

	public AdaptiveArray() {
		nbuffer = 16;
		x = new T[nbuffer];
		n = 0;
	}

	public AdaptiveArray(int nBig) {
		nbuffer = nBig;
		x = new T[nbuffer];
		n = 0;
	}

	public void growArray(int nBig) {
		if (nBig < nbuffer)
			return;
		nbuffer = nBig;
		T [] temp = new T[nBig];
		for (int i = 0; i < n; i++)
			temp [i] = x [i];
		x = temp;
	}

	public void growArray() {
		if (n < nbuffer)
			return;
		nbuffer *= 2;
		T [] temp = new T[nbuffer];
		for (int i = 0; i < n; i++)
			temp [i] = x [i];
		x = temp;
	}

	public int push(T value) {
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

	public T pop() {
		n--;
		return x [n];
	}

	public T pop(int j) {
		T value = x [j];
		n--;
		for (int i = j; i < n; i++)
			x [i] = x [i + 1];
		return value;
	}

	public T[] pop(int j, int length) {
		T[] value = new T[length];
		for (int i = 0; i < length; i++) {
			value [i] = x [j + i];
		}
		n-=length;
		for (int i = j; i < n; i++)
			x [i] = x [i + length];
		return value;
	}

	public T[] pop(int [] j) {
		T[] value = new T[j.Length];
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

	public void push(int j, T value) {
		growArray ();
		for (int i = n; i > j; j--) {
			x [i] = x [i - 1];
		}
		x [j] = value;
	}

	public T peek(int j) {
		return x [j];
	}

	public T[] toArray() {
		T[] array = new T[n];
		for (int i = 0; i < n; i++) {
			array [i] = x [i];
		}
		return array;
	}

	public void set(int i,T value) {
		if (i < n)
			x [i] = value;
		else {
			growArray (2 * i);
			n = i + 1;
			set (i, value);
		}
	}


	public int contains(T value) {
		for (int i = 0; i < n; i++) {
			if (value.Equals(x [i])) {
				return i;
			}
		}
		return -1;
	}

}
