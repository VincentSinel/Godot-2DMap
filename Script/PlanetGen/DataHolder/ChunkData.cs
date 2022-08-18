using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class ChunkData
{
    public static readonly int ChunkSize = 40; // Taille d'un chunk en tile

    // Private Data
    private Planet planet;
    private Vector2 position;
    private ushort[] tiles_Front;
    private byte[] tiles_Color;
    private ushort[] tiles_Back;
    private int realX;
    private int realY;
    private DateTime StartGen;
    private bool EventRunning = false;
    private Queue<Action> Actions = new Queue<Action>(); 

    // Public Data
    public Chunk_State State = Chunk_State.Initiate;
    public Chunk_DrawState ChunkDrawState = Chunk_DrawState.WaitForGeneration;
    public Vector2 Position { get { return position; } }
    public ushort[] Tiles_Front { get { return tiles_Front; } }
    public byte[] Tiles_Color { get { return tiles_Color; } }
    public ushort[] Tiles_Back { get { return tiles_Back; } }
    public Planet Planet { get { return planet; } }
    public Planet_Info Info { get { return Planet.Info; } }
    public int RealX { get { return realX; } }
    public int RealY { get { return realY; } }
    public int X { get { return (int)Position.x; } }
    public int Y { get { return (int)Position.y; } }


    /// <summary>
    /// Create a new instance of ChunkData at a given position end associated to a specific planet
    /// </summary>
    /// <param name="_position">Position of the chunk (in chunk coordinate)</param>
    /// <param name="_info">Planet associated</param>
    public ChunkData(Vector2 _position, Planet _info)
    {
        position = _position;

        // Calcul de la position réel du chunk
        realX = X * ChunkSize;
        realY = Y * ChunkSize;

        tiles_Front = new ushort[ChunkSize * ChunkSize];
        tiles_Back = new ushort[ChunkSize * ChunkSize];
        tiles_Color = new byte[ChunkSize * ChunkSize];

        planet = _info ?? throw new ArgumentNullException(nameof(_info));
        Actions.Enqueue(Action.Generate);
    }

    public ChunkData(Vector2 _position, Planet _info, ushort[] front, ushort[] back, byte[] color)
    {
        position = _position;

        // Calcul de la position réel du chunk
        realX = X * ChunkSize;
        realY = Y * ChunkSize;

        tiles_Back = back;
        tiles_Front = front;
        tiles_Color = color;
        State = Chunk_State.Generated;
        ChunkDrawState = Chunk_DrawState.NeedRedraw;

        planet = _info;
    }

    public void A_GenerateAndDraw()
    {
        Actions.Enqueue(Action.Generate);
        Actions.Enqueue(Action.Draw);
        RunActions();
    }
    public void A_Generate()
    {
        Actions.Enqueue(Action.Generate);
        RunActions();
    }
    public void A_Draw()
    {
        Actions.Enqueue(Action.Draw);
        RunActions();
    }
    public void A_Clear()
    {
        Actions.Enqueue(Action.Clear);
        RunActions();
    }

    /// <summary>
    /// Launch chunk generation
    /// </summary>
    /// <returns>True if it's the first time generation is launch</returns>
    private async void RunActions()
    {
        if (EventRunning)
            return;
        EventRunning = true;
        while(Actions.Count > 0)
        {
            switch(Actions.Dequeue())
            {
                case Action.Generate:
                    await Generate();
                    break;
                case Action.Draw:
                    await Draw();
                    break;
                case Action.Clear:
                    Clear();
                    break;
            }
        }
        EventRunning = false;
    }

    private async Task<bool> Generate()
    {
        if (State == Chunk_State.Initiate)
        {
            StartGen = DateTime.Now;
            State = Chunk_State.Generating;
            await Task.Run(() => G_Tiles());
            //await G_Tiles(); // Wait for Tiles to generate;
            //await // Wait for Objects to generate;
            //await // Wait for Objects to render;
            //await // Wait for Data save;
            State = Chunk_State.Generated;
            ChunkDrawState = Chunk_DrawState.Redrawing;
            GD.Print($"Chunk {X,4} ; {Y,4} Generated in {(DateTime.Now - StartGen).Milliseconds} ms");
            return true;
        }
        return false;
    }

    private async Task<bool> Draw()
    {
        if (ChunkDrawState == Chunk_DrawState.NeedRedraw ||
            ChunkDrawState == Chunk_DrawState.Redrawing)
        {
            StartGen = DateTime.Now;
            D_Tiles(await Task.Run(() => CD_Tiles()));
            GD.Print($"Chunk {X,4} ; {Y,4} Drawn     in {(DateTime.Now - StartGen).Milliseconds} ms");
            return true;
        }
        return false;
    }

    private void Clear()
    {
        if (ChunkDrawState == Chunk_DrawState.Drawn)
        {
            StartGen = DateTime.Now;
            ChunkDrawState = Chunk_DrawState.Clearing;
            ClearDraw();
            GD.Print($"Chunk {X,4} ; {Y,4} Clear     in {(DateTime.Now - StartGen).Milliseconds} ms");
        }
    }

    /// <summary>
    /// Generate tiles in the chunk
    /// </summary>
    private void G_Tiles()
    {
        for (int i = 0; i < ChunkSize; i++)
        {
            for (int j = 0; j < ChunkSize; j++)
            {
                ushort[] tiles = Info.GenerateTile(i + RealX, j + RealY);
                Tiles_Front[i * ChunkSize + j] = tiles[0];
                Tiles_Back[i * ChunkSize + j] = tiles[1];
                Tiles_Color[i * ChunkSize + j] = (byte)tiles[2];
            }
        }
    }
    /// <summary>
    /// Generate Tile data for collision (marching square)
    /// </summary>
    /// <returns>Collisions tiles array</returns>
    private int[] CD_Tiles()
    {
        int[] tiles = new int[(ChunkSize + 1) * (ChunkSize + 1)];

        int size = ChunkSize + 4;
        bool[] isovalue = new bool[size];

        for (int i = 0; i < (ChunkSize + 2); i++)
        {
            for (int j = 0; j < (ChunkSize + 2); j++)
            {
                int ri = i - 1;
                int rj = j - 1;
                int id = (i * (ChunkSize + 2) + j) % (size);
                ushort tile = GetFrontTileAt(ri, rj);
                isovalue[id] = tile != 0;

                if (i > 0 && j > 0)
                {
                    int c1 = (ri * (ChunkSize + 2) + rj) % (size);
                    int c2 = ( i * (ChunkSize + 2) + rj) % (size);
                    int c3 = (ri * (ChunkSize + 2) +  j) % (size);
                    int v = (byte)(isovalue[c1] ? 8 : 0);
                    v += (byte)(isovalue[c2] ? 4 : 0);
                    v += (byte)(isovalue[id] ? 2 : 0);
                    v += (byte)(isovalue[c3] ? 1 : 0);
                    tiles[ri * (ChunkSize + 1) + rj] = v;
                }
            }
        }
        return tiles;
    }
    /// <summary>
    /// Draw everything on tilemap (outside multithreading)
    /// </summary>
    /// <param name="tiles">Collisions tiles array</param>
    private void D_Tiles(int[] tiles)
    {
        for (int i = 0; i < (ChunkSize + 1); i++)
        {
            for (int j = 0; j < (ChunkSize + 1); j++)
            {
                Planet_Generator.TileMap_Collision.SetCell(RealX + i, RealY + j, tiles[i * (ChunkSize + 1) + j]);
                if (i < ChunkSize && j < ChunkSize)
                {
                    int id = i * ChunkSize + j;
                    Planet_Generator.TileMap_Front.SetCell(RealX + i, RealY + j, Tiles_Front[id]);
                    Planet_Generator.TileMap_Back.SetCell(RealX + i, RealY + j, Tiles_Back[id]);
                }
            }
        }
        ChunkDrawState = Chunk_DrawState.Drawn;
    }

    
    /// <summary>
    /// Clear this chunk from the tilemap
    /// </summary>
    private void ClearDraw()
    {
        for (int i = 0; i < (ChunkSize + 1); i++)
        {
            for (int j = 0; j < (ChunkSize + 1); j++)
            {
                Planet_Generator.TileMap_Collision.SetCell(RealX + i, RealY + j, -1);
                if (i < ChunkSize && j < ChunkSize)
                {
                    Planet_Generator.TileMap_Front.SetCell(RealX + i, RealY + j, -1);
                    Planet_Generator.TileMap_Back.SetCell(RealX + i, RealY + j, -1);
                }
            }
        }
        ChunkDrawState = Chunk_DrawState.NeedRedraw;
    }

    /// <summary>
    /// Get front tile at a specific position
    /// </summary>
    /// <param name="x">X position relative to this Chunk</param>
    /// <param name="y">Y position relative to this Chunk</param>
    /// <returns>Tile ID</returns>
    public ushort GetFrontTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= ChunkSize || y >= ChunkSize)
        { // Tile outside this chunk
            return planet.GetFrontTileAt(RealX + x, RealY + y);
        }
        else
        { // Tile in this chunk
            return Tiles_Front[x * ChunkSize + y];
        }
    }

    /// <summary>
    /// Get back tile at a specific position
    /// </summary>
    /// <param name="x">X position relative to this Chunk</param>
    /// <param name="y">Y position relative to this Chunk</param>
    /// <returns>Tile ID</returns>
    public ushort GetBackTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= ChunkSize || y >= ChunkSize)
        { // Tile outside this chunk
            return planet.GetBackTileAt(x + (int)Position.x * ChunkSize, y + (int)Position.y * ChunkSize);
        }
        else
        { // Tile in this chunk
            return Tiles_Back[x * ChunkSize + y];
        }
    }


    /// <summary>
    /// State of chunk generation
    /// </summary>
    public enum Chunk_State
    {
        Initiate,
        Generating,
        Generated
    }
    /// <summary>
    /// State of chunk drawing
    /// </summary>
    public enum Chunk_DrawState
    {
        WaitForGeneration,
        NeedRedraw,
        Redrawing,
        Drawn,
        Clearing
    }

    public enum Action
    {
        Generate,
        Draw,
        Clear
    }
}
