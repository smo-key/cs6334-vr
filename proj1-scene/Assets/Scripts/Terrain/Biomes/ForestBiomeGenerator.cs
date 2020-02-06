using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Terrain.Biomes
{
    public class ForestBiomeGenerator : BiomeGenerator
    {
        public const float ITEMS_PER_BLOCK_MEAN = 40f;
        public const float ITEMS_PER_BLOCK_STDDEV = 5f;

        public ForestBiomeGenerator()
        {

        }
        public void Generate(TerrainGenerator terrain, Vector2 blockLowerLeft, Vector2 blockUpperRight, float strength)
        {
            
        }
    }
}
