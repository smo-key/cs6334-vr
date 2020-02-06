using Assets.Scripts.Terrain.Biomes;
using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Terrain 
{
    [System.Serializable]
    public class Biome
    {
        public string name;
        public float biomeMapHeight = 0.0f;

        public AnimationCurve heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public float biomeHeightScale = 1.0f;
        public float biomeHeightOffset = 0.0f;
        public float biomeDetailScale = 1.0f;

        public Texture2D texture;
        public float textureScale = 1.0f;

        public BiomeObject[] objects = new BiomeObject[] { };
        public float meanObjectsPerBlock = 30f;
        public float stdDevObjectsPerBlock = 10f;

        [Serializable]
        public class BiomeObject
        {
            public string name;

            [Range(0, 1)]
            public float probability;
            public GameObject[] objects;
            public bool selectConsistent;

            public float meanScale = 1.0f;
            public float stdDevScale = 0.1f;

            [NonSerialized]
            public int selectedIndex = -1;
        }

        public void Generate(TerrainGenerator terrain, Vector2 blockLowerLeft, Vector3 blockUpperRight, float strength)
        {
            if (objects.Length == 0) return;

            // preselect this block's objects
            foreach (var obj in objects)
            {
                if (obj.selectConsistent)
                {
                    obj.selectedIndex = RandomUtil.RandomInt(0, obj.objects.Length);
                }
            }

            // generate choices list
            float[] choices = objects.Select(o => o.probability).ToArray();

            //generate items
            int itemsToGenerate = Mathf.Clamp(Mathf.FloorToInt(strength * RandomUtil.RandomNormal(meanObjectsPerBlock, stdDevObjectsPerBlock)), 0, 2 * (int)meanObjectsPerBlock);
            for (int i = 0; i < itemsToGenerate; i++)
            {
                // select an item at random
                int biomeObjectChoice = RandomUtil.RandomChoice(choices);
                BiomeObject biomeObject = objects[biomeObjectChoice];

                // select a location at random
                float x = RandomUtil.RandomFloat(blockLowerLeft.x, blockUpperRight.x);
                float y = RandomUtil.RandomFloat(blockLowerLeft.y, blockUpperRight.y);

                // select object from list
                int objectIndex = biomeObject.selectConsistent ? biomeObject.selectedIndex : RandomUtil.RandomInt(0, biomeObject.objects.Length);
                GameObject template = biomeObject.objects[objectIndex];

                // scale object
                float scale = RandomUtil.RandomNormal(biomeObject.meanScale, biomeObject.stdDevScale);
                GameObject obj = terrain.PlaceObjectAt(template, x, y, 0.0f);
                obj.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}