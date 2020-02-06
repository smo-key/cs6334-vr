using System;

namespace Assets.Scripts.Util
{
    public static class RandomUtil
    {
        private static int seed = DateTime.Now.Millisecond;
        private static Random rand = new System.Random(seed);

        public static void SetSeed(int seed)
        {
            RandomUtil.seed = seed;
            rand = new Random(seed);
        }

        public static float RandomNormal(float mean, float stdDev)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return (float)randNormal;
        }

        public static bool RandomBoolean(float probTrue)
        {
            int prob = rand.Next(100000);
            return prob < (int)(probTrue * 100000);
        }

        public static float RandomFloat(float min, float max)
        {
            return (float)(rand.NextDouble() * (max - min) + min);
        }

        public static int RandomInt(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static int RandomChoice(float[] choices)
        {
            float f = RandomFloat(0, 1);
            for (int i = 0; i < choices.Length; i++)
            {
                f -= choices[i];
                if (f <= 0) return i;
            }
            return choices.Length - 1;
        }

        public static int GetSeed() => seed;
    }
}
