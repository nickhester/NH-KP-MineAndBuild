using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileGrid : MonoBehaviour {
	
	public GameObject dirtTile;
	public Vector2 m_tileSize = new Vector2(1, 1);
	public int m_earthRows;
	public int m_skyRows;
	public int m_columns;
	public AnimationCurve rockSpawnChance;
	
	private ITile[] tiles;
	private int m_totalRows;
	
	// Use this for initialization
	void Start () {
		tiles = new ITile[(m_earthRows + m_skyRows) * m_columns];
		m_totalRows = m_skyRows + m_earthRows;
		createTiles();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void createTiles()
	{
		// build sky tiles
		for (int yPos = m_skyRows; yPos > 0; --yPos) {
			for (int xPos = 0; xPos < m_columns; ++xPos) {
				GameObject tilePrefab;
				ITile newTile;
				
				tilePrefab = Instantiate(Resources.Load("tiles/SkyTile")) as GameObject;
				newTile = tilePrefab.GetComponent<SkyTile>();
				
				tilePrefab.transform.parent = this.transform;
				
				newTile = tilePrefab.GetComponent<SkyTile>();
				
				this[xPos, yPos] = newTile;
			}
		}
	
		// build earth tiles
		for (int yPos = 0; yPos > -m_earthRows; --yPos) {
			float stoneChance = rockSpawnChance.Evaluate(-yPos / (float)m_earthRows);
			print(-yPos / (float)m_earthRows);
			print(stoneChance);
			for (int xPos = 0; xPos < m_columns; ++xPos) {
			
				GameObject tilePrefab;
				ITile newTile;
				
				if (Random.Range(0.0f, 1.0f) < stoneChance)
				{
					tilePrefab = Instantiate(Resources.Load("tiles/StoneTile")) as GameObject;
					newTile = tilePrefab.GetComponent<StoneTile>();
				}
				else
				{
					tilePrefab = Instantiate(Resources.Load("tiles/DirtTile")) as GameObject;
					newTile = tilePrefab.GetComponent<DirtTile>();
				}
				
				tilePrefab.transform.parent = this.transform;
				
				this[xPos, yPos] = newTile;
			}
		}
		
		// initialize tiles
		for (int yPos = m_skyRows; yPos > -m_earthRows; --yPos) {
			for (int xPos = 0; xPos < m_columns; ++xPos) {
				this[xPos, yPos].init(this, new PointInt(xPos, yPos));
			}
		}
	}
	
	public ITile this[int x, int y]
	{
		get {
			y = m_skyRows - y;
			if (x < 0 || x >= m_columns || y < 0 || y >= m_totalRows)
				return null;
			return tiles[y * m_columns + x];
		}
		set {
			y = m_skyRows - y;
			tiles[y * m_columns + x] = value;
		}
	}
	
	public ITile this[PointInt pos]
	{
		get {
			return tiles[pos.y * m_columns + pos.x];
		}
		set {
			tiles[pos.y * m_columns + pos.x] = value;
		}
	}
	
}
