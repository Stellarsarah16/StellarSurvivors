namespace StellarSurvivors.WorldGen.Steps;
using StellarSurvivors.Core;
using static StellarSurvivors.Data.TileIDs;

public class BaseTerrainStep : IGenerationStep
{
    public void Process(WorldData worldData, int seed)
    {
        // --- Define your world's "layers" ---
        int waterLevel = 60; // Any surface below y=60 will be a lake
        int dirtDepth = 5;   // How many blocks of dirt under the grass
        // -----------------------------------

        for (int x = 0; x < worldData.Width; x++)
        {
            // Get the surface height we calculated in Step 1
            int surfaceY = worldData.SurfaceHeightMap[x];
        
            for (int y = 0; y < worldData.Height; y++)
            {
                // LOGIC: We go from top (y=0) to bottom (y=Height)
            
                if (y < surfaceY)
                {
                    // We are ABOVE the ground
                    worldData.TileMap[x, y] = (y < waterLevel) ? TILE_AIR : TILE_WATER;
                }
                else if (y == surfaceY)
                {
                    // We are AT the surface
                    worldData.TileMap[x, y] = (y < waterLevel) ? TILE_GRASS : TILE_SAND; // Lakes get sand
                }
                else if (y <= surfaceY + dirtDepth)
                {
                    // We are in the DIRT layer (just below the surface)
                    worldData.TileMap[x, y] = TILE_DIRT;
                }
                else
                {
                    // We are in the DEEP earth
                    worldData.TileMap[x, y] = TILE_STONE;
                }
            }
        }
    }
// Note: You'll need to define TILE_AIR, TILE_WATER, TILE_GRASS, etc. as constants (e.g., 0, 1, 2...)
}