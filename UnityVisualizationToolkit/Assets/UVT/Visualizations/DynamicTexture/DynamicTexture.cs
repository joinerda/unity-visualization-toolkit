using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamic texture object provides drawing primitives for a
/// texture object in unity scene
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class DynamicTexture {

	public float xmin = -10.0f;
	public float xmax = 10.0f;
	public float ymin = -10.0f;
	public float ymax = 10.0f;
	int width = 100;
	int height = 100;
	Texture2D tex;
	public int lw = 2;
	Color32 foreground = (Color32)Color.black;
	Color32 background = (Color32)Color.white;

	/// <summary>
	/// Gets the boundary of the texture in "real" values mapped
	/// to object
	/// </summary>
	/// <returns>The real bounds.</returns>
    public Rect getRealBounds()
    {
        Rect bounds = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        return bounds;
    }

	/// <summary>
	/// Grows the real bounds.
	/// </summary>
	/// <param name="p">P.</param>
	public void growRealBounds(Vector2 [] p) {
		for (int i = 0; i < p.Length; i++) {
			growRealBounds (p [i].x, p [i].y);
		}
	}

	/// <summary>
	/// Grows the real bounds.
	/// </summary>
	/// <param name="p">P.</param>
	public void growRealBounds(Vector2 p) {
		growRealBounds (p.x, p.y);
	}

	/// <summary>
	/// Grows the real bounds.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void growRealBounds(float [] x, float [] y) {
		for (int i = 0; i < x.Length; i++) {
			growRealBounds (x [i], y [i]);
		}
	}

	/// <summary>
	/// Grows the real bounds.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	public void growRealBounds(float x1, float y1) {
		xmin = Mathf.Min (x1, xmin);
		xmax = Mathf.Max (x1, xmax);
		ymin = Mathf.Min (y1, ymin);
		ymax = Mathf.Max (y1, ymax);
	}

	/// <summary>
	/// Sets the real bounds.
	/// </summary>
	/// <param name="p">P.</param>
	public void setRealBounds(Vector2 [] p) {
		setRealBounds (getPolyRBounds (p));
	}

	/// <summary>
	/// Sets the real bounds.
	/// </summary>
	/// <param name="xrange">Xrange.</param>
	/// <param name="yrange">Yrange.</param>
	public void setRealBounds(float [] xrange, float [] yrange) {
		if (xrange.Length != 2 || yrange.Length != 2) {
			Debug.LogAssertion ("Incorrect length of extents in DynamicTexture.setRealBounds");
		}
		xmin = xrange [0];
		xmax = xrange [1];
		ymin = yrange [0];
		ymax = yrange [1];
	}

	/// <summary>
	/// Sets the real bounds.
	/// </summary>
	/// <param name="lowerLeft">Lower left.</param>
	/// <param name="upperRight">Upper right.</param>
	public void setRealBounds(Vector2 lowerLeft, Vector2 upperRight) {
		xmin = lowerLeft.x;
		xmax = upperRight.x;
		ymin = lowerLeft.y;
		ymax = upperRight.y;
	}

	/// <summary>
	/// Sets the real bounds.
	/// </summary>
	/// <param name="xmin">Xmin.</param>
	/// <param name="xmax">Xmax.</param>
	/// <param name="ymin">Ymin.</param>
	/// <param name="ymax">Ymax.</param>
	public void setRealBounds(float xmin,float xmax,float ymin, float ymax) {
		this.xmin = xmin;
		this.xmax = xmax;
		this.ymin = ymin;
		this.ymax = ymax;
	}

	/// <summary>
	/// Sets the real bounds.
	/// </summary>
	/// <param name="bounds">Bounds.</param>
	public void setRealBounds(Rect bounds) {
		xmin = bounds.min.x;
		xmax = xmin + bounds.size.x;
		ymin = bounds.min.y;
		ymax = ymin + bounds.size.y;
	}

	/// <summary>
	/// Given a polygon, get bounds to contain the polygon
	/// </summary>
	/// <returns>The bounds.</returns>
	/// <param name="p">P.</param>
	public static Rect getPolyRBounds(Vector2 []p) {
		float xmin = p [0].x;
		float xmax = p [0].x;
		float ymin = p [0].y;
		float ymax = p [0].y;
		for (int i = 1; i < p.Length; i++) {
			xmax = Mathf.Max (xmax, p [i].x);
			xmin = Mathf.Min (xmin, p [i].x);
			ymax = Mathf.Max (ymax, p [i].y);
			ymin = Mathf.Min (ymin, p [i].y);
		}
		Rect retval = new Rect (xmin, ymin, xmax - xmin, ymax - ymin);
		return retval;
	}

	/// <summary>
	/// Given a polygon, get bounds to contain the polygon
	/// </summary>
	/// <returns>The bounds.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public static Rect getPolyRBounds(float [] x, float [] y) {
		float xmin = x [0];
		float xmax = x [0];
		float ymin = y [0];
		float ymax = y [0];
		for (int i = 1; i < x.Length; i++) {
			xmax = Mathf.Max (xmax, x [i]);
			xmin = Mathf.Min (xmin, x [i]);
			ymax = Mathf.Max (ymax, y [i]);
			ymin = Mathf.Min (ymin, y [i]);
		}
		Rect retval = new Rect (xmin, ymin, xmax - xmin, ymax - ymin);
		return retval;
	}

	/// <summary>
	/// Clear to the background color
	/// </summary>
	public void clear() {
		Color32[] pixels = tex.GetPixels32 ();
		for (int i = 0; i < pixels.Length; i++) {
			pixels [i] = background;
		}
		tex.SetPixels32 (pixels);
	}

	/// <summary>
	/// Sets the width.
	/// </summary>
	/// <param name="lw">Lw.</param>
	public void setWidth(int lw) {
		this.lw = lw;
	}

	/// <summary>
	/// Sets the foreground.
	/// </summary>
	/// <param name="color">Color.</param>
	public void setForeground(Color color) {
		foreground = (Color32)color;
	}
	/// <summary>
	/// Sets the foreground.
	/// </summary>
	/// <param name="color">Color.</param>
	public void setForeground(Color32 color) {
		foreground = color;
	}
	/// <summary>
	/// Sets the background.
	/// </summary>
	/// <param name="color">Color.</param>
	public void setBackground(Color32 color) {
		background = color;
	}
	/// <summary>
	/// Sets the background.
	/// </summary>
	/// <param name="color">Color.</param>
	public void setBackground(Color color) {
		background = (Color32) color;
	}

	/// <summary>
	/// Gets the texture.
	/// </summary>
	/// <returns>The texture.</returns>
	public Texture2D getTexture() {
		return tex;
	}

	/// <summary>
	/// Apply this instance.
	/// </summary>
	public void Apply() {
		tex.Apply ();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicTexture"/> class.
	/// </summary>
	public DynamicTexture() {
		tex = new Texture2D (width, height);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicTexture"/> class.
	/// </summary>
	/// <param name="width">Width.</param>
	/// <param name="height">Height.</param>
	public DynamicTexture(int width, int height) {
		this.width = width;
		this.height = height;
		tex = new Texture2D (width, height);
	}

	/// <summary>
	/// converts real values to pixel values in X
	/// </summary>
	/// <returns>The pix x.</returns>
	/// <param name="real">Real.</param>
	public int real2PixX(float real) {
		return (int)((real - xmin) / (xmax - xmin) * width);
	}
	/// <summary>
	/// converts real values to pixel values in X
	/// </summary>
	/// <returns>The pix x.</returns>
	/// <param name="real">Real.</param>
	public int [] real2PixX(float [] real) {
		int[] retval = new int[real.Length];
		for(int i=0;i<real.Length;i++)
			retval[i] = (int)((real[i] - xmin) / (xmax - xmin) * width);
		return retval;
	}
	/// <summary>
	/// converts real values to pixel values in Y
	/// </summary>
	/// <returns>The pix y.</returns>
	/// <param name="real">Real.</param>
	public int real2PixY(float real) {
		return (int)(((real - ymin) / (ymax - ymin)) * height);
	}
	/// <summary>
	/// converts real values to pixel values in Y
	/// </summary>
	/// <returns>The pix y.</returns>
	/// <param name="real">Real.</param>
	public int []real2PixY(float []real) {
		int[] retval = new int[real.Length];
		for (int i = 0; i < real.Length; i++)
			retval [i] = (int)(((real[i] - ymin) / (ymax - ymin)) * height);
		return retval;
	}
	/// <summary>
	/// converts pixel values to real values in X
	/// </summary>
	/// <returns>The real x.</returns>
	/// <param name="pix">Pix.</param>
	public float pix2RealX(int pix) {
		return ((float)pix / (float)width) * (xmax - xmin) + xmin;
	}
	/// <summary>
	/// converts pixel values to real values in Y
	/// </summary>
	/// <returns>The real y.</returns>
	/// <param name="pix">Pix.</param>
	public float [] pix2RealY(int [] pix) {
		float[] retval = new float[pix.Length];
		for(int i=0;i<pix.Length;i++)
			retval[i] = ((float)pix[i] / (float)height) * (ymax - ymin) + ymin;
		return retval;
	}
	/// <summary>
	/// converts pixel values to real values in X
	/// </summary>
	/// <returns>The real x.</returns>
	/// <param name="pix">Pix.</param>
	public float [] pix2RealX(int [] pix) {
		float[] retval = new float[pix.Length];

		for(int i=0;i<pix.Length;i++)
			retval[i] = ((float)pix[i] / (float)width) * (xmax - xmin) + xmin;
		return retval;
	}
	/// <summary>
	/// converts pixel values to real values in Y
	/// </summary>
	/// <returns>The real y.</returns>
	/// <param name="pix">Pix.</param>
	public float pix2RealY(int pix) {
		return ((float)pix / (float)height) * (ymax - ymin) + ymin;
	}

	/// <summary>
	/// Sets a pixel with bound checking
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void safeSetPixel(int x, int y) {
		if (x >= 0 && x <= width && y >= 0 && y <= height)
			tex.SetPixel (x, y, foreground);
	}

	/// <summary>
	/// Draws the point.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="size">Size.</param>
	public void drawPoint(int x1, int y1, int size) {
		for(int x = x1-size/2;x<=x1+size/2;x++) {
			for (int y = y1 - size / 2; y <= y1 + size / 2; y++) {
				safeSetPixel (x, y);
			}
		}
	}

	/// <summary>
	/// Draws the point using real values
	/// </summary>
	/// <param name="point">Point.</param>
	/// <param name="size">Size.</param>
	public void drawPointR(Vector2 point, int size) {
		drawPointR (point.x, point.y, size);
	}

	/// <summary>
	/// Draws the point using real values
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="size">Size.</param>
	public void drawPointR(float x, float y, int size) {
		drawPoint (real2PixX (x), real2PixY (y), size);
	}

	/// <summary>
	/// draws a line using real values
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	public void drawLineR(Vector2 start, Vector2 end) {
		drawLineR (start.x, start.y, end.x, end.y);
	}

	/// <summary>
	/// draws a line using real values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void drawLineR(float x1, float y1, float x2, float y2) {
		drawLine (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2), this.lw);
	}

	/// <summary>
	/// draw a line using pixel values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void drawLine(int x1, int y1, int x2, int y2) {
		drawLine (x1, y1, x2, y2, this.lw);
	}

	/// <summary>
	/// draw a line using real values
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	/// <param name="lw">Lw.</param>
	public void drawLineR(Vector2 start, Vector2 end, int lw) {
		drawLineR (start.x, start.y, end.x, end.y, lw);
	}

	/// <summary>
	/// draw a line using real values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	/// <param name="lw">Lw.</param>
	public void drawLineR(float x1, float y1, float x2, float y2,int  lw) {
		drawLine (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2), lw);
	}

	/// <summary>
	/// draw a line using pixel values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	/// <param name="lw">Lw.</param>
	public void drawLine(int x1, int y1, int x2, int y2, int lw) {

		float slope = (float)(y2 - y1) / (float)(x2 - x1);
		float islope = (float)(x2 - x1) / (float)(y2 - y1);
		float eps = 0.1f;


		int lx = Mathf.Min(lw,(int)(Mathf.Abs(slope) * lw));
		int ly = Mathf.Min(lw,(int)(Mathf.Abs(islope) * lw));
		if (slope == 0.0f) {
			ly = lw;
			lx = 0;
		}
		if (islope == 0.0f) {
			ly = 0;
			lx = lw;
		}
		//Debug.Log ("" + lx + " " + ly);

		if (Mathf.Abs (slope) > 1.0 - eps && Mathf.Abs (slope) < 1.0 + eps) {

			if (x2 > x1) {
				for (int x = x1 + 1; x < x2; x++) {
					int y = (int)(slope * (x - x1)) + y1;
					for (int yd = y - lw / 2; yd <= y + lw / 2; yd++) {
						safeSetPixel (x, yd);
					}
				}
			} else {
				for (int x = x2 + 1; x < x1; x++) {
					int y = (int)(slope * (x - x1)) + y1;
					for (int yd = y - lw / 2; yd <= y + lw / 2; yd++) {
						safeSetPixel (x, yd);
					}
				}
			}

			if (y2 > y1) {
				for (int y = y1 + 1; y < y2; y++) {
					int x = (int)(islope * (y - y1) + x1);
					for (int xd = x - lw / 2; xd <= x + lw / 2; xd++) {
						safeSetPixel (xd, y);
					}
				}
			} else {
				for (int y = y2 + 1; y < y1; y++) {
					int x = (int)(islope * (y - y1) + x1);
					for (int xd = x - lw / 2; xd <= x + lw / 2; xd++) {
						safeSetPixel (xd, y);
					}
				}
			}

			int sign = 1;

			if (slope < 0.0) {
				if (x1 > x2)
					sign = -1;
				for (int x = x1 - sign * lx / 2; sign * x <= sign * x1; x += sign * 1) {
					for (int y = y1 + sign * (ly+1) / 2; sign * y >= sign * y1; y -= sign * 1) {
						safeSetPixel (x, y);
					}
				}
				for (int x = x2 + sign * lx / 2; sign * x >= sign * x2; x -= sign * 1) {
					for (int y = y2 - sign * (ly+1) / 2; sign * y <= sign * y2; y += sign * 1) {
						safeSetPixel (x, y);
					}
				}
			} else {
				if (x1 > x2)
					sign = -1;
				for (int x = x1 - sign * lx / 2; sign * x <= sign * x1; x += sign * 1) {
					for (int y = y1 - sign * (ly+1) / 2; sign * y <= sign * y1; y += sign * 1) {
						safeSetPixel (x, y);
					}
				}
				for (int x = x2 + sign * lx / 2; sign * x >= sign * x2; x -= sign * 1) {
					for (int y = y2 + sign * (ly+1) / 2; sign * y >= sign * y2; y -= sign * 1) {
						safeSetPixel (x, y);
					}
				}
			}
		} else if (Mathf.Abs (slope) < 1.0) {
			int sign = 1;
			if (x2 < x1)
				sign = -1;
			for (int x = x1; sign * x < sign * x2; x += sign * 1) {
				int y = (int)(slope * (x - x1)) + y1;
				for (int yd = y - (ly) / 2; yd <= y + (ly) / 2; yd++) {
					safeSetPixel (x, yd);
				}
			}
		} else {
			int sign = 1;
			if (y2 < y1)
				sign = -1;
			for (int y = y1; sign * y < sign * y2; y += sign * 1) {
				int x = (int)(islope * (y - y1)) + x1;
				for (int xd = x - (lx+1) / 2; xd <= x + (lx) / 2; xd++) {
					safeSetPixel (xd, y);
				}
			}
		}
	}

	/// <summary>
	/// Fills the oval using real values.
	/// </summary>
	/// <param name="lowerLeft">Lower left.</param>
	/// <param name="upperRight">Upper right.</param>
	public void fillOvalR(Vector2 lowerLeft, Vector2 upperRight) {
		fillOval (real2PixX (lowerLeft.x), real2PixY (lowerLeft.y), real2PixX (upperRight.x), real2PixY (upperRight.y));
	}
	/// <summary>
	/// Fills the oval using real values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void fillOvalR(float x1, float y1, float x2, float y2) {
		fillOval (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2));
	}

	/// <summary>
	/// Fills the oval using pixel values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void fillOval(int x1, int y1, int x2, int y2) {
		int cx = (x1 + x2) / 2;
		int cy = (y1 + y2) / 2;
		int sx = Mathf.Abs (x2 - x1);
		int sy = Mathf.Abs (y2 - y1);


		for (int x = cx - (sx+1) / 2; x <= cx + sx / 2; x++) {
			int yp = (int) (
				(float)sy / 2.0f * Mathf.Sqrt (
					Mathf.Max(0.0f,1.0f - ((float)(x-cx) / ((float)sx / 2.0f)) * ((float)(x-cx) / ((float)sx / 2.0f)))
				)
			);

			for (int y = cy - yp; y <= cy + yp; y++) {
				safeSetPixel (x, y);
			}
		}

	}

	/// <summary>
	/// Draws the oval using real values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void drawOvalR(float x1,float y1,float x2,float y2) {
		drawOval (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2));
	}

	/// <summary>
	/// Draws the oval using real values
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	public void drawOvalR(Vector2 p1, Vector2 p2) {
		drawOval (real2PixX (p1.x), real2PixY (p1.y), real2PixX (p2.x), real2PixY (p2.y));
	}

	/// <summary>
	/// Draws the oval using pixel values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void drawOval(int x1, int y1, int x2, int y2) {
		drawOval (x1, y1, x2, y2, this.lw);
	}

	/// <summary>
	/// Draws the oval using real values
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	/// <param name="lw">Lw.</param>
	public void drawOvalR(Vector2 p1, Vector2 p2, int lw) {
		drawOval (real2PixX (p1.x), real2PixY (p1.y), real2PixX (p2.x), real2PixY (p2.y), lw);
	}

	/// <summary>
	/// Draws the oval using real values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="y2">The second y value.</param>
	/// <param name="lw">Lw.</param>
	public void drawOvalR(float x1, float x2, float y1, float y2, int lw) {
		drawOval (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2), lw);
	}

	/// <summary>
	/// Draws the oval using pixel values
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	/// <param name="lw">Lw.</param>
	public void drawOval(int x1, int y1, int x2, int y2, int lw) {
		int cx = (x1 + x2) / 2;
		int cy = (y1 + y2) / 2;
		int sx = Mathf.Abs (x2 - x1);
		int sy = Mathf.Abs (y2 - y1);


		for (int x = cx - (sx+1) / 2; x <= cx + sx / 2; x++) {

			int yp = (int) (
				(float)sy / 2.0f * Mathf.Sqrt (
					Mathf.Max(0.0f,1.0f - ((float)(x-cx) / ((float)sx / 2.0f)) * ((float)(x-cx) / ((float)sx / 2.0f)))
				)
			);

			drawPoint (x, cy+yp, lw);
			drawPoint (x, cy-yp, lw);
		}

		for (int y = cy - (sy+1) / 2; y <= cy + sy / 2; y++) {

			int xp = (int) (
				(float)sx / 2.0f * Mathf.Sqrt (
					Mathf.Max(0.0f,1.0f - ((float)(y-cy) / ((float)sy / 2.0f)) * ((float)(y-cy) / ((float)sy / 2.0f)))
				)
			);

			drawPoint (cx+xp, y, lw);
			drawPoint (cx-xp, y, lw);
		}
	}

	/// <summary>
	/// Draws the rect using real values.
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	public void drawRectR(Vector2 p1, Vector2 p2) {
		drawRect (real2PixX (p1.x), real2PixY (p1.y), real2PixX (p2.x), real2PixY (p2.y));
	}

	/// <summary>
	/// Draws the rect using real values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void drawRectR(float x1, float y1, float x2, float y2) {
		drawRect (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2));
	}

	/// <summary>
	/// Draws the rect using pixel values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void drawRect(int x1, int y1, int x2, int y2) {
		drawRect (x1, y1, x2, y2, this.lw);
	}

	/// <summary>
	/// Draws the rect using real values.
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	/// <param name="lw">Lw.</param>
	public void drawRectR(Vector2 p1, Vector2 p2, int lw) {
		drawRect (real2PixX (p1.x), real2PixY (p1.y), real2PixX (p2.x), real2PixY (p2.y), lw);
	}

	/// <summary>
	/// Draws the rect using real values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	/// <param name="lw">Lw.</param>
	public void drawRectR(float x1, float y1, float x2, float y2, int lw) {
		drawRect (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2), lw);
	}

	/// <summary>
	/// Draws the rect using pixel values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	/// <param name="lw">Lw.</param>
	public void drawRect(int x1, int y1, int x2, int y2, int lw) {
		int sign = 1;
		if (y2 < y1)
			sign = -1;
		drawLine (x1, y1-sign*lw/2, x1, y2+sign*lw/2, lw);
		drawLine (x2, y2+sign*lw/2, x2, y1-sign*lw/2, lw);

		sign = 1;
		if (x2 < x1)
			sign = -1;
		drawLine (x1-sign*lw/2, y2, x2+sign*lw/2, y2, lw);
		drawLine (x2+sign*lw/2, y1, x1-sign*lw/2, y1, lw);

	}

	/// <summary>
	/// Draws the poly using real values.
	/// </summary>
	/// <param name="points">Points.</param>
	/// <param name="closed">If set to <c>true</c> closed.</param>
	public void drawPolyR(Vector2 [] points, bool closed = true) {
		float[] x = new float[points.Length];
		float[] y = new float[points.Length];
		for (int i = 0; i < points.Length; i++) {
			x [i] = points [i].x;
			y [i] = points [i].y;
		}
		drawPoly (real2PixX (x), real2PixY (y), closed);
	}
	/// <summary>
	/// Draws the poly using real values.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="closed">If set to <c>true</c> closed.</param>
	public void drawPolyR(float [] x, float [] y, bool closed = true) {
		drawPoly (real2PixX (x), real2PixY (y), closed);
	}

	/// <summary>
	/// Draws the poly using pixel values.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="closed">If set to <c>true</c> closed.</param>
	public void drawPoly(int [] x, int [] y, bool closed = true) {

		drawPoly (x, y, this.lw, closed);
	}

	/// <summary>
	/// Draws the poly using pixel values.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="lw">Lw.</param>
	/// <param name="closed">If set to <c>true</c> closed.</param>
	public void drawPoly(float [] x, float [] y, int lw, bool closed = true) {
		drawPoly (real2PixX (x), real2PixY (y), lw, closed);
	}


	/// <summary>
	/// Draws the poly using pixel values.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="lw">Lw.</param>
	/// <param name="closed">If set to <c>true</c> closed.</param>
	public void drawPoly(int [] x, int [] y, int lw, bool closed = true) {
		int stop = x.Length - 1;
		if (closed)
			stop = x.Length;
		for (int i = 0; i < stop; i++) {
			drawLine (x [i], y [i], x [(i + 1)%x.Length], y [(i + 1)%y.Length], lw);
		}
	}

	/// <summary>
	/// Fills the rect using real values.
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	public void fillRectR(Vector2 p1, Vector2 p2) {
		fillRect (real2PixX (p1.x), real2PixY (p1.y), real2PixX (p2.x), real2PixY (p2.y));
	}
	/// <summary>
	/// Fills the rect using real values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void fillRectR(float x1, float y1, float x2, float y2) {
		fillRect (real2PixX (x1), real2PixY (y1), real2PixX (x2), real2PixY (y2));
	}
	/// <summary>
	/// Fills the rect using pixel values.
	/// </summary>
	/// <param name="x1">The first x value.</param>
	/// <param name="y1">The first y value.</param>
	/// <param name="x2">The second x value.</param>
	/// <param name="y2">The second y value.</param>
	public void fillRect(int x1, int y1, int x2, int y2) {
		int xstart = x1;
		int ystart = y1;
		int xstop = x2;
		int ystop = y2;
		if (x2 < x1) {
			xstart = x2;
			xstop = x1;
		}
		if (y2 < y1) {
			ystart = y2;
			ystop = y1;
		}
		for (int x = xstart; x <= xstop; x++) {
			for (int y = ystart; y <= ystop; y++) {
				safeSetPixel (x, y);
			}
		}
	}
		
}
