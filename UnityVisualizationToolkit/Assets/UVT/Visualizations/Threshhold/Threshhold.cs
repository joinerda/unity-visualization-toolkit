using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Threshhold plot draws cubes based on an OctTree storage of all
/// space within a range that falls within a threshhold value.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Threshhold : MonoBehaviour {

	public GameObject cubePRE = null;

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
	public DataObject.DataTypes fileType = DataObject.DataTypes.UNSTRUCTURED_CSV;
	public string xName = "x";
	public string yName = "y";
	public string zName = "z";
	public string depName = "data";

	public LinkedList cubes;

	public float [] xRange = null;
	public float [] yRange = null;
	public float [] zRange = null;
	public float minValue;
	public float maxValue;
	public enum ThreshholdType {
		min, max, range
	}
	int nx = 10;
	int ny = 10;
	int nz = 10;

	System.Object dataObject = null;

	public Color color = Color.blue;

	public ThreshholdType threshholdType;
	public int maxDepth = 20;

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

		public int getDistance(){
			return distance;
		}

		public void buildCube(GameObject gameParent, Color color, GameObject cubePRE){
			GameObject go = null;
			if (cubePRE == null) {
				go = Instantiate (Resources.Load ("ThreshholdCube") as GameObject);
				//go = GameObject.CreatePrimitive (PrimitiveType.Cube);
			} else {
				go = Instantiate (cubePRE, Vector3.zero, Quaternion.identity);
			}
			go.transform.parent = gameParent.transform;
			go.transform.localPosition = new Vector3 (onx, ony, onz);
			go.transform.localScale = Vector3.one * size;
			go.GetComponent<Renderer> ().material.color = color;

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
		public void flagCubesByMin(float minValue) {
			if (!populated)
				return;
			if (depth == 0) {
				findMin ();
				findMax ();
			}
			if (min > minValue) {
				doDraw=true;
			} else {
				if (children != null) {
					// pass command to children
					for(int i=0;i<8;i++) children[i].flagCubesByMin(minValue);
				}
			}
		}*/

		public void addCubesByMin(LinkedList cubes, float filterMin){
			if (!populated)
				return;
			if (depth == 0) {
				findMin ();
				findMax ();
			}
			if (min > filterMin) {
				//doDraw=true;
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
				//doDraw=true;
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
				//doDraw=true;
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

		public OctTreeNode(float x, float y, float z, float size) {
			this.depth = 0;
			this.parent = null;

			onx = x;
			ony = y;
			onz = z;
			this.size = size;
			children = null;
		}

		public OctTreeNode(float x, float y, float z, float size, int maxDepth) {
			this.depth = 0;
			this.parent = null;
			this.maxDepth =maxDepth;

			onx = x;
			ony = y;
			onz = z;
			this.size = size;
			children = null;
		}

		public OctTreeNode(float x, float y, float z, float size, int depth, OctTreeNode parent) {
			this.depth = depth;
			this.parent = parent;
			this.maxDepth = parent.maxDepth;

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
				Debug.Log (" END POINT" + depth + " " + vx + " " + vy + " " + vz + " " + value);
			} else {
				Debug.Log (" FILLED NODE " + min + " " + mean + " " + max + " " + count);
				for(int i=0;i<8;i++) {
					children [i].print ();
				}
			}


		}

		public bool push(float x, float y, float z, float value) {
			if (depth > maxDepth) {
				//Debug.Log ("recursing too far!!!");
				return false;
			}
			//Debug.Log ("push at depth " + depth);
			if (x >= this.onx - size / 2.0f && x <= this.onx + size / 2.0f &&
				y >= this.ony - size / 2.0f && y <= this.ony + size / 2.0f &&
				z >= this.onz - size / 2.0f && z <= this.onz + size / 2.0f) {
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


	public void setData(StructuredData dataObject) {
		this.dataObject = dataObject;
	}

	public void setData(UnstructuredData dataObject) {
		this.dataObject = dataObject;
	}

	public void setObjectFromFile(string filename, DataObject.DataTypes fileType) {
		string filepath = Application.dataPath+"/StreamingAssets/LocalFiles/"+filename;
		if (filename != null) {
			if (!filename.Equals ("")) {
				switch (fileType) {
				case DataObject.DataTypes.UNSTRUCTURED_CSV:
					dataObject = ScriptableObject.CreateInstance ("UnstructuredData");
					((UnstructuredData)dataObject).readCSV (filepath);
					break;
				case DataObject.DataTypes.UNSTRUCTURED_WOD:
					dataObject = ScriptableObject.CreateInstance ("UnstructuredData");
					((UnstructuredData)dataObject).readWODHeaders (filepath);
					((UnstructuredData)dataObject).readWODSet ();
					break;
				case DataObject.DataTypes.STRUCTURED_WOD:
					dataObject = ScriptableObject.CreateInstance ("StructuredData");
					((StructuredData)dataObject).readWOD (filepath);
					break;
				default:
					Debug.Log ("Unrecognized file type");
					break;
				}
			}
		}
	}

	public static GameObject CreateGameObject(Vector3 pos, Vector3 scale, Transform parent) {
		GameObject glyphSetGO = new GameObject ();

		glyphSetGO.transform.parent = parent;
		glyphSetGO.transform.position = pos;
		glyphSetGO.transform.localPosition = Vector3.zero;
		glyphSetGO.transform.localScale = scale;
		glyphSetGO.AddComponent<Threshhold> ();

		return glyphSetGO;
	}

	public void drawThreshhold() {
		cubes = new LinkedList ();
		theTree = new OctTreeNode (0.5f,0.5f,0.5f,1.0f,maxDepth);

		if (dataObject.GetType () == typeof(UnstructuredData)) {
			float[] x = ((UnstructuredData)dataObject).getColumn (xName);
			float[] y = ((UnstructuredData)dataObject).getColumn (yName);
			float[] z = ((UnstructuredData)dataObject).getColumn (zName);
			float[] data = ((UnstructuredData)dataObject).getColumn (depName);
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
			float[,,] data = ((StructuredData)dataObject).getValues3D (depName);
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


		switch (threshholdType) {
		case ThreshholdType.min:
			Debug.Log ("min: " + minValue);
			theTree.addCubesByMin (cubes, minValue);
			break;
		case ThreshholdType.max:
			Debug.Log ("max: " + maxValue);
			theTree.addCubesByMax (cubes, maxValue);
			break;
		case ThreshholdType.range:
			Debug.Log ("range: " + minValue + " - " + maxValue);
			theTree.addCubesByRange (cubes, minValue, maxValue);
			break;
		}
	}

	void Start(){
		if (filename != null) {
			setObjectFromFile( filename, fileType);
		}

		if (dataObject != null) {
			drawThreshhold ();
		}

	}


	float timer = 0.0f;
	float tick = 0.01f;

	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		if (timer < 0.0f) {
			theTree.drawCubesFromList (cubes, 1000, gameObject, color, cubePRE);
			timer = tick;
		}
		//insert function call to update distance of cubes from camera in case person moves
	}
}