using DormGrapple;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedBot : IBot
{
    public System.Tuple<Position, Position> Move(List<List<ICell>> cells)
    {
        var moves = Analytics.AllMoves(cells);

        List<Dictionary<Owner, double>> dictList = new List<Dictionary<Owner, double>>();
        foreach (var move in moves)
        {
            var damageDictionary = new Dictionary<Owner, double>();
            var LocalField = new List<List<ICell>>();

            for (int i = 0; i < cells.Count; i++)
            {
                LocalField.Add(new List<ICell>(cells.Count));
                for (int j = 0; j < cells.Count; j++)
                {
                    LocalField[i].Add(cells[i][j]);
                }
            }

            ICell tempCell = LocalField[move.combination[0].Row][move.combination[0].Column];
            LocalField[move.combination[0].Row][move.combination[0].Column] =
                LocalField[move.combination[1].Row][move.combination[1].Column];
            LocalField[move.combination[1].Row][move.combination[1].Column] = tempCell;

            do
            {
                var dictionary = Analytics.Remove(LocalField, Owner.Player);
                foreach (var pair in dictionary)
                {
                    if (!damageDictionary.ContainsKey(pair.Key))
                        damageDictionary[pair.Key] = 0;
                    damageDictionary[pair.Key] += pair.Value;
                }
            }
            while (Analytics.HasCombinations(LocalField));

            dictList.Add(damageDictionary);
        }

        double maxValue = 0.0;
        int maxPositon = 0;
        foreach (var pair in dictList[0])
        {
            if (pair.Key == Owner.Player)
                maxValue += pair.Value;
            else
                maxValue -= pair.Value;
        }

        for (int i = 0; i < dictList.Count; i++)
        {
            double localValue = 0.0;
            foreach (var pair in dictList[i])
            {
                if (pair.Key != Owner.Player)
                    localValue += pair.Value;
                else
                    localValue -= pair.Value;
            }

            if (localValue > maxValue)
            {
                maxPositon = i;
                maxValue = localValue;
            }
        }

        return new System.Tuple<Position, Position>(moves[maxPositon].combination[0], moves[maxPositon].combination[1]);
    }
}
