using UnityEngine;
using System.Collections;
using System.IO;


/// <summary>
/// Helper routines for input/output
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class IOExtras {

	/**
	 * ReadLine2(StreamReader sr, out string line) is a whitespace stripping, 
	 * blank line skipping, comment ignoring version of readline
	 * 
	 * as input it takes a streamreader and a string output variable
	 * it returns true or false based on whether a line was there to read.
	 * 
	 **/

	// need to strip double spaces

	/// <summary>
	/// The comment char.
	/// </summary>
	public static char [] commentChar = new char[]{'#'};
	/// <summary>
	/// The whitespace char.
	/// </summary>
	public static char [] whitespaceChar = new char[]{' ','\t'};
	/// <summary>
	/// The delim char.
	/// </summary>
	public static char [] delimChar = new char[]{' ','\t',',',';'};

	/// <summary>
	/// Strips double spaces.
	/// </summary>
	/// <returns>The line without double spaces.</returns>
	/// <param name="line">Line.</param>
	public static string stripDoubleSpaces(string line) {
		foreach (char c in delimChar) {
			string pattern = c + " ";
			while (line.IndexOf (pattern)>-1) {
				line = line.Replace (pattern, c.ToString());
			}
			pattern = " "+c;
			while (line.IndexOf (pattern)>-1) {
				line = line.Replace (pattern, c.ToString());
			}
		}

		// fix errors of space with delimit character
		return line;
	}

	/// <summary>
	/// Read a line from streamreader, remove blank lines, double spaces, and comments
	/// </summary>
	/// <returns><c>true</c>, if line2 was  read, <c>false</c> otherwise.</returns>
	/// <param name="sr">Sr.</param>
	/// <param name="line">Line.</param>
	public static bool ReadLine2(StreamReader sr, out string line) {
		if (sr.Peek ()>=0) {
			while (true) {
				string testLine = sr.ReadLine ();
				testLine = testLine.Trim ();
				string[] halves = testLine.Split (commentChar, 2);
				testLine = halves [0].Trim ();
				testLine = stripDoubleSpaces (testLine);
				if (testLine.Equals ("")) {
					if (sr.Peek ()<0) {
						line = null;
						return false;
					}
				} else {
					line = testLine;
					return true;
				}
			}
		} else {
			line = null;
			return false;
		}
	}
		
	/// <summary>
	/// Given a line return as an array of integers
	/// </summary>
	/// <returns>The array.</returns>
	/// <param name="line">Line.</param>
	public static int [] IntArray(string line) {
		string[] words = line.Split (delimChar);
		int[] values = new int[words.Length];
		for (int i = 0; i < words.Length; i++) {
			values [i] = int.Parse (words [i]);
		}
		return values;
	}

	/// <summary>
	/// Given a line, return as an array of floats
	/// </summary>
	/// <returns>The array.</returns>
	/// <param name="line">Line.</param>
	public static float [] FloatArray(string line) {
		string[] words = line.Split (delimChar);
		float[] values = new float[words.Length];
		for (int i = 0; i < words.Length; i++) {
			values [i] = float.Parse (words [i]);
		}
		return values;
	}

	/// <summary>
	/// Given a line, return as an array of strings
	/// </summary>
	/// <returns>The array.</returns>
	/// <param name="line">Line.</param>
	public static string [] StringArray(string line) {
		string[] words = line.Split (delimChar);
		return words;
	}

	/// <summary>
	/// Given a line, break into a first token (key) and remaining line (value)
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="line">Line.</param>
	/// <param name="key">Key.</param>
	/// <param name="val">Value.</param>
	public static int KeyVal(string line, out string key, out string val) {
		string[] words = line.Split (delimChar, 2);
		int n = words.Length;
		if (n > 0)
			key = words [0];
		else
			key = null;
		if (n > 1)
			val = words [1];
		else
			val = null;
		return n;
	}
}
