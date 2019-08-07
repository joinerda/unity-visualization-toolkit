using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A generic data object to simplify the process of adding data to a scene in the
/// Unity editor, and allow for setting data type and filename in the editor or
/// through a script.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class DataObject : MonoBehaviour {

    /// <summary>
    /// supported data types
    /// </summary>
    public enum DataTypes
    {
        STRUCTURED_WOD, STRUCTURED_C2D, UNSTRUCTURED_WOD, UNSTRUCTURED_CSV, MOLECULE_MOL, MANUAL
    };
    string[] DataTypeNames = {"Structured (WOD)","Structured (C2D)",
        "Unstructured (WOD)",
        "Unstructured (CSV)",
        "Molecule (MOL)", "MANUAL (coded)"};

    public string filename = "csv_input.txt";
	string filepath;
	public DataTypes dataType = DataTypes.UNSTRUCTURED_CSV;
	System.Object data = null;

	/// <summary>
	/// Gets the data.
	/// </summary>
	/// <returns>The data.</returns>
	public System.Object getData() {
		return data;
	}

	public void setData(System.Object data)
	{
		if(dataType != DataTypes.MANUAL)
		{
			Debug.Log("setData only allowed for data type MANUAL");
			return;
		}
		if(data.GetType() == typeof(StructuredData))
		{
			this.data = data;
		} else if(data.GetType() == typeof(UnstructuredData))
		{
			this.data = data;
		}
		else if(data.GetType()==typeof(Molecule))
		{
			this.data = data;
		}
		else
		{
			Debug.Log("Manual setting of data not supported for this type");
		}
	}

	void processFilepath(string filepath){
		switch (dataType) {
			case DataTypes.MANUAL:
				Debug.Log("processFilePath not supported for data type MANUAL");
				break;
		    case DataTypes.UNSTRUCTURED_CSV:
			    data = ScriptableObject.CreateInstance ("UnstructuredData");
			    ((UnstructuredData)data).readCSV (filepath);
			    break;
		    case DataTypes.UNSTRUCTURED_WOD:
			    data = ScriptableObject.CreateInstance ("UnstructuredData");
			    ((UnstructuredData)data).readWODHeaders (filepath);
			    ((UnstructuredData)data).readWODSet ();

			    break;
            case DataTypes.STRUCTURED_C2D:
                data = ScriptableObject.CreateInstance("StructuredData");
                ((StructuredData)data).readC2D(filepath);

                break;
            case DataTypes.STRUCTURED_WOD:
		        data = ScriptableObject.CreateInstance ("StructuredData");
		        ((StructuredData)data).readWOD (filepath);

		        break;
		    case DataTypes.MOLECULE_MOL:
			    data = Molecule.readSybMol (filepath);

			    break;
		    default:
			    Debug.Log ("Unrecognized file type");
			    break;
		}
		//dibDropdown.value = (int)dataType;

	}

	// Use this for initialization
	void Start () {
		filepath = Application.dataPath+"/StreamingAssets/LocalFiles/"+filename;
		if(dataType!= DataTypes.MANUAL) { 
			processFilepath (filepath);
		}
		/*
		dataInfoBox = (GameObject)Instantiate (dataInfoPRE, GameObject.Find ("Canvas").transform);
		dibButton = dataInfoBox.GetComponentInChildren<Button> () as Button;
		dibInput = dataInfoBox.gameObject.GetComponentInChildren<InputField> () as InputField;
		dibDropdown = dataInfoBox.GetComponentInChildren<Dropdown> () as Dropdown;
		List<string> options = new List<string>();
		for (int i = 0; i < DataTypeNames.Length; i++) {
			options.Add (DataTypeNames [i]);
		}
		dibDropdown.AddOptions (options);

		dibButton.onClick.AddListener (delegate{processForm();});
		dataInfoBox.SetActive (false);
		*/

	}

	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Advance WOD unstructured file to the next set
	/// </summary>
	public void nextSet(bool skipData=false) {
		switch (dataType) {
		    case DataTypes.UNSTRUCTURED_CSV:
			    break;
		    case DataTypes.UNSTRUCTURED_WOD:
			    ((UnstructuredData)data).readWODSet ();// add data skip at some point
			    break;
            case DataTypes.STRUCTURED_C2D:
				((StructuredData)data).readC2DSet(skipData);
				break;
            case DataTypes.STRUCTURED_WOD:
			    break;
		    case DataTypes.MOLECULE_MOL:
			    break;
			case DataTypes.MANUAL:
				Debug.Log("set reading not supported for data type MANUAL");
				break;
		    default:
			    break;
		}
	}
}
