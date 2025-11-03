using System;

namespace StellarSurvivors.WorldGen.Utilities
{
    public static class Perlin
    {
        private static readonly int[] p = new int[512];
        
        // This is the "seed"
        public static void Init(int seed)
        {
            Random rand = new Random(seed);
            int[] permutation = new int[256];
            for (int i = 0; i < 256; i++)
            {
                permutation[i] = i;
            }

            // Shuffle the permutation array
            for (int i = 255; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                (permutation[i], permutation[n]) = (permutation[n], permutation[i]);
            }
            
            // Duplicate the permutation array
            for (int i = 0; i < 256; i++)
            {
                p[256 + i] = p[i] = permutation[i];
            }
        }

        public static double Noise(double x, double y = 0, double z = 0)
        {
            // Find unit cube that contains point.
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;
            int Z = (int)Math.Floor(z) & 255;

            // Find relative X,Y,Z of point in cube.
            x -= Math.Floor(x);
            y -= Math.Floor(y);
            z -= Math.Floor(z);

            // Compute fade curves for each of X,Y,Z.
            double u = Fade(x);
            double v = Fade(y);
            double w = Fade(z);

            // Hash coordinates of the 8 cube corners
            int A = p[X] + Y;
            int AA = p[A] + Z;
            int AB = p[A + 1] + Z;
            int B = p[X + 1] + Y;
            int BA = p[B] + Z;
            int BB = p[B + 1] + Z;

            // And add blended results from 8 corners of cube
            double res = Lerp(w, Lerp(v, Lerp(u, Grad(p[AA], x, y, z),
                                                Grad(p[BA], x - 1, y, z)),
                                        Lerp(u, Grad(p[AB], x, y - 1, z),
                                                Grad(p[BB], x - 1, y - 1, z))),
                                Lerp(v, Lerp(u, Grad(p[AA + 1], x, y, z - 1),
                                                Grad(p[BA + 1], x - 1, y, z - 1)),
                                        Lerp(u, Grad(p[AB + 1], x, y - 1, z - 1),
                                                Grad(p[BB + 1], x - 1, y - 1, z - 1))));
            
            // We need to scale the result to be in the 0.0 to 1.0 range.
            return (res + 1.0) / 2.0;
        }

        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        private static double Grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            // Convert low 4 bits of hash code into 12 gradient directions
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}
