using UnityEngine;
using System.Collections;


/// <summary>
/// Glyph set.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class GlyphSet : MonoBehaviour {

	UnstructuredData uData;
	GameObject glyphPRE;
	GameObject glyph3DPRE;
	float xOffset=0.0f;
	float yOffset=0.0f;
	float zOffset=0.0f;
	float xScale=1.0f;
	float yScale=1.0f;
	float zScale=1.0f;
	string xName = null;
	string yName = null;
	string zName = null;
	string cName = null;
	string sName = null;
	float size = 1.0f;
	string [] vNames = null;
	Color solidColor = Color.white;
	//string colorName = null;
	//string xGlyphName = null;
	// yglyphname and zglpyhname, isvector ...
	bool is3D = false;
	public Vector3 glyphScale = Vector3.one;

	GameObject [] theGlyphs;
	float[] colorScale = { 0.0f, 0.5f, 1.0f };
	Color[] colorMap = { Color.red, Color.green, Color.blue };
	float alpha = 1.0f;

	Gradient gradient;
	GradientColorKey[] gck;
	GradientAlphaKey[] gak;
	double minGradientScale = -1.0;
	double maxGradientScale = 1.0;
	int mask = 50;// how best to implement masking for large datasets? User set? helper functions?
	bool useMask = true;
	bool drawBar = false;
	int nData;
	int nObjects;
	Deck maskDeck;

	/// <summary>
	/// Sets the alpha.
	/// </summary>
	/// <param name="alpha">Alpha.</param>
	public void setAlpha(float alpha) {
		this.alpha = alpha;
	}
	/// <summary>
	/// Gets the alpha.
	/// </summary>
	/// <returns>The alpha.</returns>
	public float getAlpha() {
		return alpha;
	}

	/// <summary>
	/// Sets whether to draw glyphs as "bars" from the x-z plane
	/// </summary>
	/// <param name="drawBar">If set to <c>true</c> draw bar.</param>
	public void setDrawBar(bool drawBar) {
		this.drawBar = drawBar;
	}

	/// <summary>
	/// Sets the X scale.
	/// </summary>
	/// <param name="xScale">X scale.</param>
	public void setXScale(float xScale) {
		this.xScale = xScale;
	}
	/// <summary>
	/// Sets the Y scale.
	/// </summary>
	/// <param name="yScale">Y scale.</param>
	public void setYScale(float yScale) {
		this.yScale = yScale;
	}
	/// <summary>
	/// Sets the Z scale.
	/// </summary>
	/// <param name="zScale">Z scale.</param>
	public void setZScale(float zScale) {
		this.zScale = zScale;
	}
	/// <summary>
	/// autscales values to fit in a box 1 wide
	/// </summary>
	public void autoScale() {
		autoScale (1.0f, false);
	}

	/// <summary>
	/// autoscales to a desired size
	/// </summary>
	/// <param name="desiredScale">Desired scale.</param>
	public void autoScale(float desiredScale) {
		autoScale (desiredScale, false);
	}


	/// <summary>
	/// autoscales with option to update glyphs now or later
	/// </summary>
	/// <param name="desiredScale">Desired scale.</param>
	/// <param name="update">If set to <c>true</c> update.</param>
	public void autoScale(float desiredScale, bool update) {
		int nObjects = uData.getNData ();
		float xMax = 0.0f;
		float yMax = 0.0f;
		float zMax = 0.0f;
		for (int i = 0; i < nObjects; i++) {
			if (xName != null) {
				float x = Mathf.Abs (uData.getValue (xName, i) - xOffset);
				if (x > xMax)
					xMax = x;

			}
			if (yName != null) {
				float y = Mathf.Abs (uData.getValue (yName, i) - yOffset);
				if (y > yMax)
					yMax = y;			}
			if (zName != null) {
				float z = Mathf.Abs (uData.getValue (zName, i) - zOffset);
				if (z > zMax)
					zMax = z;			}
		}
		if (xMax == 0.0f)
			xMax = 1.0f;
		if (yMax == 0.0f)
			yMax = 1.0f;
		if (zMax == 0.0f)
			zMax = 1.0f;

		xScale = desiredScale/xMax;
		yScale = desiredScale/yMax;
		zScale = desiredScale/zMax;

		if (update)
			updateGlyphs ();
	}

	/// <summary>
	/// Sets the X offset.
	/// </summary>
	/// <param name="xOffset">X offset.</param>
	public void setXOffset(float xOffset) {
		this.xOffset = xOffset;
	}
	/// <summary>
	/// Sets the Y offset.
	/// </summary>
	/// <param name="yOffset">Y offset.</param>
	public void setYOffset(float yOffset) {
		this.yOffset = yOffset;
	}
	/// <summary>
	/// Sets the Z offset.
	/// </summary>
	/// <param name="zOffset">Z offset.</param>
	public void setZOffset(float zOffset) {
		this.zOffset = zOffset;
	}
	/// <summary>
	/// Centers the glyphs.
	/// </summary>
	public void centerGlyphs() {
		centerGlyphs (false);
	}
		
	/// <summary>
	/// Centers the glyphs
	/// </summary>
	/// <param name="update">If set to <c>true</c> update.</param>
	public void centerGlyphs(bool update) {
		int nObjects = uData.getNData ();
		float xsum = 0.0f;
		float ysum = 0.0f;
		float zsum = 0.0f;
		for (int i = 0; i < nObjects; i++) {
			if (xName != null) {
				xsum += uData.getValue (xName, i);

			}
			if (yName != null) {
				ysum += uData.getValue (yName, i);
			}
			if (zName != null) {
				zsum += uData.getValue (zName, i);
			}
		}
		xOffset = xsum/nObjects;
		if(!drawBar  ) yOffset = ysum/nObjects;
		zOffset = zsum/nObjects;

		if (update)
			updateGlyphs ();
	}
	/// <summary>
	/// Creates a game object containing the glyphset
	/// </summary>
	/// <returns>The game object.</returns>
	/// <param name="pos">Position.</param>
	/// <param name="scale">Scale.</param>
	/// <param name="parent">Parent.</param>
	public static GameObject CreateGameObject(Vector3 pos, Vector3 scale, Transform parent) {
		GameObject glyphSetGO = new GameObject ();

		glyphSetGO.transform.parent = parent;
		glyphSetGO.transform.position = pos;
		glyphSetGO.transform.localPosition = Vector3.zero;
		glyphSetGO.transform.localScale = scale;
		glyphSetGO.AddComponent<GlyphSet> ();

		return glyphSetGO;
	}
	/// <summary>
	/// Autosets the gradient range.
	/// </summary>
	public void autosetGradientRange() {
		int nObjects = uData.getNData ();
		float minRange = uData.getValue (cName, 0);
		float maxRange = minRange;
		for (int i = 1; i < nObjects; i++) {
			float cVal = uData.getValue (cName, i);
			if (cVal < minRange) {
				minRange = cVal;
			}
			if (cVal > maxRange) {
				maxRange = cVal;
			}
		}
		setGradientRange (minRange, maxRange);
	}
	/// <summary>
	/// Sets the gradient range.
	/// </summary>
	/// <param name="minGradientScale">Minimum gradient scale.</param>
	/// <param name="maxGradientScale">Max gradient scale.</param>
	void setGradientRange(float minGradientScale, float maxGradientScale) {
		this.minGradientScale = minGradientScale;
		this.maxGradientScale = maxGradientScale;
	}
	/// <summary>
	/// gets a color based on gradient
	/// </summary>
	/// <returns>The G scale.</returns>
	/// <param name="value">Value.</param>
	Color evalGScale(double value) {
		double tval = (value - minGradientScale) / (maxGradientScale - minGradientScale);
		return gradient.Evaluate ((float)tval);
	}
	/// <summary>
	/// Sets the mask.
	/// </summary>
	/// <param name="mask">Mask.</param>
	public void setMask(int mask) {
		this.mask = mask;
	}
	/// <summary>
	/// Sets to use mask.
	/// </summary>
	/// <param name="useMask">If set to <c>true</c> use mask.</param>
	public void setUseMask(bool useMask) {
		this.useMask = useMask;
	}
	/// <summary>
	/// Sets the data.
	/// </summary>
	/// <param name="uData">U data.</param>
	public void setData(UnstructuredData uData) {
		this.uData = uData;
	}
	/// <summary>
	/// Sets the data.
	/// </summary>
	/// <param name="sData">S data.</param>
	public void setData(StructuredData sData) {
		this.uData = sData.getUnstructured ();
	}
	/// <summary>
	/// Sets the variable names to pull from the data object
	/// </summary>
	/// <param name="vNames">V names.</param>
	public void setVNames(string [] vNames) {
		if (vNames.Length != 3  && vNames.Length != 0) {
			return;
		}
		this.vNames = vNames;
	}
	/// <summary>
	/// Sets the name of the X dimension.
	/// </summary>
	/// <param name="xName">X name.</param>
	public void setXName(string xName) {
		this.xName = xName;
	}
	/// <summary>
	/// Sets the name of the Y dimension.
	/// </summary>
	/// <param name="yName">Y name.</param>
	public void setYName(string yName) {
		this.yName = yName;
	}
	/// <summary>
	/// Sets the name of the Z dimension.
	/// </summary>
	/// <param name="zName">Z name.</param>
	public void setZName(string zName) {
		this.zName = zName;
	}
	/// <summary>
	/// Sets the name of the color variable.
	/// </summary>
	/// <param name="cName">C name.</param>
	public void setCName(string cName) {
		this.cName = cName;
	}
	/// <summary>
	/// Sets the solid color.
	/// </summary>
	/// <param name="solidColor">Solid color.</param>
	public void setColor(Color solidColor) {
		this.solidColor = solidColor;
	}
	/// <summary>
	/// Sets the name of the size variable.
	/// </summary>
	/// <param name="sName">S name.</param>
	public void setSName(string sName) {
		this.sName = sName;
	}
	/// <summary>
	/// sets the gradient range for color scaling
	/// </summary>
	/// <param name="colorScale">Color scale.</param>
	public void setColorScale(float [] colorScale) {
		if (colorScale.Length >= 2) {
			setGradientRange (colorScale [0],
				colorScale [colorScale.Length - 1]);
		}
		this.colorScale = colorScale;
	}
	/// <summary>
	/// sets the colors used in gradient scaling
	/// </summary>
	/// <param name="colorMap">Color map.</param>
	public void setColorMap(Color [] colorMap) {
		this.colorMap = colorMap;
	}
	/// <summary>
	/// sets the default size
	/// </summary>
	/// <param name="size">Size.</param>
	public void setSize(float size) {
		this.size = size;
	}
	/// <summary>
	/// sets the prefab object for 1D glyphs
	/// </summary>
	/// <param name="glyphPRE">Glyph PR.</param>
	public void setGlyphPRE(GameObject glyphPRE) {
		this.glyphPRE = glyphPRE;
	}
	/// <summary>
	/// sets the prefab object for 3D glyphs
	/// </summary>
	/// <param name="glyph3DPRE">Glyph3 DPR.</param>
	public void setGlyph3DPRE(GameObject glyph3DPRE) {
		this.glyph3DPRE = glyph3DPRE;
	}

	/*
	public int [] arrayRange(int low, int high) {
		int n = (high - low)+1;
		int[] retval = new int[n];
		retval [0] = low;
		retval [n - 1] = high;
		for(int i=1;i<n-1;i++) {
			retval [i] = retval [i - 1] + 1;
		}
		return retval;
	}
	*/

	/// <summary>
	/// Creates the glyphs.
	/// </summary>
	public void createGlyphs() {
		nData = uData.getNData ();
		nObjects = nData;
		maskDeck = new Deck ();
		if (useMask) {
			nObjects = Mathf.Min (mask, nData);
			maskDeck.init (nData);
		}
		theGlyphs = new GameObject[nObjects];
		gradient = new Gradient ();
		gck = new GradientColorKey[colorScale.Length];
		gak = new GradientAlphaKey[2];

		for (int i = 0; i < colorScale.Length; i++) {
			gck [i].color =  colorMap[i];
			gck [i].time = (float)((colorScale [i]-minGradientScale)/
				(maxGradientScale-minGradientScale));
		}
		gak [0].alpha = 1.0f;
		gak [0].time = 0.0f;
		gak [1].alpha = 1.0f;
		gak[1].time=1.0f;
		gradient.SetKeys(gck,gak);
		for (int iOb = 0; iOb < nObjects; iOb++) {
			int i = iOb;
			if(useMask) i = maskDeck.draw ();
			float xval, yval, zval, sval;
			if (xName == null) {
				xval = 0.0f;
			} else {
				xval = (float)uData.getValue (xName, i);
			}
			if (yName == null) {
				yval = 0.0f;
			} else {
				yval = (float)uData.getValue (yName, i);
			}
			if (zName == null) {
				zval = 0.0f;
			} else {
				zval = (float)uData.getValue (zName, i);
			}
			Color color = solidColor;
			if (cName == null || cName.Equals("")) {
			} else {
				color = evalGScale (uData.getValue (cName,i));
				//color.a = glyphPRE.GetComponent<Renderer> ().sharedMaterial.color.a;
				color.a=alpha;
			}
			if (sName == null|| sName.Equals("")) {
				sval = size;
			} else {
				sval = (float)uData.getValue (sName, i);
			}
			if (vNames == null || vNames.Length == 0) {
				theGlyphs [iOb] = (GameObject)Instantiate (glyphPRE, Vector3.zero, Quaternion.identity);
				theGlyphs [iOb].transform.parent = transform;
				theGlyphs [iOb].GetComponent<Renderer> ().material.color = color;
				is3D = false;
			} else {
				Vector3 lookAt = new Vector3(uData.getValue(vNames[0],i),uData.getValue(vNames[1],i),uData.getValue(vNames[2],i));
				Quaternion rot = Quaternion.LookRotation (lookAt);
				theGlyphs [iOb] = (GameObject)Instantiate (glyph3DPRE, Vector3.zero, Quaternion.identity);
				theGlyphs [iOb].transform.parent = transform;

				theGlyphs [iOb].transform.localRotation = rot;

				is3D = true;

			}
			foreach (MeshRenderer ren in theGlyphs[iOb].GetComponentsInChildren<MeshRenderer>()) {
				//ren.material = new Material (Shader.Find ("Standard"));
				ren.material.color = color;
			}
			if (!is3D) {
				theGlyphs [iOb].transform.localScale = 
					Vector3.Scale(glyphScale,glyphPRE.transform.localScale * sval);
			} else {

				theGlyphs [iOb].transform.localScale = 
					Vector3.Scale(glyphScale,glyph3DPRE.transform.localScale * sval);

			}	
			if (drawBar) {
				theGlyphs [iOb].transform.localPosition = new Vector3 (xScale * (xval - xOffset), yScale * (yval - yOffset)/2, zScale * (zval - zOffset));
				theGlyphs [iOb].transform.localScale = new Vector3 (theGlyphs [iOb].transform.localScale.x,
					yScale * (yval - yOffset),
					theGlyphs [iOb].transform.localScale.z);
			} else {
				theGlyphs [iOb].transform.localPosition = new Vector3 (xScale * (xval - xOffset), yScale * (yval - yOffset), zScale * (zval - zOffset));
			}
		}
	}
	/// <summary>
	/// Destroies the glyphs.
	/// </summary>
	public void destroyGlyphs() {
		int n = theGlyphs.Length;
		for (int i = n - 1; i >= 0; i++) {
			Destroy (theGlyphs [i]);
		}
	}
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update() {
		updateGlyphs ();
	}
	/// <summary>
	/// Updates the glyphs.
	/// </summary>
	public void updateGlyphs() {
		gradient = new Gradient ();
		gck = new GradientColorKey[colorScale.Length];
		gak = new GradientAlphaKey[2];

		for (int i = 0; i < colorScale.Length; i++) {
			gck [i].color =  colorMap[i];
			gck [i].time = (float)((colorScale [i]-minGradientScale)/
				(maxGradientScale-minGradientScale));
		}
		gak [0].alpha = 1.0f;
		gak [0].time = 0.0f;
		gak [1].alpha = 1.0f;
		gak[1].time=1.0f;
		gradient.SetKeys(gck,gak);
		for (int iOb = 0; iOb < nObjects; iOb++) {
			int i = iOb;
			if(useMask) i = maskDeck.peekStack (iOb);
			float xval, yval, zval, sval;
			if (xName == null) {
				xval = 0.0f;
			} else {
				xval = (float)uData.getValue (xName, i);
			}
			if (yName == null) {
				yval = 0.0f;
			} else {
				yval = (float)uData.getValue (yName, i);
			}
			if (zName == null) {
				zval = 0.0f;
			} else {
				zval = (float)uData.getValue (zName, i);
			}
			Color color = solidColor;
			if (colorScale.Length==0||cName == null || cName.Equals("")) {
			} else {
				color = evalGScale (uData.getValue (cName,i));
				//color.a = glyphPRE.GetComponent<Renderer> ().sharedMaterial.color.a;
				color.a=alpha;
			}
			if (sName == null|| sName.Equals("")) {
				sval = size;
			} else {
				sval = (float)uData.getValue (sName, i);
			}
			if (vNames == null || vNames.Length == 0) {
				theGlyphs [iOb].transform.parent = transform;
				theGlyphs [iOb].GetComponent<Renderer> ().material.color = color;
				is3D = false;
			} else {
				Vector3 lookAt = new Vector3(uData.getValue(vNames[0],i),uData.getValue(vNames[1],i),uData.getValue(vNames[2],i));
				Quaternion rot = Quaternion.LookRotation (lookAt);
				theGlyphs [iOb].transform.parent = transform;
				theGlyphs [iOb].transform.localRotation = rot;
				is3D = true;
			}
			foreach (MeshRenderer ren in theGlyphs[iOb].GetComponentsInChildren<MeshRenderer>()) {
				//ren.material = new Material (Shader.Find ("Standard"));
				ren.material.color = color;
			}
			if (!is3D) {
				theGlyphs [iOb].transform.localScale = 
					Vector3.Scale(glyphScale,glyphPRE.transform.localScale * sval);
			} else {

				theGlyphs [iOb].transform.localScale = 
					Vector3.Scale(glyphScale,glyph3DPRE.transform.localScale * sval);

			}	
			if (drawBar) {
				theGlyphs [iOb].transform.localPosition = new Vector3 (xScale * (xval - xOffset), yScale * (yval - yOffset)/2, zScale * (zval - zOffset));
				theGlyphs [iOb].transform.localScale = new Vector3 (theGlyphs [iOb].transform.localScale.x,
					yScale * (yval - yOffset),
					theGlyphs [iOb].transform.localScale.z);
			} else {
				theGlyphs [iOb].transform.localPosition = new Vector3 (xScale * (xval - xOffset), yScale * (yval - yOffset), zScale * (zval - zOffset));
			}
		}
	}
		
}
