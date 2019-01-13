using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combination : IEquatable<Combination>
{
    public List<Position> combination;

    public int Length
    {
        get { return combination.Count; }
    }

    public Combination()
    {
        combination = new List<Position>();
    }

    public Combination(List<Position> newCombination)
    {
        combination = new List<Position>();

        foreach (var position in newCombination)
        {
            combination.Add(new Position(position.Row, position.Column));
        }
    }

    public bool Equals(Combination other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        var lengthComparison = Length.Equals(other.Length);
        if (!lengthComparison) return lengthComparison;
        for (int i = 0; i < Length; i++)
        {
            var elementComparison = combination[i].Equals(other.combination[i]);
            if (!elementComparison) return elementComparison;
        }
        return true;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Combination)obj);
    }

    public override int GetHashCode()
    {
        return (combination != null ? combination.GetHashCode() : 0);
    }

    public override string ToString()
    {
        var result = "";
        foreach (var elem in combination)
        {
            result += "(" + (elem.Row + 1) + ", " + (elem.Column + 1) + ") ";
        }
        return result;
    }
}

public class Position : IEquatable<Position>, IComparable<Position>
{
    public int Column { get; set; }
    public int Row { get; set; }

    public Position()
    {
        Row = 0;
        Column = 0;
    }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public bool Equals(Position other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Row == other.Row && Column == other.Column;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Position)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Row * 397) ^ Column;
        }
    }

    public int CompareTo(Position other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var rowComparison = Row.CompareTo(other.Row);
        if (rowComparison != 0) return rowComparison;
        return Column.CompareTo(other.Column);
    }
}
