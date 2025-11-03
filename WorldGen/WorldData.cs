
public enum TileType
{
    None,   // Or Empty
    Grass,
    Dirt,
    Water,
    Stone,
    Wood,   
    Iron,
    Gold
}

public class WorldData
{
    public int[,] TileMap { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    
    // NEW: A 1D array to store the surface Y-coordinate for each column
    public int[] SurfaceHeightMap { get; set; }

    public WorldData(int width, int height)
    {
        Width = width;
        Height = height;
        TileMap = new int[width, height];
        SurfaceHeightMap = new int[width]; 
    }

    public TileType GetTileType(int x, int y)
    {
        // Safety check to prevent crashing if we ask for a tile off-map
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return TileType.None; // Return "empty" for out-of-bounds
        }

        // Read the int from the array and cast it to our enum
        return (TileType)TileMap[x, y];
    }
    public void SetTileType(int x, int y, TileType newType)
    {
        // Safety check
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return; // Do nothing if out-of-bounds
        }

        // Cast the enum to an int and store it in the array
        TileMap[x, y] = (int)newType;
    }
    
}