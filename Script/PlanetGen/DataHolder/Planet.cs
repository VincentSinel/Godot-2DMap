using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using static Planet_Info;

public class Planet
{
    // Private Data
    private Dictionary<Vector2, ChunkData> generatedChunks = new Dictionary<Vector2, ChunkData>();
    private Planet_Info info;

    // Public Data
    public bool RecheckZone = false;
    public Planet_Info Info { get { return info; } }
    public Dictionary<Vector2, ChunkData> GeneratedChunks { get { return generatedChunks; } }
    public static int ChunkSize { get { return ChunkData.ChunkSize; } }
    public string Name { get { return $"{Info.X}_{Info.Y}_{Info.Z}"; } }
    public string SaveName { get { return $"{Info.X}_{Info.Y}_{Info.Z}"; } }

    /// <summary>
    /// Instanciate a new planet
    /// </summary>
    /// <param name="_info">Generator info defining planet type</param>
    public Planet(Planet_Info _info)
    {
        info = _info;
    }

    /// <summary>
    /// Set front tile at a specific position
    /// TODO : Take care of tile in a inexistant chunk
    /// TODO : Take care of Chunk not totally generated
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <param name="tile">Tile index</param>
    public void SetFrontTileAt(int x, int y, ushort tile)
    {
        int x_ = MathAddon.Mod(x, Info.IntTileW) / ChunkSize;
        int y_ = y / ChunkSize;

        Vector2 key = new Vector2(x_, y_);
        if (GeneratedChunks.ContainsKey(key))
        {
            int dx = MathAddon.Mod(x, ChunkSize);
            int dy = MathAddon.Mod(y, ChunkSize);
            GeneratedChunks[key].Tiles_Front[dx * ChunkSize + dy] = tile;
            GeneratedChunks[key].ChunkDrawState = ChunkData.Chunk_DrawState.NeedRedraw;
            RecheckZone = true;
        }
    }
    /// <summary>
    /// Set back tile at a specific position
    /// TODO : Take care of tile in a inexistant chunk
    /// TODO : Take care of Chunk not totally generated
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <param name="tile">Tile index</param>
    public void SetBackTileAt(int x, int y, ushort tile)
    {
        int x_ = MathAddon.Mod(x, Info.IntTileW) / ChunkSize;
        int y_ = y / ChunkSize;

        Vector2 key = new Vector2(x_, y_);
        if (GeneratedChunks.ContainsKey(key))
        {
            int dx = MathAddon.Mod(x, ChunkSize);
            int dy = MathAddon.Mod(y, ChunkSize);
            GeneratedChunks[key].Tiles_Back[dx * ChunkSize + dy] = tile;
            GeneratedChunks[key].ChunkDrawState = ChunkData.Chunk_DrawState.NeedRedraw;
            RecheckZone = true;
        }
    }
    /// <summary>
    /// Set color tile at a specific position
    /// TODO : Take care of color in a inexistant chunk
    /// TODO : Take care of Chunk not totally generated
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <param name="color">color value (including back and front)</param>
    public void SetColorTileAt(int x, int y, byte color)
    {
        int x_ = MathAddon.Mod(x, Info.IntTileW) / ChunkSize;
        int y_ = y / ChunkSize;

        Vector2 key = new Vector2(x_, y_);
        if (GeneratedChunks.ContainsKey(key))
        {
            int dx = MathAddon.Mod(x, ChunkSize);
            int dy = MathAddon.Mod(y, ChunkSize);
            GeneratedChunks[key].Tiles_Color[dx * ChunkSize + dy] = color;
            GeneratedChunks[key].ChunkDrawState = ChunkData.Chunk_DrawState.NeedRedraw;
            RecheckZone = true;
        }
    }
    /// <summary>
    /// Get front tile at a specific position
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <returns>Tile index</returns>
    public ushort GetFrontTileAt(int x, int y)
    {
        int x_ = MathAddon.Mod(x, Info.IntTileW) / ChunkSize;
        int y_ = y / ChunkSize;

        Vector2 key = new Vector2(x_, y_);
        if (GeneratedChunks.ContainsKey(key))
        {
            if (GeneratedChunks[key].State == ChunkData.Chunk_State.Generated)
            {
                int dx = MathAddon.Mod(x, ChunkSize);
                int dy = MathAddon.Mod(y, ChunkSize);
                return GeneratedChunks[key].Tiles_Front[dx * ChunkSize + dy];
            }
        }
        return Info.GenerateTile(MathAddon.Mod(x, Info.IntTileW), y)[0];
    }
    /// <summary>
    /// Get back tile at a specific position
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <returns>Tile index</returns>
    public ushort GetBackTileAt(int x, int y)
    {
        int x_ = MathAddon.Mod(x, Info.IntTileW) / ChunkSize;
        int y_ = y / ChunkSize;

        Vector2 key = new Vector2(x_, y_);
        if (GeneratedChunks.ContainsKey(key))
        {
            if (GeneratedChunks[key].State == ChunkData.Chunk_State.Generated)
            {
                int dx = MathAddon.Mod(x, ChunkSize);
                int dy = MathAddon.Mod(y, ChunkSize);
                return GeneratedChunks[key].Tiles_Back[dx * ChunkSize + dy];
            }
        }
        return Info.GenerateTile(MathAddon.Mod(x, Info.IntTileW), y)[1];
    }
    /// <summary>
    /// Get color of tile at a specific position
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <returns>Tile color</returns>
    public byte GetColorTileAt(int x, int y)
    {
        int x_ = MathAddon.Mod(x, Info.IntTileW) / ChunkSize;
        int y_ = y / ChunkSize;

        Vector2 key = new Vector2(x_, y_);
        if (GeneratedChunks.ContainsKey(key))
        {
            if (GeneratedChunks[key].State == ChunkData.Chunk_State.Generated)
            {
                int dx = MathAddon.Mod(x, ChunkSize);
                int dy = MathAddon.Mod(y, ChunkSize);
                return GeneratedChunks[key].Tiles_Color[dx * ChunkSize + dy];
            }
        }
        return (byte)Info.GenerateTile(MathAddon.Mod(x, Info.IntTileW), y)[2];
    }
    /// <summary>
    /// Convert extract front color from tile color
    /// </summary>
    /// <param name="c">Tile color</param>
    /// <returns>Front tile color</returns>
    public int GetFrontColor(byte c)
    {
        return c % 16;
    }
    /// <summary>
    /// Convert extract back color from tile color
    /// </summary>
    /// <param name="c">Tile color</param>
    /// <returns>Back tile color</returns>
    public int GetBackColor(byte c)
    {
        return c / 16;
    }



