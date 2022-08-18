using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Base class for create a planet generator
/// TODO : Add object generation function
/// TODO : Add dungeon generation function 
/// </summary>²
public class Planet_Info : Godot.Object
{
    // Private Data
    private Dictionary<Vector2, ChunkData> generatedChunks = new Dictionary<Vector2, ChunkData>();
    internal int IntTileW { get { return W * ChunkSize; } }
    internal float TileW { get { return W * ChunkSize * 1.0f; } }
    internal int TileH { get { return W * ChunkSize; } }
    internal float Radius = 0;

    // Public Data
    public static int ChunkSize { get { return ChunkData.ChunkSize; } }
    public bool NeedRefreshOfData = true;
    public Planet_Size Size = Planet_Size.Small;
    public Planet_Type Type = Planet_Type.Gas_Giant;
    public bool CanLandOn = true;
    public int X = 0;
    public int Y = 0;
    public int Z = 0;
    public byte W = 0;
    public byte H = 0;
    public int Layer_Space;
    public int Layer_Atmosphere;
    public int Layer_Surface;
    public int Layer_Subsurface;
    public int Layer_ShallowUnderground;
    public int Layer_MidUnderground;
    public int Layer_DeepUnderground;
    public int Layer_Core;
    //public int BigStructur = 0;

    // Size of different layer
    public int Layer_SpaceSize { get { return Layer_Space; } }
    public int Layer_AtmosphereSize { get { return Layer_Atmosphere - Layer_Space; } }
    public int Layer_SurfaceSize { get { return Layer_Surface - Layer_Atmosphere; } }
    public int Layer_SubsurfaceSize { get { return Layer_Subsurface - Layer_Surface; } }
    public int Layer_ShallowUndergroundSize { get { return Layer_ShallowUnderground - Layer_Subsurface; } }
    public int Layer_MidUndergroundSize { get { return Layer_MidUnderground - Layer_ShallowUnderground; } }
    public int Layer_DeepUndergroundSize { get { return Layer_DeepUnderground - Layer_MidUnderground; } }
    public int Layer_CoreSize { get { return Layer_Core - Layer_DeepUnderground; } }



    /// <summary>
    /// Must be done after setting X, Y, Z, Size, Type.
    /// </summary>
    public void RefreshData()
    {
        if (Type == Planet_Type.Gas_Giant)
        {
            CanLandOn = false;
            return;
        }

        switch (Size)
        {
            case Planet_Size.Micro: W = (byte)Mathf.Ceil(50 / (ChunkSize * 1.0f)); break;
            case Planet_Size.Very_Small: W = (byte)Mathf.Ceil(1000 / (ChunkSize * 1.0f)); break;
            case Planet_Size.Small: W = (byte)Mathf.Ceil(3000 / (ChunkSize * 1.0f)); ; break;
            case Planet_Size.Medium: W = (byte)Mathf.Ceil(4000 / (ChunkSize * 1.0f)); ; break;
            case Planet_Size.Large: W = (byte)Mathf.Ceil(6000 / (ChunkSize * 1.0f)); ; break;
        }
        int tsize;
        switch (Size)
        {
            case Planet_Size.Micro:
                H = (byte)Mathf.Ceil(50 / (ChunkSize * 1.0f));
                tsize = H * ChunkSize;
                Layer_Space = (int)(tsize * 1 / 6f);
                Layer_Atmosphere = (int)(tsize * 2 / 6f);
                Layer_Surface = (int)(tsize * 7 / 10f);
                Layer_Subsurface = (int)(tsize * 3 / 4f);
                Layer_ShallowUnderground = (int)(tsize * 4 / 5f);
                Layer_MidUnderground = (int)(tsize * 17 / 20f);
                Layer_DeepUnderground = (int)(tsize * 9 / 10f);
                Layer_Core = tsize;
                break;
            case Planet_Size.Very_Small:
            case Planet_Size.Small:
                H = (byte)Mathf.Ceil(2000 / (ChunkSize * 1.0f));
                tsize = H * ChunkSize;
                Layer_Space = (int)(tsize * 300 / 2000.0);
                Layer_Atmosphere = (int)(tsize * 900 / 2000.0);
                Layer_Surface = (int)(tsize * 1400 / 2000.0);
                Layer_Subsurface = (int)(tsize * 1500 / 2000.0);
                Layer_ShallowUnderground = (int)(tsize * 1600 / 2000.0);
                Layer_MidUnderground = (int)(tsize * 1700 / 2000.0);
                Layer_DeepUnderground = (int)(tsize * 1800 / 2000.0);
                Layer_Core = tsize;
                break;
            case Planet_Size.Medium:
            case Planet_Size.Large:
                H = (byte)Mathf.Ceil(3000 / (ChunkSize * 1.0f));
                tsize = H * ChunkSize;
                Layer_Space = (int)(tsize * 1000 / 3000.0);
                Layer_Atmosphere = (int)(tsize * 1600 / 3000.0);
                Layer_Surface = (int)(tsize * 2100 / 3000.0);
                Layer_Subsurface = (int)(tsize * 2200 / 3000.0);
                Layer_ShallowUnderground = (int)(tsize * 2400 / 3000.0);
                Layer_MidUnderground = (int)(tsize * 2600 / 3000.0);
                Layer_DeepUnderground = (int)(tsize * 2800 / 3000.0);
                Layer_Core = tsize;
                break;
        }

        Radius = TileW / (Mathf.Pi * 2f);


        Refresh_SpecificData();



    }
    /// <summary>
    /// Mets a jours les paramètre particulier des planets
    /// </summary>
    internal virtual void Refresh_SpecificData()
    {
    }
    /// <summary>
    /// Return position around the planet from x position
    /// </summary>
    /// <param name="x">Horizontal position</param>
    /// <returns>Position around th planet</returns>
    internal virtual Vector2 GetCPosition(int x)
    {
        float ang = Mathf.Pi * 2 * x / TileW;
        return new Vector2(Radius * Mathf.Cos(ang), Radius * Mathf.Sin(ang));
    }

