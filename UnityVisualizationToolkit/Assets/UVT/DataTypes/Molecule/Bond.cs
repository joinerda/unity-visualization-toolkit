using UnityEngine;
using System.Collections;

/// <summary>
/// Bond.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Bond  {
	int fromAtom;
	int toAtom;
	int type;

	/// <summary>
	/// Gets the "from" link in the bond.
	/// </summary>
	/// <returns>The from atom.</returns>
	public int getFromAtom() {
		return fromAtom;
	}

	/// <summary>
	/// Gets the "to" link in the bond.
	/// </summary>
	/// <returns>The to atom.</returns>
	public int getToAtom() {
		return toAtom;
	}

	/// <summary>
	/// Sets the atoms to and from which the bond exists, as well
	/// as the type of bond.
	/// </summary>
	/// <param name="fromAtom">From atom.</param>
	/// <param name="toAtom">To atom.</param>
	/// <param name="type">Type.</param>
	public void setValues(int fromAtom, int toAtom, int type) {
		this.fromAtom = fromAtom;
		this.toAtom = toAtom;
		this.type = type;
	}

}
