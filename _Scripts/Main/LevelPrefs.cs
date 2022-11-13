using System.Collections.Generic;

[System.Serializable]
public class LevelPrefs
{
    public List<PiecePrefs> piecesPrefs;
}

[System.Serializable]
public class PiecePrefs
{
    public int id;
    public string boardPosition;
}