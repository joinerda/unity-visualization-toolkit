using UnityEngine;
using System.Collections;

/// <summary>
/// cylinders for ball-stick molecule visualizations
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class CylinderSegment {

	public static GameObject Create(Vector3 begin, Vector3 end, float width) {
		float length = (begin - end).magnitude*0.5f;
		Vector3 pos = 0.5f * (begin + end);
		Vector3 scale = Vector3.one * width;
		scale.y = length;

		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
		go.transform.localScale = scale;
		go.transform.rotation = Quaternion.LookRotation (end - begin);
		go.transform.Rotate ( 90,0,0);
		go.transform.position = pos;

		return go;
	}

}
