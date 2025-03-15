using System;
using System.Collections.Generic;
using Godot;

public class TileLibrary
{
  private Dictionary<string, TileType> _tileNames;

  public TileLibrary()
  {
    _tileNames = new Dictionary<string, TileType>();
    
    //Add tiletypes
    _tileNames["ground_grass"] = TileType.ground_grass;
    _tileNames["ground_stone"] = TileType.ground_stone;
    _tileNames["water"] = TileType.water;
    _tileNames["water_spring"] = TileType.water_spring;
    _tileNames["hole"] = TileType.hole;
    _tileNames["dirt_pile"] = TileType.dirtPile;
    _tileNames["stone"] = TileType.stone;
  }

  public TileType GetTileType(string tileName)
  {
    return _tileNames.GetValueOrDefault(tileName);
  }

  public int GetTileSetSourceId(TileType tileType)
  {
    switch (tileType)
    {
      case TileType.ground_grass:
        return 4;
      case TileType.ground_stone:
        return 4;
      case TileType.water:
        return 3;
      case TileType.water_spring:
        return 3;
      case TileType.hole:
        return 7;
      case TileType.dirtPile:
        return 7;
      case TileType.stone:
        return 7;
      case TileType.flower:
        return 2;
      default:
        return -1;
    }
  }

  public Vector2I GetTileAtlasCoords(TileType tileType)
  {
    switch (tileType)
    {
      case TileType.ground_grass:
        return new Vector2I(-1, -1);
      case TileType.ground_stone:
        return new Vector2I(-1, -1);
      case TileType.water:
        return new Vector2I(-1, -1);
      case TileType.water_spring:
        return new Vector2I(-1, -1);
      case TileType.hole:
        return new Vector2I(0, 1);
      case TileType.dirtPile:
        return new Vector2I(1, 0);
      case TileType.stone:
        return new Vector2I(-1, -1);
      case TileType.flower:
        return new Vector2I(-1, -1);
      default:
        return new Vector2I(-1, -1);
    }
  }

  public enum TileType
  {
    none,
    ground_grass,
    ground_stone,
    water,
    water_spring,
    hole,
    dirtPile,
    stone,
    flower
  }
}