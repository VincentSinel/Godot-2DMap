using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Create planet type of forest
/// </summary>
internal class Planet_Info_Forest : Planet_Info
{

    public Planet_Info_Forest()
    {
        Type = Planet_Type.Forest;
    }

    // Create noise function for map generation
    private OpenSimplexNoise _noise_Ground1 = new OpenSimplexNoise();
    private OpenSimplexNoise _noise_Ground2 = new OpenSimplexNoise();
    private OpenSimplexNoise _noise_Texture = new OpenSimplexNoise();
    private OpenSimplexNoise _noise_Cave = new OpenSimplexNoise();

    private float cavecoef = 0.02f;
    private float cavecoefsize = 0.2f;


    /// <summary>
    /// Initialise value for noise function
    /// </summary>
    internal override void Refresh_SpecificData()
    {
        int randint1 = (int)(MathAddon.Random(X, Y) * int.MaxValue);
        int randint2 = (int)(MathAddon.Random(X, Z) * int.MaxValue);
        int randint3 = (int)(MathAddon.Random(Y, Z) * int.MaxValue);
        int randint4 = (int)(MathAddon.Random(randint1, randint2) * randint3);

        _noise_Texture.Seed = randint1; // (int)DateTime.Now.Ticks;
        _noise_Texture.Octaves = 4;
        _noise_Texture.Period = 2000.0f;
        _noise_Texture.Persistence = 0.8f;

        _noise_Ground1.Seed = randint2; // (int)DateTime.Now.Ticks + 2;
        _noise_Ground1.Octaves = 8;
        _noise_Ground1.Period = 300f;
        _noise_Ground1.Persistence = 0.6f;

        _noise_Ground2.Seed = randint3; // (int)DateTime.Now.Ticks + 1;
        _noise_Ground2.Octaves = 2;
        _noise_Ground2.Period = 50f;
        _noise_Ground2.Persistence = 0.5f;

        _noise_Cave.Seed = randint4; // (int)DateTime.Now.Ticks;
        _noise_Cave.Octaves = 8;
        _noise_Cave.Period = 20.0f;
        _noise_Cave.Persistence = 0.5f;
    }


    internal override ushort[] GT_Subsurface(int x, int y)
    {
        return GT_Surface(x, y);
    }
    internal override ushort[] GT_ShallowUnderground(int x, int y)
    {
        return GT_Surface(x, y);
    }
    internal override ushort[] GT_MidUnderground(int x, int y)
    {
        return GT_Surface(x, y);
    }
    internal override ushort[] GT_DeepUnderground(int x, int y)
    {
        return GT_Surface(x, y);
    }
    internal override ushort[] GT_Core(int x, int y)
    {
        return GT_Surface(x, y);
    }
    internal override ushort[] GT_Surface(int x, int y)
    {
        ushort[] tiles = new ushort[3];

        Vector2 cp = GetCPosition(x);

        float groundnoise1 = (_noise_Ground1.GetNoise2d(cp.x, cp.y) + 1) / 2 * 0.9f;
        float groundnoise2 = (_noise_Ground2.GetNoise2d(cp.x, cp.y) + 1) / 2 * 0.1f;
        float c = (groundnoise1 + groundnoise2) * MathAddon.CustomGenFunction1(groundnoise1);
        float surfacey = Layer_Surface - Layer_SurfaceSize * c;
        if (y < surfacey)
        {
            return new ushort[] { 0, 0, 0 };
        }
        else if(y - surfacey < 1)
        {
            float coef = (_noise_Texture.GetNoise3d(cp.x * 10, cp.y * 10, y * 10) + 1) / 2;

            tiles[1] = (ushort)(1 + (coef * 7 + MathAddon.Random(x, y) + 5.5) % 7);
        }
        else if (y - surfacey < 20)
        {
            float coef = (_noise_Texture.GetNoise3d(cp.x * 10, cp.y * 10, y * 10) + 1) / 2;

            tiles[1] = (ushort)(8 + (coef * 7 + MathAddon.Random(x, y) + 5.5) % 7);
        }
        else
        {
            float coef = (_noise_Texture.GetNoise3d(cp.x * 10, cp.y * 10, y * 10) + 1) / 2;

            tiles[1] = (ushort)(8 + (coef * 21 + MathAddon.Random(x, y) + 5.5) % 21);
        }

        tiles[0] = tiles[1];
        if (Cave( y, cp))
           tiles[0] = 0;

        return tiles;
    }

    /// <summary>
    /// This function tell if a position is a cave or not
    /// </summary>
    /// <param name="y">Y position</param>
    /// <param name="cp">Position around the planet</param>
    /// <returns></returns>
    private bool Cave(int y, Vector2 cp)
    {
        float c = cavecoef / 2f * (1f - Mathf.Pow(Mathf.E, -(y - Layer_Atmosphere) * 0.005f));

        float n = (_noise_Cave.GetNoise3d(cp.x * cavecoefsize, cp.y * cavecoefsize, y * cavecoefsize) + 1) / 2;
        if (n < 0.5f + c && n > 0.5f - c)
            return true;
        else
        {
            c = cavecoef * 1.5f / 2f * (1f - Mathf.Pow(Mathf.E, -(y - Layer_Surface) * 0.005f));
            n = (_noise_Cave.GetNoise3d(cp.x * cavecoefsize * 2f, cp.y * cavecoefsize * 2f, (y + TileH * 200) * cavecoefsize * 2f) + 1) / 2;
            if (n < 0.5f + c && n > 0.5f - c)
                return true;
            else
            {
                c = cavecoef * 2 / 2f * (1f - Mathf.Pow(Mathf.E, -(y - Layer_Subsurface) * 0.005f));
                n = (_noise_Cave.GetNoise3d(cp.x * cavecoefsize * 4f, cp.y * cavecoefsize * 4f, (y + TileH * 20000) * cavecoefsize * 4f) + 1) / 2;
                if (n < 0.5f + c && n > 0.5f - c)
                    return true;
                else
                {
                    return false;
                }
            }
        }
    }
}
