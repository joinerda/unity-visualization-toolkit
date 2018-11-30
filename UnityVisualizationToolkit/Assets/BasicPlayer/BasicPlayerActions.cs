using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.VR;


public class BasicPlayerActions : MonoBehaviour {


	float movementSpeed = 20.0f;
	float lookScale = 2.0f;
	float VRrollScale = 0.1f;
	float VRmouseScale = 0.03f;
	Camera maincam;
	Vector3 upright = Vector3.zero;
	public GameObject dataObjectPRE;

	public void putDataInScene () {
		Instantiate (dataObjectPRE, transform.position + transform.forward * 5.0f, Quaternion.identity);
	}


	public static Vector3 Rolerp(Vector3 a, Vector3 b, float t) {
		Vector3 lerped = new Vector3 ();
		lerped = a;
		lerped.x += Mathf.DeltaAngle (a.x, b.x) * t;
		lerped.y += Mathf.DeltaAngle (a.y, b.y) * t;
		lerped.z += Mathf.DeltaAngle (a.z, b.z) * t;
		return lerped;
	}

	CharacterController cc;
	// Use this for initialization
	void Start () {
		cc = GetComponent<CharacterController> ();
	
	}
	
	// Update is called once per frame
	void Update () {

		float mouseX = 0.0f;
		float mouseY = 0.0f;
		if(Input.GetMouseButton(1)) {
			mouseX = Input.GetAxis ("Mouse X");
			mouseY = Input.GetAxis ("Mouse Y");
		}
		mouseX += Input.GetAxis("Joystick X");
		mouseY += Input.GetAxis("Joystick Y");
		float horizontal = Input.GetAxis ("Horizontal");
		float vertical = Input.GetAxis ("Vertical");
		float roll = Input.GetAxis ("Roll");
		float jump = Input.GetAxis ("Jump");


		if (roll==0.0f) {
			Vector3 rotationVector = transform.parent.rotation.eulerAngles;
			//upright.x = rotationVector.x;
			//upright.y = rotationVector.y;
			//rotationVector = Rolerp (transform.parent.rotation.eulerAngles, upright, 2.5f*Time.deltaTime);
			//transform.parent.rotation = Quaternion.Euler(rotationVector);
			float z = upright.z-rotationVector.z;
			while (z > 180)
				z -= 360;
			while (z < -180)
				z += 360;

			transform.parent.Rotate (new Vector3 (0, 0, 1), 1.1f * (z)*Mathf.Max(Time.deltaTime,0.02f));

		}

		if (UnityEngine.XR.XRSettings.enabled) {
			Quaternion Angles = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
			transform.localRotation = Angles;
			transform.parent.RotateAround (transform.forward, -VRrollScale*roll);
			transform.parent.RotateAround (transform.right, -VRmouseScale*mouseY);
			transform.parent.RotateAround (transform.up, VRmouseScale*mouseX);
			//transform.parent.Rotate (-lookScale*mouseY, lookScale*mouseX,-roll);
		} else {
			//Camera.main.transform.rotation = transform.rotation;
			transform.parent.Rotate (-lookScale*mouseY, lookScale*mouseX,-roll);

		}
		if(roll!=0.0f) {
			upright.z = transform.parent.rotation.eulerAngles.z;
		}



		cc.Move (movementSpeed*transform.forward * vertical*Time.deltaTime + transform.right * horizontal*movementSpeed*Time.deltaTime
		         +movementSpeed*transform.up*jump*Time.deltaTime);

		Vector3 temp = transform.position;
		transform.parent.position = temp;
		transform.localPosition = Vector3.zero;

		if(Input.GetButtonDown("Terminate")) {
			/*
			ModelActions model = GameObject.Find("Model").GetComponent<ModelActions>();
			if(model.threaded) {
				model.threadRunning=false;
				model.modelThread.Abort();
			}
			*/
			Application.Quit();
		}

		/*
		if (Input.GetButtonDown ("SceneFlip")) {
			Debug.Log ("SceneFlip");
			int current = SceneManager.GetActiveScene ().buildIndex;
			int next = (current + 1) % SceneManager.sceneCountInBuildSettings;
			SceneManager.LoadScene (next);
		}
		*/

	}
}
