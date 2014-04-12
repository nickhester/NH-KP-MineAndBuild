using UnityEngine;
using System.Collections;

public class winningScreen : MonoBehaviour {

	private resourceMgr resourceManager;
	public bool isWinningScreenOn = false;

	public GUIStyle winScreenStyle;

	// Use this for initialization
	void Start () {
		resourceManager = GameObject.Find("resourceMgr").GetComponent<resourceMgr>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void YouWin()
	{
		isWinningScreenOn = true;
	}

	void OnGUI()
	{
		if (isWinningScreenOn)
		{
			GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "YOU WIN!\n\n" + "stats:\n\n" +
			        "highest building: " + resourceManager.highestBuildingLevel + "\n" +
			        "number of residences: " + resourceManager.countResidence + "\n" + 
			        "number of communities: " + resourceManager.countCommunity + "\n" + 
			        "number of mines: " + resourceManager.countMining + "\n" + 
			        "number of mills: " + resourceManager.countMill + "\n" + 
			        "number of factories: " + resourceManager.countFactory + "\n" + 
			        "ending value of dirt: " + resourceManager.valueOfDirt + "\n" + 
			        "ending cost to mine rock: " + resourceManager.costToMineRock + "\n", winScreenStyle);
		}
	}
}
