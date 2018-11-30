using UnityEngine;
using System.Collections;

/// <summary>
/// Molecule renderer.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class MoleculeRenderer : MonoBehaviour {

	BallStickObject bso = null;
	public Molecule mol = null;
	float bondWidth = 0.15f;
	float atomSize = 0.6f;

	public void Start () {
	}

	public void Update() {
	}

	/// <summary>
	/// An object to hold a ball-stick model of a molecule
	/// </summary>
	public class BallStickObject {
		public float bondWidth = 0.15f;
		public float ballSize = 0.6f;
		public GameObject parent;
		public GameObject [] balls;
		public GameObject [] sticks;

		/// <summary>
		/// Destroy the object
		/// </summary>
		public void DestroyBSO() {
			foreach( GameObject ball in balls) {
				Destroy(ball);
			}
			foreach (GameObject stick in sticks) {
				Destroy (stick);
			}
		}
	}

	/// <summary>
	/// Determine a color based on an atom's name
	/// </summary>
	/// <returns>The color.</returns>
	/// <param name="atom">Atom.</param>
	public static Color atomColor(Atom atom) {

		// should set up to use CPK coloring

		int atomicNumber = atom.getAtomicNum ();

		switch (atomicNumber) {
		case 1:
			return Color.white;
		case 6:
			return Color.black;
		case 7:
			return Color.blue;
		case 8:
			return Color.red;
		default:
			Debug.Log ("MoleculeRenderer coloring scheme hasn't fully been implemented yet!");

			return Color.gray;
		}

	}

	/// <summary>
	/// Create a gameobject with a MoleculeRenderer component
	/// </summary>
	public static GameObject Create() {
		GameObject go = new GameObject ();
		MoleculeRenderer mr = go.AddComponent<MoleculeRenderer> ();
		return go;
	}

	/// <summary>
	/// Create a gameobject with a MoleculeRenderer component, populated with mol
	/// </summary>
	/// <param name="mol">Mol.</param>
	public static GameObject Create(Molecule mol) {
		GameObject go = new GameObject ();
		MoleculeRenderer mr = go.AddComponent<MoleculeRenderer> ();
		mr.mol = mol;
		mr.Populate ();
		return go;
	}

	/// <summary>
	/// Update variables and redraw, after a change in viewing parameters
	/// </summary>
	/// <param name="width">Width.</param>
	/// <param name="size">Size.</param>
	public void updateParams(float width, float size) {
		bondWidth = width;
		atomSize = size;
		if (bso != null)
			bso.DestroyBSO ();
		if(mol != null)
			Populate ();
	}

	/// <summary>
	/// Based on mol member variable, create the ball-stick object
	/// </summary>
	public void Populate() {
		bso = createBallStickModel (mol);
	}

	/// <summary>
	/// Loads a syb mol file and create a Molecule and BallStickObject
	/// </summary>
	/// <param name="filename">Filename.</param>
	public void LoadFile (string filename) {
		mol = Molecule.readSybMol (Application.dataPath+"/StreamingAssets/LocalFiles/"+filename);
		bso = createBallStickModel (mol);
	}
		
	/// <summary>
	/// Creates the ball stick model from a Molecule
	/// </summary>
	/// <returns>The ball stick model.</returns>
	/// <param name="mol">Mol.</param>
	public BallStickObject createBallStickModel(Molecule mol) {

		BallStickObject bso = new BallStickObject ();
		bso.bondWidth = bondWidth;
		bso.ballSize = atomSize;
		bso.balls = new GameObject[mol.getNumAtoms()];
		bso.sticks = new GameObject[mol.getNumBonds()];

		int numbonds = mol.getNumBonds ();
		bso.parent = transform.gameObject;

		for (int i = 0; i < numbonds; i++) {
			Bond bond = mol.getBond (i);
			int begin = bond.getFromAtom ();
			int end = bond.getToAtom ();
			Vector3 beginV = mol.getAtom (begin).getPosition ();
			Vector3 endV = mol.getAtom (end).getPosition ();
			bso.sticks [i] = CylinderSegment.Create (beginV, endV, bso.bondWidth);
			bso.sticks [i].transform.parent = transform;
			bso.sticks [i].transform.localPosition = 0.5f * (beginV + endV);
		}

		for(int i=0;i<mol.getNumAtoms();i++) {
			Atom atom = mol.getAtom (i);
			bso.balls[i] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			bso.balls[i].transform.localScale = bso.ballSize * Vector3.one;
			//bso.balls[i].transform.localPosition = atom.getPosition();
			Color color = atomColor(atom);
			bso.balls[i].GetComponent<Renderer>().material.color = color;
			bso.balls [i].transform.parent = transform;
			bso.balls [i].transform.localPosition = atom.getPosition ();


		} 
		return bso;
	}





}
