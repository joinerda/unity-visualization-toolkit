using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for storing a integer "Deck" for the purposes of sampling
/// a subset of data.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Deck  {
	int [] deck;
	int [] stack;
	int nPadded;
	int nStacked;
	int nPaddedStack;
	int n;

	/// <summary>
	/// Initializes a new instance of the <see cref="Deck"/> class.
	/// </summary>
	public Deck() {
		deck = null;
		stack = null;
		n = 0;
		nPadded = 0;
		nStacked = 0;
		nPaddedStack = 0;
	}

	/// <summary>
	/// Allocates memory for a stack of size "n"
	/// </summary>
	/// <param name="n">N.</param>
	public void init(int n) {
		this.n = n;
		this.nPadded = n;
		this.nStacked = 0;
		this.nPaddedStack = n;
		deck = new int[n];
		stack = new int[n];
		for (int i = 0; i < n; i++) {
			deck [i] = i;
			stack [i] = -1;
		}
	}

	/// <summary>
	/// Draws a value at random from the deck.
	/// Places the value on the stack, grows the stack if needed.
	/// </summary>
	public int draw() {
		int drawIndex = Random.Range (0, n);
		int retval = deck [drawIndex];
		for (int i = drawIndex; i < n-1; i++) {
			deck [i] = deck [i + 1];
		}
		n--;
		if (nStacked-1 >= nPaddedStack) {
			int[] temp = new int[nPaddedStack * 2];
			for (int i = 0; i < nStacked; i++) {
				temp [i] = stack [i];
			}
			stack = temp;
			nPaddedStack *= 2;
		}
		stack [nStacked++] = retval;
		return retval;
	}

	/// <summary>
	/// Push a value onto the deck.
	/// </summary>
	/// <param name="value">Value.</param>
	public void push(int value) {
		if (n >= nPadded) {
			int[] temp = new int[nPadded * 2];
			for (int i = 0; i < n; i++) {
				temp [i] = deck [i];
			}
			deck = temp;
			nPadded *= 2;
		}
		deck [n++] = value;
	}

	/// <summary>
	/// Peek at an index in the deck.
	/// </summary>
	/// <param name="index">Index.</param>
	public int peek(int index) {
		return deck [index];
	}

	/// <summary>
	/// Peek at a value in the stack.
	/// </summary>
	/// <returns>The stack.</returns>
	/// <param name="index">Index.</param>
	public int peekStack(int index) {
		return stack [index];
	}

}
