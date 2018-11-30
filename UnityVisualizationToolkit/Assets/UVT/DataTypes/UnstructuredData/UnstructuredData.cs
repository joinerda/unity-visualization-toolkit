using UnityEngine;
using System;
using System.Collections;
using System.IO;

/// <summary>
/// Unstructured data scriptable object.
/// Used in World of Data for storing unstructured (point-like) data objects.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class UnstructuredData : ScriptableObject  {
	int n_data;
	int n_data_padded=64;
	int n_columns;
	float [] values;
	string [] names;
	bool setRewound=false;
	string filepath = null;

	StreamReader sr_wod = null;

	/// <summary>
	/// Reads the headers from file in WOD unstructured format.
	/// </summary>
	/// <param name="filepath">Filepath.</param>
	public void readWODHeaders(string filepath){
		this.filepath = filepath;
		sr_wod = new StreamReader (filepath);
		string line, key, val;
		// just read headers
		string [] namesToAdd = null;
		while (IOExtras.ReadLine2 (sr_wod, out line)) {
			IOExtras.KeyVal (line, out key, out val);
			if (key.ToLower ().Equals ("header")) {
				namesToAdd = IOExtras.StringArray (val);
				break;
			}
		}
		if (namesToAdd == null) {
			Debug.Log ("Never found headers in readWODHeaders");
		}
		// add empty columns
		setColumns(namesToAdd);

	}

	/// <summary>
	/// read individual dataset from WOD file format 
	/// </summary>
	/// <returns>The WOD set.</returns>
	public string readWODSet() {
		purgeValues ();
		Debug.Log ("Reading Set");
		Debug.Log (filepath);
		if (sr_wod == null) {
			Debug.Log ("File is null");
			return null;
		}
		string comment = "";
		string line, key, val;

		// read one set
		bool setFound = false;
		while (IOExtras.ReadLine2 (sr_wod, out line)) {
			IOExtras.KeyVal (line, out key, out val);
			if (key.ToLower ().Equals ("data")) {
				comment = val;
				setFound = true;
				setRewound = false;
				break;
			}
		}
		if (setFound) {
			while (IOExtras.ReadLine2 (sr_wod, out line)) {
				IOExtras.KeyVal (line, out key, out val);
				if (key.ToLower ().Equals ("end")) {
					break;
				}
				float[] valuesToAdd = IOExtras.FloatArray (line);
				addRow (valuesToAdd);
			}
			return comment;
		} else {
			if (setRewound) {
				Debug.Log ("File has no sets");
				return null;
			} else {
				Debug.Log ("Rewinding");
				sr_wod.BaseStream.Position = 0;
				sr_wod.DiscardBufferedData ();
				setRewound = true;
				return readWODSet ();
			}
		}
	}

	/// <summary>
	/// read file in CSV format
	/// </summary>
	/// <param name="filepath">Filepath.</param>
	public void readCSV(string filepath) {
		string[] namesToAdd;
		string line;
		// test to make sure filepath exsists, if it doesnt return error gracefully
		StreamReader sr = new StreamReader(filepath);
		// read headings
		if (!IOExtras.ReadLine2 (sr, out line)) {
			// if fail to read headings return error
			Debug.Log("No lines in CSV file");
			return;
		}
		namesToAdd = IOExtras.StringArray (line);
		// add empty columns
		setColumns(namesToAdd);
		// read each line, adding to data using addrow
		while (IOExtras.ReadLine2 (sr, out line)) {
			float[] valuesToAdd = IOExtras.FloatArray (line);
			addRow (valuesToAdd);
		}
	}

	/// <summary>
	/// Init this instance to zero data and zero columns
	/// </summary>
	public void init() {
		n_data = 0;
		n_columns = 0;
	}

	/// <summary>
	/// Purges the values but leaves structure of columns intact
	/// </summary>
	void purgeValues() {
		values = new float[n_data_padded * names.Length];
		n_data = 0;
	}

	/// <summary>
	/// Set the specified names and values.
	/// </summary>
	/// <param name="names">Names.</param>
	/// <param name="values">Values.</param>
	public void set(string [] names, float [] values) {
		if (values.Length % names.Length != 0) {
			Debug.LogAssertion ("UnstructuredData.set length of values not divisible by number of columns");
			return;
		}
		n_columns = names.Length;
		n_data = values.Length / n_columns;
		this.names = names;
		this.values = values;
		n_data_padded = n_data;
	}

	/// <summary>
	/// adjust the padding of arrays
	/// </summary>
	/// <param name="new_n_data">New n data.</param>
	public void setNData(int new_n_data) {
		if (new_n_data > n_data_padded) {
			// grow arrays
			float [] tvalues = new float[new_n_data*2*n_columns];
			for (int i = 0; i < n_columns; i++) {
				for (int j = 0; j < n_data; j++) {
					tvalues [i * new_n_data * 2 + j] = values [i * n_data_padded + j];
				}
			}
			values = tvalues;
			n_data_padded = new_n_data*2;
		}
	}

	/// <summary>
	/// Sets the column names.
	/// </summary>
	/// <param name="names">Names.</param>
	public void setColumns(string [] names) {
		float [] tvalues = new float[n_data_padded*names.Length];
		for (int i = 0; i < n_columns; i++) {
			for (int j = 0; j < n_data; j++) {
				tvalues [i * n_data_padded + j] = values [i * n_data_padded + j];
			}
		}
		this.names = names;
		n_columns = names.Length;
		values = tvalues;
	}

	/// <summary>
	/// get the number of rows
	/// </summary>
	/// <returns>The N data.</returns>
	public int getNData() {
		return n_data;
	}

	/// <summary>
	/// get the number of columns
	/// </summary>
	/// <returns>The N columns.</returns>
	public int getNColumns() {
		return n_columns;
	}

	/// <summary>
	/// get the names of the columns
	/// </summary>
	/// <returns>The names.</returns>
	public string [] getNames() {
		return names;
	}

	/// <summary>
	/// get a single row
	/// </summary>
	/// <returns>The row.</returns>
	/// <param name="index">Index.</param>
	public float [] getRow(int index) {
		// does row exist?
		float [] retval = new float[n_data];
		for(int i=0;i<n_columns;i++) {
			retval[i]=values[index*n_data_padded+n_data];
		}
		return retval;
	}

	/// <summary>
	/// add a row to the data
	/// </summary>
	/// <param name="data">Data.</param>
	public void addRow(float [] data) {
		// length of input data?
		if (data.Length != n_columns) {
			Debug.LogAssertion ("UnstructuredData.addRow array lengths do not match");
			return;
		}
		if (n_data == n_data_padded) {
			// grow arrays
			float [] tvalues = new float[n_data_padded*2*n_columns];
			for (int i = 0; i < n_columns; i++) {
				for (int j = 0; j < n_data; j++) {
					tvalues [i * n_data_padded * 2 + j] = values [i * n_data_padded + j];
				}
			}
			values = tvalues;
			n_data_padded *= 2;
		}
		for(int i=0;i<n_columns;i++) {
			values[i*n_data_padded+n_data]=data[i];
		}
		n_data++;
	}

	/// <summary>
	/// get a value by name of column from a row
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="name">Name.</param>
	/// <param name="row">Row.</param>
	public float getValue(string name, int row) {
		int col = Array.IndexOf (names, name);
		if (col < 0) {
			//Debug.LogAssertion ("UnstructuredData.getValue column name does not exist");
			return 0.0f;
		}
		if (row < 0 || row >= n_data) {
			//Debug.LogAssertion ("UnstructuredData.getValue row does not exist");
			return 0.0f;
		}
		return values [col * n_data_padded + row];
	}

	/// <summary>
	/// Sets the value of a named column by row.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="row">Row.</param>
	/// <param name="new_value">New value.</param>
	public void setValue(string name, int row, float new_value) {
		int col = Array.IndexOf (names, name);
		if (col < 0) {
			Debug.LogAssertion ("UnstructuredData.setValue column name does not exist");
			return;
		}
		if (row < 0 || row >= n_data) {
			Debug.LogAssertion ("UnstructuredData.setValue row does not exist");
			return;
		}
		values [col * n_data_padded + row] = new_value;
	}

	/// <summary>
	/// set a row of data
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="data">Data.</param>
	public void setRow(int index, float [] data) {
		// length of input data?
		if (data.Length != n_columns) {
			Debug.LogAssertion ("UnstructuredData.setRow array lengths do not match");
			return;
		}
		if (index < 0 || index >= n_data) {
			Debug.LogAssertion ("UnstructuredData.setRow index out of range");
			return;
		}
		for(int i=0;i<n_columns;i++) {
			values[index*n_data+n_data]=data[i];
		}
	}

	/// <summary>
	/// set a column of data
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="data">Data.</param>
	public void setColumn(string name, float [] data) {
		//need to check for existenace of column name
		// array size mismatch?
		if (data.Length != n_data) {
			Debug.LogAssertion ("UnstructuredData.setColumn array lengths do not match");
			return;
		}
		int col = Array.IndexOf (names, name);
		if (col < 0) {
			Debug.LogAssertion ("UnstructuredData.setColumn column name does not exist");
			return;
		}
		for (int i = 0; i < n_data; i++) {
			values [n_data_padded*col+i] = data [i];
		}
	}

	/// <summary>
	/// add a new column to the data structure
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="data">Data.</param>
	public void addColumn(string name, float [] data) {
		// need to check for existance of column name
		// array size mismatch?
		if (data.Length != n_data) {
			Debug.LogAssertion ("UnstructuredData.addColumn array lengths do not match");
			return;
		}
		int col = Array.IndexOf (names, name);
		if (col >= 0) {
			Debug.LogAssertion ("UnstructuredData.addColumn column name already exists");
			return;
		}
		string[] tnames = new string[n_columns + 1];
		for (int i = 0; i < n_columns; i++) {
			tnames [i] = names [i];
		}
		names = tnames;
		float[] tvalues = new float[n_data_padded * (n_columns + 1)];
		for (int i = 0; i < n_data * n_columns; i++) {
			tvalues [i] = values [i];
		}
		values = tvalues;
		names [n_columns] = name;
		for (int i = 0; i < n_data; i++) {
			values [n_data_padded*n_columns+i] = data [i];
		}
		n_columns++;
	}

	/// <summary>
	/// get a column of data by name
	/// </summary>
	/// <returns>The column.</returns>
	/// <param name="name">Name.</param>
	public float [] getColumn(string name) {
		int col = Array.IndexOf (names, name);
		if (col < 0) {
			Debug.LogAssertion ("UnstructuredData.getColumn column name does not exist");
			return null; // column does not exist
		}
		float[] retval = new float[n_data];
		for (int i = 0; i < n_data; i++) {
			retval [i] = values [col * n_data_padded + i];
		}
		return retval;
	}
		

}
