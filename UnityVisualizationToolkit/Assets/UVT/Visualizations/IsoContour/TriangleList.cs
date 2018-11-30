using UnityEngine;
using System.Collections;


/// <summary>
/// triangle list class for isocontour creation
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class TriangleList {

	int n_vertices_padded = 20;
	int n_vertices;
	Vector3 [] vertices;
	public Color [] colors;
	int[] triangles;
	int n_triangles_padded = 60;
	int n_triangles;
	float tolerance = 1.0e-3f;
	float toleranceSquared;

	public TriangleList() {
		vertices = new Vector3[n_vertices_padded];
		colors = new Color[n_vertices_padded];
		for (int i = 0; i < n_vertices_padded; i++) {
			colors [i] = Color.white;
		}
		triangles = new int[n_triangles_padded*3];
		n_vertices = 0;
		n_triangles = 0;
		toleranceSquared = tolerance * tolerance;
	}

	void growVertices() {
		Vector3 [] tempVertices = new Vector3[n_vertices_padded * 2];
		Color [] tempColors = new Color[n_vertices_padded * 2];
		for (int i = 0; i < n_vertices; i++) {
			tempVertices [i] = vertices [i];
			tempColors [i] = colors [i];
		}
		vertices = tempVertices;
		colors = tempColors;
		n_vertices_padded *= 2;
	}

	void growTriangles() {
		int [] tempTriangles = new int[n_triangles_padded *3* 2];
		for (int i = 0; i < n_triangles; i++) {
			tempTriangles [3*i] = triangles [3*i];
			tempTriangles [3*i+1] = triangles [3*i+1];
			tempTriangles [3*i+2] = triangles [3*i+2];
		}
		triangles = tempTriangles;
		n_triangles_padded *= 2;
	}

	/// <summary>
	/// Adds a vertex to tthe triangle list
	/// </summary>
	/// <returns>The vertex.</returns>
	/// <param name="v">V.</param>
	/// <param name="c">C.</param>
	public int addVertex(Vector3 v, Color c) {
		if (n_vertices == n_vertices_padded)
			growVertices ();
		colors [n_vertices] = c;
		vertices [n_vertices++] = v;
		return n_vertices - 1;
	}

	public int addVertex(Vector3 v) {
		return addVertex (v, Color.white);
	}

	public int vertexIndex(Vector3 v) {
		return vertexIndex (v, Color.white);
	}

	public int vertexIndex(Vector3 v, Color c) {
		/*
		for (int i = 0; i < n_vertices; i++) {
			if ((v - vertices [i]).sqrMagnitude < toleranceSquared) {
				return i;
			}
		}
		*/

		return addVertex (v,c);
	}

	public void addList(TriangleList tl) {
		int []tlt = tl.getTriangles ();
		Vector3[] tlv = tl.getVertices ();
		Color[] tlc = tl.getColors ();
		for (int i = 0; i < tlt.Length ; i+=3) {
			addTriangle (tlv [tlt [i]], tlv [tlt [i + 1]], tlv [tlt [i + 2]], tlc [tlt [i]], tlc [tlt [i + 1]], tlc [tlt [i + 2]]);
		}
	}

	public void addTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3) {
		//is vertex a new vertex?
		int i1 = vertexIndex (v1,c1);
		int i2 = vertexIndex (v2,c2);
		int i3 = vertexIndex (v3,c3);
		if (n_triangles +6 >= n_triangles_padded)
			growTriangles ();
		triangles [3 * n_triangles] = i1;
		triangles [3 * n_triangles + 1] = i2;
		triangles [3 * n_triangles + 2] = i3;
		n_triangles += 1;
	}

	public void addTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		addTriangle (v1, v2, v3, Color.white, Color.white, Color.white);
	}

	public Vector3[] getUpNormals() {
		Vector3 [] retval = new Vector3[n_vertices];
		for (int i = 0; i < n_vertices; i++)
			retval [i] = Vector3.up;
		return retval;
	}

	public Vector3[] getVertices() {
		Vector3 [] retval = new Vector3[n_vertices];
		for (int i = 0; i < n_vertices; i++)
			retval [i] = vertices [i];
		return retval;
	}

	public Color[] getColors() {
		Color[] retval = new Color[n_vertices];
		for (int i = 0; i < n_vertices; i++) {
			retval [i] = colors [i];
		}
		return retval;
	}

	public Vector2[] getYViewUVs() {
		Vector2[] retval = new Vector2[n_vertices];
		float min_x, max_x, min_z, max_z;
		min_x = vertices [0].x;
		max_x = min_x;
		min_z = vertices [0].z;
		max_z = min_z;
		for (int i = 1; i < n_vertices; i++) {
			if (vertices [i].x < min_x)
				min_x = vertices [i].x;			
			if (vertices [i].z < min_z)
				min_z = vertices [i].z;			
			if (vertices [i].x > max_x)
				max_x = vertices [i].x;			
			if (vertices [i].z > max_z)
				max_z = vertices [i].z;
		}
		for (int i = 0; i < n_vertices; i++) {
			retval [i] = new Vector2 ((vertices [i].x - min_x) / (max_x - min_x),
				(vertices [i].z - min_z) / (max_z - min_z));
		}
		return retval;
	}

	public int [] getTriangles() {
		int[] retval = new int[n_triangles * 3];
		for (int i = 0; i < n_triangles * 3; i++)
			retval [i] = triangles [i];
		return retval;
	}



}
