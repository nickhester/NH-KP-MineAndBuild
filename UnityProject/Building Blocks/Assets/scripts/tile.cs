﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tile : MonoBehaviour {

	public enum tileType
	{
		dirt,
		rock,
		bedrock,
		empty,
		none,
		structure,
		structure_fortified,
		structure_fortified_2,
		structure_mining,
		structure_factory,
		structure_mill,
		structure_residence,
		structure_community,
		structure_sonar,
		structure_sonar_positive,
		structure_gemMount,
		rockWithGem
	};

	public tileType thisTileType;
	public List<Material> allMaterials = new List<Material>();
	public List<Material> alternateMaterials = new List<Material>();
	[HideInInspector]
	public List<tileType> structureTypes = new List<tileType>();
	[HideInInspector]
	public List<tileType> structureTypesNonLoadBearing = new List<tileType>();
	[HideInInspector]
	public int thisTileY;
	[HideInInspector]
	public int thisTileX;
	public tile myNeighborUp;
	public tile myNeighborDown;
	public tile myNeighborLeft;
	public tile myNeighborRight;

	public bool updateOnDrawGizmos = true;

	// Use this for initialization
	void Start () {
		UpdateMat();
		int[] thisTileXandY = GetTilePosition();
		thisTileX = thisTileXandY[0];
		thisTileY = thisTileXandY[1];

		// specify which tile types are structural
		structureTypes.Add(tileType.structure);
		structureTypes.Add(tileType.structure_fortified);
		structureTypes.Add(tileType.structure_fortified_2);
		structureTypes.Add(tileType.structure_mining);
		structureTypes.Add(tileType.structure_factory);
		structureTypes.Add(tileType.structure_mill);
		structureTypes.Add(tileType.structure_residence);
		structureTypes.Add(tileType.structure_community);

		// specify which tile types are non-load bearing structural
		structureTypesNonLoadBearing.Add(tileType.structure);
		structureTypesNonLoadBearing.Add(tileType.structure_mining);
		structureTypesNonLoadBearing.Add(tileType.structure_factory);
		structureTypesNonLoadBearing.Add(tileType.structure_mill);
		structureTypesNonLoadBearing.Add(tileType.structure_residence);
		structureTypesNonLoadBearing.Add(tileType.structure_community);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		if (updateOnDrawGizmos) { UpdateMat(); }
	}

	int[] GetTilePosition()
	{
		int[] result = { (int)transform.position.x, (int)transform.position.y };
		return result;
	}

	// Make sure that all tiles are named and material'd correctly based on their settings
	void UpdateMat()
	{
		if (thisTileType == tileType.dirt)
		{
			renderer.material = allMaterials[0];
			name = "dirt";
		}
		else if (thisTileType == tileType.rock)
		{
			renderer.material = allMaterials[1];
			name = "rock";
		}
		else if (thisTileType == tileType.bedrock)
		{
			renderer.material = allMaterials[2];
			name = "bedrock";
		}
		else if (thisTileType == tileType.empty)
		{
			renderer.material = allMaterials[3];
			name = "empty";
		}
		else if (thisTileType == tileType.structure)
		{
			renderer.material = allMaterials[4];
			name = "structure";
		}
		else if (thisTileType == tileType.structure_fortified)
		{
			renderer.material = allMaterials[5];
			name = "structure_fortified";
		}
		else if (thisTileType == tileType.structure_fortified_2)
		{
			renderer.material = allMaterials[6];
			name = "structure_fortified_2";
		}
		else if (thisTileType == tileType.structure_mining)
		{
			renderer.material = allMaterials[7];
			name = "structure_mining";
		}
		else if (thisTileType == tileType.structure_factory)
		{
			renderer.material = allMaterials[8];
			name = "structure_factory";
		}
		else if (thisTileType == tileType.structure_mill)
		{
			renderer.material = allMaterials[9];
			name = "structure_mill";
		}
		else if (thisTileType == tileType.structure_residence)
		{
			renderer.material = allMaterials[10];
			name = "structure_residence";
		}
		else if (thisTileType == tileType.structure_community)
		{
			renderer.material = allMaterials[11];
			name = "structure_community";
		}
		else if (thisTileType == tileType.structure_sonar)
		{
			renderer.material = allMaterials[12];
			name = "structure_sonar";
		}
		else if (thisTileType == tileType.structure_gemMount)
		{
			renderer.material = allMaterials[13];
			name = "structure_gemMount";
		}
		else if (thisTileType == tileType.rockWithGem)
		{
			renderer.material = allMaterials[14];
			name = "rockWithGem";
		}

		// alternate materials
		else if (thisTileType == tileType.structure_sonar_positive)
		{
			renderer.material = alternateMaterials[0];
			name = "structure_sonar_positive";
		}
	}

	public void ChangeTileType(tileType newType)
	{
		thisTileType = newType;
		UpdateMat();
	}
}