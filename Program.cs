using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowEngine
{
    public static class Program
    {
        public static Texture2D Light;
        public static bool[][] TileData;
        public static XNAInterface xnaInterface;
        public static void Main(string[] args)
        {
            xnaInterface = new XNAInterface(Update, ShadowUpdate);
            Ground = xnaInterface.LoadTexture("ShadowEngine.Ground.png");
            Wall = xnaInterface.LoadTexture("ShadowEngine.Wall.png");
            Level = xnaInterface.LoadTexture("ShadowEngine.Level.png");
            Light = xnaInterface.LoadTexture("ShadowEngine.Light.png");

            TileData = new bool[xnaInterface.RenderWidth][];
            for (int i = 0; i < xnaInterface.RenderWidth; i++)
            {
                TileData[i] = new bool[xnaInterface.RenderHeight];
            }

            Color[] data = 

            for (int x = 0; x < xnaInterface.RenderWidth; x++)
            {
                for (int y = 0; y < xnaInterface.RenderHeight; y++)
                {
                    TileData[x][y] = Level.get
                }
            }

            xnaInterface.Run();
        }
        public static void Update()
        {
            xnaInterface.DrawSprite(testScene, 0, 0);
        }
        public static void ShadowUpdate()
        {
            xnaInterface.DrawSprite(roundShadow, 0, 0);
        }
    }
}
