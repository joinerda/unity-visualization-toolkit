using UnityEngine;
using System.Collections;


/// <summary>
/// example of isocontour creation
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class IsoContourTest : MonoBehaviour {

	Mesh m;
	Material mat;

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
		int n = 35;
		int nx = n;
		int ny = n;
		int nz = n;
		float min = -3.0f;
		float max = 3.0f;
		float[] x = linspace (min, max, nx);
		float[] y = linspace (min, max, ny);
		float[] z = linspace (min, max, nz);
		float[,,] funky = new float[nx, ny, nz];
		float[,,] expo = new float[nx, ny, nz];
		float[,,] simple = new float[nx, ny, nz];
		float[,,] ygrid = new float[nx, ny, nz];
		float[,,] siney = new float[nx, ny, nz];
		for (int i = 0; i < nx; i++)
			for (int j = 0; j < ny; j++)
				for (int k = 0; k < nz; k++) {
					simple [i, j, k] = x [i] + y [j] + z [k];
					funky [i, j, k] = x [i] * x [i] - y [j] * y [j] + Mathf.Sin (3.0f * z [k]) * z [k] - 0.25f;
					expo [i, j, k] = Mathf.Exp(- (x [i] * x [i] + y [j] * y [j] + z [k] * z [k]));
					ygrid [i, j, k] = y [j];
					siney [i, j, k] = Mathf.Sin( (x [i] * x [i] + y [j] * y [j] + z [k] * z [k]));

				}

		//ColorMap cm = new ColorMap(new Color[] {Color.red,Color.blue}, new float[]{min,max});
		ColorMap cm = new ColorMap(new Color[] {Color.red,Color.green,Color.blue}, new float[]{0,0.5f,1});
		//ColorMap cm = new ColorMap(new Color[] {Color.blue}, new float[]{0});
		//ColorMap cm = new ColorMap();
		GameObject ic = IsoContour.CreateGameObject(transform.position,Vector3.one,transform);
		ic.GetComponent<IsoContour>().buildMesh (x, y, z, funky, simple, 0.0f, cm);
		//m.Optimize ();
	
	}
	
	// Update is called once per frame
	void Update () {
		//mat = new Material (Shader.Find ("AlphaVertexUnlit"));
		//Matrix4x4 scale = Matrix4x4.TRS (transform.position, transform.rotation, transform.localScale);
		//Graphics.DrawMesh (m, scale, mat, 0);
	}
}