    /// <summary>
    /// Generate Tile at specific index asynchronously
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <returns>Ushort Array with a lenght of 3 : 0 -> front tile id; 1 -> back tile id; 2 -> color</returns>
    public async Task<ushort[]> AsyncGenerateTile(int x, int y)
    { return await Task.Run(() => Info.GenerateTile(x, y)); }
    

    public void ClearChunkOutsideBox(byte x1, byte x2, byte y1, byte y2)
    {
        foreach (KeyValuePair<Vector2, ChunkData> c in GeneratedChunks)
        {
            if (
                (x1 <= x2) && (c.Key.x < (x1 - 1) || c.Key.x > (x2 + 1)) ||
                (x1 > x2) && (c.Key.x < (x1 - 1) && c.Key.x > (x2 + 1)) ||
                c.Key.y < y1 || c.Key.y >= y2
                )
            {
                if (c.Value.ChunkDrawState == ChunkData.Chunk_DrawState.Drawn)
                    c.Value.A_Clear();
            }
        }
    }
    /// <summary>
    /// Launch generation of chunk in a specified zone
    /// </summary>
    /// <param name="x1">chunk X start</param>
    /// <param name="x2">chunk X end</param>
    /// <param name="y1">chunk Y start</param>
    /// <param name="y2">chunk Y end</param>
    public void GenerateChunkBetween(byte x1, byte x2, byte y1, byte y2)
    {
        RecheckZone = false;
        if (x1 <= x2)
        {
            for (byte i = x1; i <= x2; i++)
            {
                for (byte j = y1; j < y2; j++)
                {
                    GenerateChunk(i, j, x1, x2);
                }
            }
        }
        else
        {
            for (byte i = 0; i <= x2; i++)
            {
                for (byte j = y1; j < y2; j++)
                {
                    GenerateChunk(i, j, x1, x2);
                }
            }
            for (byte i = x1; i < Info.W; i++)
            {
                for (byte j = y1; j < y2; j++)
                {
                    GenerateChunk(i, j, x1, x2);
                }
            }
        }
    }
    /// <summary>
    /// Asynchonously create chunk at specified index
    /// </summary>
    /// <param name="x">Chunk X</param>
    /// <param name="y">Chunk Y</param>
    /// <returns>Boolean specified if chunk as been reloaded</returns>
    public void GenerateChunk(byte x, byte y, byte x1, byte x2)
    {
        Vector2 key = new Vector2(x, y);
        if (!GeneratedChunks.ContainsKey(key))
        {
            ChunkData chunk = new ChunkData(key, this);
            GeneratedChunks.Add(key, chunk);
        }
        GeneratedChunks[key].A_GenerateAndDraw();
    }
}
