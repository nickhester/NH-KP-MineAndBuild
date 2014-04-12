using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class help : MonoBehaviour {

	private List<tile> allExampleTiles = new List<tile>();
	private bool helpIsOn = false;
	public GameObject screenCover;
	public GUIStyle helpScreenStyle;

	// Use this for initialization
	void Start () {
		allExampleTiles.AddRange(GetComponentsInChildren<tile>());
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.H))
		{
			if (!helpIsOn)
			{
				screenCover.collider.enabled = true;
				screenCover.renderer.enabled = true;
				foreach (var _tiles in allExampleTiles)
				{
					_tiles.gameObject.renderer.enabled = true;
					helpIsOn = true;
				}
			}
			else
			{
				screenCover.collider.enabled = false;
				screenCover.renderer.enabled = false;
				foreach (var _tiles in allExampleTiles)
				{
					_tiles.gameObject.renderer.enabled = false;
					helpIsOn = false;
				}
			}
		}
	}

	void OnGUI ()
	{
		if (helpIsOn)
		{
			GUI.Box(new Rect(0, 0, Screen.width, Screen.height),
			        "dirt:\nmine to gain resources\n\n" +
			        "rock:\nrequires a factory to mine\n\n" +
			        "empty structure:\ncan be converted into other structures\n\n" +
			        "residence:\nrequired to build productive structures\n\n" +
			        "community:\nupgrade from residence. required to build many residences\n\n" +
			        "fortified structure:\nincreases strength of structures to allow structures building higher\n\n" +
			        "double fortified structure:\nfurther increases strength of structures\n\n" +
			        "mine:\nmines allow mining resources on rows near the mine\n\n" +
			        "mill:\nupgrade from mine. each mill increases the amount\nof resources gained from dirt\n\n" +
			        "factory:\nupgrade from mine. allows mining rocks. each\nfactory decreases the cost of mining rocks\n\n" +
			        "sonar:\nupgrade from factory. sonar will show whether a gem\nexists below it in its same column\n\n" +
			        "mounted gem:\nonce a gem is found it can be mounted at the top\nof a structure. gems must be displayed at a minimum height\n\n"
			        , helpScreenStyle);

			GUI.Box(new Rect(120, 10, 700, 40), "Instructions: Mine resources and build structures in order to find all the valuable gems on each level\n" +
			        "The gems must be mounted high in the sky at the top of your structures to complete each level.");
		}
	}
}
