using UnityEngine;
using System.Collections;

[System.Serializable]
public class PointInt : System.Object {

	public int x;
	public int y;

	public PointInt(int posX, int posY)
	{
		x = posX;
		y = posY;
	}
	
	public PointInt()
	{
		x = 0;
		y = 0;
	}
	
	public override bool Equals (object obj)
	{
		if (obj.GetType() == this.GetType())
		{
			PointInt pointObj = obj as PointInt;
			return pointObj.x == x && pointObj.y == y;
		}
		return false;
	}
	
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
	
	static public PointInt operator +(PointInt lhs, PointInt rhs)
	{
		PointInt newPoint = new PointInt();
		newPoint.x = lhs.x + rhs.x;
		newPoint.y = lhs.y + rhs.y;
		return newPoint;
	}
	static public PointInt operator -(PointInt lhs, PointInt rhs)
	{
		PointInt newPoint = new PointInt();
		newPoint.x = lhs.x - rhs.x;
		newPoint.y = lhs.y - rhs.y;
		return newPoint;
	}
	static public bool operator ==(PointInt lhs, PointInt rhs)
	{
		return (lhs.x == rhs.x && lhs.y == rhs.y);
	}
	static public bool operator !=(PointInt lhs, PointInt rhs)
	{
		return (lhs.x != rhs.x || lhs.y != rhs.y);
	}

}
