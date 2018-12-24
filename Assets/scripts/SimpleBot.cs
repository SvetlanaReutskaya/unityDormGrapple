using DormGrapple;
using System;
using System.Collections;
using System.Collections.Generic;

public class SimpleBot : IBot
{
    private Random rand;

    public SimpleBot()
    {
        rand = new Random();
    }

    public System.Tuple<Position, Position> Move(List<List<ICell>> cells)
    {
        var moves = Analytics.AllMoves(cells);
        moves.RemoveAll(move => cells[move.combination[0].Row][move.combination[0].Column].Owner == Owner.Player);
        if (moves.Count == 0)
            moves = Analytics.AllMoves(cells);
        int randomIndex = rand.Next(0, moves.Count);
        return new System.Tuple<Position, Position>(moves[randomIndex].combination[0], moves[randomIndex].combination[1]);
    }
}
