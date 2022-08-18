using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MathAddon
{
    /// <summary>
    /// Return true mathematic modulo of two number (no negative value)
    /// </summary>
    /// <param name="a">dividande</param>
    /// <param name="b">divisor</param>
    /// <returns>a mod b (only positive)</returns>
    public static int Mod(int a, int b)
    {
        return (Math.Abs(a * b) + a) % b;
    }
    /// <summary>
    /// Return a random value from 2D position
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <returns>Float random</returns>
    public static float Random(float x, float y)
    {
        return Fract(Mathf.Sin((new Vector2(x, y)).Dot(
            new Vector2(12.9898f, 78.233f))) * 43758.5453123f);
    }
    /// <summary>
    /// Return fract part of a float
    /// </summary>
    /// <param name="x">float value</param>
    /// <returns>fractionnal part</returns>
    public static float Fract(float x)
    {
        return x - Mathf.Floor(x);
    }

    /// <summary>
    /// Custom function used as coefficient in map generation.
    /// See : https://www.desmos.com/calculator/cyznkahund
    /// latex function : -8\cdot\operatorname{mod}\left(x,\ 0.5\right)^{3}\ +\ 6\cdot\operatorname{mod}\left(x,\ 0.5\right)^{2}\ \ +\ 0.5\cdot\operatorname{floor}\left(\frac{\operatorname{mod}\left(x,\ 1\right)}{0.5}\right)
    /// </summary>
    /// <param name="x">x value</param>
    /// <returns>new value</returns>
    public static float CustomGenFunction1(float x)
    {
        var _x = x % 0.5f;
        return (-8 * Mathf.Pow(_x, 3) + 6 * _x * _x) + (x >= 0.5f ? 0.5f : 0);
    }

    /// <summary>
    /// Convert boolean array (max size 8) to a single byte
    /// </summary>
    /// <param name="source">Boolean array</param>
    /// <returns>Byte representing the boolean array</returns>
    public static byte ConvertBoolArrayToByte(bool[] source)
    {
        if (source.Length > 8)
            throw new Exception("Bool array is to long. Only 8 value is allowed");
        
        byte result = 0;
        // This assumes the array never contains more than 8 elements!
        int index = 8 - source.Length;

        // Loop through the array
        foreach (bool b in source)
        {
            // if the element is 'true' set the bit at that position
            if (b)
                result |= (byte)(1 << (7 - index));

            index++;
        }

        return result;
    }
    /// <summary>
    /// Convert a byte to a array of boolean
    /// </summary>
    /// <param name="b">Byte</param>
    /// <returns>Boolean array representing the byte</returns>
    public static bool[] ConvertByteToBoolArray(byte b)
    {
        // prepare the return result
        bool[] result = new bool[8];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 8; i++)
            result[i] = (b & (1 << i)) != 0;

        // reverse the array
        Array.Reverse(result);

        return result;
    }

}