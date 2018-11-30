using UnityEngine;
using System.Collections;

/// <summary>
/// Script on prefab object for creating isocontours
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class IsoContourPRE : MonoBehaviour {

	Mesh m;
	Material mat;

	public float isoLevel = 50.0f;
	public string filename = "TestFarnumVisual.txt";
	public float cmin = 0.0f;
	public float cmax = 5.0f;
	public string xname = "x";
	public string yname = "y";
	public string zname = "z";
	public string vname = "R";
	public string cname = "R";

	public static float [] linspace(float min, float max, int n) {
		float[] rv = new float[n];
		rv [0] = min;
		rv [n - 1] = max;
		float step = (max - min) / (float)(n - 1);
		for (int i = 0; i < n - 1; i++)
			rv [i] = min + (float)i * step;
		return rv;
	}

	// Use this for initialization
	void Start () {
		

		StructuredData theData = (StructuredData)ScriptableObject.CreateInstance ("StructuredData");
		theData.readWOD (Application.dataPath+"/StreamingAssets/LocalFiles/"+filename);
		float [] x = theData.getDimension(xname);
		float [] y = theData.getDimension(yname);
		float [] z = theData.getDimension(zname);
		float [,,] vals = theData.getValues3D(vname);
		float [,,] cols = theData.getValues3D(cname);
		/*
		 * for (int i = 0; i < z.Length; i++) {
			for (int j = 0; j < y.Length; j++) {
				for (int k = 0; k < x.Length; k++) {
					cols [i, j, k] = y [j];
				}
			}
		}
		*/


		/*
		//SMOOTHING PASS
		for (int i = 1; i < z.Length-1; i++) {
			for (int j = 1; j < y.Length-1; j++) {
				for (int k = 1; k < x.Length-1; k++) {
					R [i, j, k] = 1.0f / 7.0f * (R [i, j, k] + R [i + 1, j, k] + R [i - 1, j, k]
						+R[i,j+1,k]+R[i,j-1,k]
						+R[i,j,k+1]+R[i,j,k-1]);
				}
			}
		}
		*/



		//ColorMap cm = new ColorMap(new Color[] {Color.red,Color.blue}, new float[]{min,max});
		ColorMap cm = new ColorMap(new Color[] {Color.red,Color.green,Color.blue}, 0.1f,
			new float[]{cmin,0.5f*(cmin+cmax),cmax});
		//ColorMap cm = new ColorMap(new Color[] {Color.blue}, new float[]{0});
		//ColorMap cm = new ColorMap();
		GameObject ic = IsoContour.CreateGameObject(transform.position,Vector3.one,transform);
		ic.GetComponent<IsoContour>().buildMesh (z, y, x, vals,vals, isoLevel, cm);
		//m.Optimize ();
	
	}
	
	// Update is called once per frame
	void Update () {
		//mat = new Material (Shader.Find ("AlphaVertexUnlit"));
		//Matrix4x4 scale = Matrix4x4.TRS (transform.position, transform.rotation, transform.localScale);
		//Graphics.DrawMesh (m, scale, mat, 0);
	}
}
