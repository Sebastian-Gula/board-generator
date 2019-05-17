using System;
using UnityEngine;

public class MatrixNode
{
    private double h = -1;
    private double g = 0;

    public double FunctionF
    {
        get
        {
            return FunctionG + FunctionH;
        }
    }

    public double FunctionG
    {
        get
        {
            if (Parent != null)
            {
                g = Parent.FunctionG;
                g++;
            }

            return g;
        }
    }

    public double FunctionH
    {
        get
        {
            if (h == -1)
            {
                h = Math.Sqrt((Destination - Position).sqrMagnitude);
            }

            return h;
        }
    }

    public Vector2 Position;
    public Vector2 Destination;
    public MatrixNode Parent;

    public MatrixNode(Vector2 position, Vector2 destination)
    {
        Position = position;
        Destination = destination;
    }

    public MatrixNode(Vector2 position, Vector2 destination, MatrixNode parent)
    {
        Position = position;
        Destination = destination;
        Parent = parent;
    }
}