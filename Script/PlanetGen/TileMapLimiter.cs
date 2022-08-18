using Godot;

public class TileMapLimiter : TileMap
{
    static Camera2D Camera;
    static KinematicBody2D Camera_Body;
    static Vector2 ViewportSize;

    public Planet_Info Info { get { return Planet_Generator.CurrentPlanet.Info; } }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Camera = GetNode<Camera2D>(GlobalData.CameraPath);
        Camera_Body = GetNode<KinematicBody2D>(GlobalData.CameraBodyPath);

        UpdateCameraSize();
    }

    /// <summary>
    /// Must be call on camera zoom or size change
    /// </summary>
    public void UpdateCameraSize()
    {
        ViewportSize = WorldToMap(Camera_Body.GetViewportRect().Size / Camera.Zoom.x);
        GD.Print(ViewportSize.ToString());
    }

    /// <summary>
    /// Change SetCell function to make planet warp in x axis
    /// Generate multiple tile in tile map from camera viewport
    /// </summary>
    public new void SetCell(int x, int y, int tile, bool flipX = false, bool flipY = false, bool transpose = false, Vector2? autotileCoord = null)
    {
        base.SetCell(x, y, tile, flipX, flipY, transpose, autotileCoord);
        int ux = x - Info.IntTileW;
        while (ux >= -ViewportSize.x)
        {
            base.SetCell(ux, y, tile, flipX, flipY, transpose, autotileCoord);
            ux -= Info.IntTileW;
        }
        ux = x + Info.IntTileW;
        while (ux <= Info.IntTileW + ViewportSize.x)
        {
            base.SetCell(ux, y, tile, flipX, flipY, transpose, autotileCoord);
            ux += Info.IntTileW;
        }
    }

}
