using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;

/// <summary>
/// The Threshhold plot draws cubes based on an OctTree storage of all
/// space within a range that falls within a threshhold value.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Threshhold2 : MonoBehaviour {

	[DllImport("ThreshholdMeshReduction",EntryPoint="reduceMesh")]
	private static extern void reduceMesh (double [] x, double [] y, double [] z, 
		double [] nx, double [] ny, double [] nz, int [] tri, int [] sizes, double size, int maxDepth);


	public GameObject cubePRE = null;
	Mesh m;
	AdaptiveArrayVector3 vertices;
	AdaptiveArrayVector3 normals;
	AdaptiveArray<int> triangles;


	float [] findRange(float [] x) {
		float [] xRange = new float[2];
		xRange [0] = x [0];
		xRange [1] = x [0];
		for (int i = 1; i < x.Length; i++) {
			xRange [0] = Mathf.Min (xRange [0], x [i]);
			xRange [1] = Mathf.Max (xRange [1], x [i]);
		}
		return xRange;
	}

	OctTreeNode theTree;

	public string filename = "";
	public enum FileType {
		STRUCTURED, UNSTRUCTURED, CSV
	};
	public FileType fileType = FileType.CSV;
	public string xName = "x";
	public string yName = "y";
	public string zName = "z";
	public string dataName = "data";

	public LinkedList cubes;

	public float [] xRange = null;
	public float [] yRange = null;
	public float [] zRange = null;
	public float cmin;
	public float cmax;
	public enum valueChoice {
		min, max, range
	}
	int nx = 10;
	int ny = 10;
	int nz = 10;

	public Color color = Color.blue;

	public valueChoice choice;

	public float [] linspace(float min, float max, int n) {
		float[] retval = new float[n];
		retval [0] = min;
		retval [n - 1] = max;
		float step = (max - min) / (float)(n - 1);
		for (int i = 1; i < n - 1; i++) {
			retval [i] = min + i * step;
		}
		return retval;
	}

	public class LinkedList {


		public class ListNode {

			public OctTreeNode storedObject = null;
			public float value = 0.0f;
			public ListNode next = null;

			public OctTreeNode getStoredObject(){
				return storedObject;
			}

			public ListNode(OctTreeNode storedObject, float value) {
				this.storedObject = storedObject;
				this.value = value;
				next = null;
			}
			public ListNode(OctTreeNode storedObject) {
				this.storedObject= storedObject;
			}
			public ListNode(float value) {
				this.value= value;
			}
			public ListNode() {
				value = 0.0f;
				next = null;
			}

			public ListNode(ListNode ln) {
				storedObject = ln.storedObject;
				value = ln.value;
				next = ln.next;
			}

			public int remaining() {
				if (next == null) {
					return 1;
				} else {
					return 1 + next.remaining ();
				}
			}

			public ListNode find(int n) {
				if (n == 0) {
					return this;
				} else {
					return next.find (n - 1);
				}
			}
			public ListNode prior(ListNode ln) {
				if (next == ln) {
					return this;
				} else {
					return next.prior (ln);
				}
			}
			public void printNode() {
				Debug.Log (storedObject);
				Debug.Log(value);
			}
			public void printNodes() {
				Debug.Log (storedObject);
				Debug.Log(value);
				if (next != null) {
					next.printNodes ();
				}
			}
		}

		public ListNode first = null;
		public ListNode last = null;

		public LinkedList copy() {
			LinkedList retval = new LinkedList ();
			for (int i = 0; i < length (); i++) {
				retval.pushLast (first.find (i));
			}
			return retval;
		}

		public ListNode getHead(){
			if (first == null) {
				return null;
			}
			return first.find(0);
		}

		public OctTreeNode getObject(ListNode head){
			return head.getStoredObject ();
		}

		public void push(ListNode ln_in) {
			ListNode ln = new ListNode (ln_in);
			ListNode oldFirst = first;
			first = ln;
			if (oldFirst == null) {
				last = first;
			}
			first.next = oldFirst;
		}
		public void push(OctTreeNode storedObject) {
			ListNode ln = new ListNode (storedObject);
			push (ln);
		}
		public void push(OctTreeNode storedObject, float value) {
			ListNode ln = new ListNode (storedObject,value);
			push (ln);
		}
		public void push(float value) {
			ListNode ln = new ListNode (value);
			push (ln);
		}
		public void pushLast(ListNode ln_in) {
			ListNode ln = new ListNode (ln_in);
			if (last == null) {
				first = ln;
			} else {
				last.next = ln;
			}
			last = ln;
		}
		public void pushLast(float value) {
			ListNode ln = new ListNode (value);
			pushLast(ln);
		}
		public void pushLast(OctTreeNode storedObject, float value) {
			ListNode ln = new ListNode (storedObject,value);
			pushLast(ln);
		}
		public void pushLast(OctTreeNode storedObject) {
			ListNode ln = new ListNode (storedObject);
			pushLast(ln);
		}
		public ListNode pop() {
			ListNode retval = first;
			first = first.next;
			return retval;
		}
		public ListNode popLast() {
			ListNode oldLast = last;
			last = first.prior (last);
			last.next = null;
			return oldLast;
		}
		public int length() {
			if (first != null)
				return first.remaining ();
			else
				return 0;
		}
		public void printList() {
			first.printNodes ();
		}
		public LinkedList subList(int count) {
			LinkedList retval = new LinkedList ();
			for (int i = 0; i < count; i++) {
				retval.pushLast (first.find (i));
			}
			retval.last.next = null;
			return retval;
		}
		public LinkedList subList(int start, int stop) {
			LinkedList retval = new LinkedList ();
			for (int i = start; i < stop; i++) {
				retval.pushLast (first.find (i));
			}
			retval.last.next = null;
			return retval;
		}

		public LinkedList mergeSort() {
			if (length () < 2) {
				return this.copy();
			} else if (length () == 2) {
				if (first.value > last.value) {
					ListNode temp = last;
					last = first;
					first = temp;
					first.next = last;
					last.next = null; 
				}
				return this.copy ();
			} else {
				LinkedList firstHalf = subList (0, length () / 2);
				LinkedList secondHalf = subList (length () / 2, length());
				return merge (firstHalf.mergeSort (), secondHalf.mergeSort ());
			}
		}

		public static LinkedList merge(LinkedList a, LinkedList b) {
			LinkedList retval = new LinkedList ();
			a = a.copy ();
			b = b.copy ();
			while (b.length()>0) {
				if (a.first != null) {
					while (a.first.value < b.first.value) {
						retval.pushLast (a.pop ());
						if (a.first == null)
							break;
					}
				}
				retval.pushLast (b.pop());
			}
			while (a.length () > 0) {
				retval.pushLast (a.pop ());
			}
			return retval;
		}
	}

	//____________________________________________________________________________________________________________

	public class OctTreeNode {
		public OctTreeNode parent;
		public OctTreeNode [] children;
		public float value = 0.0f;
		public float mean = 0.0f;
		public float max = 0.0f;
		public float min = 0.0f;
		public float onx = 0.0f;
		public float ony = 0.0f;
		public float onz = 0.0f;
		public float vx;
		public float vy;
		public float vz;
		public float size = 1.0f;
		bool populated = false;
		int depth = 0;
		int maxDepth = 20;
		int distance = 0;
		int count = 0 ;
		public bool doDraw = false;
		public bool doDrawBelow = false;

		public int getDistance(){
			return distance;
		}

		public void buildCube(GameObject gameParent, Color color, GameObject cubePRE){
			GameObject go=null;

			if (cubePRE == null) {
				go = GameObject.CreatePrimitive (PrimitiveType.Cube);
			} else {
				go = Instantiate (cubePRE, Vector3.zero, Quaternion.identity);
			}
			go.transform.parent = gameParent.transform;
			go.transform.localPosition = new Vector3 (onx, ony, onz);
			go.transform.localScale = Vector3.one * size;
			Renderer prend = go.GetComponent<Renderer> ();
			if(prend!=null)
				prend.material.color = color;
			Renderer[] children = go.GetComponentsInChildren<Renderer> ();
			foreach (Renderer child in children) {
				child.material.color = color;
			}

		}
			
		/*
		public int drawCubesByFlag(int count) {
			if (count <= 0)
				return 0;
			else
				if (doDraw) {
					buildCube ();
					doDraw = false;
					return 1;
				} else {
					int drawn = 0;
					if (children != null) {
						for (int i = 0; i < 8; i++) {
							drawn += children [i].drawCubesByFlag (count);
							count -= drawn;
						}
					}
					return drawn;
				}
		}*/

		public void drawCubesFromList (LinkedList cubes, int count, GameObject gameParent, Color color, GameObject cubePRE) {
			while ((cubes.getHead() != null) && count != 0) {
				OctTreeNode n = cubes.getObject (cubes.getHead());
				n.buildCube (gameParent, color, cubePRE);
				cubes.pop ();
				count -= 1;
			}
		}

		public void drawMeshFromList (LinkedList cubes, int count, GameObject gameParent, Color color, GameObject cubePRE,ref AdaptiveArrayVector3 vertices,
			ref AdaptiveArray<int> triangles, ref AdaptiveArrayVector3 normals, int maxDepth) {
			while ((cubes.getHead() != null) && count != 0) {
				OctTreeNode n = cubes.getObject (cubes.getHead());
				n.setMesh (gameParent, maxDepth, ref vertices, ref triangles, ref normals);
				cubes.pop ();
				count -= 1;
			}
		}
		/*
		public void drawCubesFromList (Linked_List cubes) {
			Node currentNode = cubes.getHead();
			while ((currentNode != null)) {
				OctTreeNode n = cubes.getData (currentNode);
				n.buildCube ();
				currentNode = currentNode.next;
			}
		} */

		//public Linked_List sortList(Linked_List list){

		//}
		/*
		public void flagCubesByMin(float cmin) {
			if (!populated)
				return;
			if (depth == 0) {
				findMin ();
				findMax ();
			}
			if (min > cmin) {
				doDraw=true;
			} else {
				if (children != null) {
					// pass command to children
					for(int i=0;i<8;i++) children[i].flagCubesByMin(cmin);
				}
			}
		}*/

		public void setDoDrawBelow() {
			if (parent != null) {
				parent.doDrawBelow = true;
				parent.setDoDrawBelow ();
			}
		}

		public void addCubesByMin(LinkedList cubes, float filterMin){
			if (!populated)
				return;
			if (depth == 0) {
				findMin ();
				findMax ();
			}
			if (min > filterMin) {
				doDraw=true;
				setDoDrawBelow ();
				Vector3 nodePosition = new Vector3(this.onx, this.ony, this.onz);
				var dist = Vector3.Distance(nodePosition, Camera.main.transform.position);
				cubes.push (this, dist);
			} else {
				if (children != null) {
					// pass command to children
					for(int i=0;i<8;i++) children[i].addCubesByMin(cubes, filterMin);
				}
			}
		}

		public void addCubesByMax(LinkedList cubes, float filterMax){
			if (!populated)
				return;
			if (depth == 0) {
				findMin ();
				findMax ();
			}
			if (max < filterMax) {
				doDraw=true;
				Vector3 nodePosition = new Vector3(this.onx, this.ony, this.onz);
				var dist = Vector3.Distance(nodePosition, Camera.main.transform.position);
				//Debug.Log (dist);
				cubes.push (this, dist);
			} else {
				if (children != null) {
					// pass command to children
					for(int i=0;i<8;i++) children[i].addCubesByMax(cubes, filterMax);
				}
			}
		}

		public void addCubesByRange(LinkedList cubes, float filterMin, float filterMax){
			if (!populated)
				return;
			if (depth == 0) {
				findMin ();
				findMax ();
			}
			if (max < filterMax && min > filterMin) {
				doDraw=true;
				Vector3 nodePosition = new Vector3(this.onx, this.ony, this.onz);
				var dist = Vector3.Distance(nodePosition, Camera.main.transform.position);
				//Debug.Log (dist);
				cubes.push (this, dist);
			} else {
				if (children != null) {
					// pass command to children
					for(int i=0;i<8;i++) children[i].addCubesByRange(cubes, filterMin, filterMax);
				}
			}
		}

		public float findMax() {
			if (children == null) {
				max = value;
			} else {
				max = value;
				for (int i = 0; i < 8; i++) {
					if (children [i].populated) {
						float childMax = children [i].findMax ();
						if (childMax > max)
							max = childMax;
					}
				}
			}
			return max;
		}

		public float findMin() {
			if (children == null) {
				min = value;
			} else {
				min = value;
				for (int i = 0; i < 8; i++) {
					if (children [i].populated) {
						min = Mathf.Min (min, children [i].findMin ());
					}
				}
			}
			return min;
		}

		public int findCount() {
			if (!populated) {
				count = 0;
			} else if (children == null) {
				count = 1;
			} else {
				int sum = 0;
				for (int i = 0; i < 8; i++) {
					sum += children [i].findCount ();
				}
				count = sum;
			}
			return count;
		}

		public float findMean() {
			if (depth == 0)
				findCount ();
			if (children == null) {
				mean = value;
			} else {
				float sum = 0;
				int weight = 0;
				for (int i = 0; i < 8; i++) {
					sum += children [i].count * children [i].findMean ();
					weight += children [i].count;
				}
				mean = sum/weight;
			}
			return mean;
		}

		public int findDepth() {
			if (!populated)
				return depth-1;
			if (children == null)
				return depth;
			int cdepth = depth;
			for (int i = 0; i < children.Length; i++) {
				if (children [i] != null) {
					int cd = children [i].findDepth ();
					if (cd > cdepth)
						cdepth = cd;
				}
			}
			return cdepth;
		}

		public float [] linspace(float a, float b, int n) {
			float[] retval = new float[n];
			retval [0] = a;
			retval [n - 1] = b;
			float h = (b - a) / (n - 1);
			for (int i = 1; i < n - 1; i++) {
				retval [i] = a + (float)i * h;
			}
			return retval;
		}

		public bool containsPoint(Vector3 point) {
			float x = point.x;
			float y = point.y;
			float z = point.z;
			OctTreeNode root = findRoot ();
			if (x >= this.onx - size / 2.0f && (x < this.onx + size / 2.0f || x == root.onx + root.size/2)   &&
				y >= this.ony - size / 2.0f && (y < this.ony + size / 2.0f || y == root.ony + root.size/2) &&
				z >= this.onz - size / 2.0f && (z < this.onz + size / 2.0f || z == root.onz + root.size/2) ) {
				return true;
			}
			return false;
		}

		public OctTreeNode findDrawableCube(Vector3 point, bool ascending) {
			bool cp = containsPoint (point);
			if (cp && doDraw) {
				return this;
			}
			if (!cp) {
				if (ascending) {
					if (depth > 0) {
						return parent.findDrawableCube (point, true);
					} else {
						return null;
					}
				} else {
					return null;
				}
			} else {
				if (ascending) {
					return findDrawableCube (point, false);
				} else if (children != null&&doDrawBelow) {
					for (int i = 0; i < 8; i++) {
						if (children [i] != null) {
							OctTreeNode childResult = children [i].findDrawableCube (point, false);
							if (childResult != null)
								return childResult;
						}
					}
				}
			}
			return null;
		}
			


		public void setMesh(GameObject parent, int maxDepth, ref AdaptiveArrayVector3 vertices, ref AdaptiveArray<int> triangles, ref AdaptiveArrayVector3 normals) {
			if (doDraw) { // if this level draws, add to mesh and return
				//buildCube(parent,Color.green,null);
				// onx, ony, onz

				int p = maxDepth-depth;
				int n = 1;
				for (int i = 0; i < p; i++)
					n *= 2;
				//n = 2;
				float[] x = linspace (onx - size / 2, onx + size / 2, n+1);
				float[] y = linspace (ony - size / 2, ony + size / 2, n+1);
				float[] z = linspace (onz - size / 2, onz + size / 2, n+1);
				float dx = (x [1] - x [0]) * 0.1f;
				float dy = (y [1] - y [0]) * 0.1f;
				float dz = (z [1] - z [0]) * 0.1f;
				Vector3 dxhat = new Vector3 (dx, 0, 0);
				Vector3 dyhat = new Vector3 (0, dy, 0);
				Vector3 dzhat = new Vector3 (0, 0, dz);
				Vector3 center = new Vector3 (onx, ony, onz);
				for (int i = 0; i < n; i++) {
					for (int j = 0; j < n; j++) {
						int index = vertices.Length();
						//Debug.Log (index);
						Vector3 ul = new Vector3 (x [i], y [j], z [0]);
						Vector3 ur = new Vector3 (x [i + 1], y [j], z [0]);
						Vector3 ll = new Vector3 (x [i], y [j + 1], z [0]);
						Vector3 lr = new Vector3 (x [i + 1], y [j + 1], z [0]);
						Vector3 mid = (ul + ur + ll + lr) / 4.0f;
						if (findDrawableCube (mid - dzhat,true)==null) {
							int uli = vertices.pushUnique (ul);
							int uri = vertices.pushUnique (ur);
							int lli = vertices.pushUnique (ll);
							int lri = vertices.pushUnique (lr);

							triangles.push (uli);
							triangles.push (lli);
							triangles.push (lri);
							triangles.push (lri);
							triangles.push (uri);
							triangles.push (uli);

							normals.set (uli,ul - center);
							normals.set (uri,ur - center);
							normals.set (lli,ll - center);
							normals.set (lri,lr - center);
						}
						ul = new Vector3 (x [i+1], y [j], z [n]);
						ur = new Vector3 (x [i], y [j], z [n]);
						ll = new Vector3 (x [i+1], y [j + 1], z [n]);
						lr = new Vector3 (x [i], y [j + 1], z [n]);
						mid = (ul + ur + ll + lr) / 4.0f;
						if (findDrawableCube (mid + dzhat,true) == null) {
							int uli = vertices.pushUnique (ul);
							int uri = vertices.pushUnique (ur);
							int lli = vertices.pushUnique (ll);
							int lri = vertices.pushUnique (lr);

							triangles.push (uli);
							triangles.push (lli);
							triangles.push (uri);
							triangles.push (uri);
							triangles.push (lli);
							triangles.push (lri);

							normals.set (uli,ul - center);
							normals.set (uri,ur - center);
							normals.set (lli,ll - center);
							normals.set (lri,lr - center);
						}
						ul = new Vector3 (x [i], y [0], z [j]);
						ur = new Vector3 (x [i + 1], y [0], z [j]);
						ll = new Vector3 (x [i], y [0], z [j + 1]);
						lr = new Vector3 (x [i + 1], y [0], z [j + 1]);
						mid = (ul + ur + ll + lr) / 4.0f;
						if (findDrawableCube (mid -dyhat,true) == null) {
							int uli = vertices.pushUnique (ul);
							int uri = vertices.pushUnique (ur);
							int lli = vertices.pushUnique (ll);
							int lri = vertices.pushUnique (lr);

							triangles.push (uli);
							triangles.push (uri);
							triangles.push (lli);
							triangles.push (uri);
							triangles.push (lli);
							triangles.push (lri);

							normals.set (uli,ul - center);
							normals.set (uri,ur - center);
							normals.set (lli,ll - center);
							normals.set (lri,lr - center);
						}

						ul = new Vector3 (x [i], y [n], z [j]);
						ur = new Vector3 (x [i + 1], y [n], z [j]);
						ll = new Vector3 (x [i], y [n], z [j + 1]);
						lr = new Vector3 (x [i + 1], y [n], z [j + 1]);
						mid = (ul + ur + ll + lr) / 4.0f;
						if (findDrawableCube (mid + dyhat, true) == null) {
							int uli = vertices.pushUnique (ul);
							int uri = vertices.pushUnique (ur);
							int lli = vertices.pushUnique (ll);
							int lri = vertices.pushUnique (lr);

							triangles.push (uli);
							triangles.push (lli);
							triangles.push (uri);
							triangles.push (uri);
							triangles.push (lli);
							triangles.push (lri);

							normals.set (uli, ul - center);
							normals.set (uri, ur - center);
							normals.set (lli, ll - center);
							normals.set (lri, lr - center);
						}
						ul = new Vector3 (x [0], y [j], z [i]);
						ur = new Vector3 (x [0], y [j], z [i + 1]);
						ll = new Vector3 (x [0], y [j + 1], z [i]);
						lr = new Vector3 (x [0], y [j + 1], z [i + 1]);
						mid = (ul + ur + ll + lr) / 4.0f;
						if (findDrawableCube (mid - dxhat,true) == null) {
							int uli = vertices.pushUnique (ul);
							int uri = vertices.pushUnique (ur);
							int lli = vertices.pushUnique (ll);
							int lri = vertices.pushUnique (lr);

							triangles.push (uli);
							triangles.push (uri);
							triangles.push (lli);
							triangles.push (uri);
							triangles.push (lri);
							triangles.push (lli);

							normals.set (uli, ul - center);
							normals.set (uri, ur - center);
							normals.set (lli, ll - center);
							normals.set (lri, lr - center);
						}
						ul = new Vector3 (x [n], y [j], z [i]);
						ur = new Vector3 (x [n], y [j], z [i + 1]);
						ll = new Vector3 (x [n], y [j + 1], z [i]);
						lr = new Vector3 (x [n], y [j + 1], z [i + 1]);
						mid = (ul + ur + ll + lr) / 4.0f;
						if (findDrawableCube (mid + dxhat,true) == null) {
							int uli = vertices.pushUnique (ul);
							int uri = vertices.pushUnique (ur);
							int lli = vertices.pushUnique (ll);
							int lri = vertices.pushUnique (lr);

							triangles.push (uli);
							triangles.push (lli);
							triangles.push (uri);
							triangles.push (uri);
							triangles.push (lli);
							triangles.push (lri);

							normals.set (uli, ul - center);
							normals.set (uri, ur - center);
							normals.set (lli, ll - center);
							normals.set (lri, lr - center);
						}
					}
				}
			} 

			/*
			 * else {
				// call on children
				if (children != null) {
					for (int i = 0; i < 8; i++) {
						if (children [i] != null) {
							children [i].setMesh (parent, maxDepth, ref vertices, ref triangles, ref normals);
						}
					}
				}
			}
			*/

		}


		public OctTreeNode(float x, float y, float z, float size) {
			this.depth = 0;
			this.parent = null;

			onx = x;
			ony = y;
			onz = z;
			this.size = size;
			children = null;
		}

		public OctTreeNode(float x, float y, float z, float size, int depth, OctTreeNode parent) {
			this.depth = depth;
			this.parent = parent;

			onx = x;
			ony = y;
			onz = z;
			this.size = size;
			children = null;
		}

		public void print() {
			if (depth == 0) {
				findMean ();
				findMax ();
				findMin ();
			}
			if(!populated) {
				return;
			}
			if(children==null) {
				//Debug.Log (" END POINT" + depth + " " + vx + " " + vy + " " + vz + " " + value);
			} else {
				//Debug.Log (" FILLED NODE " + min + " " + mean + " " + max + " " + count);
				for(int i=0;i<8;i++) {
					children [i].print ();
				}
			}


		}

		public OctTreeNode findRoot() {
			if (depth == 0)
				return this;
			return parent.findRoot ();
		}

		public bool push(float x, float y, float z, float value) {
			if (depth > maxDepth) {
				//Debug.Log ("recursing too far!!!");
				return false;
			}
			//Debug.Log ("push at depth " + depth);
			float rootXMin, rootXMax, rootYMin, rootYMax, rootZMin, rootZMax;
			OctTreeNode root = findRoot ();
			if (containsPoint(new Vector3(x,y,z))) {
				// point is within node
				if (!populated) {
					// add value to node, populate it
					populated = true;
					this.value = value;
					this.vx = x;
					this.vy = y;
					this.vz = z;
					return true;
				} else {
					if (children == null) {
						// no children, so create 8 children, loop through each to pass the current value
						// if no "true" happens send error and return false
						children = new OctTreeNode[8];
						for (int i = 0; i < 8; i++) {
							switch (i) {
							case 0: 
								children [i] = new OctTreeNode (onx - size / 4.0f, ony - size / 4.0f, onz - size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 1: 
								children [i] = new OctTreeNode (onx - size / 4.0f, ony - size / 4.0f, onz + size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 2: 
								children [i] = new OctTreeNode (onx - size / 4.0f, ony + size / 4.0f, onz - size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 3: 
								children [i] = new OctTreeNode (onx - size / 4.0f, ony + size / 4.0f, onz + size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 4: 
								children [i] = new OctTreeNode (onx + size / 4.0f, ony - size / 4.0f, onz - size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 5: 
								children [i] = new OctTreeNode (onx + size / 4.0f, ony - size / 4.0f, onz + size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 6: 
								children [i] = new OctTreeNode (onx + size / 4.0f, ony + size / 4.0f, onz - size / 4.0f, size / 2.0f, depth+1,this);
								break;
							case 7: 
								children [i] = new OctTreeNode (onx + size / 4.0f, ony + size / 4.0f, onz + size / 4.0f, size / 2.0f, depth+1,this);
								break;
							}
						}
						bool addedCurrent = false;
						for (int i = 0; i < 8; i++) {
							addedCurrent = children[i].push (vx, vy, vz, this.value);
							if (addedCurrent)
								break;
						}
						if (!addedCurrent) {
							//Debug.Log ("failed to move current node down in OctTreeNode");
							return false;
						}
					} 
					// loop through children add new value
					// if no true result send error and return false
					bool addedNew = false;
					for (int i = 0; i < 8; i++) {
						addedNew = children[i].push (x, y, z, value);
						if (addedNew)
							break;
					}
					if (!addedNew) {
						//Debug.Log ("failed to move current node down in OctTreeNode");
						return false;
					}
					return true;
				}
			} else {
				return false;
			}
		}


	}

	// have a data set to build the tree ____________________________________________________________________

	void prepMesh() {
		triangles = new AdaptiveArray<int> (20000);
		vertices = new AdaptiveArrayVector3 (20000);
		normals = new AdaptiveArrayVector3 (20000);
	}

	void drawMesh() {
		// find smallest cube size used.
		//int maxDepth = theTree.findDepth();

		//theTree.setMesh (gameObject,maxDepth,ref vertices,ref triangles, ref normals);

		//removeDuplicateVertices ();
		//removeUnattachedVertices ();

		// build mesh
		m.vertices = vertices.toArray ();
		m.triangles = triangles.toArray();
		Vector3[] normA = normals.toArray ();
		Color[] cols = new Color[normA.Length];
		Vector2[] uvs = new Vector2[normA.Length];

		for (int i = 0; i < normA.Length; i++) {
			normA [i] = vertices.x[i].normalized;
			cols [i] = color;
			uvs [i] = new Vector2 (0.5f,0.5f);
		}
		GetComponent<MeshRenderer> ().material.color = color;
		m.normals = normA;
		m.uv = uvs;
		m.colors = cols;
		//GetComponent<MeshRenderer> ().material = cubePRE.GetComponent<Renderer>().sharedMaterial;
		m.RecalculateNormals ();
		m.RecalculateBounds ();



	}

	void removeDuplicateTriangles(float size, int maxDepth) {
		int n = 1;
		for (int i = 0; i < maxDepth; i++)
			n *= 2;
		float far = 2.0f * size / n;
		far = 1.0f;
		float eps = 1.0e-5f;
		for (int i = 0; i < triangles.n; i += 3) {
			Vector3 centeri = (vertices.x [triangles.x [i]] +
			                  vertices.x [triangles.x [i + 1]] +
			                  vertices.x [triangles.x [i + 2]])/3 ;
			float cxi = centeri.x;
			float cyi = centeri.y;
			float czi = centeri.z;
			for (int j = i + 3; j < triangles.n; j += 3) {
				if(Mathf.Abs(vertices.x[triangles.x[j]].x-cxi)<far) {
					if (Mathf.Abs (vertices.x [triangles.x [j]].y - cyi) < far) {
						if (Mathf.Abs (vertices.x [triangles.x [j]].z - czi) < far) {
							bool removeJ = false;
							Vector3 centerj = (vertices.x [triangles.x [j]] +
								vertices.x [triangles.x [j + 1]] +
								vertices.x [triangles.x [j + 2]])/3 ;
							//do triangles share a center
							Vector3 del = (centeri - centerj);
							if (Mathf.Abs(del.x)<eps) {
								if (Mathf.Abs(del.y) < eps) {
									if (Mathf.Abs(del.z) < eps) {
										float eps2=eps*eps;
										// do triangles share a point?
										Vector3 del1 = vertices.x [triangles.x [i]] - vertices.x [triangles.x [j]];
										Vector3 del2 = vertices.x [triangles.x [i + 1]] - vertices.x [triangles.x [j]];
										Vector3 del3 = vertices.x [triangles.x [i + 2]] - vertices.x [triangles.x [j]];
										if (del1.sqrMagnitude < eps2 || del2.sqrMagnitude < eps2 || del3.sqrMagnitude < eps2) {
											removeJ = true;
										}
									}
								}
							}
							if (removeJ) {
								//triangles.pop (new int[]{ i, i + 1, i + 2 });
								triangles.pop (i,3);
								//triangles.pop (i);
								//triangles.pop (i);
								i = i - 3;
					
								//triangles.pop (new int[]{ j-3, j-2, j-1 });

								triangles.pop (j-3,3);
								//triangles.pop (j-3);
								//triangles.pop (j-3);
								j = j - 6;
								break;
							}
						}
					}
				}

			}
		}
	}


	void removeUnattachedVertices() {
		for(int i=vertices.Length()-1;i>=0;i--) {
			bool foundVertex = false;
			for (int j = 0; j < triangles.n; j++) {
				if (triangles.x [j] == i) {
					foundVertex = true;
					break;
				}
			}
			if (!foundVertex) {
				for (int k = 0; k < triangles.n; k += 1) {
					if (triangles.x [k] >i) {
						triangles.x[k]=triangles.x[k]-1;
					}
				}
				vertices.pop (i);
				normals.pop (i);
			}
		}
	}

	void removeDuplicateVertex(int i) {
		float eps = 1.0e-5f;
		float eps2=eps*eps;
		for (int j = i+1; j < vertices.n; j++) {
			float dx = Mathf.Abs (vertices.x [i].x - vertices.x [j].x);
			float dy = Mathf.Abs (vertices.x [i].y - vertices.x [j].y);
			float dz = Mathf.Abs (vertices.x [i].z - vertices.x [j].z);
			if(dx<eps&&dy<eps&&dz<eps) {
				//if ((vertices [i] - vertices [j]).sqrMagnitude < eps2) {
				//Debug.Log ("removing vertex");
				//Debug.Log (j);
				for (int k = 0; k < triangles.n; k += 1) {
					if (triangles.x [k] == j) {
						triangles.x[k]= i;
					} else if (triangles.x [k] > j) {
						triangles.x[k]=triangles.x[k]-1;
					}
				}
				vertices.pop (j);
				normals.x[i]= 0.5f * (normals.x [i] + normals.x [j]);
				normals.pop (j);
			}
		}
	}

	void removeDuplicateVertices() {
		float eps = 1.0e-5f;
		float eps2=eps*eps;
		for (int i = 0; i < vertices.n; i++) {
			for (int j = i+1; j < vertices.n; j++) {
				float dx = Mathf.Abs (vertices.x [i].x - vertices.x [j].x);
				if (dx < eps) {
					float dy = Mathf.Abs (vertices.x [i].y - vertices.x [j].y);
					if (dy < eps) {
						float dz = Mathf.Abs (vertices.x [i].z - vertices.x [j].z);
						if(dz<eps) {
							//if ((vertices [i] - vertices [j]).sqrMagnitude < eps2) {
							//Debug.Log ("removing vertex");
							//Debug.Log (j);
							for (int k = 0; k < triangles.n; k += 1) {
								if (triangles.x [k] == j) {
									triangles.x[k]= i;
								} else if (triangles.x [k] > j) {
									triangles.x[k]=triangles.x[k]-1;
								}
							}
							vertices.pop (j);
							normals.x[i]= 0.5f * (normals.x [i] + normals.x [j]);
							normals.pop (j);
						}
					}

				}

			}
		}
	}

	void Start(){

		prepMesh ();

		m = GetComponent<MeshFilter> ().mesh;

		string filepath = Application.dataPath+"/StreamingAssets/LocalFiles/"+filename;
		System.Object dataObject = null;

		if (filename != null) {
			if (!filename.Equals ("")) {
				switch (fileType) {
				case FileType.CSV:
					dataObject = ScriptableObject.CreateInstance ("UnstructuredData");
					((UnstructuredData)dataObject).readCSV (filepath);
					break;
				case FileType.UNSTRUCTURED:
					dataObject = ScriptableObject.CreateInstance ("UnstructuredData");
					((UnstructuredData)dataObject).readWODHeaders (filepath);
					((UnstructuredData)dataObject).readWODSet ();
					break;
				case FileType.STRUCTURED:
					dataObject = ScriptableObject.CreateInstance ("StructuredData");
					((StructuredData)dataObject).readWOD (filepath);
					break;
				default:
					Debug.Log ("Unrecognized file type");
					break;
				}
			}
		}

		// test data set at random
		if (dataObject==null) {
			dataObject = ScriptableObject.CreateInstance ("UnstructuredData") as UnstructuredData;
			int n = 30;
			nx = n;
			ny = n;
			nz = n;
			float [] x = linspace (0, 10, nx);
			float [] y = linspace (0, 10, ny);
			float [] z = linspace (0, 10, nz);
			string[] names = { "x", "y", "z", "data" };
			((UnstructuredData)dataObject).setColumns (names);
			for (int i = 0; i < nx; i++) {
				for (int j = 0; j < ny; j++) {
					for (int k = 0; k < nz; k++) {
						float[] row = { x [i], y [j], z [k], UnityEngine.Random.Range (0.0f, 1.0f) };
						((UnstructuredData)dataObject).addRow (row);
					}
				}
			}
		}

		cubes = new LinkedList ();
		theTree = new OctTreeNode (0.5f,0.5f,0.5f,1.0f);

		if (dataObject.GetType () == typeof(UnstructuredData)) {
			float[] x = ((UnstructuredData)dataObject).getColumn (xName);
			float[] y = ((UnstructuredData)dataObject).getColumn (yName);
			float[] z = ((UnstructuredData)dataObject).getColumn (zName);
			float[] data = ((UnstructuredData)dataObject).getColumn (dataName);
			if (xRange == null || xRange.Length==0) {
				xRange = findRange (x);
			}
			if (yRange == null|| yRange.Length==0) {
				yRange = findRange (y);
			}
			if (zRange == null|| zRange.Length==0) {
				zRange = findRange (z);
			}

			for (int i = 0; i < data.Length; i++) {
				theTree.push ((x [i]-xRange[0])/(xRange[1]-xRange[0]), (y [i]-yRange[0])/(yRange[1]-yRange[0]), (z [i]-zRange[0])/(zRange[1]-zRange[0]), data [i]);
			}
		} else if (dataObject.GetType () == typeof(StructuredData)) {
			float[] x = ((StructuredData)dataObject).getDimension (xName);
			float[] y = ((StructuredData)dataObject).getDimension (yName);
			float[] z = ((StructuredData)dataObject).getDimension (zName);
			float[,,] data = ((StructuredData)dataObject).getValues3D (dataName);
			if (xRange == null || xRange.Length==0) {
				xRange = findRange (x);
			}
			if (yRange == null|| yRange.Length==0) {
				yRange = findRange (y);
			}
			if (zRange == null|| zRange.Length==0) {
				zRange = findRange (z);
			}
			for (int i = 0; i < x.Length; i++) {
				for (int j = 0; j < y.Length; j++) {
					for (int k = 0; k < z.Length; k++) {
						theTree.push ((x [i]-xRange[0])/(xRange[1]-xRange[0]), 
							(y [j]-yRange[0])/(yRange[1]-yRange[0]),
							(z [k]-zRange[0])/(zRange[1]-zRange[0]), data [i, j, k]);
					}
				}
			}
		}
			
		switch (choice) {
		case valueChoice.min:
			Debug.Log ("min: " + cmin);
			theTree.addCubesByMin (cubes, cmin);
			break;
		case valueChoice.max:
			Debug.Log ("max: " + cmax);
			theTree.addCubesByMax (cubes, cmax);
			break;
		case valueChoice.range:
			Debug.Log ("range: " + cmin + " - " + cmax);
			theTree.addCubesByRange (cubes, cmin, cmax);
			break;
		}

		//cubes = cubes.mergeSort ();
		//cubes.printList();
		//cubes = theTree.sortList (cubes);
		//theTree.drawCubesFromList (cubes);
		//cubes.printAllNodes ();

		//replaceCubesWithMesh ();
	}


	float timer = 0.0f;
	float tick = 1.0f;

	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		if (timer < 0.0f) {
			//theTree.drawCubesFromList (cubes, 1000, gameObject, color, cubePRE);
			int maxDepth = theTree.findDepth();
			Debug.Log ("Starting Draw");
			theTree.drawMeshFromList (cubes, 1, gameObject, color, cubePRE,ref vertices,ref triangles,ref normals, maxDepth);
			drawMesh ();
			timer = tick;
		}
		//insert function call to update distance of cubes from camera in case person moves

	}
}