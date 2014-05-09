using UnityEngine;
using System.Collections;

public interface ITile
{
	bool canHoldWeight(int weight);
	void init(TileGrid grid, PointInt pos);
}
