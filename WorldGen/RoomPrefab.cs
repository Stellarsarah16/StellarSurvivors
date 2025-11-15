namespace StellarSurvivors.WorldGen;

using System.Drawing;

public class RoomPrefab
{
    public string ID { get; set; }
    public int[,] TileData { get; set; }
    
    
    /// </summary>
    public Point Entrance { get; set; }

    // Helper properties to get width/height from the array
    public int Width => TileData.GetLength(0);
    public int Height => TileData.GetLength(1);
}