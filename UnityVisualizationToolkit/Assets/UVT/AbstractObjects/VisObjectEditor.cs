#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Vis object editor.
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
[CustomEditor(typeof(VisObject))]
public class VisObjectEditor : Editor {
	override public void OnInspectorGUI() {
		VisObject visObject = target as VisObject;
		SerializedObject serializedObject = new SerializedObject (target);

		visObject.forceRefresh = EditorGUILayout.Toggle ("Force Refresh", visObject.forceRefresh);

		visObject.dataObject = (GameObject) EditorGUILayout.ObjectField ("Data Object",visObject.dataObject,typeof(GameObject),true);
		visObject.animate = EditorGUILayout.Toggle ("Animate", visObject.animate);
		if(visObject.animate)
		{
			visObject.animationLag = EditorGUILayout.FloatField("Animation Lag Time", visObject.animationLag);

		}
		visObject.visType = (VisObject.VisType) EditorGUILayout.EnumPopup ("Vis Type", visObject.visType);
		visObject.alpha = EditorGUILayout.FloatField("Alpha", visObject.alpha);

		if (visObject.visType != VisObject.VisType.MOLECULE) {
			SerializedProperty indVarNames = serializedObject.FindProperty ("indVarNames");
			EditorGUILayout.PropertyField (indVarNames, new GUIContent ("Independent Variable Names"), true);
			if (visObject.visType == VisObject.VisType.SURFACE || 
				visObject.visType == VisObject.VisType.ISOCONTOUR ||
				visObject.visType == VisObject.VisType.THRESHHOLD ||
				visObject.visType == VisObject.VisType.VOLUME
			) {
				visObject.depVar = EditorGUILayout.TextField ("Dependent Var. Name", visObject.depVar);
			}
			if (visObject.visType == VisObject.VisType.VOLUME) {
				SerializedProperty depLimits = serializedObject.FindProperty ("depLimits");
				EditorGUILayout.PropertyField (depLimits, new GUIContent ("Dependent Var. Limits"), true);
				visObject.opacity = EditorGUILayout.FloatField ("Opacity", visObject.opacity);
				visObject.emissivity = EditorGUILayout.FloatField ("Emissivity", visObject.emissivity);

			}
			if (visObject.visType == VisObject.VisType.SURFACE)
			{
				visObject.zScale = EditorGUILayout.FloatField("Z Scale", visObject.zScale);
				visObject.mapSurfaceToSphere = EditorGUILayout.Toggle("Map to Sphere", visObject.mapSurfaceToSphere);
				if(visObject.mapSurfaceToSphere)
				{
					visObject.latLonFlipped = EditorGUILayout.Toggle("Flip Lat/Lon", visObject.latLonFlipped);

					SerializedProperty mapSurfaceBounds = serializedObject.FindProperty("mapSurfaceBounds");
					EditorGUILayout.PropertyField(mapSurfaceBounds, new GUIContent("Lat/Lon/R"), true);
				}
			}
			if (visObject.visType == VisObject.VisType.ISOCONTOUR) {
				//visObject.isoValue = EditorGUILayout.FloatField("Isocontour Value", visObject.isoValue);
				SerializedProperty isoRange = serializedObject.FindProperty("isoRange");
				EditorGUILayout.PropertyField(isoRange, new GUIContent("Isocontour Values"), true);
				SerializedProperty suppliedMat = serializedObject.FindProperty("suppliedMat");
				EditorGUILayout.PropertyField(suppliedMat, new GUIContent("Material"), true);
			}
			if (visObject.visType == VisObject.VisType.THRESHHOLD) {
				visObject.threshholdType = (Threshhold.ThreshholdType) 
					EditorGUILayout.EnumPopup ("Threshhold Type", visObject.threshholdType);

				visObject.minValue = EditorGUILayout.FloatField ("Min Value", visObject.minValue);
				visObject.maxValue = EditorGUILayout.FloatField ("Max Value", visObject.maxValue);

				visObject.threshholdPRE = (GameObject) EditorGUILayout.ObjectField ("Threshhold PRE",visObject.threshholdPRE,typeof(GameObject),true);
				visObject.maxDepth = EditorGUILayout.IntField ("Max Depth", visObject.maxDepth);

			}
			if (visObject.visType != VisObject.VisType.THRESHHOLD) {
				visObject.colorVar = EditorGUILayout.TextField ("Color Variable Name", visObject.colorVar);
			}
			if (visObject.colorVar == null || visObject.visType == VisObject.VisType.THRESHHOLD) {
				visObject.color = EditorGUILayout.ColorField ("Solid Color", visObject.color);
			} else if (visObject.colorVar.Equals ("")) {
				visObject.color = EditorGUILayout.ColorField ("Solid Color", visObject.color);
			} else {
				if (visObject.colorScale.Length != visObject.colorMap.Length) {
					EditorGUILayout.LabelField ("Warning colorScale.Length != ");
					EditorGUILayout.LabelField ("        colorMap.Length != ");
				}
				SerializedProperty colorScale = serializedObject.FindProperty ("colorScale");
				EditorGUILayout.PropertyField (colorScale, new GUIContent ("Color Scale"), true);
				SerializedProperty colorMap = serializedObject.FindProperty ("colorMap");
				EditorGUILayout.PropertyField (colorMap, new GUIContent ("Color Map"), true);

			}
		}
		if (visObject.visType == VisObject.VisType.GLYPH) {
			SerializedProperty sizeVars = serializedObject.FindProperty ("sizeVars");
			EditorGUILayout.PropertyField (sizeVars,new GUIContent("Size Variable Name(s)"),true);

			visObject.glyph1DPRE = (GameObject) EditorGUILayout.ObjectField ("1D Glyph",visObject.glyph1DPRE,typeof(GameObject),true);
			visObject.glyph3DPRE = (GameObject) EditorGUILayout.ObjectField ("3D Glyph",visObject.glyph3DPRE,typeof(GameObject),true);
			visObject.glyphScale = EditorGUILayout.Vector3Field ("Glyph Scale", visObject.glyphScale);
			visObject.useMask = EditorGUILayout.Toggle ("Use Mask", visObject.useMask);
			if(visObject.useMask) visObject.mask = EditorGUILayout.IntField ("Mask", visObject.mask);
			visObject.drawBars = EditorGUILayout.Toggle ("Draw as Bars", visObject.drawBars);
			visObject.alpha = EditorGUILayout.FloatField ("Transparency", visObject.alpha);
		}

		if (visObject.visType == VisObject.VisType.MOLECULE) {
			visObject.bondWidth = EditorGUILayout.FloatField ("Bond Width",visObject.bondWidth);
			visObject.atomSize = EditorGUILayout.FloatField ("Atom Size",visObject.atomSize);
		}
			
		serializedObject.ApplyModifiedProperties ();

		if (GUI.changed) {
			EditorUtility.SetDirty (target);
			if (!EditorApplication.isPlaying) {
				EditorSceneManager.MarkSceneDirty (((VisObject)target).gameObject.scene);
			}
		}
	}
}
#endif
