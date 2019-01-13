using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBot
{
    System.Tuple<Position, Position> Move(List<List<ICell>> cells);
}
