using UnityEngine;
using System.Collections;

/// <summary>
/// Glyph set PRE.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class GlyphSetPRE : MonoBehaviour {

	public GameObject template;
	public GameObject template3D;
	UnstructuredData uData;
	public string filename = "csv_input.txt";
	public int mask = 50;
	public bool useMask = false;
	public string xName = "x";
	public string yName = "y";
	public string zName = "z";
	public string[] vNames = null;
	public string cName = null;
	public string sName = null;
	public bool drawBar = false;
	public bool unstructured = true;

	// Use this for initializtion
	void Start () {
		uData = (UnstructuredData)ScriptableObject.CreateInstance ("UnstructuredData");
		string filepath = Application.dataPath+"/StreamingAssets/LocalFiles/"+filename;
		uData.readCSV (filepath);
		

		 
		GameObject glyphSetGO = GlyphSet.CreateGameObject(transform.position, Vector3.one, transform);
		GlyphSet glyphSet = glyphSetGO.GetComponent<GlyphSet> ();
		glyphSet.setMask (mask);
		glyphSet.setUseMask (useMask);
		glyphSet.setDrawBar (drawBar);
		glyphSet.setData (uData);
		glyphSet.setGlyphPRE (template);
		glyphSet.setGlyph3DPRE (template3D);
		glyphSet.setXName (xName);
		glyphSet.setYName (yName);
		glyphSet.setZName (zName);
		//glyphSet.setColor (Color.green);
		glyphSet.setCName (cName);
		glyphSet.setSName (sName);
		glyphSet.setVNames (vNames);
		glyphSet.centerGlyphs (false);
		glyphSet.autoScale (3.0f,false);
		glyphSet.createGlyphs ();

	}
	
	// Update is called once per frame
	void Update () {

	
	}
}
