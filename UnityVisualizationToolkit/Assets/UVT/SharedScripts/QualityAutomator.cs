using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quality automator.
/// 
/// https://answers.unity.com/questions/205473/changing-quality-setting-bases-on-fps-.html
/// 
/// From Unity message board post
/// 
/// </summary>
public class QualityAutomator : MonoBehaviour {
	// The min and max frameRate determines at what point it will switch up or down
	public double maxFrameRate;
	public double minFrameRate;
	// Sometimes, there are lag spikes that happen over one or two frames,
	// but are not necessarily related to the quality settings. The 
	// switchThreshold is the number of frames in a row that must be too
	// slow or fast before a switch occurs.
	public int switchThreshold;

	int upCounter;
	int downCounter;

	void Update () {
		if((1 / Time.smoothDeltaTime) > maxFrameRate)
		{
			upCounter++;
			if(upCounter > switchThreshold)
			{
				QualitySettings.IncreaseLevel();
				upCounter = -3;
			}
		} else {
			upCounter = 0;
		}
		//Debug.Log(upCounter);

		if((1 / Time.smoothDeltaTime) < minFrameRate)
		{
			downCounter++;
			if(downCounter > switchThreshold)
			{
				QualitySettings.DecreaseLevel();

				downCounter = -3;
			}
		} else {
			downCounter = 0;
		}
	}
}
