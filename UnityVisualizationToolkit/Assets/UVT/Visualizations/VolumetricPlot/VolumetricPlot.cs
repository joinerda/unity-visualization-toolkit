using UnityEngine;
using System.Collections;

/// <summary>
/// Volumetric plot.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class VolumetricPlot : MonoBehaviour {

	public Texture3D tex;
	public Texture3D tex2;
	public StructuredData theData;
	public string xName = "x";
	public string yName = "y";
	public string zName = "z";
	public string depName = null;
	public string tempName = null;
	public string colName = null;
	public bool fillTexture = false;
	public float[] depLimits = { 0.0f, 1.0f };
	public float[] colorScale = { 0.0f, 1.0f };
	public float[] tempScale = { 0.0f, 1.0f };
	public Color color = Color.white;
	int delayUpdate = 0;
	int delayMax = 100;

	float []gx;
	float []gy;
	float []gz;
	int nx ;
	int ny;
	int nz ;
	float[,,] depVals;
	float[,,] colVals;
	float[,,] tempVals;
	ColorMap cm = null;
	Color [] cols;
	Color [] cols2;
	int tnx;
	int tny;
	int tnz;
	float emissivity = 8.0f;
	float opacity = 1.0f;
	float shaderSize;

	public string filename = null;

	public void setColorMap(ColorMap cm) {
		this.cm = cm;
	}

	public void setData(StructuredData theData) {
		this.theData = theData;
		//makeTexture ();
	}

	public static GameObject CreateGameObject() {
		GameObject retval = GameObject.CreatePrimitive (PrimitiveType.Cube);
		VolumetricPlot vp = retval.AddComponent<VolumetricPlot> ();
		Material mat = new Material (Resources.Load ("Material/Volumetric", typeof(Material)) as Material);
		retval.GetComponent<Renderer> ().material = mat;
		return retval;
	}

	public static GameObject CreateGameObject(StructuredData theData) {
		GameObject retval = GameObject.CreatePrimitive (PrimitiveType.Cube);
		VolumetricPlot vp = retval.AddComponent<VolumetricPlot> ();
		Material mat = Resources.Load ("Material/Volumetric", typeof(Material)) as Material;
		retval.GetComponent<Renderer> ().material = mat;
		vp.setData (theData);
		return retval;
	}

	public static GameObject CreateGameObjectFromWOD(string filepath) {
		GameObject retval = GameObject.CreatePrimitive (PrimitiveType.Cube);
		VolumetricPlot vp = retval.AddComponent<VolumetricPlot> ();
		Material mat = Resources.Load ("Material/Volumetric", typeof(Material)) as Material;
		retval.GetComponent<Renderer> ().material = mat;
		StructuredData thisData = (StructuredData)ScriptableObject.CreateInstance ("StructuredData");
		thisData.readWOD (filepath);
		retval.GetComponent<VolumetricPlot>().setData (thisData);
		return retval;
	}


	public void makeTexture() {

		gx = theData.getDimension (xName);
		gy = theData.getDimension (yName);
		gz = theData.getDimension (zName);
		nx = gx.Length;
		ny = gy.Length;
		nz = gz.Length;

		if(cm==null)
		cm = new ColorMap(new Color[] {Color.blue,Color.green,Color.red}, new float[]{colorScale[0],0.5f*(colorScale[0]+colorScale[1]),colorScale[1]});
		//ColorMap cm = new ColorMap(new Color[] {Color.black,Color.gray,Color.white}, new float[]{colorScale[0],0.5f*(colorScale[0]+colorScale[1]),colorScale[1]});


		tnx = 1;
		while (tnx < nx)
			tnx *= 2;
		tny = 1;
		while (tny < ny)
			tny *= 2;
		tnz = 1;
		while (tnz < nz)
			tnz *= 2;
		tex = new Texture3D (tnx, tny, tnz, TextureFormat.ARGB32, true);
		tex2 = new Texture3D (tnx, tny, tnz, TextureFormat.ARGB32, true);
		cols = new Color[tnx*tny*tnz];
		cols2 = new Color[tnx*tny*tnz];
		updateTexture ();
	}

	public void updateTexture() {

		depVals = theData.getValues3D (depName);
		colVals = theData.getValues3D (colName);
		tempVals = theData.getValues3D (tempName);
		if (depVals == null) {
			Debug.Log ("depVals = null in VolumetricPlot.cs");
			Debug.Log (depName);
		}

		int idx = 0;
		for (int z = 0; z < nz; ++z)
		{
			for (int y = 0; y < ny; ++y)
			{

				for (int x = 0; x < nx; ++x)
				{
					idx = z * tnx * tny + y * tnx + x;
					// set temperature map for shader
					if (tempVals != null) {
						cols2 [idx].r = Mathf.Max (0.0f, 
							Mathf.Min (1.0f,
								(tempVals [x, y, z] - tempScale [0]) /
								(tempScale [1] - tempScale [0]) -
								tempScale [0]));
					} else {
						cols2 [idx].r = 1.0f;
					}
					// set color map for shader
					Color col = color;
					if (colVals != null) {
						col = cm.eval (colVals [x, y, z]);
					} 

					cols[idx].r = col.r;
					cols[idx].g = col.g;
					cols[idx].b = col.b;

					// set density map for shader
					cols [idx].a = Mathf.Max (0.0f, Mathf.Min (1.0f, (depVals [x, y, z] - depLimits [0]) / (depLimits [1] - depLimits [0])-depLimits[0]));
				}
			}
		}
		tex.SetPixels (cols);
		tex.Apply ();
		tex2.SetPixels (cols2);
		tex2.Apply ();
		GetComponent<Renderer>().sharedMaterial.SetTexture ("_Volume", tex);
		GetComponent<Renderer>().sharedMaterial.SetTexture ("_Volume2", tex2);
		GetComponent<Renderer>().sharedMaterial.SetFloat ("_minT", tempScale[0]);
		GetComponent<Renderer>().sharedMaterial.SetFloat ("_maxT", tempScale[1]);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_bX", (float)nx / (float)tnx);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_bY", (float)ny / (float)tny);
		GetComponent<Renderer> ().sharedMaterial.SetFloat ("_bZ", (float)nz / (float)tnz);
	}



	void Start ()
	{
		if (filename != null) {
			if (!filename.Equals ("")) {
				string filepath = Application.dataPath + "/StreamingAssets/LocalFiles/" + filename;
				theData = (StructuredData)ScriptableObject.CreateInstance ("StructuredData");
				theData.readWOD (filepath);
				makeTexture ();
			}
		}
	}  
	
	// Update is called once per frame
	void Update () {
		// redraw Texture3D as needed based on data.

		/*
		if (Input.GetButton ("Fire1")) {
			float changeEmissivity = Input.GetAxis ("Mouse X");
			float changeOpacity = Input.GetAxis ("Mouse Y");
			emissivity += 5.0f*changeEmissivity * Time.deltaTime;
			opacity += 5.0f*changeOpacity * Time.deltaTime;
			emissivity = Mathf.Max (0.0f, emissivity);
			opacity = Mathf.Max (0.0f, opacity);
			Debug.Log (emissivity);
			GetComponent<Renderer> ().material.SetFloat ("_emissivity", emissivity);
			GetComponent<Renderer> ().material.SetFloat ("_opacity", opacity);
		}
		*/

		//GetComponent<Renderer> ().material.SetFloat ("_ObX", transform.position.x);
		//GetComponent<Renderer> ().material.SetFloat ("_ObY", transform.position.y);
		//GetComponent<Renderer> ().material.SetFloat ("_ObZ", transform.position.z);
		//float size = Mathf.Max(Mathf.Max(transform.lossyScale.x,transform.lossyScale.y),transform.lossyScale.z);
		//GetComponent<Renderer> ().material.SetFloat ("_size", 5*size);

		//transform.Rotate(transform.right,50.0f*Time.deltaTime,Space.Self);
		GetComponent<Renderer>().material.SetFloat ("_opacity", opacity);
		GetComponent<Renderer>().material.SetFloat ("_emissivity", emissivity);
		if (delayUpdate++ > delayMax) {
			shaderSize = 1.7f * transform.lossyScale.magnitude;
			GetComponent<Renderer> ().material.SetFloat ("_size", shaderSize);
			delayUpdate = 0;
		}
	}

	public void setOpacity(float opacity) {
		this.opacity = opacity;
	}

	public void setEmissivity(float emissivity) {
		this.emissivity = emissivity;
	}
}
