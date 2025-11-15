namespace StellarSurvivors.WorldGen.Steps;

using System.Drawing;
using StellarSurvivors.Core;
using StellarSurvivors.Enums;

public class VaultGenerationStep : IGenerationStep
{
    private readonly RoomLibrary _roomLibrary;
    private readonly Random _random;
    private readonly int _vaultsToPlace;

    public VaultGenerationStep(Random random, RoomLibrary roomLibrary, int vaultCount = 3)
    {
        _random = random;
        _roomLibrary = roomLibrary;
        _vaultsToPlace = vaultCount;
    }

    public void Process(WorldData worldData, int seed)
    {
        // 1. Get a room to place (we just use the test one for now)

        for (int i = 0; i < _vaultsToPlace; i++)
        {
            RoomPrefab prefab = _roomLibrary.GetRandomPrefab(_random);
            // 2. Find a valid (all solid stone) place to put it
            Point? spawnPoint = FindSolidSpot(worldData, prefab.Width, prefab.Height);

            if (spawnPoint == null)
            {
                // Failed to find a spot, just stop
                Console.WriteLine("Warning: Could not find solid spot for vault.");
                continue;
            }

            int spawnX = spawnPoint.Value.X;
            int spawnY = spawnPoint.Value.Y;
            Console.WriteLine(prefab.ID);
            // 3. "Stamp" the room onto the main TileMap
            for (int x = 0; x < prefab.Width; x++)
            {
                for (int y = 0; y < prefab.Height; y++)
                {
                    int tileID = prefab.TileData[x, y];
                    worldData.TileMap[spawnX + x, spawnY + y] = tileID;
                }
            }
            
            // 4. Calculate the WORLD coordinate of the entrance
            Point entrancePos = new Point(
                spawnX + prefab.Entrance.X, 
                spawnY + prefab.Entrance.Y
            );

            // 5. Find the closest cave (TileIDs.TILE_AIR)
            List<Point> path = FindClosestCave(worldData, entrancePos);

            // 6. Carve the path
            if (path != null)
            {
                CarvePath(worldData, path);
            }
        }
    }

    private Point? FindSolidSpot(WorldData worldData, int roomWidth, int roomHeight)
    {
        // Try 1000 times to find a random spot
        for (int i = 0; i < 1000; i++)
        {
            int checkX = _random.Next(1, worldData.Width - roomWidth - 1);
            int checkY = _random.Next(1, worldData.Height - roomHeight - 1);

            if (IsAreaSolid(worldData, checkX, checkY, roomWidth, roomHeight))
            {
                return new Point(checkX, checkY);
            }
        }
        return null; // Failed to find a spot
    }

    private bool IsAreaSolid(WorldData worldData, int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                // --- THIS IS THE FIX ---
                // Get the type using the helper method
                TileType type = worldData.GetTileType(x, y);

                // If any tile in the area is NOT stone, this spot is invalid
                if (type != TileType.Stone) 
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    private List<Point> FindClosestCave(WorldData worldData, Point start)
    {
        // --- THIS IS THE MISSING LINE ---
        // You must create the queue before you can use it.
        Queue<Point> frontier = new Queue<Point>();
        // ---
    
        frontier.Enqueue(start);
    
        // This dictionary is magic: it stores "where did I come from?"
        // e.g., CameTo[neighbor] = current
        Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
        cameFrom[start] = Point.Empty; 
    
        // Define neighbors (up, down, left, right)
        Point[] neighbors = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };

        while (frontier.Count > 0)
        {
            Point current = frontier.Dequeue();

            // 1. --- GOAL FOUND! ---
            // Is this tile an existing cave (air)?
            if (worldData.GetTileType(current.X, current.Y) == TileType.None)
            {
                // 2. Reconstruct the path (this logic is unchanged)
                List<Point> path = new List<Point>();
                Point pathStep = current;
                while (pathStep != start)
                {
                    path.Add(pathStep);
                    pathStep = cameFrom[pathStep];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            // 3. Explore neighbors
            foreach (var neighborDir in neighbors)
            {
                Point next = new Point(current.X + neighborDir.X, current.Y + neighborDir.Y);

                // Check bounds and if we've already visited
                if (next.X > 0 && next.X < worldData.Width &&
                    next.Y > 0 && next.Y < worldData.Height &&
                    !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }
    
        return null; // No path found
    }

    /// <summary>
    /// Digs a 1-tile wide tunnel along the given path.
    /// </summary>
    private void CarvePath(WorldData worldData, List<Point> path)
    {
        foreach (Point tile in path)
        {
            // --- THIS IS THE FIX ---
            // Use the safe SetTileType method to carve air
            worldData.SetTileType(tile.X, tile.Y, TileType.None);
        }
    }
}