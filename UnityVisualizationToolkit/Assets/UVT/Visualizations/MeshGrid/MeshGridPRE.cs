using UnityEngine;
using System.Collections;


/// <summary>
/// script on prefab object for using MeshGrid
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class MeshGridPRE : MonoBehaviour {

	public string filename = "StructuredInput.txt";

	public string xVar = "x";
	public string yVar = "y";
	public string zVar = "z";
	public string cVar = "z";
	public float cMin = 0.0f;
	public float cMax = 1.0f;

	// Use this for initialization
	void Start () {

		StructuredData theData = (StructuredData)ScriptableObject.CreateInstance ("StructuredData");
		theData.readWOD (Application.dataPath+"/StreamingAssets/LocalFiles/"+filename);

	

		/*int nx = 30;
		int ny = 30;
		float[] x = MathCommon.linspace(-2.0f,2.0f,nx);
		float[] y = MathCommon.linspace(-2.0f,2.0f,ny);

		float[] vals = new float[nx*ny];
		for (int i = 0; i < nx; i++) {
			for (int j = 0; j < ny; j++) {
				vals [ny * i + j] = x [i] *x[i]+ y [j]*y[j];
			}
		}
		theData.addDimension ("x", x);
		theData.addDimension ("y", y);
		theData.addVariable ("z", vals);
		*/


		GameObject mgGO = MeshGrid.CreateGameObject (transform.localPosition, Vector3.one, transform);
		ColorMap cm = new ColorMap ();
		cm.init (new Color[]{ Color.red, Color.green, Color.blue }, 1.0f,
			new float[]{ 0.0f, 0.5f, 1.0f });
		mgGO.GetComponent<MeshGrid> ().setColorMap (cm);
		mgGO.GetComponent<MeshGrid> ().createGrid (theData, xVar, yVar, zVar, cVar);

	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