    // Following functions must be override by a specific planet generation algorithm
    // Every function must return ushort array of 3 element :
    // id 0 : Front tile
    // id 1 : Back tile
    // id 2 : Color (will be converted to byte)

    internal virtual ushort[] GT_Space(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_Atmosphere(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_Surface(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_Subsurface(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_ShallowUnderground(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_MidUnderground(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_DeepUnderground(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }
    internal virtual ushort[] GT_Core(int x, int y)
    { return new ushort[3] { 0, 0, 0 }; }

    /// <summary>
    /// Get layer from y position
    /// </summary>
    /// <param name="y">Y position</param>
    /// <returns>Layer</returns>
    public virtual CurrentLayer GetCurrentLayer(int y)
    {
        if (y <= Layer_Space)
            return CurrentLayer.Space;
        else if (y <= Layer_Atmosphere)
            return CurrentLayer.Atmosphere;
        else if (y <= Layer_Surface)
            return CurrentLayer.Surface;
        else if (y <= Layer_Subsurface)
            return CurrentLayer.Subsurface;
        else if (y <= Layer_ShallowUnderground)
            return CurrentLayer.ShallowUnderground;
        else if (y <= Layer_MidUnderground)
            return CurrentLayer.MidUnderground;
        else if (y <= Layer_DeepUnderground)
            return CurrentLayer.DeepUnderground;
        else if (y <= Layer_Core)
            return CurrentLayer.Core;
        else
            return CurrentLayer.None;
    }

    /// <summary>
    /// Generate Tile at specific index
    /// </summary>
    /// <param name="x">X position relative to tilemap</param>
    /// <param name="y">Y position relative to tilemap</param>
    /// <returns>Ushort Array with a lenght of 3 : 0 -> front tile id; 1 -> back tile id; 2 -> color</returns>
    public ushort[] GenerateTile(int x, int y)
    {
        CurrentLayer layer = GetCurrentLayer(y);

        switch (layer)
        {
            case CurrentLayer.Space:
                return GT_Space(x, y);
            case CurrentLayer.Atmosphere:
                return GT_Atmosphere(x, y);
            case CurrentLayer.Surface:
                return GT_Surface(x, y);
            case CurrentLayer.Subsurface:
                return GT_Subsurface(x, y);
            case CurrentLayer.ShallowUnderground:
                return GT_ShallowUnderground(x, y);
            case CurrentLayer.MidUnderground:
                return GT_MidUnderground(x, y);
            case CurrentLayer.DeepUnderground:
                return GT_DeepUnderground(x, y);
            case CurrentLayer.Core:
                return GT_Core(x, y);
            case CurrentLayer.None:
                return new ushort[3] { ushort.MaxValue, ushort.MaxValue, 0 };
            default:
                return new ushort[3] { 1, 1, 0 };
        }
    }


    public enum CurrentLayer
    {
        Space,
        Atmosphere,
        Surface,
        Subsurface,
        ShallowUnderground,
        MidUnderground,
        DeepUnderground,
        Core,
        None
    }

    public enum Planet_Size
    {
        Micro,
        Very_Small,
        Small,
        Medium,
        Large
    }
    public enum Planet_Type
    {
        Gas_Giant,
        Moon,
        Acient_Gateway,
        Asteroid_Fields,
        Barren,
        Garden,
        Desert,
        Forest,
        Ocean,
        Savannah,
        Snow,
        Jungle,
        Mutated,
        Toxic,
        Artic,
        Midnight,
        Tundra,
        Decayed,
        Magma,
        Volcanic,
        Other
    }
    public enum Planet_Space_Biome
    {
        None,
        Asteroid
    }
    public enum Planet_Atmosphere_Biome
    {
        None
    }
    public enum Planet_Surface_Biome
    {
        None,
        Barren,
        Garden,
        Desert,
        Forest,
        Ocean,
        Savannah,
        Snow,
        Jungle,
        Mutated,
        Toxic,
        Artic,
        Midnight,
        Tundra,
        Decayed,
        Magma,
        Volcanic
    }
    public enum Planet_Subsurface_Biome
    {
        None,
        Barren_Underground,
        Desert_Subsurface,
        Underground_0a,
        Underground_0b,
        Underground_1a,
        Underground_1b,
        Ocean_Floor,
        Toxic_Ocean_Floor,
        Arctic_Ocean_Floor,
        Magma_Ocean_Floor,
    }
    public enum Planet_ShallowUnderground_Biome
    {
        None,
        Barren_Underground,
        Mini_Village,
        Mushrooms,
        Tarpit,
        Wilderness,
        Underground_0a,
        Underground_0b,
        Underground_1a,
        Underground_1b
    }
    public enum Planet_MidUnderground_Biome
    {
        None,
        Barren_Underground,
        Bone_Caves,
        Ice_Caves,
        Luminous_Caves,
        Stone_Caves,
        Underground_0c,
        Underground_1c,
        Underground_1d,
        Underground_3a,
        Underground_3b,
        Underground_3c,
        Underground_3d
    }
    public enum Planet_DeepUnderground_Biome
    {
        None,
        Barren_Underground,
        Cell_Caves,
        Flesh_Caves,
        Slime_Caves,
        Underground_0d,
        Underground_5a,
        Underground_5b,
        Underground_5c,
        Underground_5d
    }
    public enum Planet_Core_Biome
    {
        None,
        Barren_Underground,
        Blaststone,
        Garden,
        Magmarock,
        Obsidian
    }
    
}
