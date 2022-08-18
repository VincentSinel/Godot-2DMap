using Godot;
using System;

public class Planet_Generator : Node2D
{
    //Game Object
    static KinematicBody2D Camera_Body;
    static Camera2D Camera;
    public static TileMapLimiter TileMap_Collision;
    public static TileMapLimiter TileMap_Front;
    public static TileMapLimiter TileMap_Back;
    public static Planet CurrentPlanet;

    // Private Data
    static private Rect2 previousPos = new Rect2(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
    static private float planetWidth = 10;
    static private Vector2 CameraView_S;
    static private Vector2 CameraView_E;
    static private bool InitialisationComplet = false;

    // Public Data
    public static Planet_Generator Instance;
    public static float TileScale = 2f; // Zoom du tilemap
    public static float PlanetWidth
    {
        get
        {
            return planetWidth * TileScale;
        }
    }
    public static int ChunkSize
    {
        get
        {
            return ChunkData.ChunkSize;
        }
    }
    public static float ScaleChunkSize
    {
        get
        {
            return ChunkSize * TileScale;
        }
    }
    public static Planet_Info Info
    {
        get
        {
            return CurrentPlanet.Info;
        }
    }

    /// <summary>
    /// Executed at tilemap creation
    /// </summary>
    public override void _Ready()
    {
        Instance = this;
        Planet_Generator.Ready();
    }

    /// <summary>
    /// Executed every frame
    /// </summary>
    /// <param name="delta">Time since last frame</param>
    public override void _Process(float delta)
    {
        if (InitialisationComplet)
            Process(delta);
    }

    // Since Planet_Generator need to be unique, everythink can be static

    /// <summary>
    /// Executed at tilemap creation
    /// </summary>
    public static async void Ready()
    {
        Instance.Scale = new Vector2(TileScale, TileScale);

        Planet_Info info = new Planet_Info_Forest();
        info.X = 0;
        info.Y = 0;
        info.Z = 0;
        info.Size = Planet_Info.Planet_Size.Large;
        info.RefreshData();

        CurrentPlanet = new Planet(info);
        await Planet_Binary.LoadPlanet(CurrentPlanet);
        planetWidth = Info.TileW * 8;

        Camera_Body = Instance.GetNode<KinematicBody2D>(GlobalData.CameraBodyPath);
        Camera = Instance.GetNode<Camera2D>(GlobalData.CameraPath);

        TileMap_Collision = Instance.GetNode<TileMapLimiter>("TileMap_Collision");
        TileMap_Back = Instance.GetNode<TileMapLimiter>("TileMap_Back");
        TileMap_Front = Instance.GetNode<TileMapLimiter>("TileMap_Front");
        InitialisationComplet = true;
    }

    /// <summary>
    /// Executed every frame
    /// </summary>
    /// <param name="delta">Time since last frame</param>
    public static void Process(float delta)
    {
        Rect2 view = Camera_Body.GetViewportRect();
        view.Position = Camera_Body.Position;
        Vector2 pos_s = TileMap_Front.WorldToMap(view.Position);
        Vector2 pos_e = TileMap_Front.WorldToMap(view.Size);// * Camera.Zoom.x); // Permet de prendre en compte le zoom de la camera pour definir la zone a charger
        CameraView_S = pos_s - pos_e / 2;
        CameraView_E = pos_s + pos_e / 2;

        byte sx = (byte)MathAddon.Mod((int)Math.Floor(CameraView_S.x / ScaleChunkSize), Info.W);
        byte sy = (byte)Math.Max(0, (int)Math.Floor(CameraView_S.y / ScaleChunkSize));
        byte ex = (byte)MathAddon.Mod((int)Math.Floor(CameraView_E.x / ScaleChunkSize), Info.W);
        byte ey = (byte)Math.Min(Info.H, (int)Math.Floor(CameraView_E.y / ScaleChunkSize) + 1);

        if (previousPos.Position.x != sx || previousPos.Position.y != sy ||
            previousPos.Size.x != ex || previousPos.Size.y != ey ||
            CurrentPlanet.RecheckZone)
        {
            //GD.Print(sx," ", ex, " ", sy, " ", ey);   // DEBUG
            CurrentPlanet.ClearChunkOutsideBox(sx, ex, sy, ey);
            CurrentPlanet.GenerateChunkBetween(sx, ex, sy, ey); 

            previousPos.Position = new Vector2(sx, sy);
            previousPos.Size = new Vector2(ex, ey);
        }
    }
}
