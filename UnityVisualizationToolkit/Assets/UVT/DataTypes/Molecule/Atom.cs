using UnityEngine;
using System.Collections;

/// <summary>
/// Atom.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Atom  {
	string name;
	Vector3 position;

	/// <summary>
	/// Returns the position of an atom in XYZ coordinates
	/// </summary>
	/// <returns>The position.</returns>
	public Vector3 getPosition() {
		return position;
	}

	/// <summary>
	/// Sets the position of an atom in XYZ coordinates.
	/// </summary>
	/// <param name="pos">Position.</param>
	public void setPosition(Vector3 pos) {
		position = pos;
	}

	/// <summary>
	/// Sets the name of an atom.
	/// </summary>
	/// <param name="name">Name.</param>
	public void setName(string name) {
		this.name = name;
	}

	/// <summary>
	/// Stes the position and name of an atom.
	/// </summary>
	/// <param name="pos">Position.</param>
	/// <param name="name">Name.</param>
	public void setValues(Vector3 pos, string name) {
		position = pos;
		this.name = name;
	}

	/// <summary>
	/// Uses the name of an atom to return an atomic number
	/// </summary>
	/// <returns>The atomic number.</returns>
	public int getAtomicNum() {
		if (name.Equals ("H")) {
			return 1;
		} else if (name.Equals ("C")) {
			return 6;
		} else if (name.Equals ("N")) {
			return 7;
		} else if (name.Equals ("O")) {
			return 8;
		} else {
			Debug.Log ("Atom atomic number hasn't fully been implemented yet!");
			return -1;
		}
	}
}
