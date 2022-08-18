using Godot;
using System;

public class Simple_Camera2D : KinematicBody2D
{
    [Export] public float speed = 10000f;
    [Export] public float jump_speed = -20000f;
    [Export] public float gravity = 1000f;
    [Export] public float friction = 0.25f;
    [Export] public float acceleration = 0.25f;
    [Export] public float maxFallSpeed = 2000f;

    Camera2D Camera;

    public Vector2 velocity = new Vector2(0,5000);
    private Vector2 Snap = Vector2.Down * 8;

    public override void _Ready()
    {
        Camera = GetNode<Camera2D>("/root/World/Camera/Camera2D");

    }

    public void GetInput(float delta)
    {
        var dir = 0;

        if (Input.IsActionPressed("Right"))
            dir += 1;
        if (Input.IsActionPressed("Left"))
            dir -= 1;
        if (Input.IsActionJustPressed("Cliquegauche"))
            Position = GetGlobalMousePosition();

        if (Input.IsActionPressed("Cliquedroit"))
        {
            Vector2 m = Planet_Generator.TileMap_Front.WorldToMap(GetGlobalMousePosition());
            m = m / Planet_Generator.TileScale;
            Planet_Generator.CurrentPlanet.SetFrontTileAt(MathAddon.Mod((int)(m.x), Planet_Generator.Info.IntTileW), (int)(m.y), 0);
        }
        if (Input.IsActionJustPressed("Refreshp"))
        {
            Camera.Zoom *= 1.1f;
        }
        if (Input.IsActionJustPressed("Refreshm"))
        {
            Camera.Zoom /= 1.1f;
        }

        if (dir != 0)
            velocity.x = Mathf.Lerp(velocity.x, dir * speed * delta, acceleration);
        else
            velocity.x = Mathf.Lerp(velocity.x, 0, friction);
    }

    public override void _PhysicsProcess(float delta)
    {
        GetInput(delta);

        velocity.y += gravity * delta;

        velocity.y = Mathf.Min(velocity.y, maxFallSpeed);

        velocity = MoveAndSlideWithSnap(velocity, Snap, Vector2.Up);

        Snap = Vector2.Down * 8;

        if (Input.IsActionPressed("Up"))
        {
            if (IsOnFloor())
            { 
                velocity.y = jump_speed * delta;
                Snap = Vector2.Up;
            }
        }


        if (this.Position.x < 0)
        {
            this.Position = new Vector2(Planet_Generator.PlanetWidth + this.Position.x % Planet_Generator.PlanetWidth, this.Position.y);
        }
        if (this.Position.x >= Planet_Generator.PlanetWidth)
        {
            this.Position = new Vector2(this.Position.x % Planet_Generator.PlanetWidth, this.Position.y);
        }
    }
}
