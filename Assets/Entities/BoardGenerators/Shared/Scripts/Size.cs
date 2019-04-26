using UnityEngine;

[System.Serializable]
public class Size
{
    public int Min { get; set; }
    public int Max { get; set; }

    public int GetRandomSize()
    {
        return Random.Range(Min, Max);
    }
}