/* This implementation is based on Stefan Gustavson (stegu@itn.liu.se) 
 * simplex noise, located on:
 *      http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/simplexnoise1234.cpp
 *      2013-06-18 | 16:17 | 19K	 
 *      Under GPL 2 or later
 *      
 * Changes:
 *      Removed 2D/3D/4D
 *      Conversion to C#
 *      Stateful interface
 */

namespace XLoad.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class Noise
    {
        public  int     Seed            { get; private set; }
        public  float   Scale           { get; private set; }
        private Random  Random          { get; set; }
        private byte[]  RandomTable     { get; set; }
        private int     CurrentIndex    { get; set; }

        public Noise(int seed, float scale)
        {
            this.CurrentIndex   = 0;
            this.Seed           = seed;
            this.Scale          = scale;
            this.Random         = new Random(seed);
            this.RandomTable    = new byte[512];

            this.Random.NextBytes(RandomTable);
        }

        // Stateful
        public IEnumerable<byte> NextValue()
        {
            yield return CalculateNextValue();
        }

        public byte CalculateNextValue()
        {
            byte value = CalculateNextValueFrom(this.CurrentIndex, this.Scale);

            this.CurrentIndex++;

            return value;
        }

        public byte[] CalculateNextValues(int width)
        {
            var values = CalculateNextValuesFrom(this.CurrentIndex, width, this.Scale);

            this.CurrentIndex += width;

            return values;
        }

        public void Reset()
        {
            this.CurrentIndex = 0;
        }

        // Stateless except seed
        public byte CalculateNextValueFrom(int from, float scale)
        {
            return (byte)(Generate(from * scale) * 128 + 128);
        }

        public byte[] CalculateNextValuesFrom(int from, int width, float scale)
        {
            var values = new byte[width];

            for (var i = 0; i < width; i++)
            {
                values[i] = CalculateNextValueFrom(i + from, scale);
            }

            return values;
        }

        // Stateless
        private float Generate(float x)
        {
            var lower = FastFloor(x);
            var upper = lower + 1;
            var fracP = x - lower;
            var fracN = fracP - 1.0f;

            var t0 = 1.0f - fracP * fracP;
            t0 *= t0;
            var n0 = t0 * t0 * Gradient(RandomTable[lower & 0xff], fracP);

            var t1 = 1.0f - fracN * fracN;
            t1 *= t1;
            var n1 = t1 * t1 * Gradient(RandomTable[upper & 0xff], fracN);

            return 0.395f * (n0 + n1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Gradient(int hash, float dist)
        {
            var h = hash & 15;
            var grad = 1.0f + (h & 7);

            if ((h & 8) != 0)
            {
                grad = -grad;
            }

            return (grad * dist);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FastFloor(float x)
        {
            return x > 0 ? (int)x : (int)(x - 1);
        }
    }
}
