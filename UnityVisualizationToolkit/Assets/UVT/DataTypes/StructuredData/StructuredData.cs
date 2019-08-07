//#define SD_CHECK_LIMITS

using UnityEngine;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// Structured data scriptable object. Stores gridded
/// data on regular grid. Arbitrary dimension, dimensions
/// added in order of most rapidly changing to slowly changing
/// in a single flattened data array.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class StructuredData : ScriptableObject {

	int nd; //# dimensions
	int [] n; // size of each dimension
	string [] namesd; //names of each dimension
	float [] grid; //values of each grid dimension
	int nvar; //#variables
	float [] values; //variable values at each grid point
	string [] namesv; // variable names

	StreamReader sr_c2d = null;

	/// <summary>
	/// Copy structured data into an unstructured format
	/// </summary>
	/// <returns>The unstructured.</returns>
	public UnstructuredData getUnstructured() {
		UnstructuredData uData = 
			(UnstructuredData) ScriptableObject.CreateInstance("UnstructuredData");


		// set up names
		string [] columns = new string[nd+nvar];
		for (int d = 0; d < nd; d++) {
			columns [d] = namesd [d];
		}
		for (int v = nd; v < nd + nvar; v++) {
			columns [v] = namesv [v - nd];
		}
		uData.setColumns (columns);

		// every row
		float [] row = new float[nd+nvar];
		for (int i = 0; i < calcStride (); i++) {
			int[] ijkv = ijkvFromIval (i);
			int gridSkip = 0;
			for (int d = 0; d < nd; d++) {
				row [d] = grid [gridSkip + ijkv [d]];
				gridSkip += n [d];
			}
			for (int v = 0; v < nvar; v++) {
				ijkv [nd] = v;
				row [nd + v] = getValue (ijkv);
			}
			uData.addRow (row);
		}

		return uData;
	}

	public void purgeValues()
	{
		nvar=0; //#variables
		values=null; //variable values at each grid point
		namesv=null; // variable names
	}

	/// <summary>
	/// Read a file in CSV2D format and populate the data object
	/// </summary>
	/// <param name="filepath">Filepath.</param>
	public void readC2DSet(bool skipData=false)
	{
		// read a 2D structured file in CSV format. First row is dimension 1
		// first element on each row is dimension 2. var name in first corner element
		/*
         
        DIMENSIONS x y
        z 1 2 3 4
        1 0 1 2 0
        2 3 2 1 .2
        3 7 2 8 9
        4 2 6 3 4
        NEWVAR
        z2 1 2 3 4
        1 0 1 2 0
        2 3 2 1 .2
        3 7 2 8 9
        4 2 6 3 4

         */
		StreamReader sr = sr_c2d;
		string line;

		// purge values
		if(!skipData) purgeValues();
		string[] words = null;

		// do we need to rewind?
		if (!IOExtras.ReadLine2(sr,out line))
		{
			Debug.Log("Rewinding");
			sr.BaseStream.Position = 0;
			sr.DiscardBufferedData();
			// metadata line
			IOExtras.ReadLine2(sr, out line);
			words = IOExtras.StringArray(line);
			if (!words[0].Equals("DIMENSION", System.StringComparison.OrdinalIgnoreCase))
			{
				Debug.Log("INVALID FILE TYPE readCSV2D");
				return;
			}
			if (words.Length < 3)
			{
				Debug.Log("INVALID FILE TYPE readCSV2D");
				return;
			}
			IOExtras.ReadLine2(sr, out line);

		}

		if(skipData) // if not reading data just go to end of file or next NEWSET
		{
			while (IOExtras.ReadLine2(sr, out line))
			{
				//IOExtras.ReadLine2(sr, out line);
				string[] words2 = IOExtras.StringArray(line);
				if (words2[0].Equals("NEWSET", System.StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
			}
			return;
		}


		bool varsToAdd = true;
		bool firstLine = true;

		int n1 = 0;
		int n2 = 0;
		string varname = "";
		ArrayList dim2List = null;
		ArrayList[] valuesList = null;
		float[] dim1array = null;

		while (varsToAdd)
		{

			// read first line
			if (firstLine)
			{
				firstLine = false;
				//IOExtras.ReadLine2(sr, out line);
				words = IOExtras.StringArray(line);
				varname = words[0];
				n1 = words.Length - 1;
				dim1array = new float[n1];
				for (int i = 0; i < n1; i++)
				{
					dim1array[i] = float.Parse(words[i + 1]);
				}

				n2 = 0;
				dim2List = new ArrayList();
				if(!skipData) { 
					valuesList = new ArrayList[n1];
					for (int i = 0; i < n1; i++)
					{
						valuesList[i] = new ArrayList();
					}
				}
			}
			else
			{
				varsToAdd = false;

				while (IOExtras.ReadLine2(sr, out line))
				{
					//IOExtras.ReadLine2(sr, out line);
					string[] words2 = IOExtras.StringArray(line);
					if (words2[0].Equals("NEWSET", System.StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
					else if (words2[0].Equals("NEWVAR", System.StringComparison.OrdinalIgnoreCase))
					{
						varsToAdd = true;
						firstLine = true;
						break;
					}
					else if(!skipData)
					{
						float[] values = IOExtras.FloatArray(line);
						if (values.Length != n1 + 1)
						{
							Debug.Log("INVALID LINE LENGTH IN readCSV2D");
							Debug.Log(n1 + 1);
							Debug.Log(values.Length);
							Debug.Log(line);
							return;
						}
						dim2List.Add(values[0]);
						for (int i = 0; i < n1; i++)
						{
							valuesList[i].Add(values[i + 1]);
						}
						n2++;
					}
				}

				if(!skipData) { 
					float[] currentvalues = new float[n1 * n2];

					for (int i = 0; i < n2; i++)
					{
						for (int j = 0; j < n1; j++)
						{
							currentvalues[i * n1 + j] = (float)valuesList[j][i];
						}
					}
					addVariable(varname, currentvalues);
				}
			}

		}

	}

	/// <summary>
	/// Read a file in CSV2D format and populate the data object
	/// </summary>
	/// <param name="filepath">Filepath.</param>
	public void readC2D(string filepath)
    {
        // read a 2D structured file in CSV format. First row is dimension 1
        // first element on each row is dimension 2. var name in first corner element
        /*
         
        DIMENSIONS x y
        z 1 2 3 4
        1 0 1 2 0
        2 3 2 1 .2
        3 7 2 8 9
        4 2 6 3 4
        NEWVAR
        z2 1 2 3 4
        1 0 1 2 0
        2 3 2 1 .2
        3 7 2 8 9
        4 2 6 3 4

         */
        StreamReader sr = new StreamReader(filepath);
		sr_c2d = sr;
        string line;
        // metadata line
        IOExtras.ReadLine2(sr, out line);
        string[] words = IOExtras.StringArray(line);
        if (!words[0].Equals("DIMENSION", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("INVALID FILE TYPE readCSV2D");
            return;
        }
        if (words.Length < 3)
        {
            Debug.Log("INVALID FILE TYPE readCSV2D");
            return;
        }
        string dim1name = words[1];
        string dim2name = words[2];

        bool varsToAdd = true;
        bool dimensionsAdded = false;
        bool firstLine = true;

        int n1 = 0;
        int n2 = 0;
        string varname = "";
        ArrayList dim2List = null;
        ArrayList[] valuesList = null;
        float[] dim1array = null;

        while (varsToAdd)
        {

          
            // read first line
            if (firstLine)
            {
                firstLine = false;
                IOExtras.ReadLine2(sr, out line);
                words = IOExtras.StringArray(line);
                varname = words[0];
                n1 = words.Length - 1;
                dim1array = new float[n1];
                for (int i = 0; i < n1; i++)
                {
                    dim1array[i] = float.Parse(words[i + 1]);
                }

                n2 = 0;
                dim2List = new ArrayList();
                valuesList = new ArrayList[n1];
                for (int i = 0; i < n1; i++)
                {
                    valuesList[i] = new ArrayList();
                }
            }
            else
            {
                varsToAdd = false;
                while (IOExtras.ReadLine2(sr, out line))
                {
                    //IOExtras.ReadLine2(sr, out line);
                    string[] words2 = IOExtras.StringArray(line);
					if(words2[0].Equals("NEWSET",System.StringComparison.OrdinalIgnoreCase))
					{
						break;
					} else if (words2[0].Equals("NEWVAR", System.StringComparison.OrdinalIgnoreCase))
                    {
                        varsToAdd = true;
                        firstLine = true;
                        break;
                    }
                    else
                    {
                        float[] values = IOExtras.FloatArray(line);
                        if (values.Length != n1 + 1)
                        {
                            Debug.Log("INVALID LINE LENGTH IN readCSV2D");
                            Debug.Log(n1 + 1);
                            Debug.Log(values.Length);
                            Debug.Log(line);
                            return;
                        }
                        dim2List.Add(values[0]);
                        for (int i = 0; i < n1; i++)
                        {
                            valuesList[i].Add(values[i + 1]);
                        }
                        n2++;
                    }
                }

                if (!dimensionsAdded)
                {
                    addDimension(dim2name, dim2List);
                    addDimension(dim1name, dim1array);
                    dimensionsAdded = true;
                }

                float[] currentvalues = new float[n1 * n2];

                for (int i = 0; i < n2; i++)
                {
                    for (int j = 0; j < n1; j++)
                    {
                        currentvalues[i * n1 + j] = (float)valuesList[j][i];
                    }
                }
                addVariable(varname, currentvalues);
            }
        }

    }


    /// <summary>
    /// Read a file in WOD format and populate the data object
    /// </summary>
    /// <param name="filepath">Filepath.</param>
    public void readWOD(string filepath) {
		StreamReader sr = new StreamReader (filepath);
		string currentVarName = "";
		int currentVarCount = 0;
		int currentVarSize = 0;
		bool readingDimension = false;
		bool readingVariable = false;
		int currentVarIndex = 0;
		float[] currentVarValues = new float[currentVarCount];

		string line;
		while (IOExtras.ReadLine2(sr,out line))
		{
			string[] words = IOExtras.StringArray (line);

			if (words [0].Equals ("DIMENSION", System.StringComparison.OrdinalIgnoreCase)) {
				currentVarName = words [1];
				currentVarCount = int.Parse (words [2]);
				readingDimension = true;
				currentVarIndex = 0;
				currentVarValues = new float[currentVarCount];

			} else if (words [0].Equals ("END", System.StringComparison.OrdinalIgnoreCase)) { 
				if (readingDimension) {
					readingDimension = false;
					addDimension (currentVarName, currentVarValues);
				} else if (readingVariable) {
					readingVariable = false;
					addVariable (currentVarName, currentVarValues);
				}
			} else if (words [0].Equals ("VARIABLE", System.StringComparison.OrdinalIgnoreCase)) {
				currentVarName = words [1];
				readingVariable = true;
				currentVarIndex = 0;
				int[] nDim = getNDimensions ();
				currentVarSize = 1;
				for (int i = 0; i < nDim.Length; i++) {
					currentVarSize *= nDim [i];
				}
				currentVarValues = new float[currentVarSize];
			} else if (readingDimension||readingVariable) {
				for (int i = 0; i < words.Length; i++) {
					if (words [i].Equals ("END", System.StringComparison.OrdinalIgnoreCase)) {
						if (readingDimension) {
							readingDimension = false;
							addDimension (currentVarName, currentVarValues);
						} else if (readingVariable) {
							readingVariable = false;
							addVariable (currentVarName, currentVarValues);
						}
						break;
					} else {
						currentVarValues [currentVarIndex++] = float.Parse (words [i]);
					}
				}
			} 

		}

	}

	/// <summary>
	/// Gets the number of dimensions.
	/// </summary>
	/// <returns>The N dimensions.</returns>
	public int [] getNDimensions() {
		return n;
	}

	/// <summary>
	/// Used for setting the data values array after
	/// dimensions are defined.
	/// </summary>
	/// <param name="namesv">Names of the variables.</param>
	/// <param name="values">Values of the variables. Format is 
	/// flattened, with the variable as the most slowly changing
	/// index
	/// </param>
	public void setVariables(string [] namesv,float [] values) {
		nvar = namesv.Length;
		this.namesv = namesv;
		// add an error check at some point to make sure name are unique
		int stride = calcStride();
		if (values.Length != stride * nvar) {
			Debug.LogAssertion ("StructuredData.setVariables values length does not match grid spacing");
			return;
		}
		this.values = values;
	}

	/// <summary>
	/// routine to flatten a [,,] style 3D array
	/// </summary>
	/// <returns>flattened array</returns>
	/// <param name="values">Values.</param>
	public float [] array3DTo1D(float[,,] values) {
		int nx = values.GetLength (0);
		int ny = values.GetLength (1);
		int nz = values.GetLength (2);
		float[] retval = new float[nx * ny * nz];
		for (int i = 0; i < nx; i++) {
			for (int j = 0; j < ny; j++) {
				for (int k = 0; k < nz; k++) {
					retval [i * ny * nz + j * nz * k] = values [i, j, k];
				}
			}
		}
		return retval;
	}

	/// <summary>
	/// routine to flatten a [,,] style 3D array
	/// </summary>
	/// <returns>flattened array</returns>
	/// <param name="values">Values.</param>
	public float [] array3DTo1D(double[,,] values) {
		int nx = values.GetLength (0);
		int ny = values.GetLength (1);
		int nz = values.GetLength (2);
		float[] retval = new float[nx * ny * nz];
		for (int i = 0; i < nx; i++) {
			for (int j = 0; j < ny; j++) {
				for (int k = 0; k < nz; k++) {
					retval [i * ny * nz + j * nz + k] = (float)values [i, j, k];
				}
			}
		}
		return retval;
	}

	/// <summary>
	/// Add a new variable given input in [,,] format
	/// </summary>
	/// <param name="name">variable name.</param>
	/// <param name="dvalues">values.</param>
	public void addVariable3D(string name, float [,,] dvalues) {
		addVariable(name,array3DTo1D(dvalues));
	}

	/// <summary>
	/// Add a new variable given input in [,,] format
	/// </summary>
	/// <param name="name">variable name.</param>
	/// <param name="dvalues">values.</param>
	public void addVariable3D(string name, double [,,] dvalues) {
		addVariable(name,array3DTo1D(dvalues));
	}

	public void addVariable(string name, double[] dvalues)
	{
		float [] fvalues = new float[dvalues.Length];
		for(int i=0;i<fvalues.Length;i++) fvalues[i]=(float)dvalues[i];
		addVariable(name,fvalues);
	}

		/// <summary>
		/// Add a new variable given input in [,,] format
		/// </summary>
		/// <param name="name">variable name.</param>
		/// <param name="dvalues">values.</param>
		public void addVariable(string name, float [] dvalues) {
		int stride = calcStride ();
		if (dvalues.Length != stride) {
			Debug.LogAssertion ("StructuredData.addVariable length of values does not match grid spacing");
			return;
		}
		if (nvar == 0) {
			// add first variable
			nvar = 1;
			namesv = new string[1]{ name };
			values = new float[stride];
			for (int i = 0; i < stride; i++) {
				values [i] = dvalues [i];
			}
		} else {
			string [] tnamesv = new string[nvar + 1];
			for (int v = 0; v < nvar; v++) {
				tnamesv [v] = namesv [v];
			}
			tnamesv [nvar] = name;
			namesv = tnamesv;
			float [] tvalues = new float[stride*(nvar+1)];
			for(int i=0;i<stride*nvar;i++) tvalues[i]=values[i];
			for(int i=0;i<stride;i++) tvalues[stride*nvar+i]=dvalues[i];
			values = tvalues;
			nvar++;
		}
	}

	/// <summary>
	/// Set all of the dimensions and grid values
	/// </summary>
	/// <param name="namesd">Names of dimensions, most rapidly changing first</param>
	/// <param name="n">Array of length of each dimension</param>
	/// <param name="grid">single array with each grids values listed in order</param>
	public void setDimensions(string [] namesd, int [] n, float [] grid) {
		nd = namesd.Length;
		if (n.Length != nd) {
			Debug.LogAssertion ("StructuredData.setDimensions namesd and n are different lengths");
			return;
		}
		if (grid.Length % nd != 0) {
			Debug.LogAssertion ("StructuredData.setDimensions grid not evenly divisible");
			return;
		}
		this.n = n;
		this.namesd = namesd;
		// add an error check at some point to make sure name are unique
		this.grid = grid;
	}

    /// <summary>
    /// Add an additional dimension when setting up object.
    /// </summary>
    /// <param name="dname">dimension name.</param>
    /// <param name="dgrid">grid values.</param>
    public void addDimension(string dname, ArrayList algrid)
    {
        float[] fgrid = new float[algrid.Count];
        for (int i = 0; i < algrid.Count; i++)
        {
            fgrid[i] = (float)algrid[i];
        }
        addDimension(dname, fgrid);
    }

    /// <summary>
    /// Add an additional dimension when setting up object.
    /// </summary>
    /// <param name="dname">dimension name.</param>
    /// <param name="dgrid">grid values.</param>
    public void addDimension(string dname, double [] dgrid) {
		float [] fgrid = new float[dgrid.Length];
		for (int i = 0; i < dgrid.Length; i++) {
			fgrid [i] = (float)dgrid [i];
		}
		addDimension (dname, fgrid);
	}

	/// <summary>
	/// Add an additional dimension when setting up object.
	/// </summary>
	/// <param name="dname">dimension name.</param>
	/// <param name="dgrid">grid values.</param>
	public void addDimension(string dname, float [] dgrid) {
		if (nd == 0) {
			// make first grid added the initial grid values
			nd = 1;
			namesd = new string[1]{ dname };
			n = new int[1]{ dgrid.Length };
			grid = new float[dgrid.Length];
			for (int i = 0; i < dgrid.Length; i++)
				grid [i] = dgrid [i];
			
		} else {
			// grow grid variables as needed
			string [] tnamesd = new string[nd + 1];
			for (int d = 0; d < nd; d++) {
				tnamesd [d] = namesd [d];
			}
			tnamesd [nd] = dname;
			namesd = tnamesd;
			int [] tn = new int[nd + 1];
			for (int d = 0; d < nd; d++)
				tn [d] = n [d];
			tn [nd] = dgrid.Length;
			n = tn;
			float[] tgrid = new float[grid.Length + dgrid.Length];
			for (int i = 0; i < grid.Length; i++)
				tgrid [i] = grid [i];
			for (int i = 0; i < dgrid.Length; i++)
				tgrid [grid.Length + i] = dgrid [i];
			grid = tgrid;
			nd++;
		}
	}

	/// <summary>
	/// Given a string for a dimension name, return index
	/// of dimension.
	/// </summary>
	/// <returns>The dimension index.</returns>
	/// <param name="name">Name.</param>
	public int getDimensionIndex(string name) {
		int dindex = Array.IndexOf (namesd, name);
		#if SD_CHECK_LIMITS
		if (dindex < 0) {
			Debug.LogAssertion ("In StructuredData.getValue(ijk,name) name not found");
		}
		#endif
		return dindex;
	}
	public int getVariableIndex(string name) {
		int vindex = Array.IndexOf (namesv, name);
		#if SD_CHECK_LIMITS
		if (vindex < 0) {
			Debug.LogAssertion ("In StructuredData.getValue(ijk,name) name not found");
		}
		#endif
		return vindex;
	}

	/// <summary>
	/// Given position in global data array,
	/// return index for each dimension and for
	/// which variable is stored at that position
	/// </summary>
	/// <returns>grid and variable indeces</returns>
	/// <param name="ival">position in global array</param>
	int [] ijkvFromIval(int ival) {
		int[] ijkv = new int[nd+1];
		for (int d = 0; d<nd; d++) {
			ijkv [d] = ival % n [d];
			ival = ival / n [d];
		}
		ijkv [nd] = ival;
		return ijkv;
	}

	/// <summary>
	/// Return global position in values array based on
	/// grid position and choice of variable.
	/// </summary>
	/// <returns>global array position</returns>
	/// <param name="ijkv">grid and variable indeces</param>
	int ivalFromIJKV(int [] ijkv) {
		int ival = 0;
		//map index in each dimension to global index
		//is the length of ijkv equal to ndims + 1? If not throw an error
		#if(SD_CHECK_LIMITS)
		if (ijkv.Length != nd + 1) {
			Debug.LogAssertion ("In ScriptableObject.ivalFromIJKV array limits do not match");
			return 0;
		}
		#endif
		// find offset in variable based on diemsions
		int stride = 1;
		for (int d = 0; d < nd; d++) {
			// is ijkv[d] outside of acceptable range?
			#if(SD_CHECK_LIMITS)
			if(ijkv[d]<0||ijkv[d]>=n[d]) {
				Debug.LogAssertion ("In ScriptableObject.ivalFromIJKV array limits out of bounds");
				return 0;
			}
			#endif
			ival += ijkv [d] * stride;
			stride *= n[d];
		}
		//stride through variables
		int vindex = ijkv[ijkv.Length-1];
		// is vindex valid?
		#if(SD_CHECK_LIMITS)
		if (vindex<0||vindex>=nvar) {
			Debug.LogAssertion ("In ScriptableObject.ivalFromIJKV variable index out of bounds");
			return 0;
		}
		#endif
		ival += stride*vindex;
		return ival;
	}

	int calcStride() {
		int stride = 1;
		for(int d=0;d<nd;d++) {
			stride *= n [d];
		}
		return stride;
	}

	public void initTest() {
		nd = 3;
		nvar = 2;
		n = new int[]{ 3, 4, 5 };
		namesd = new string[]{"z","y","x" };
		namesv = new string[]{ "foo", "boo" };
		grid = new float[]{0.0f,0.5f,1.0f,0.0f,0.333f,0.667f,1.0f,0.0f,0.25f,0.5f,0.75f,1.0f };
		int stride = calcStride ();
		values = new float[stride* nvar];
		for (int k = 0; k < n [2]; k++) {
			for (int j = 0; j < n [1]; j++) {
				for (int i = 0; i < n [0]; i++) {
					values [(k * n [2] + j) * n [1] + i] = UnityEngine.Random.value;
					values [(k * n [2] + j) * n [1] + i+stride] = UnityEngine.Random.value+2.0f;
				}
			}
		}

	}

	/// <summary>
	/// Gets the grid stored for a given dimension
	/// </summary>
	/// <returns>The grid values.</returns>
	/// <param name="name">dimension name.</param>
	public float [] getDimension(string name) {
		int stride = calcStride ();
		int dindex = getDimensionIndex (name);
		int offset = 0;
		for (int d = 0; d < dindex; d++) {
			offset += n [d];
		}
		float[] retval = new float[n[dindex]];
		for (int d = 0; d < n[dindex]; d++) {
			retval [d] = grid [offset + d];
		}
		return retval;
	}
	

	/// <summary>
	/// Get the values for a specific variable in [,,] format
	/// </summary>
	/// <returns>values in [,,] format</returns>
	/// <param name="name">variable name.</param>
	public float [,,] getValues3D(string name) {
		float[] buffer = getValues (name);
		if (buffer == null)
			return null;
		if (nd != 3) {
			Debug.Log ("INCORRECT # dimensions");
		}
		float[,,] retval = new float[n [0], n [1], n [2]]; 
		for (int i = 0; i < buffer.Length; i++) {
			int iz = i % n [2];
			int it = i / n [2];
			int iy = it % (n [1]);
			int ix = it / (n [1]);
			retval [ix, iy, iz] = buffer [i];
		}
		return retval;
	}

	/// <summary>
	/// Get the values for a specific variable in [] format
	/// </summary>
	/// <returns>values in [] format</returns>
	/// <param name="name">variable name.</param>
	public float [] getValues(string name) {
		int stride = calcStride ();
		int vindex = getVariableIndex (name);
		if (vindex < 0)
			return null;
		float[] retval = new float[stride];
		for (int ival = 0; ival < stride; ival++) {
			retval [ival] = values [vindex * stride + ival];
		}
		return retval;
	}

	/// <summary>
	/// Get a single value based on indeces
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="ijkv">the indeces.</param>
	public float getValue(int [] ijkv) {
		return values [ivalFromIJKV (ijkv)];
	}

	/// <summary>
	/// Get a single value based on grid indeces
	/// and variable name
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="ijkv">the indeces.</param>
	public float getValue(int [] ijk, string name) {
		int vindex = getVariableIndex (name);
		int[] ijkv = new int[nd + 1]; // can a predefined array be used here to make this more efficient?
		for (int d = 0; d < nd; d++)
			ijkv [d] = ijk [d];
		ijkv [nd] = vindex;
		return values [ivalFromIJKV (ijkv)];
	}

	/// <summary>
	/// Get a single value by index for 1D file
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="i,v">the indeces.</param>
	public float getValue(int i,int v) {
		int[] ijkv = new int[2];
		#if (SD_CHECK_LIMITS)
		if(nd!=1) {
			Debug.LogAssertion("StructuredData.getValue(i,v) wrong number of dimensions");
			return 0.0;
		}
		#endif
		ijkv[0]=i;
		ijkv[1]=v;
		return(values[ivalFromIJKV(ijkv)]);
	} // shortcut for 1 dimension only

	/// <summary>
	/// Get a single value by index for 2D file
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="i,j,v">the indeces.</param>
	public float getValue(int i, int j,int v) {
		int[] ijkv = new int[3];
		#if (SD_CHECK_LIMITS)
		if(nd!=2) {
			Debug.LogAssertion("StructuredData.getValue(i,v) wrong number of dimensions");
			return 0.0;
		}
		#endif
		ijkv[0]=i;
		ijkv[1]=j;
		ijkv[2]=v;
		return(values[ivalFromIJKV(ijkv)]);
	} // shortcut for 2 dimension only

	/// <summary>
	/// Get a single value by index for 3D file
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="i,j,k,v">the indeces.</param>
	public float getValue(int i, int j, int k,int v) {
		int[] ijkv = new int[4];
		#if (SD_CHECK_LIMITS)
		if(nd!=3) {
			Debug.LogAssertion("StructuredData.getValue(i,v) wrong number of dimensions");
			return 0.0;
		}
		#endif
		ijkv[0]=i;
		ijkv[1]=j;
		ijkv[2]=k;
		ijkv[3]=v;
		return(values[ivalFromIJKV(ijkv)]);
	} // shortcut for 3 dimension only

	/// <summary>
	/// Get a single value by index for 4D file
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="i,j,k,l,v">the indeces.</param>
	public float getValue(int i, int j, int k, int l, int v) {
		int[] ijkv = new int[5];
		#if (SD_CHECK_LIMITS)
		if(nd!=4) {
			Debug.LogAssertion("StructuredData.getValue(i,v) wrong number of dimensions");
			return 0.0;
		}
		#endif
		ijkv[0]=i;
		ijkv[1]=j;
		ijkv[2]=k;
		ijkv[3]=l;
		ijkv[4]=v;
		return(values[ivalFromIJKV(ijkv)]);
	} // shortcut for 4 dimension only
}
