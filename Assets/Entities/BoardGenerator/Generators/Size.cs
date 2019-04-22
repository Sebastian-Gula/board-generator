using UnityEngine;

[System.Serializable]
public class Size
{
    public int Min;
    public int Max;

    public int GetRandomSize()
    {
        return Random.Range(Min, Max);
    }
}