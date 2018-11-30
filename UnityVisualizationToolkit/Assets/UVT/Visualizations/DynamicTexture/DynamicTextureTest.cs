using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamic texture test.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class DynamicTextureTest : MonoBehaviour {

	public int width = 1024;
    public int height = 1024;
	DynamicTexture dt = null;
    public int lineWidth = 4;
	SpriteRenderer sr;

	/// <summary>
	/// Creates a set of test data for development purposes.
	/// This routine will eventually be removed.
	/// </summary>
	/// <returns>The data.</returns>
	public Vector2[] testData() {
		int n = 100;
		Vector2[] retval = new Vector2[n];
		float step = 10.0f / (n - 1);
		for (int i = 0; i < n; i++) {
			retval [i].x = step * i;
			retval [i].y = Mathf.Sin (retval [i].x);
		}
		return retval;
	}

	// Use this for initialization
	void Awake () {
		sr = GetComponentInChildren<SpriteRenderer> ();
		dt = new DynamicTexture (width,height);
		dt.clear();

		Vector2[] segments = testData ();
        
        dt.setRealBounds (segments);

        dt.setWidth(lineWidth);
        dt.drawPolyR(segments,false);

        dt.Apply ();
		sr.sprite = Sprite.Create (dt.getTexture(), new Rect (0, 0, width, height), new Vector2 (0.5f, 0.5f));
		//sr.material.shader = 

        //Bounds spriteBounds = sr.sprite.bounds;
        //transform.localScale = new Vector3(1.0f / spriteBounds.size.x, 1.0f / spriteBounds.size.y,1.0f);

    }

	// Update is called once per frame
	void Update () {

	}
}
