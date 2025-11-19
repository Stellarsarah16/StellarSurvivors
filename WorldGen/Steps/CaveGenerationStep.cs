namespace StellarSurvivors.WorldGen.Steps;
using System.Collections.Generic;
using StellarSurvivors.Core;
using StellarSurvivors.WorldGen.TileData;
using static StellarSurvivors.Data.TileIDs;

public class CaveGenerationStep : IGenerationStep
{
    // --- Tweak these values ---
    private const int NumWalkers = 10;          // How many cave systems
    private const int WalkerLifetime = 1000;    // How long each cave is
    private const int CarveRadius = 3;          // 0=1x1, 1=3x3, 2=5x5. (for "corridors")
    private const int BottomBorder = 10;

    private const int SideBorder = 10;
    // --------------------------

    private Random _random;

    // A simple class to keep track of our diggers
    private class Walker
    {
        public int X;
        public int Y;
        public int Lifetime;
        public double DownwardBias; // 0.0 = no bias, 0.5 = strong bias

        public Walker(int x, int y, int lifetime, double bias = 0.0)
        {
            X = x;
            Y = y;
            Lifetime = lifetime;
            DownwardBias = bias;
        }
    }

    public void Process(WorldData worldData, int seed)
    {
        _random = new Random(seed);
        var walkers = new List<Walker>();

        // --- 1. Create the Surface Entrance ---
        // Find a random spot on the surface (not too close to the edge)
        int entranceX = _random.Next(worldData.Width / 4, worldData.Width * 3 / 4);
        int entranceX2 = _random.Next(worldData.Width / 4, worldData.Width * 3 / 4);
        int entranceY = worldData.SurfaceHeightMap[entranceX] + 1; // Start just below the grass
        
        // Add a special walker with a long life and a bias to dig DOWN
        walkers.Add(new Walker(entranceX, entranceY, WalkerLifetime *2, 0.1));
        walkers.Add(new Walker(entranceX2, entranceY, WalkerLifetime *3, 0.3));

        // --- 2. Create the Deep Caves ---
        for (int i = 0; i < NumWalkers; i++)
        {
            // Start walkers deep in the stone layer
            int startX = _random.Next(0, worldData.Width);
            int startY = _random.Next(entranceY + 20, worldData.Height); // Start deep
            walkers.Add(new Walker(startX, startY, WalkerLifetime));
        }

        // --- 3. Run the Simulation ---
        int totalLifetime = WalkerLifetime * NumWalkers + (WalkerLifetime * 2);
        for (int i = 0; i < totalLifetime; i++)
        {
            foreach (var walker in walkers)
            {
                if (walker.Lifetime > 0)
                {
                    MoveWalker(walker, worldData);
                    Carve(walker.X, walker.Y, worldData);
                    walker.Lifetime--;
                }
            }
        }
    }

    private void MoveWalker(Walker walker, WorldData worldData)
    {
        // Pick a direction
        int dir = _random.Next(4); // 0=N, 1=E, 2=S, 3=W

        // Apply downward bias for the entrance walker
        if (_random.NextDouble() < walker.DownwardBias)
        {
            dir = 2; // Force "South" (down)
        }

        // Move the walker
        if (dir == 0) walker.Y--;
        else if (dir == 1) walker.X++;
        else if (dir == 2) walker.Y++;
        else if (dir == 3) walker.X--;

        // Clamp walker to map bounds
        int leftLimit = SideBorder + CarveRadius;
        int rightLimit = worldData.Width - SideBorder - CarveRadius - 1;

        if (walker.X < leftLimit)
        {
            walker.X = leftLimit;
        }
        if (walker.X > rightLimit)
        {
            walker.X = rightLimit;
        }
        if (walker.Y < CarveRadius) walker.Y = CarveRadius;
        int bottomLimit = worldData.Height - BottomBorder - CarveRadius;
        if (walker.Y > bottomLimit)
        {
            walker.Y = bottomLimit;
            // Optional: Force it to turn around
            // dir = 0; // 0 = North (up)
        }
        //

        // --- This is important! ---
        // Don't let walkers dig up past the surface
        int surfaceY = worldData.SurfaceHeightMap[walker.X];
        if (walker.Y <= surfaceY)
        {
            walker.Y = surfaceY + 1;
        }
    }

    private void Carve(int x, int y, WorldData worldData)
    {
        // Carve a square area around the walker's position
        for (int i = x - CarveRadius; i <= x + CarveRadius; i++)
        {
            for (int j = y - CarveRadius; j <= y + CarveRadius; j++)
            {
                // Check bounds one last time
                if (i < 0 || i >= worldData.Width || j < 0 || j >= worldData.Height)
                    continue;
                
                // Only carve stone and dirt. Leave everything else alone.
                int tile = worldData.TileMap[i, j];
                if (tile == TILE_STONE || tile == TILE_DIRT)
                {
                    worldData.TileMap[i, j] = TILE_AIR;
                }
            }
        }
    }
}
