using UnityEngine;
using System.Collections;

/// <summary>
/// 3D mesh of a function f(x,)
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class MeshGrid : MonoBehaviour {

	Material mat;

	Mesh m;
	float [] y;
	float [] x;
	float [] z;
	float [] u; 
	float alpha = 1.0f;
	public float zScale = 1.0f;
	Mesh mFlipped = null;

	ColorMap cm = null;
	Color color = Color.white;

	public void setColorMap(ColorMap cm) {
		this.cm = cm;
	}

	public static GameObject CreateGameObject(Vector3 pos, Vector3 scale, Transform parent) {
		GameObject mgGO = new GameObject ();
		MeshGrid mg = mgGO.AddComponent<MeshGrid> () as MeshGrid;
		mgGO.transform.parent = parent;
		mgGO.transform.position = pos;
		mgGO.transform.localPosition = Vector3.zero;
		mgGO.transform.localScale = Vector3.one;
		return mgGO;
	}

	Color evalGScale(float value) {
		return cm.eval (value);
	}

	Color evalGScale(float value, float minGradientScale, float maxGradientScale) {
		return cm.eval (value, minGradientScale, maxGradientScale);
	}

	public void setColor(Color color) {
		this.color = color;
	}

	public Mesh createGrid(StructuredData theData, string xName, string yName, string zName, string cName) {
		x = theData.getDimension (xName);
		y = theData.getDimension (yName);
		z = theData.getValues (zName);
		if (cName != null) {
			u = theData.getValues (cName);
			return createGrid (x, y, z, u);
		} else {
			return createGrid (x, y, z, null);
		}
	}

	public Mesh createGrid (double [] xd, double [] yd, double [] zd) {
		x = new float[xd.Length];
		y = new float[yd.Length];
		z = new float[zd.Length];

		for (int i = 0; i < xd.Length; i++)
			x [i] = (float)xd [i];
		for (int j = 0; j < yd.Length; j++)
			y [j] = (float)yd [j];
		for (int ij = 0; ij < zd.Length; ij++)
			z [ij] = (float)zd [ij];
		return createGrid (x,y,z,null);
	}

	public Mesh createGrid (double [] xd, double [] yd, double [] zd, double [] ud) {
		x = new float[xd.Length];
		y = new float[yd.Length];
		z = new float[zd.Length];
		u = new float[ud.Length];

		for (int i = 0; i < xd.Length; i++)
			x [i] = (float)xd [i];
		for (int j = 0; j < yd.Length; j++)
			y [j] = (float)yd [j];
		for (int ij = 0; ij < zd.Length; ij++)
			z [ij] = (float)zd [ij];
		for (int ij = 0; ij < ud.Length; ij++)
			u [ij] = (float)ud [ij];
		return createGrid (x,y,z,u);
	}

	public Mesh remapGrid(StructuredData theData,string xname, string yname) {
		float[] x_mapped = theData.getValues (xname);
		float[] y_mapped = theData.getValues (yname);
		return remapGrid (x_mapped, y_mapped);
	}

	public Mesh remapToSphere()
	{
		return remapToSphere(-90,90,-180,180,1);
	}

	static Mesh CopyMesh(Mesh mesh)
	{
		Mesh newmesh = new Mesh();
		newmesh.vertices = mesh.vertices;
		newmesh.triangles = mesh.triangles;
		newmesh.uv = mesh.uv;
		newmesh.normals = mesh.normals;
		newmesh.colors = mesh.colors;
		newmesh.tangents = mesh.tangents;
		return newmesh;
	}

	public Mesh remapToSphere(float latMin, float latMax, float lonMin, float lonMax, float R, bool latLonFlipped = false)
	{

		// scale x and y from min/max to lat/lon around sphere.
		int nx = x.Length;
		int ny = y.Length;

		Vector3[] vertices = new Vector3[nx * ny];
	
		for (int i = 0; i < nx; i++)
		{
			for (int j = 0; j < ny; j++)
			{
				int iv = ny * i + j;
				float lon = 0;
				float lat = 0;

				if(latLonFlipped)
				{
					lon = (y[j] - lonMin) / (lonMax - lonMin) * 2.0f * Mathf.PI;
					lat = (latMax - x[i]) / (latMax - latMin) * Mathf.PI;
				} else
				{
					lon = (x[i] - lonMin) / (lonMax - lonMin) * 2.0f * Mathf.PI;
					lat = (latMax - y[j]) / (latMax - latMin) * Mathf.PI;
				}

				float xmapped = R * Mathf.Sin(lat) * Mathf.Cos(lon);
				float ymapped = R * Mathf.Sin(lat) * Mathf.Sin(lon);
				float zmapped = R * Mathf.Cos(lat);

				vertices[iv] = new Vector3(xmapped,zmapped, ymapped);
				vertices[iv] += vertices[iv].normalized*zScale*z[i*ny+j];
			}
		}
				
		m.vertices = vertices;
		
		m.RecalculateBounds();
		m.RecalculateNormals();

		mFlipped = CopyMesh(m);


		int[] triangles = mFlipped.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int temp = triangles[i + 1];
			triangles[i + 1] = triangles[i + 2];
			triangles[i + 2] = temp;
		}
		mFlipped.triangles = triangles;

		vertices = mFlipped.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] += 0.000001f * Vector3.one;
		}
		mFlipped.vertices = vertices;

		return m;
	}

	public Mesh remapGrid(float [] x_mapped, float []y_mapped) {
		// remap grid to a non-rectilinear x-y set of values
		int nx = x.Length;
		int ny = y.Length;

		Vector3[] vertices = new Vector3[nx * ny];
		Vector2[] uvs = new Vector2[nx * ny];
		Color[] colors = new Color[nx * ny];
		int[] triangles = new int[(nx - 1) * (ny - 1) * 6];

		for(int i=0;i<nx;i++) {
			for (int j = 0; j < ny; j++) {
				int iv = nx * i + j;

				vertices [iv] = new Vector3 ((float)x_mapped [i*ny+j], zScale*(float)z[i*ny+j], (float)y_mapped [i*ny+j]);
				uvs [iv] = new Vector2 ((float)i / (float)nx, (float)j / (float)ny);
				if(i<nx-1&&j<ny-1) {
					int it = i * (ny - 1) + j;
					triangles [it * 6] = iv;
					triangles [it * 6 + 1] = iv + 1;
					triangles [it * 6 + 2] = iv + ny + 1;
					triangles [it * 6 + 3] = iv;
					triangles [it * 6 + 4] = iv + ny + 1;
					triangles [it * 6 + 5] = iv + ny;
				}
				if (u != null)
					colors [iv] = evalGScale (u [i * ny + j]);
				else
					colors [iv] = color;
			}
		}
		m.vertices = vertices;
		//m.uv = uvs;
		m.triangles = triangles;
		m.colors = colors;
		m.RecalculateBounds ();
		m.RecalculateNormals ();
		//mr.material = new Material (Shader.Find ("Unlit/VertexColorUnlit"));
		//mr.material = new Material (Shader.Find ("AlphaVertexUnlit"));
		//mf.mesh = m;
		//mc.sharedMesh = m;

		mFlipped = CopyMesh(m);


		triangles = mFlipped.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int temp = triangles[i + 1];
			triangles[i + 1] = triangles[i + 2];
			triangles[i + 2] = temp;
		}
		mFlipped.triangles = triangles;

		vertices = mFlipped.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] += 0.000001f * Vector3.one;
		}
		mFlipped.vertices = vertices;

		return m;
	}

	public Mesh updateGrid() {
		
		int nx = x.Length;
		int ny = y.Length;

		Vector3[] vertices = new Vector3[nx * ny];
		Vector2[] uvs = new Vector2[nx * ny];
		Color[] colors = new Color[nx * ny];
		int[] triangles = new int[(nx - 1) * (ny - 1) * 6];


		for(int i=0;i<nx;i++) {
			for (int j = 0; j < ny; j++) {
				int iv = ny * i + j;

				vertices [iv] = new Vector3 ((float)x [i], zScale*(float)z[i*ny+j], (float)y [j]);
				uvs [iv] = new Vector2 ((float)i / (float)nx, (float)j / (float)ny);
				if(i<nx-1&&j<ny-1) {
					int it = i * (ny - 1) + j;
					triangles [it * 6] = iv;
					triangles [it * 6 + 1] = iv + 1;
					triangles [it * 6 + 2] = iv + ny + 1;
					triangles [it * 6 + 3] = iv;
					triangles [it * 6 + 4] = iv + ny + 1;
					triangles [it * 6 + 5] = iv + ny;
				}
				if (u != null)
					colors [iv] = evalGScale (u [i * ny + j]);
				else
					colors [iv] = color;
			}
		}
		m.vertices = vertices;
		//m.uv = uvs;
		m.triangles = triangles;
		m.colors = colors;
		m.RecalculateBounds ();
		m.RecalculateNormals ();
		//mr.material = new Material (Shader.Find ("Unlit/VertexColorUnlit"));
		//mr.material = new Material (Shader.Find ("AlphaVertexUnlit"));
		//mf.mesh = m;
		//mc.sharedMesh = m;


		mFlipped = CopyMesh(m);


		triangles = mFlipped.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int temp = triangles[i + 1];
			triangles[i + 1] = triangles[i + 2];
			triangles[i + 2] = temp;
		}
		mFlipped.triangles = triangles;

		vertices = mFlipped.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] += 0.000001f * Vector3.one;
		}
		mFlipped.vertices = vertices;

		return m;
	}

	public void setZ(double [] zd) {
		z = new float[zd.Length];
		for (int ij = 0; ij < z.Length; ij++)
			z [ij] = (float)zd [ij];
	}

	public void setZ(float [] z) {
		this.z = z;
	}

	public Mesh  createGrid(float [] x, float [] y, float [] z) {
		return createGrid (x, y, z, null);
	}

	public Mesh createGrid() {
		return createGrid (x, y, z, u);
	}

	public void setColors (double [] ud) {
		u = new float[ud.Length];

		for (int ij = 0; ij < ud.Length; ij++) {
			u [ij] = (float)ud [ij];
		}
		setColors (u);
	}

	public void setColors(float [] u) {
		this.u = u;
		int nx = x.Length;
		int ny = y.Length;
		Color[] colors = new Color[nx * ny];

		for(int i=0;i<nx;i++) {
			for (int j = 0; j < ny; j++) {
				int iv = ny * i + j;
				if (u != null)
					colors [iv] = evalGScale (u [i * ny + j]);
				else
					colors [iv] = color;
			}
		}
		m.colors = colors;
		//mf.mesh = m;
		//mc.sharedMesh = m;
	}

	public Mesh createGrid(float [] x, float [] y, float [] z, float [] u) {
			
		int nx = x.Length;
		int ny = y.Length;

		Vector3[] vertices = new Vector3[nx * ny];
		Vector2[] uvs = new Vector2[nx * ny];
		Color[] colors = new Color[nx * ny];
		int[] triangles = new int[(nx - 1) * (ny - 1) * 6];

		//mgo = new GameObject ("MeshGrid");
		//mf = mgo.AddComponent (typeof(MeshFilter)) as MeshFilter;
		//mr = mgo.AddComponent (typeof(MeshRenderer)) as MeshRenderer;
		//mc = mgo.AddComponent (typeof(MeshCollider)) as MeshCollider;
		m = new Mesh ();

		for(int i=0;i<nx;i++) {
			for (int j = 0; j < ny; j++) {
				int iv = ny * i + j;

				vertices [iv] = new Vector3 ((float)x [i], zScale*(float)z[iv], (float)y [j]);
				uvs [iv] = new Vector2 ((float)i / (float)nx, (float)j / (float)ny);
				if(i<nx-1&&j<ny-1) {
					int it = i * (ny - 1) + j;
					triangles [it * 6] = iv;
					triangles [it * 6 + 1] = iv + 1;
					triangles [it * 6 + 2] = iv + ny + 1;
					triangles [it * 6 + 3] = iv;
					triangles [it * 6 + 4] = iv + ny + 1;
					triangles [it * 6 + 5] = iv + ny;
				}
				if (u != null)
					colors [iv] = evalGScale (u [iv]);
				else
					colors [iv] = color;
			}
		}
		m.vertices = vertices;
		//m.uv = uvs;
		m.triangles = triangles;
		m.colors = colors;
		m.RecalculateBounds ();
		m.RecalculateNormals ();
		//mr.material = new Material (Shader.Find ("Unlit/VertexColorUnlit"));
		//mr.material = new Material (Shader.Find ("AlphaVertexUnlit"));
		//mf.mesh = m;
		//mc.sharedMesh = m;

		mFlipped = CopyMesh(m);


		triangles = mFlipped.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int temp = triangles[i + 1];
			triangles[i + 1] = triangles[i + 2];
			triangles[i + 2] = temp;
		}
		mFlipped.triangles = triangles;

		vertices = mFlipped.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] += 0.000001f * Vector3.one;
		}
		mFlipped.vertices = vertices;
		return m;
	}

	public void Update() {
		if (m != null) {

			//Material mat = 	new Material (Shader.Find ("AlphaVertexUnlit"));
			Material mat = 	new Material (Shader.Find ("Particles/Standard Unlit"));
			mat.SetFloat("_Mode", 2);
			mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			mat.SetInt("_ZWrite", 0);
			mat.DisableKeyword("_ALPHATEST_ON");
			mat.EnableKeyword("_ALPHABLEND_ON");
			mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			mat.renderQueue = 3000;
			Matrix4x4 trs = Matrix4x4.TRS (transform.position, transform.rotation, 
				                Vector3.Scale (transform.parent.localScale, transform.localScale));
			//Graphics.DrawMesh (m, transform.position, Quaternion.identity, mat, 0);
			Graphics.DrawMesh (m, trs, mat, 0);
			Graphics.DrawMesh(mFlipped, trs, mat, 1);

		}
	}

}
