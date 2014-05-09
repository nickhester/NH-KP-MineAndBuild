using UnityEngine;
using System.Collections;

public abstract class BaseTile : MonoBehaviour {

	public int m_weight;
	public int m_strength;
	public bool m_strengthDecreasesWeight;

	private PointInt m_position;
	private TileGrid m_grid;
	
	// GETTERS & SETTERS #################################################
	
	public ITile tileUp { get; protected set; }
	public ITile tileDown { get; protected set; }
	public ITile tileLeft { get; protected set; }
	public ITile tileRight { get; protected set; }
	
	virtual public void init(TileGrid grid, PointInt pos)
	{
		m_grid = grid;
		m_position = pos;
		
		transform.localPosition = new Vector2(pos.x, pos.y);
		
		tileUp = grid[pos.x, pos.y + 1];
		tileDown = grid[pos.x, pos.y - 1];
		tileLeft = grid[pos.x - 1, pos.y];
		tileRight = grid[pos.x + 1, pos.y];
		
	}
	
	/**
		Recursively checks tiles below to determine if the tile can hold a certain amount of weight.
	*/
	virtual public bool canHoldWeight(int weight)
	{
		if (m_strength < weight)
			return false;
		
		if (m_strengthDecreasesWeight)
			weight -= m_strength;
		
		weight += m_weight;
		
		if (weight <= 0 || tileDown == null)
			return true;
		
		return tileDown.canHoldWeight(weight);
	}
	
}
