﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic visualization object. This is used as a front end to specific visualizations
/// to allow for selection and setting of visualization type and variables in the 
/// Unity editor.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class VisObject : MonoBehaviour {

	public GameObject dataObject = null;
	public bool animate = false;

	public enum DataState {
		UNSET,PENDING,READY
	}
	/// <summary>
	/// Types of visualizations supported
	/// </summary>
	public enum VisType {
		SURFACE,ISOCONTOUR,GLYPH,MOLECULE,VOLUME,THRESHHOLD
	};
	public VisType visType = VisType.GLYPH;
	public string [] indVarNames = {"x","y","z"};
	public string depVar = "";
	public float [] depLimits = {0.0f,10.0f};
	public string colorVar = "";
	public Color color = Color.green;
	public string [] sizeVars = {"z","c","s"};
	public GameObject glyph1DPRE = null;
	public GameObject glyph3DPRE = null;
	public Vector3 glyphScale = Vector3.one;
	public int mask = 100;
	public bool useMask = false;
	public bool drawBars = false;
	public float alpha = 1.0f;
	public float[] colorScale = { 0.0f,0.5f, 1.0f };
	public Color[] colorMap = { Color.red, Color.green, Color.blue };
	public float isoValue = 0.0f;
	public float bondWidth = 0.1f;
	public float atomSize = 0.5f;
	string depVarLast = null;
	string colorVarLast = null;
	float isoValueLast = 0.0f;
	public bool forceRefresh = false;
	public float opacity = 1.0f;
	public float emissivity = 2.0f;
	public Threshhold.ThreshholdType threshholdType;
	public float minValue = 0.0f;
	public float maxValue = 1.0f;
	public GameObject threshholdPRE=null;
	public int maxDepth = 10;

	GameObject visGO = null;

	DataState dataState = DataState.UNSET;

	/// <summary>
	/// Pending a completely read, or re-read, data object, create and
	/// initialize gameobjects for the visualization
	/// </summary>
	void Init() {
		if (visGO != null) {
			Destroy (visGO);
		}
		GetComponent<Renderer> ().enabled = false;
		switch (visType) {
		case(VisType.GLYPH):
			// create a GlyphSet object using the dataset
			visGO = GlyphSet.CreateGameObject (transform.position, Vector3.one, transform);
			GlyphSet glyphSet = visGO.GetComponent<GlyphSet> ();
			if (dataObject.GetComponent<DataObject> ().dataType == DataObject.DataTypes.STRUCTURED_WOD) {
				glyphSet.setData ((StructuredData)dataObject.GetComponent<DataObject> ().getData ());
			} else {
				glyphSet.setData ((UnstructuredData)dataObject.GetComponent<DataObject> ().getData ());
			}
			applySettings ();
			glyphSet.centerGlyphs (false);
			glyphSet.autoScale (3.0f,false);
			glyphSet.createGlyphs ();
			break;
		case(VisType.SURFACE):
			{
				visGO = MeshGrid.CreateGameObject (transform.localPosition, Vector3.one, transform);
				ColorMap cm = new ColorMap ();
				cm.init (colorMap, alpha, colorScale);
				visGO.GetComponent<MeshGrid> ().setColorMap (cm);
				visGO.GetComponent<MeshGrid> ().createGrid ((StructuredData)dataObject.GetComponent<DataObject> ().getData (),
					indVarNames [0], indVarNames [1], depVar, colorVar);
				depVarLast = depVar;
				colorVarLast = colorVar;
			}
			break;
		case(VisType.ISOCONTOUR):
			{
				//ColorMap cm = new ColorMap(new Color[] {Color.red,Color.blue}, new float[]{min,max});
				ColorMap cm = new ColorMap (colorMap,alpha,colorScale);
				//ColorMap cm = new ColorMap(new Color[] {Color.blue}, new float[]{0});
				//ColorMap cm = new ColorMap();
				visGO = IsoContour.CreateGameObject (transform.position, Vector3.one, transform);
				StructuredData sd = (StructuredData)dataObject.GetComponent<DataObject> ().getData ();
				float[] x = sd.getDimension (indVarNames [0]);
				float[] y = sd.getDimension (indVarNames [1]);
				float[] z = sd.getDimension (indVarNames [2]);
				float[,,] depValues = sd.getValues3D (depVar);
				float[,,] colorValues = sd.getValues3D (colorVar);
				visGO.GetComponent<IsoContour>().buildMesh (x, y, z, depValues, colorValues, isoValue, cm);
				//m.Optimize ();
				isoValueLast =  isoValue;
				depVarLast = depVar;
				colorVarLast = colorVar;
			}
			break;
		case(VisType.MOLECULE):
			Molecule mol = (Molecule)dataObject.GetComponent<DataObject> ().getData ();
			//Debug.Log (mol);
			visGO = MoleculeRenderer.Create (mol);
			//visGO.GetComponent<MoleculeRenderer> ().mol = ((Molecule)dataObject.GetComponent<DataObject> ().getData ());
			visGO.GetComponent<MoleculeRenderer> ().updateParams (bondWidth, atomSize);
			visGO.GetComponent<MoleculeRenderer> ().Populate ();
			visGO.transform.position = transform.position;
			visGO.transform.parent = transform;
			//visGO.GetComponent<MoleculeRenderer.BallStickObject> ().ballSize = atomSize;
			//visGO.GetComponent<MoleculeRenderer.BallStickObject> ().bondWidth = bondWidth;
			break;
		case(VisType.VOLUME):
			{
				StructuredData sd = (StructuredData)dataObject.GetComponent<DataObject> ().getData ();
				visGO = VolumetricPlot.CreateGameObject ();

				VolumetricPlot vp = visGO.GetComponent<VolumetricPlot> ();

				ColorMap cm = new ColorMap ();
				cm.init (colorMap, alpha, colorScale);
				vp.setColorMap (cm);
				vp.setData (sd);

				vp.xName = indVarNames [0];
				vp.yName = indVarNames [1];
				vp.zName = indVarNames [2];
				vp.depName = depVar;

				vp.depLimits = depLimits;
				vp.colName = colorVar;
				vp.makeTexture ();
				visGO.transform.position = transform.position;
				visGO.transform.parent = transform;
				visGO.transform.localScale = Vector3.one;

				depVarLast = depVar;
				colorVarLast = colorVar;
			}
			break;
		case(VisType.THRESHHOLD):
			{
				visGO = Threshhold.CreateGameObject (transform.position, Vector3.one, transform);
				Threshhold threshhold = visGO.GetComponent<Threshhold> ();
				if (dataObject.GetComponent<DataObject> ().dataType == DataObject.DataTypes.STRUCTURED_WOD) {
					threshhold.setData ((StructuredData)dataObject.GetComponent<DataObject> ().getData ());
				} else {
					threshhold.setData ((UnstructuredData)dataObject.GetComponent<DataObject> ().getData ());
				}
				threshhold.xName = indVarNames [0];
				threshhold.yName = indVarNames [1];
				threshhold.zName = indVarNames [2];
				threshhold.depName = depVar;
				threshhold.color = color;
				threshhold.minValue = minValue;
				threshhold.maxValue = maxValue;
				threshhold.maxDepth = maxDepth;
				threshhold.cubePRE = threshholdPRE;
				threshhold.threshholdType = threshholdType;
				threshhold.drawThreshhold ();
			}
			break;
		default:
			break;
		}	
	}

	void applySettings() {
		if (visGO == null) {
			return;
		}
		switch (visType) {
		case(VisType.GLYPH):
			// create a GlyphSet object using the dataset
			GlyphSet glyphSet = visGO.GetComponent<GlyphSet> ();
			glyphSet.setGlyphPRE (glyph1DPRE);
			glyphSet.setGlyph3DPRE (glyph3DPRE);
			glyphSet.glyphScale = glyphScale;
			glyphSet.setUseMask (useMask);
			glyphSet.setMask (mask);
			glyphSet.setDrawBar (drawBars);
			glyphSet.setXName (indVarNames [0]);
			glyphSet.setYName (indVarNames [1]);
			glyphSet.setZName (indVarNames [2]);
			glyphSet.setColor (color);
			glyphSet.setCName (colorVar);
			glyphSet.setSName (depVar);
			glyphSet.setVNames (sizeVars);
			glyphSet.setAlpha (alpha);
			glyphSet.setColorScale (colorScale);
			glyphSet.setColorMap (colorMap);

			break;
		case(VisType.SURFACE):
			{
				ColorMap cm = new ColorMap ();
				cm.init (colorMap, alpha, colorScale);
				if (!depVarLast.Equals (depVar) || !colorVarLast.Equals (colorVar)) {
					StructuredData sd = (StructuredData)dataObject.GetComponent<DataObject> ().getData ();
					float[] depValues = sd.getValues (depVar);
					float[] colorValues = sd.getValues (colorVar);
					if (depValues != null) {
						visGO.GetComponent<MeshGrid> ().setZ (depValues);
					}
					visGO.GetComponent<MeshGrid> ().setColors (colorValues);
					depVarLast = depVar;
					colorVarLast = colorVar;
				}
				visGO.GetComponent<MeshGrid> ().setColorMap (cm);
				visGO.GetComponent<MeshGrid> ().updateGrid ();

			}
			break;
		case(VisType.ISOCONTOUR):
			{
				if (depVarLast != depVar || colorVarLast != colorVar || isoValueLast != isoValue) {
					ColorMap cm = new ColorMap (colorMap, alpha, colorScale);
					//ColorMap cm = new ColorMap(new Color[] {Color.blue}, new float[]{0});
					//ColorMap cm = new ColorMap();
					if(visGO!=null) Destroy(visGO);
					visGO = IsoContour.CreateGameObject (transform.position, Vector3.one, transform);
					StructuredData sd = (StructuredData)dataObject.GetComponent<DataObject> ().getData ();
					float[] x = sd.getDimension (indVarNames [0]);
					float[] y = sd.getDimension (indVarNames [1]);
					float[] z = sd.getDimension (indVarNames [2]);
					float[,,] depValues = sd.getValues3D (depVar);
					float[,,] colorValues = sd.getValues3D (colorVar);
					visGO.GetComponent<IsoContour> ().buildMesh (x, y, z, depValues, colorValues, isoValue, cm);
					//m.Optimize ();
					isoValueLast = isoValue;
					depVarLast = depVar;
					colorVarLast = colorVar;
				}
			}
			break;
		case(VisType.MOLECULE):
			visGO.GetComponent<MoleculeRenderer> ().updateParams (bondWidth, atomSize);

			break;
		case(VisType.VOLUME):
			VolumetricPlot vp = visGO.GetComponent<VolumetricPlot> ();

			if (depVarLast != depVar || colorVarLast != colorVar) {
				StructuredData sd = (StructuredData)dataObject.GetComponent<DataObject> ().getData ();
				ColorMap cm = new ColorMap ();
				cm.init (colorMap, alpha, colorScale);
				vp.setColorMap (cm);
				vp.depName = depVar;
				vp.depLimits = depLimits;
				vp.colName = colorVar;
				vp.makeTexture ();
				visGO.transform.position = transform.position;
				visGO.transform.parent = transform;
				visGO.transform.localScale = Vector3.one;
				depVarLast = depVar;
				colorVarLast = colorVar;
			}
			vp.setOpacity (opacity);
			vp.setEmissivity (emissivity);

			break;
		case(VisType.THRESHHOLD):
			{

			}
			break;
		default:
			break;
		}	
	}

	// Use this for initialization
	void Start () {
		dataState = DataState.PENDING;
	}

	float animationTimer = 0.0f;
	float animationLag = 3.0f;
	
	// Update is called once per frame
	void Update () {

		if (dataState == DataState.PENDING) {
			// check to see of data is ready
			if (dataObject.GetComponent<DataObject> ().getData () != null) {
				dataState = DataState.READY;

				Init ();
			}
		} else if (dataState == DataState.READY) {
			animationTimer += Time.deltaTime;
			if (animationTimer > animationLag && animate) {
				DataObject dob = dataObject.GetComponent<DataObject> ();
				dob.nextSet ();
			}
			applySettings ();
			if (forceRefresh) {
				forceRefresh = false;
				Init ();
			}
		}

	}
}