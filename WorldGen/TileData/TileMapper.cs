namespace StellarSurvivors.WorldGen.TileData;
using StellarSurvivors.Enums;
using StellarSurvivors.Data;

public static class TileMapper
{
    public static TileType IdToEnum(int tileId)
    {
        return tileId switch
        {
            TileIDs.TILE_GRASS => TileType.Grass,
            TileIDs.TILE_DIRT => TileType.Dirt,
            TileIDs.TILE_WATER => TileType.Water,
            TileIDs.TILE_SAND => TileType.Sand,
            TileIDs.TILE_STONE => TileType.Stone,
            TileIDs.TILE_IRON_ORE => TileType.Iron,
            TileIDs.TILE_GOLD_ORE => TileType.Gold,
            TileIDs.TILE_COAL => TileType.Coal,
            TileIDs.TILE_HARD => TileType.Hard,
            _ => TileType.None
        };
    }

    public static int EnumToId(TileType type)
    {
        return type switch
        {
            TileType.None => TileIDs.TILE_AIR,
            TileType.Grass => TileIDs.TILE_GRASS,
            TileType.Dirt => TileIDs.TILE_DIRT,
            TileType.Water => TileIDs.TILE_WATER,
            TileType.Sand => TileIDs.TILE_SAND,
            TileType.Stone => TileIDs.TILE_STONE,
            TileType.Iron => TileIDs.TILE_IRON_ORE,
            TileType.Gold => TileIDs.TILE_GOLD_ORE,
            TileType.Coal => TileIDs.TILE_COAL,
            _ => TileIDs.TILE_AIR
        };
    }
}