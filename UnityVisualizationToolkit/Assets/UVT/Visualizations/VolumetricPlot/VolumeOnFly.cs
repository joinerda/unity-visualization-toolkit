using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Volume on fly.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class VolumeOnFly : MonoBehaviour {


	public GameObject VolumetricPRE;
	Nebula nebula;
	StructuredData theData;
	VolumetricPlot vol;

	// Use this for initialization
	void Start () {
		// generating some data
        nebula = new Nebula();
        nebula.CalculateDensity();

		// create data object
		theData = (StructuredData) ScriptableObject.CreateInstance ("StructuredData");
		theData.addDimension ("z", nebula.z);
		theData.addDimension ("y", nebula.y);
		theData.addDimension ("x", nebula.x);
		theData.addVariable3D ("f", nebula.values);
		GameObject go = Instantiate (VolumetricPRE, transform.position, Quaternion.identity);
		vol = go.GetComponent<VolumetricPlot> ();
		vol.zName = "z";
		vol.yName = "y";
		vol.xName = "x";

		vol.depName = "f";
		vol.colName = "f";
		//vol.color = Color.green;
		vol.setData (theData);
		vol.depLimits = new float[]{0.0f ,5.0e-1f };
		vol.colorScale = new float[]{0.0f ,5.0e-1f };
		vol.tempScale = new float[]{0.0f ,10.0f };
		vol.makeTexture ();

	}
	
	// Update is called once per frame
	void Update () {
		//nebula.alpha = System.Math.Cos (Time.time)*System.Math.Cos (Time.time);
		//nebula.phiJ = (60.0+30.0*System.Math.Cos (Time.time))*System.Math.PI/180.0;
		//nebula.CalculateDensity ();
		//theData.setValues3D ("f", nebula.values);
		//vol.updateTexture ();
		
	}
}



