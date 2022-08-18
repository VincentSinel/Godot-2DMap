using Godot;
using System;
using System.Collections.Generic;

public class DebugDrawing : Sprite
{
    KinematicBody2D MainCamera;
    private Font DefaultFont;

    public static int TotalTileDraw = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MainCamera = GetNode<KinematicBody2D>(GlobalData.CameraBodyPath);

        DefaultFont = (new Control()).GetFont("font");
    }
    public override void _Draw()
    {
        Rect2 view = MainCamera.GetViewportRect();
        Vector2 Position = MainCamera.Position;
        view.Position = MainCamera.Position;
        view.Position -= (view.Size / 2) / Planet_Generator.TileScale;

        Planet_Info.CurrentLayer layer = Planet_Generator.Info.GetCurrentLayer((int)(view.Position.y / (8 * Planet_Generator.TileScale)));
        DrawString(DefaultFont, view.Position + new Vector2(0, 40), "Layer : " + layer);
        DrawString(DefaultFont, view.Position + new Vector2(0, 60), "Position : " + Position.ToString());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Update();

        if (Input.IsActionJustPressed("SaveData"))
        {
            Planet_Binary.SavePlanet(Planet_Generator.CurrentPlanet);
        }
    }
}
