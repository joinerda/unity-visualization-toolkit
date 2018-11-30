using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/// <summary>
/// Line plot built into user interface
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class UIPlot : MonoBehaviour {

	public enum PlotType {LINE, MARKER, BOTH

	};

	public float xmin = -10.0f;
	public float xmax = 10.0f;
	public float ymin = -10.0f;
	public float ymax = 10.0f;
	GameObject screen;
	public GameObject UIPlotLinePre;
	public GameObject UICirclePre;
	List<PlotType> plotTypes;
	List<UILineRenderer> plotLines;
	List<float []> plotX;
	List<float []> plotY;
	List<List<UICircle>> plotCircles;
	PlotType plotTypeDefault = PlotType.MARKER;
	public float markerSize = 10.0f;

	Color [] lineColors = new Color[]{Color.green,Color.red,Color.blue,Color.gray,Color.black};

	/// <summary>
	/// Sets the X minimum.
	/// </summary>
	/// <param name="xmin">Xmin.</param>
	public void setXMin(float xmin) {
		this.xmin = xmin;
	}
	/// <summary>
	/// Sets the X max.
	/// </summary>
	/// <param name="xmax">Xmax.</param>
	public void setXMax(float xmax) {
		this.xmax = xmax;
	}
	/// <summary>
	/// Sets the Y minimum.
	/// </summary>
	/// <param name="ymin">Ymin.</param>
	public void setYMin(float ymin) {
		this.ymin = ymin;
	}
	/// <summary>
	/// Sets the Y max.
	/// </summary>
	/// <param name="ymax">Ymax.</param>
	public void setYMax(float ymax) {
		this.ymax = ymax;
	}
	/// <summary>
	/// Sets the limits
	/// </summary>
	/// <param name="xmin">Xmin.</param>
	/// <param name="xmax">Xmax.</param>
	/// <param name="ymin">Ymin.</param>
	/// <param name="ymax">Ymax.</param>
	public void setLim(float xmin, float xmax, float ymin, float ymax) {
		this.xmin = xmin;
		this.xmax = xmax;
		this.ymin = ymin;
		this.ymax = ymax;
	}
	/// <summary>
	/// Sets the X limits.
	/// </summary>
	/// <param name="xmin">Xmin.</param>
	/// <param name="xmax">Xmax.</param>
	public void setXLim(float xmin, float xmax) {
		this.xmin = xmin;
		this.xmax = xmax;
	}
	/// <summary>
	/// Sets the Y limits.
	/// </summary>
	/// <param name="ymin">Ymin.</param>
	/// <param name="ymax">Ymax.</param>
	public void setYLim(float ymin, float ymax) {
		this.ymin = ymin;
		this.ymax = ymax;
	}

	/// <summary>
	/// Sets the line colors.
	/// </summary>
	/// <param name="lineCOlors">Line C olors.</param>
	public void setLineColors(Color [] lineCOlors) {
		this.lineColors = lineColors;
	}

	public void clearLines() {
		if (plotLines != null) {
			for (int i = 0; i < plotLines.Count; i++) {
				if(plotLines [i]!=null) Destroy (plotLines [i].gameObject);
			}
		}
		if (plotCircles != null) {
			for (int i = 0; i < plotCircles.Count; i++) {
				foreach (UICircle circle in plotCircles[i]) {
					if(circle!=null) Destroy (circle.gameObject);
				}
			}
		}
		plotLines = new List<UILineRenderer> ();
		plotCircles = new List<List<UICircle>>();
		plotTypes = new List<PlotType> ();
		plotX = new List<float[]> ();
		plotY = new List<float[]> ();	}

	// Use this for initialization
	void Awake () {
		screen = transform.Find ("screen").gameObject;
		clearLines ();

		//float[] x = new float[]{ -5.0f,0.0f, 1.0f };
		//float[] y = new float[]{ -3.0f, 0.3f,2.6f };
		//addPlot (x, y);
	}

	/// <summary>
	/// Adds a line after plot has been started
	/// </summary>
	/// <returns>The plot.</returns>
	/// <param name="xy">Xy.</param>
	public int addPlot(Vector2 [] xy, PlotType plotType) {
		float[] fx = new float[xy.Length];
		float[] fy = new float[xy.Length];
		return addPlot (fx, fy,plotType);
	}

	public int addPlot(Vector2 [] xy) {
		return addPlot (xy, plotTypeDefault);
	}
		

	/// <summary>
	/// Adds a line after plot has been started
	/// </summary>
	/// <returns>The plot.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public int addPlot(double [] x, double [] y, PlotType plotType) {
		float[] fx = new float[x.Length];
		float[] fy = new float[y.Length];
		for (int i = 0; i < x.Length; i++) {
			fx [i] = (float)x [i];
		}
		for (int i = 0; i < y.Length; i++) {
			fy [i] = (float)y [i];
		}
		return addPlot (fx, fy, plotType);
	}
	public int addPlot(double [] x, double [] y) {
		return addPlot (x, y, plotTypeDefault);
	}	

	public int addPlot(float [] x, float [] y) {
		return addPlot (x, y, plotTypeDefault);
	}

	/// <summary>
	/// Adds a line after plot has been started
	/// </summary>
	/// <returns>The plot.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public int addPlot(float [] x, float [] y, PlotType plotType) {
		int index = plotLines.Count;
		if (x.Length != y.Length) {
			Debug.Log ("Array Lengths do not match in addPlot");
			return -1;
		}
		plotX.Add (x);
		plotY.Add (y);
		Color plotColor = lineColors [plotLines.Count % lineColors.Length]; 
		UILineRenderer lr = Instantiate (UIPlotLinePre, screen.transform).GetComponent<UILineRenderer>();
		Vector2[] points = new Vector2[x.Length];
		for (int i = 0; i < x.Length; i++) {
			points [i] = new Vector2 ((x [i]-xmin)/(xmax-xmin), (y [i]-ymin)/(ymax-ymin));
		}
		lr.Points = points;
		lr.color = plotColor;
		if (plotType == PlotType.LINE || plotType == PlotType.BOTH) {
			lr.gameObject.SetActive (true);
		} else {
			lr.gameObject.SetActive (false);
		}
		plotLines.Add (lr);
		List<UICircle> cl = new List<UICircle> ();
		for (int i = 0; i < x.Length; i++) {
			float screenWidth = screen.GetComponent<RectTransform> ().rect.width;
			float screenHeight = screen.GetComponent<RectTransform> ().rect.height;
			UICircle circle = Instantiate (UICirclePre, screen.transform).GetComponent<UICircle>();
			Vector3 position = new Vector3 (screenWidth*points [i].x, screenHeight*(points [i].y-1), 0);
			circle.rectTransform.anchoredPosition = position;
			circle.rectTransform.sizeDelta = markerSize*Vector2.one;
			circle.color = plotColor;
			if (plotType == PlotType.MARKER || plotType == PlotType.BOTH) {
				circle.gameObject.SetActive (true);
			} else {
				circle.gameObject.SetActive (false);
			}
			cl.Add (circle);
		}
		plotCircles.Add (cl);
		return index;
	}

	/// <summary>
	/// Adds a line after plot has been started.
	/// Empty line created for future reset.
	/// </summary>
	/// <returns>The plot.</returns>
	public int addPlot(PlotType plotType) {
		float[] x = new float[1];
		float[] y = new float[1];
		return addPlot (x, y,plotType);
	}

	public int addPlot() {
		return addPlot (plotTypeDefault);
	}

	/// <summary>
	/// Sets the color.
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="c">C.</param>
	public void setColor(int i, Color c) {
		plotLines [i].color = c;
		plotLines[i].SetAllDirty();
		for (int j = 0; j < plotCircles [i].Count; j++) {
			(plotCircles [i]) [j].color = c;
		}
	}

	public void resetPlot(int i, float [] x, float [] y) {
		resetPlot (i, x, y, plotTypeDefault);
	}

	/// <summary>
	/// Given a plot, reset the data
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void resetPlot(int i, float [] x, float [] y, PlotType plotType) {
		plotX.Insert (i, x);
		plotX.RemoveAt (i + 1);
		plotY.Insert (i, y);
		plotY.RemoveAt (i + 1);
		Vector2[] points = new Vector2[x.Length];
		for (int j = 0; j < x.Length; j++) {
			points [j] = new Vector2 ((x [j]-xmin)/(xmax-xmin), (y [j]-ymin)/(ymax-ymin));
		}
		plotLines[i].Points = points;
		if (plotType == PlotType.LINE || plotType == PlotType.BOTH) {
			plotLines[i].gameObject.SetActive (true);
		} else {
			plotLines[i].gameObject.SetActive (false);
		}
		plotLines[i].SetAllDirty();
		for (int j = 0; j < x.Length; j++) {
			float screenWidth = screen.GetComponent<RectTransform> ().rect.width;
			float screenHeight = screen.GetComponent<RectTransform> ().rect.height;
			UICircle circle = (plotCircles [i]) [j];
			Vector3 position = new Vector3 (screenWidth*points [i].x, screenHeight*(points [i].y-1), 0);
			circle.rectTransform.anchoredPosition = position;
			circle.rectTransform.sizeDelta = markerSize*Vector2.one;
			if (plotType == PlotType.MARKER || plotType == PlotType.BOTH) {
				circle.gameObject.SetActive (true);
			} else {
				circle.gameObject.SetActive (false);
			}
			circle.SetAllDirty ();
		}
	}

	/// <summary>
	/// Given a plot, reset the data
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="lx">Lx.</param>
	/// <param name="ly">Ly.</param>
	public void resetPlot(int i, List<float> lx, List<float> ly, PlotType plotType) {
		float[] x = new float[lx.Count];
		float[] y = new float[ly.Count];
		for (int j = 0; j < lx.Count; j++) {
			x [j] = lx [j];
		}
		for (int j = 0; j < ly.Count; j++) {
			y [j] = ly [j];
		}
		resetPlot (i, x, y,plotType);
	}
	public void resetPlot(int i, List<float> lx, List<float> ly) {
		resetPlot (i, lx, ly, plotTypeDefault);
	}
		
	
	// Update is called once per frame
	void Update () {
		
	}
}
