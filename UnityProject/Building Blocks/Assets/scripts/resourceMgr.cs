using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class resourceMgr : MonoBehaviour {
	
	public List<tile> allTiles = new List<tile>();

	// Mines and minable statuses
	public Dictionary<int, bool> minableTiles = new Dictionary<int, bool>();
	public List<tile> mineTiles = new List<tile>();

	// costs
	public int valueOfDirt = 1;
	[HideInInspector]
	public int valueOfDirt_Adjusted;
	public int costToMineRock = 10;
	[HideInInspector]
	public int costToMineRock_Adjusted;
	public int costOfStructure = 2;
	public int costOfStructure_reenforced = 4;
	public int costOfStructure_reenforced_2 = 6;
	public int costOfStructure_unReenforce = -1;
	public int costOfStructure_mining = 2;
	public int costOfStructure_mill = 3;
	public int costOfStructure_factory = 3;
	public int costOfStructure_sonar = 20;
	public int costOfStructure_residence = 2;
	public int costOfStructure_community = 6;
	public int costToDestroyStructure = -1;
	private int costToRemodel;					// This gets set to the costOfStructure + costToDestroyStructure in Start()
	public int PriceChangeOfRockPerFactory = 2;
	public int PriceChangeOfDirtPerMill = 1;
	public int prizeDirtForMountingGem = 30;

	// tile strengths
	public int strengthOfDirt = 1;
	public int strengthOfRock = 2;
	public int strengthOfStructures = -2;
	public int strengthOfStructures_reenforced = 2;
	public int strengthOfStructures_reenforced_2 = 6;

	// ground cover and exposure
	public GameObject groundCover;
	private Vector3 groundCoverStartingPos;
	public int highestBuildingLevel = 0;
	public int lowestMinePosition = 3;
	public int numLevelsRevealedByMines = 3;

	// action descriptions
	private string decisionTextReenforce;
	private string decisionTextReenforce2;
	private string decisionTextUnReenforce;
	private string decisionTextDestroy;
	private string decisionTextCancel;
	private string decisionTextMining;
	private string decisionTextFactory;
	private string decisionTextMill;
	private string decisionTextResidence;
	private string decisionTextCommunity;
	private string decisionTextRevert;
	private string decisionTextMountGem;
	private string decisionTextSonar;

	// quantities
	public int countMining = 0;
	public int countFactory = 0;
	public int countMill = 0;
	public int countResidence = 0;
	public int countCommunity = 0;
	public int countGemMount = 0;
	public int inventoryDirt;

	// Gem stuff
	public int inventoryGems = 0;
	public int numGems = 3;				// WARNING: this currently needs to be set manually to match placed gems! This should eventually plug into the random level generation.
	public int gemHeightRequirement = 6;
	public int gemMinimumDepth = 10;
	private List<tile> gemsInGround = new List<tile>();
	private List<tile> sonarList = new List<tile>();

	// misc
	public int buildingMaxPressure = -6;
	public int ResidencesPerCommunity = 5;
	public int MinesPerResidence = 1;
	private tile tileToConvert;
	
	// decisions
	private bool makingDecision = false;
	private List<string> decisionPossibilities;
	private bool isDisplayingMessage = false;
	private string messageToDisplay;
	public int contextButtonsPerRow = 3;

	// HUDs and menus
	public GUIStyle status_HUD;
	public GUIStyle status_Expanded;
	private bool isStatusExpanded = false;
	private bool isMenuOpen = false;			// used as a high level "is anything open?" to tell if you can click and build/mine
	
	// Use this for initialization
	void Start () {
		allTiles.AddRange(GetComponentsInChildren<tile>());
		foreach (tile t in allTiles)
		{
			FindTileNeighbors(t);		// set all tiles' neighbors
			if (t.thisTileType == tile.tileType.rockWithGem)	// find all gems in tiles
			{ gemsInGround.Add(t); }								// and add them to the gem list
		}
		numGems = gemsInGround.Count; // set the number of gems (for winning requirement) based on how many gems found in ground

		groundCoverStartingPos = groundCover.transform.position;
		costToRemodel = costOfStructure + costToDestroyStructure;
		RecalculateCosts();

		// name all context options
		decisionTextReenforce = "Re-enforce Structure\n\nThis structure will hold\nmore weight above it\n\ncost: " + costOfStructure_reenforced;
		decisionTextReenforce2 = "Further Re-enforce\n\nThis structure will hold\neven more weight\n\ncost: " + costOfStructure_reenforced_2;
		decisionTextUnReenforce = "Un-re-enforce structure\n\nRemove all re-enforcement\nfrom this structure\ncost: " + costOfStructure_unReenforce;
		decisionTextDestroy = "Destroy Structure\n\nRemove the structure\nregain some resource\n\ncost: " + costToDestroyStructure;
		decisionTextCancel = "Do Nothing";
		decisionTextMining = "Convert to Mining\n\nAllows deeper mining\n\ncost: " + costOfStructure_mining;
		decisionTextFactory = "Convert to Factory\n\nAllows removal of rocks\n\ncost: " + costOfStructure_factory;
		decisionTextMill = "Convert to Mill\n\nIncreases the amount of resources\ngained from mining\n\ncost: " + costOfStructure_mill;
		decisionTextResidence = "Convert to Residence\n\nResidences are required\nbefore mining structures\ncan be built\n\ncost: " + costOfStructure_residence;
		decisionTextCommunity = "Convert to Community\n\nA place to live in community\n\ncost: " + costOfStructure_community;
		decisionTextRevert = "Remodel Structure\n\nRevert to\nempty structure\n\ncost: " + costToRemodel;
		decisionTextMountGem = "Mount Gem\n\nMount a gem on top\nof a structure at least " + gemHeightRequirement + " blocks high\n\ncost: 1 Gem";
		decisionTextSonar = "Convert to Sonar\n\nDetects Gems\nbelow it\n\ncost: " + costOfStructure_sonar;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) { inventoryDirt += 10; } // debug secret to gain resources with space bar

		if (CheckWinningRequirements()) { GameObject.Find("Help").GetComponent<winningScreen>().SendMessage("YouWin"); }

		if (Input.GetMouseButtonDown(0))
		{
			if (!isMenuOpen)
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit))
				{
					if (hit.collider.tag != "groundCover")
					{
						tile hitTile = hit.collider.GetComponent<tile>();				// get a reference to the tile
						if (hitTile.thisTileType == tile.tileType.dirt || hitTile.thisTileType == tile.tileType.rock || hitTile.thisTileType == tile.tileType.rockWithGem)	// if you click "dirt" or "rock" or "rock with gem"
						{
							if (minableTiles[hitTile.thisTileY])			// if the tile is in the "minableTiles" list (because it has a mine near it)
							{
								if (hitTile.myNeighborUp.thisTileType == tile.tileType.empty		// if any of its neighbors are empty or structural
								    || hitTile.myNeighborDown.thisTileType == tile.tileType.empty
									|| hitTile.myNeighborLeft.thisTileType == tile.tileType.empty
									|| hitTile.myNeighborRight.thisTileType == tile.tileType.empty
								    || hitTile.structureTypes.Contains(hitTile.myNeighborDown.thisTileType)
								    || hitTile.structureTypes.Contains(hitTile.myNeighborLeft.thisTileType)
								    || hitTile.structureTypes.Contains(hitTile.myNeighborRight.thisTileType))
								{
									if (!hitTile.structureTypes.Contains(hitTile.myNeighborUp.thisTileType))	// if the tile above it is not a structure
									{
										if (FindBaseStrength(FindTileAtTop(hitTile), hitTile) >= 0)			// check to make sure nothing above it relies on its strength
										{
											if (hitTile.thisTileType == tile.tileType.dirt)					// if it's dirt
											{
												hitTile.ChangeTileType(tile.tileType.empty);					// update the block's type to "empty"
												inventoryDirt += valueOfDirt_Adjusted;								// add dirt to inventory
											}
											else if (countFactory > 0)													// otherwise, if you have the qualifications to mine rock
											{
												if (inventoryDirt >= costToMineRock_Adjusted)								// and you have enough resources
												{
													if (hitTile.thisTileType == tile.tileType.rock || hitTile.thisTileType == tile.tileType.rockWithGem)	// if it's a rock (or rock with gem)
													{
														if (hitTile.thisTileType == tile.tileType.rockWithGem)	// if it's a rock with a gem
														{
															inventoryGems++;							// add that gem to my gem inventory
															gemsInGround.Remove(hitTile);
															UpdateAllSonar();
														}
														hitTile.ChangeTileType(tile.tileType.empty);		// update the block's type to "empty"
														inventoryDirt -= costToMineRock_Adjusted;					// add dirt to inventory
													}
												}
												else { CannotPerformAction("Cannot mine rock - not enough resources\n(Resources can be gained by mining or destroying other structures)"); }
											}
											else { CannotPerformAction("Cannot mine rock - a Factory is required to mine rock\n(Upgrade a Mine to a Factory)"); }
										}
										else
										{
											CannotPerformAction("Cannot mine here - structures rely on this block\n(re-enforce this or other structures to take some weight off it)");
											ContextOptions(hitTile);
										}
									}
									else
									{
										CannotPerformAction("Cannot mine here - structures rely on this block");
										ContextOptions(hitTile);
									}
								}
								else { CannotPerformAction("Cannot mine here - cannot reach"); }
							}
							else { CannotPerformAction("Cannot mine here - too far from a mine\n(can only mine within " + numLevelsRevealedByMines + " lines of a mine)"); }
						}
						else if (hitTile.thisTileType == tile.tileType.empty)						// if you click an empty block
						{
							if (hitTile.myNeighborDown.thisTileType != tile.tileType.structure_gemMount)		// if it's not on top of a gem mount
							{
								if (inventoryDirt >= costOfStructure)
								{
									if (hitTile.myNeighborDown.thisTileType != tile.tileType.empty && FindBaseStrength(hitTile) >= 0)		// if the block below is solid
									{
										hitTile.ChangeTileType(tile.tileType.structure);	// build a structure
										inventoryDirt -= costOfStructure;
										if (hitTile.thisTileY + 1 > highestBuildingLevel)		// check to see if this is higher than the highest structure so far
										{
											highestBuildingLevel = hitTile.thisTileY + 1;
										}
									}
									else { CannotPerformAction("Cannot build here - insufficient structure\n(structure below may need to be re-enforced)"); }
								}
								else { CannotPerformAction("Cannot build - not enough resources\n(Resources can be gained by mining or destroying other structures)"); }
							}
							else { CannotPerformAction("Cannot build here - Gem Mount must be at the top of a structure"); }
						}
						else if (hitTile.structureTypes.Contains(hitTile.thisTileType))					// if you click a structure
						{
							ContextOptions(hitTile);
						}
					}
				}
			}
		}
		// Set ground exposure level
		Vector3 _groundPos = SetGroundLevel(lowestMinePosition - numLevelsRevealedByMines);
		groundCover.transform.position = Vector3.Lerp(groundCover.transform.position, _groundPos, 0.3f);
	}

	// Show contextual options for clicked tile
	void ContextOptions(tile _tile)
	{
		decisionPossibilities = new List<string>();

		// add options available for all (or most) structure types
		decisionPossibilities.Add (decisionTextCancel);
		decisionPossibilities.Add (decisionTextDestroy);
		decisionPossibilities.Add (decisionTextRevert);

		// add re-enforcement options for all structure types
		if (_tile.levelOfReenforcement == 0)
		{
			decisionPossibilities.Add (decisionTextReenforce);
		}
		else if (_tile.levelOfReenforcement == 1)
		{
			decisionPossibilities.Add (decisionTextUnReenforce);
			decisionPossibilities.Add (decisionTextReenforce2);
		}
		else if (_tile.levelOfReenforcement == 2)
		{
			decisionPossibilities.Add (decisionTextUnReenforce);
		}

		// add options to specific structure types
		if (_tile.thisTileType == tile.tileType.structure)					// if this is an empty structure
		{
			decisionPossibilities.Add (decisionTextResidence);					// to add Residence
			decisionPossibilities.Add (decisionTextMining);						// to add Mining
			if (inventoryGems > 0) { decisionPossibilities.Add (decisionTextMountGem); }			// to add a Gem Mount (if you have any gems)
			decisionPossibilities.Remove (decisionTextRevert);					// remove "revert" for this one (since it wouldn't do anything)
		}

		else if (_tile.thisTileType == tile.tileType.structure_residence)	// if this is a residence
		{
			decisionPossibilities.Add (decisionTextCommunity);					// convert to community center
		}
		else if (_tile.thisTileType == tile.tileType.structure_mining)		// if this is a mining structure
		{
			decisionPossibilities.Add (decisionTextFactory);
			decisionPossibilities.Add (decisionTextMill);
		}
		else if (_tile.thisTileType == tile.tileType.structure_mill)		// if this is a mill
		{
		}
		else if (_tile.thisTileType == tile.tileType.structure_factory)		// if this is a factory
		{
			decisionPossibilities.Add(decisionTextSonar);
		}
		else if (_tile.thisTileType == tile.tileType.structure_community)	// if this is a community
		{
		}
		else if (_tile.thisTileType == tile.tileType.structure_gemMount)	// if this is a gem mount
		{
		}
		else if (_tile.thisTileType == tile.tileType.structure_sonar || _tile.thisTileType == tile.tileType.structure_sonar_positive)	// if this is a sonar
		{
		}
		else if (_tile.thisTileType == tile.tileType.dirt)					// if this is dirt
		{
			decisionPossibilities.Remove (decisionTextDestroy);
			decisionPossibilities.Remove (decisionTextRevert);
		}
		else if (_tile.thisTileType == tile.tileType.rock)					// if this is rock
		{
			decisionPossibilities.Remove (decisionTextDestroy);
			decisionPossibilities.Remove (decisionTextRevert);
		}

		// Re-arrange options (make sure cancel is last, at least)
		if (decisionPossibilities.Contains(decisionTextCancel))
		{
			decisionPossibilities.Remove(decisionTextCancel);
			decisionPossibilities.Add(decisionTextCancel);
		}

		makingDecision = true;
		isMenuOpen = true;
		tileToConvert = _tile;
	}

	// Convert structure based on context option chosen
	void ContextOptionChoice(tile _tile, string choice)
	{
		if (choice == decisionTextCancel) { return; }				// If you chose cancel, then don't do anything
		else
		{

			// check if action is possible (unless you're just re-enforcing or un-re-enforcing)
			if (choice == decisionTextReenforce || choice == decisionTextReenforce2 || choice == decisionTextUnReenforce) { }
			else if (tileToConvert.thisTileType == tile.tileType.structure_residence)
			{
				if (!VerifyMeetPrereqs(0,0,0,-1,0) && choice != decisionTextCommunity) { CannotPerformAction("Cannot remove residence - there won't be enough for the workplaces\n(remove mines, or build another residence somewhere else)"); return; }
			}
			else if (tileToConvert.thisTileType == tile.tileType.structure_community)
			{
				if (!VerifyMeetPrereqs(0,0,0,0,-1)) { CannotPerformAction("Cannot remove community - there won't be enough for the residences\n(you need a community to sustain every " + ResidencesPerCommunity + " residences)"); return; }
			}
			else if (choice == decisionTextMining)
			{
				if (!VerifyMeetPrereqs(1,0,0,0,0)) { CannotPerformAction("Cannot build mine - not enough residences\n(1 residence sustains " + MinesPerResidence + " mine(s).)"); return; }
			}
			else if (choice == decisionTextResidence)
			{
				if (!VerifyMeetPrereqs(0,0,0,1,0)) { CannotPerformAction("Cannot build residence - not enough communities\n(you need a community to sustain every " + ResidencesPerCommunity + " residences)"); return; }
			}

			// Check costs and other requirements and perform type change
			if (choice == decisionTextDestroy)
			{
				if (!_tile.structureTypes.Contains(_tile.myNeighborUp.thisTileType))		// if the block above me is not a structure
				{
					if (FindBaseStrength(FindTileAtTop(_tile), _tile) >= 0)					// make sure it's not structurally necessary before destroying it
					{
						RemoveBenefitsOfOldType(_tile);
						_tile.ChangeTileType(tile.tileType.empty);			// set to empty
						inventoryDirt -= costToDestroyStructure;							// pay resources
					}
					else { CannotPerformAction("Cannot destroy structure - structures rely on this block"); }
				}
				else { CannotPerformAction("Cannot destroy structure - structures rely on this block"); }
			}
			else if (choice == decisionTextRevert)											// revert to empty
			{
				RemoveBenefitsOfOldType(_tile);
				_tile.ChangeTileType (tile.tileType.structure);			// set to empty
				inventoryDirt -= costToRemodel;											// pay resources
			}
			else if (choice == decisionTextReenforce && inventoryDirt - costOfStructure_reenforced >= 0)					// re-enforce structure 1
			{
				_tile.prop_levelOfReenforcement++;
				inventoryDirt -= costOfStructure_reenforced;
			}
			else if (choice == decisionTextReenforce2 && inventoryDirt - costOfStructure_reenforced_2 >= 0)					// re-enforce structure 2
			{
				_tile.prop_levelOfReenforcement++;
				inventoryDirt -= costOfStructure_reenforced_2;
			}
			else if (choice == decisionTextUnReenforce)					// un-re-enforce structure
			{
				// find the strength it will be set back to
				int _tempStrength = 0;
				if (tileToConvert.structureTypes.Contains(tileToConvert.thisTileType)) { _tempStrength = strengthOfStructures; }
				else if (tileToConvert.thisTileType == tile.tileType.dirt) { _tempStrength = strengthOfDirt; }
				else if (tileToConvert.thisTileType == tile.tileType.rock || tileToConvert.thisTileType == tile.tileType.rockWithGem) { _tempStrength = strengthOfRock; }

				if (FindBaseStrength(FindTileAtTop(_tile), _tile, _tempStrength) >= 0			// make sure the structure doesn't rely on it
				         || tileToConvert.myNeighborUp.thisTileType == tile.tileType.empty)		// or if it's the top structure
				{
					_tile.prop_levelOfReenforcement = 0;
					inventoryDirt += costOfStructure_unReenforce;
				}
				else { CannotPerformAction("Cannot un-reenforce structure - structures rely on this block's re-enforcement"); } 
			}
			else if (choice == decisionTextResidence && inventoryDirt - costOfStructure_residence >= 0)						// change to residence
			{ RemoveBenefitsOfOldType(_tile); _tile.ChangeTileType (tile.tileType.structure_residence); inventoryDirt -= costOfStructure_residence; countResidence++; }
			else if (choice == decisionTextMining && inventoryDirt - costOfStructure_mining >= 0)							// change to mining
			{
				RemoveBenefitsOfOldType(_tile);
				_tile.ChangeTileType (tile.tileType.structure_mining);
				inventoryDirt -= costOfStructure_mining;
				countMining++;
				mineTiles.Add(_tile);
				UpdateMinableTilesList();
			}
			else if (choice == decisionTextFactory && inventoryDirt - costOfStructure_factory >= 0)							// change to factory
			{ RemoveBenefitsOfOldType(_tile); _tile.ChangeTileType (tile.tileType.structure_factory); inventoryDirt -= costOfStructure_factory; countFactory++; RecalculateCosts(); }
			else if (choice == decisionTextMill && inventoryDirt - costOfStructure_mill >= 0)								// change to mill
			{ RemoveBenefitsOfOldType(_tile); _tile.ChangeTileType (tile.tileType.structure_mill); inventoryDirt -= costOfStructure_mill; countMill++; RecalculateCosts(); }
			else if (choice == decisionTextCommunity && inventoryDirt - costOfStructure_community >= 0)						// change to community center
			{ RemoveBenefitsOfOldType(_tile); _tile.ChangeTileType (tile.tileType.structure_community); inventoryDirt -= costOfStructure_community; countCommunity++; }
			else if (choice == decisionTextSonar && inventoryDirt - costOfStructure_sonar >= 0)								// change to sonar
			{
				RemoveBenefitsOfOldType(_tile);
				inventoryDirt -= costOfStructure_sonar;
				sonarList.Add(_tile);														// Add to sonar list
				UpdateAllSonar();
				RecalculateCosts();
			}
			else if (choice == decisionTextMountGem)																		// change to gem mount
			{
				if (inventoryGems > 0)												// make sure you have a gem
				{
					if (_tile.myNeighborUp.thisTileType == tile.tileType.empty)		// make sure there's no structure above
					{
						if (_tile.thisTileY >= gemHeightRequirement)				// make sure the gem is being mounted high enough
						{
							RemoveBenefitsOfOldType(_tile);
							_tile.ChangeTileType (tile.tileType.structure_gemMount);
							inventoryGems--;
							countGemMount++;
							inventoryDirt += prizeDirtForMountingGem;
						}
						else { CannotPerformAction("Cannot mount Gem here - Gem must be mounted at least " + gemHeightRequirement + " above ground level"); }
					}
					else { CannotPerformAction("Cannot mount Gem here - Gem must be mounted on the top of a structure"); }
				}
				else { CannotPerformAction("Cannot mount Gem - You don't have any Gems"); }
			}
			else { CannotPerformAction("Not enough resources\n(Resources can be gained by mining or destroying other structures)"); }
		}
	}

	// recalculate all costs (based on number of factories and mills)
	void RecalculateCosts()
	{
		// Factories and rock
		costToMineRock_Adjusted = costToMineRock - (PriceChangeOfRockPerFactory * countFactory);
		if (costToMineRock_Adjusted < 1) { costToMineRock_Adjusted = 1; }
		
		// Mills and dirt
		valueOfDirt_Adjusted = valueOfDirt + (PriceChangeOfDirtPerMill * countMill);
	}

	// Update the sonar status if gems change
	void UpdateAllSonar()
	{
		foreach (tile _sonar in sonarList)					// for each sonar
		{
			bool isPositive = false;
			foreach (tile _gem in gemsInGround)					// check every gem
			{
				if (_gem.thisTileX == _sonar.thisTileX && _gem.thisTileY < _sonar.thisTileY)	// if even one gem is within its range
				{
					isPositive = true;													// set bool to true
					break;																// and break out early (stop checking any more gems)
				}
			}
			if (isPositive) { _sonar.ChangeTileType (tile.tileType.structure_sonar_positive); }
			else { _sonar.ChangeTileType (tile.tileType.structure_sonar); }
		}
	}
	
	bool CheckWinningRequirements()
	{
		if (countGemMount >= numGems) { return true; }
		else { return false; }
	}
	
	void RemoveBenefitsOfOldType(tile _tile)
	{
		if (tileToConvert.thisTileType == tile.tileType.structure_mining)
		{
			countMining -= 1;
			mineTiles.Remove(_tile);
			UpdateMinableTilesList();
		}
		else if (tileToConvert.thisTileType == tile.tileType.structure_factory) { countFactory -= 1; RecalculateCosts(); }
		else if (tileToConvert.thisTileType == tile.tileType.structure_mill) { countMill -= 1; RecalculateCosts(); }
		else if (tileToConvert.thisTileType == tile.tileType.structure_residence) { countResidence -= 1; }
		else if (tileToConvert.thisTileType == tile.tileType.structure_community) { countCommunity -= 1; }
		else if (tileToConvert.thisTileType == tile.tileType.structure_gemMount) { inventoryGems -= 1; }
		else if (tileToConvert.thisTileType == tile.tileType.structure_sonar || tileToConvert.thisTileType == tile.tileType.structure_sonar_positive) { sonarList.Remove(_tile); }
	}

	// look above and find the top of the current stack of blocks (the first non-empty block)
	tile FindTileAtTop(tile _tile)
	{
		tile _result = _tile.myNeighborUp;
		while (_result.myNeighborUp.thisTileType != tile.tileType.empty)
		{
			_result = _result.myNeighborUp;
		}
		return _result;
	}

	// look below the structure and find the strength of the supporting tiles
	int FindBaseStrength(tile _tile, tile _tileToRemove, int _valueToReplace)
	{
		int _result = 0;
		tile _tileInQuestion = _tile.myNeighborDown;					// Start looking down at blocks
		while (_tileInQuestion.thisTileType != tile.tileType.empty)		// Until you hit an empty block
		{
			if (_tileInQuestion == _tileToRemove && _valueToReplace == -99) { break; }		// If you find the optional "tileToRemove", then stop there and consider that you've reached the bottom
			else if (_tileInQuestion == _tileToRemove) { _result += _valueToReplace; }		// If you find the optional "tileToRemove" and you have a "valueToReplace", use that value in its place
			else if (_tileInQuestion.thisTileType == tile.tileType.dirt)		// If it's dirt, add the strength of dirt
			{
				_result += strengthOfDirt;
			}
			else if (_tileInQuestion.thisTileType == tile.tileType.rock)	// If it's rock, add the strength of rock
			{
				_result += strengthOfRock;
			}
			else if (_tileInQuestion.structureTypesNonLoadBearing.Contains(_tileInQuestion.thisTileType))	// If it's a non-load bearing structure, add that strength
			{
				_result += strengthOfStructures;
			}
			else if (_tileInQuestion.thisTileType == tile.tileType.bedrock)						// If you hit "bedrock", then you're good for sure, just return positive
			{
				return 9999;
			}

			if (_tileInQuestion == _tileToRemove)		// if this is the structure being removed/replaced, don't add in its re-enforcement
			{
			}
			else if (_tileInQuestion.levelOfReenforcement == 1 )			// if the structure is re-enforced, add that strength in
			{
				_result += strengthOfStructures_reenforced;
			}
			else if (_tileInQuestion.levelOfReenforcement == 2)		// if the structure is double re-enforced, add that strength in
			{
				_result += strengthOfStructures_reenforced_2;
			}

			if (_tileInQuestion.myNeighborDown != null)						// As long as there's a block below, move on to the next one to repeat the loop
			{
				_tileInQuestion = _tileInQuestion.myNeighborDown;
			}
			else { break; }
			if (_result < buildingMaxPressure) { return -1; }	// If at any point you reach more than the "buildingMaxPressure", fail
		}
		return _result;
	}

	// Default implementation
	int FindBaseStrength(tile _tile)
	{
		return FindBaseStrength(_tile, null, -99);	// "-99" is the signal for "null" in this case
	}

	// Default implementation
	int FindBaseStrength(tile _tile, tile _tileToRemove)
	{
		return FindBaseStrength(_tile, _tileToRemove, -99);	// "-99" is the signal for "null" in this case
	}

	void UpdateMinableTilesList()
	{
		minableTiles = new Dictionary<int, bool>();
		lowestMinePosition = 3;								// reset to above ground level
		foreach (tile _tile in mineTiles)													// for each mine
		{
			for (int i = -numLevelsRevealedByMines; i <= numLevelsRevealedByMines; i++)			// go through the tiles 3 above and 3 below
			{
				minableTiles[_tile.thisTileY + i] = true;											// and set them to "true"
			}
			if (_tile.thisTileY < lowestMinePosition)				// if this is the lowest mine position
			{ lowestMinePosition = _tile.thisTileY; }					// set it as the new "lowestMinePosition"
		}
		for (int i = 0; i >= lowestMinePosition; i--)							// for all other levels
		{
			if (!minableTiles.ContainsKey(i))										// that aren't in the dictionary
			{
				minableTiles[i] = false;												// create them in the dictionary and set them to false
			}
		}
	}
	
	// Verify that all prereqs for building types if anything is changed
	bool VerifyMeetPrereqs(int _changeMining, int _changeFactory, int _changeMill, int _changeResidence, int _changeCommunity)
	{
		if (((countResidence + _changeResidence + countCommunity + _changeCommunity) * MinesPerResidence) < (countMining + _changeMining + countFactory + _changeFactory + countMill + _changeMill)) { return false; }		// there aren't enough residences per workplaces if you remove one
		if ((countResidence + _changeResidence) / ResidencesPerCommunity > countCommunity + _changeCommunity) { return false; }			// there aren't enough communities to build another residence
		return true;
	}
	
	// converts from a desired number of exposed tiles, to the position for the ground cover
	Vector3 SetGroundLevel(int numTilesExposed)
	{
		Vector3 _newPos = new Vector3(groundCover.transform.position.x, numTilesExposed + (int)groundCoverStartingPos.y -0.5f, groundCover.transform.position.z);
		return _newPos;
	}

	// set neighbors for a tile in all directions
	void FindTileNeighbors(tile t)
	{
		foreach (tile t2 in allTiles)
		{
			if (t.thisTileY + 1 == t2.thisTileY && t.thisTileX == t2.thisTileX) { t.myNeighborUp = t2; }
			else if (t.thisTileY - 1 == t2.thisTileY && t.thisTileX == t2.thisTileX) { t.myNeighborDown = t2; }
			else if (t.thisTileX + 1 == t2.thisTileX && t.thisTileY == t2.thisTileY) { t.myNeighborRight = t2; }
			else if (t.thisTileX - 1 == t2.thisTileX && t.thisTileY == t2.thisTileY) { t.myNeighborLeft = t2; }
		}
	}

	void TurnOffMessage()
	{
		isDisplayingMessage = false;
	}

	// Fail with error message
	void CannotPerformAction(string errorMessage)
	{
		isDisplayingMessage = true;
		messageToDisplay = errorMessage;
		CancelInvoke("TurnOffMessage");
		Invoke("TurnOffMessage", 10);
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(0, Screen.height - 30, 140, 35), "resources: " + inventoryDirt.ToString(), status_HUD))
		{
			if (!isStatusExpanded) { isStatusExpanded = true; isMenuOpen = true; }
		}
		if (isStatusExpanded)
		{
			if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "resources:\n" + inventoryDirt.ToString() +
			        "\n\nres to work\n" + (countResidence + countCommunity) + ":" + (countMining + countMill + countFactory) +
			        "\n\ncom to res:\n" + countCommunity + ":" + countResidence +
			        "\n\nnum gems:\n" + numGems + 
			        "\n\nfound gems:\n" + inventoryGems +
			        "\n\ngem height:\n" + gemHeightRequirement, status_Expanded))
			{
				isStatusExpanded = false;
				isMenuOpen = false;
			}
		}
		// Context Buttons
		if (makingDecision)
		{
			int buttonPos;
			int positionOnRow;

			int numDecisionsWithoutCancel = decisionPossibilities.Count - 1;
			int numRows = (numDecisionsWithoutCancel / contextButtonsPerRow);			// find number of rows needed
			if (numDecisionsWithoutCancel % contextButtonsPerRow > 0) { numRows++; }	// add a row for the remainder if there's one

			for (int i = 0; i < decisionPossibilities.Count; i++) {
				int currentRow = i / contextButtonsPerRow;					// which row will the current button be on
				int rowSpacing = 140;
				int buttonWidth = 150;

				if (numDecisionsWithoutCancel == 1) { buttonPos = Screen.width/2; }
				else
				{
					positionOnRow = i % contextButtonsPerRow;

					buttonPos = (int)((((Screen.width)/(contextButtonsPerRow - 1) * positionOnRow) - Screen.width/2)		// space out buttons, determine button order
					* (1.0f / contextButtonsPerRow - 1))							// determine spacing based on number of options
						+ Screen.width/2;
				}

				if (i == numDecisionsWithoutCancel)
				{
					buttonPos = Screen.width / 2;
					currentRow = numDecisionsWithoutCancel / contextButtonsPerRow + 1;
				}


				if (GUI.Button(new Rect(buttonPos - (buttonWidth/2), (currentRow * rowSpacing) + 100, buttonWidth, 120), decisionPossibilities[i]))
				{
					ContextOptionChoice(tileToConvert, decisionPossibilities[i]);
					makingDecision = false;
					isMenuOpen = false;
				}
			}
		}
		int _buttonWidth = (int)(Screen.width * 0.8f);
		int _buttonHeight = 40;
		if (isDisplayingMessage) { GUI.Box(new Rect(Screen.width / 2 - (_buttonWidth / 2), Screen.height - (_buttonHeight * 2), _buttonWidth, _buttonHeight), messageToDisplay); }
	}
}
