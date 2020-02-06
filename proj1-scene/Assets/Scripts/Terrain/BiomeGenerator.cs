using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Terrain
{
    public interface BiomeGenerator
    {
        void Generate(TerrainGenerator terrain, Vector2 blockLowerLeft, Vector2 blockUpperRight, float strength);
    }
}
