using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// Molecule.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Molecule {
	int nAtoms;
	int nBonds;
	Atom[] atoms;
	Bond[] bonds;

	/// <summary>
	/// Initializes a new instance of the <see cref="Molecule"/> class.
	/// </summary>
	Molecule() {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Molecule"/> class,
	/// provided a number of atoms and bonds for initialization.
	/// </summary>
	/// <param name="nAtoms">N atoms.</param>
	/// <param name="nBonds">N bonds.</param>
	Molecule(int nAtoms, int nBonds) {
		atoms = new Atom[nAtoms];
		this.nAtoms = nAtoms;
		for (int i = 0; i < nAtoms; i++)
			atoms [i] = new Atom ();
		bonds = new Bond[nBonds];
		this.nBonds = nBonds;
		for (int i = 0; i < nBonds; i++)
			bonds [i] = new Bond ();
	}

	/// <summary>
	/// Get the number of bonds
	/// </summary>
	/// <returns> nBonds.</returns>
	public int getNumBonds() {
		return nBonds;
	}

	/// <summary>
	/// Gets the number of atoms.
	/// </summary>
	/// <returns>nAtoms.</returns>
	public int getNumAtoms() {
		return nAtoms;
	}

	/// <summary>
	/// Reads a molecule in Sybyl MOL format
	/// </summary>
	/// <returns>A molecule built using the file</returns>
	/// <param name="filepath">Filepath.</param>
	public static Molecule readSybMol(string filepath) {
		StreamReader sr = new StreamReader (filepath);
		string line;
		char[] sep = new char[]{' '};
		// ignore first three lines of file
		sr.ReadLine ();
		sr.ReadLine ();
		sr.ReadLine ();
		// grab number of atoms, bonds from line 4
		line = sr.ReadLine().Trim();
		line = IOExtras.stripDoubleSpaces (line);
		string[] words = line.Split (sep, 3);
		int natoms = int.Parse (words [0]);
		int nbonds = int.Parse (words [1]);
		Molecule mol = new Molecule (natoms, nbonds);
		for (int i = 0; i < natoms; i++) {
			line = sr.ReadLine ().Trim();
			line = IOExtras.stripDoubleSpaces (line);
			words = line.Split (sep, 5);
			Vector3 pos = new Vector3 (float.Parse(words[0]),float.Parse(words[1]), float.Parse(words[2]));
			string name = words [3];
			mol.atoms [i].setValues (pos, name);
		}
		for (int i = 0; i < nbonds; i++) {
			line = sr.ReadLine ().Trim();
			line = IOExtras.stripDoubleSpaces (line);
			words = line.Split (sep, 14);
			// do something with index, what if indeces are out of order in file?
			mol.bonds [i].setValues (int.Parse (words [0])-1, int.Parse (words [1])-1, int.Parse (words [2]));
		}
		return mol;
	}

	/*
	public void setAtom(string name, Vector3 pos) {
	}

	public void setBond(int fromAtom, int toAtom, int type) {
	}
	*/

	/// <summary>
	/// Gets the ith bond
	/// </summary>
	/// <returns>The bonds.</returns>
	/// <param name="i">The index.</param>
	public Bond getBond(int i) {
		return bonds [i];
	}

	/// <summary>
	/// Gets the ith atom.
	/// </summary>
	/// <returns>The atom.</returns>
	/// <param name="i">The index.</param>
	public Atom getAtom(int i) {
		return atoms [i];
	}


}
