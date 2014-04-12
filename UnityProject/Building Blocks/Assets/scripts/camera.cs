using UnityEngine;
using System.Collections;

public class camera : MonoBehaviour {

	public float deadZone = 0.4f;
	public int movementSpeed = 10;
	public float buttonMovementSpeed = 0.25f;

	// Use this for initialization
	void Start () {


	
	}
	
	// Update is called once per frame
	void Update () {
		float mousePos = 0;
//		float mousePos = (Input.mousePosition.y / Screen.height) - 0.5f;
//		if (mousePos > deadZone)
//		{
//			mousePos -= deadZone;
//		}
//		else if (mousePos < -deadZone)
//		{
//			mousePos += deadZone;
//		}
//		else
//		{
//			mousePos = 0;
//		}

		if (Input.GetAxis("Vertical") != 0)
		{
			mousePos = Input.GetAxis("Vertical") * buttonMovementSpeed;
		}


		transform.Translate(new Vector3(0, mousePos * Time.deltaTime * movementSpeed));transform.Translate(new Vector3(0, mousePos * Time.deltaTime * movementSpeed));
	}
}
