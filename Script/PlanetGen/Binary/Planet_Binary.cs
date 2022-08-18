using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

static public class Planet_Binary
{
    const string PlanetSaveFolder = "Planet";
    const string FileTempExt = ".ptp";
    const string FileExt = ".plt";


    static private int ChunkSize
    {
        get
        {
            return ChunkData.ChunkSize;
        }
    }
    static private string Path_File
    {
        get
        {
            return PlanetSaveFolder + "/Temp_" + Current.Name + FileTempExt;
        }
    }
    static private string Path_FileCompressed
    {
        get
        {
            return PlanetSaveFolder + "/" + Current.Name + FileExt;
        }
    }
    static private FileStream Stream;
    static private int TileInfoWidth = 5;
    static private DateTime StartSaveTime;
    static private DateTime StartLoadTime;
    static private CurrentPlanet_Binary Current;

    /// <summary>
    /// Save a planet to a file
    /// TODO : Catch error
    /// TODO : Disable saving while another save is running
    /// </summary>
    /// <param name="planet">Planet to save</param>
    static public async void SavePlanet(Planet planet)
    {
        StartSaveTime = DateTime.Now;
        Godot.GD.Print("Saving Planet " + planet.SaveName);

        Current = new CurrentPlanet_Binary(planet);

        Directory.CreateDirectory(PlanetSaveFolder); // Create directory if doesn't exist

        Stream = File.Open(Path_File, FileMode.Create);
        await WriteHeader();
        CompressAndCloseStream();
    }
    /// <summary>
    /// Load a planet data
    /// </summary>
    /// <param name="planet">Planet to load</param>
    static public async Task<bool> LoadPlanet(Planet planet)
    {
        Current = new CurrentPlanet_Binary(planet);
        if (File.Exists(Path_FileCompressed))
        {
            StartLoadTime = DateTime.Now;
            UncompressStream();
            Stream.Position = 0;
            await Stream.ReadAsync(Current.ChunkExist, 0, Current.ChunkExistSize);
            int count = 0;
            for (int i = 0; i < Current.W; i++)
            {
                for (int j = 0; j < Current.H; j++)
                {
                    if (Current.IsChunkExist(i, j))
                    {
                        Godot.Vector2 pos = new Godot.Vector2(i, j);
                        Current.Planet.GeneratedChunks.Add(pos, await ReadChunkData((byte)i, (byte)j));
                        count++;
                    }
                }
            }
            Stream.Close();
            Godot.GD.Print("Planet Loaded : " + count + " Chunks loaded in " + (DateTime.Now - StartLoadTime).Milliseconds + " ms");
            return true;
        }
        else
        {
            Godot.GD.Print("No save found");
            return false;
        }
    }

    // SAVE PART

    /// <summary>
    /// Write main info like witch chunk is generated
    /// </summary>
    static private async Task WriteHeader()
    {
        Stream.SetLength(
            Current.ChunkExistSize + // Chunk exist ?
            Current.W * Current.H * ChunkSize * ChunkSize * TileInfoWidth // Tiles Info  chunk number * chunk size * TileInfoWidth
            );
        Stream.Position = 0;
        await Stream.WriteAsync(Current.ChunkExist, 0, Current.ChunkExistSize);
        foreach(KeyValuePair<Godot.Vector2, ChunkData> a in Current.Planet.GeneratedChunks)
        {
            if (a.Value.State == ChunkData.Chunk_State.Generated)
            {
                await WriteChunkData((byte)a.Key.x, (byte)a.Key.y, a.Value);
            }
        }
    }
    /// <summary>
    /// Save chunk data to the file
    /// </summary>
    /// <param name="x">X position of the chunk (relative to chunk)</param>
    /// <param name="y">Y position of the chunk (relative to chunk)</param>
    /// <param name="chunkdata">Chunk information</param>
    static private async Task WriteChunkData(byte x, byte y, ChunkData chunkdata)
    {
        byte[] u = new byte[ChunkSize * ChunkSize * TileInfoWidth];
        for (int i = 0; i < chunkdata.Tiles_Front.Length; i++)
        {
            BitConverter.GetBytes(chunkdata.Tiles_Front[i]).CopyTo(u, i * TileInfoWidth);
            BitConverter.GetBytes(chunkdata.Tiles_Back[i]).CopyTo(u, i * TileInfoWidth + 2);
            u[i * TileInfoWidth + 4] = chunkdata.Tiles_Color[i];
        }
        Stream.Position = Current.ChunkExistSize + (x * Current.H + y) * ChunkSize * ChunkSize * TileInfoWidth;
        await Stream.WriteAsync(u, 0, u.Length);
    }
    /// <summary>
    /// Compress file and close
    /// </summary>
    static private void CompressAndCloseStream()
    {
        Stream.Close();
        FileStream originalFileStream = File.Open(Path_File, FileMode.Open);
        FileStream compressedFileStream = File.Create(Path_FileCompressed);
        var compressor = new DeflateStream(compressedFileStream, CompressionMode.Compress);
        originalFileStream.CopyTo(compressor);

        originalFileStream.Close();
        compressor.Close();

        long newSize = new FileInfo(Path_FileCompressed).Length;
        long oldSize = new FileInfo(Path_File).Length;
        Godot.GD.Print("Planet Compressed and save : " + Path_FileCompressed + " from " + oldSize + " bytes to " + newSize + " bytes in "
            + (DateTime.Now - StartSaveTime).Milliseconds + " ms");


        //File.Delete(Path_File);
    }

