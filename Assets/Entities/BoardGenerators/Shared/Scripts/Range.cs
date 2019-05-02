using UnityEngine;

[System.Serializable]
public class Range
{
    public int Min;
    public int Max;

    public int GetRandomRange()
    {
        return Random.Range(Min, Max);
    }
}