    //LOAD PART

    /// <summary>
    /// Uncompress file 
    /// </summary>
    static private void UncompressStream()
    {
        FileStream compressedFileStream = File.Open(Path_FileCompressed, FileMode.Open);
        Stream = File.Create(Path_File);
        var decompressor = new DeflateStream(compressedFileStream, CompressionMode.Decompress);
        decompressor.CopyTo(Stream);
        decompressor.Close();
        Stream.Position = 0;
        Godot.GD.Print("Planet Uncompressed : " + Current.Name);
    }
    /// <summary>
    /// Read file information for a chunk
    /// </summary>
    /// <param name="x">X position of the chunk (relative to chunk)</param>
    /// <param name="y">Y position of the chunk (relative to chunk)</param>
    /// <returns>Chunk information</returns>
    static private async Task<ChunkData> ReadChunkData(byte x, byte y)
    {
        ushort[] front = new ushort[ChunkSize * ChunkSize];
        ushort[] back = new ushort[ChunkSize * ChunkSize];
        byte[] color = new byte[ChunkSize * ChunkSize];

        Stream.Position = Current.ChunkExistSize + (x * Current.H + y) * ChunkSize * ChunkSize * TileInfoWidth;
        byte[] u = new byte[ChunkSize * ChunkSize * TileInfoWidth];
        await Stream.ReadAsync(u, 0, u.Length);
        for (int i = 0; i < ChunkSize * ChunkSize; i++)
        {
            front[i] = BitConverter.ToUInt16(u, i * TileInfoWidth);
            back[i] = BitConverter.ToUInt16(u, i * TileInfoWidth + 2);
            color[i] = u[i * TileInfoWidth + 4];
        }
        ChunkData data = new ChunkData(new Godot.Vector2(x, y), Current.Planet, front, back, color);
        return data;
    }


    class CurrentPlanet_Binary
    {
        public string Name { get { return Planet.SaveName; } }
        public byte W { get { return Planet.Info.W; } }
        public byte H { get { return Planet.Info.H; } }
        public int DataSize;
        public int ChunkExistSize;
        public byte[] ChunkExist;
        public Planet Planet;

        public CurrentPlanet_Binary(Planet planet, bool generate = true)
        {
            Planet = planet;
            ChunkExistSize = (int)Math.Ceiling((W * H) / 8f);
            ChunkExist = new byte[ChunkExistSize];
            if (generate)
                GenerateChunkExist();
        }

        public void GenerateChunkExist()
        {
            int count = 0;
            int count2 = 0;
            bool[] v = new bool[8];
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    Godot.Vector2 key = new Godot.Vector2(i, j);
                    if (Planet.GeneratedChunks.ContainsKey(key))
                    {
                        if (Planet.GeneratedChunks[key].State == ChunkData.Chunk_State.Generated)
                        {
                            v[count] = true;
                        }
                        else
                        {
                            v[count] = false;
                        }
                    }
                    else
                    {
                        v[count] = false;
                    }
                    count++;
                    if (count == 8)
                    {
                        ChunkExist[count2] = MathAddon.ConvertBoolArrayToByte(v);
                        count2++;
                        count = 0;
                    }
                }
            }
            if (count > 0)
            {
                ChunkExist[count2] = MathAddon.ConvertBoolArrayToByte(v);
            }
        }

        public bool IsChunkExist(int x, int y)
        {
            int id = x * H + y;
            int byteid = (int)Math.Floor((W * H) / 8f);

            return MathAddon.ConvertByteToBoolArray(ChunkExist[byteid])[id % 8];
        }
    }
}
