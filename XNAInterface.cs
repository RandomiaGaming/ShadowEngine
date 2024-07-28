using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ShadowEngine
{
    public delegate void UpdateCallback();
    public delegate void ShadowCallback();
    public sealed class XNAInterface : Game
    {
        private static Assembly assembly = typeof(XNAInterface).Assembly;

        public readonly int RenderWidth = 256;
        public readonly int RenderHeight = 144;

        public int CameraX = 0;
        public int CameraY = 0;

        public bool ProfilerEnabled = false;
        private bool _F11LastFrame; //Fullscreen
        private bool _F1LastFrame; //Save Screenshot
        private bool _F2LastFrame; //Toggle Profiler

        public GraphicsDeviceManager MainGraphicsDeviceManager = null;
        public SpriteBatch MainSpriteBatch = null;
        public RenderTarget2D MainRenderTarget = null;
        public RenderTarget2D ShadowRenderTarget = null;

        private UpdateCallback _updateCallback;
        private ShadowCallback _shadowCallback;

        private Stopwatch _gameTimer;
        private long _ticksLastFrame;
        public XNAInterface(UpdateCallback updateCallback, ShadowCallback shadowCallback)
        {
            _updateCallback = updateCallback;
            _shadowCallback = shadowCallback;

            //this.InactiveSleepTime = default;
            this.TargetElapsedTime = new System.TimeSpan(10000000 / 120);
            this.MaxElapsedTime = new System.TimeSpan(10000000 / 120);
            this.IsFixedTimeStep = false;
            this.IsMouseVisible = true;

            MainGraphicsDeviceManager = new GraphicsDeviceManager(this);
            MainGraphicsDeviceManager.GraphicsProfile = GraphicsProfile.Reach;
            MainGraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            MainGraphicsDeviceManager.HardwareModeSwitch = false;
            MainGraphicsDeviceManager.IsFullScreen = false;
            MainGraphicsDeviceManager.PreferHalfPixelOffset = false;
            MainGraphicsDeviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
            MainGraphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight | DisplayOrientation.Portrait | DisplayOrientation.PortraitDown | DisplayOrientation.Unknown | DisplayOrientation.Default;
            //GraphicsDeviceManager.PreferredBackBufferWidth = default;
            //GraphicsDeviceManager.PreferredBackBufferHeight = default;
            MainGraphicsDeviceManager.ApplyChanges();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.Indices = default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.ResourcesLost = false;
            //GraphicsDevice.GraphicsDebug = default;
            //GraphicsDevice.Metrics = default;
            //GraphicsDevice.ScissorRectangle = default;
            //GraphicsDevice.Viewport = default;

            Window.AllowAltF4 = true;
            Window.AllowUserResizing = true;
            Window.IsBorderless = false;
            Window.Position = new Point(GraphicsDevice.Adapter.CurrentDisplayMode.Width / 4, GraphicsDevice.Adapter.CurrentDisplayMode.Height / 4);
            Window.Title = "ShadowEngine - 0.1";

            MainSpriteBatch = new SpriteBatch(GraphicsDevice /*, default*/);
            MainSpriteBatch.Name = "ShadowEngine Main Sprite Batch";
            MainSpriteBatch.Tag = null;

            MainRenderTarget = new RenderTarget2D(GraphicsDevice, RenderWidth, RenderHeight, false, SurfaceFormat.Color, DepthFormat.None);
            MainRenderTarget.Name = "ShadowEngine Main Render Target";
            MainRenderTarget.Tag = null;

            ShadowRenderTarget = new RenderTarget2D(GraphicsDevice, RenderWidth, RenderHeight, false, SurfaceFormat.Color, DepthFormat.None);
            ShadowRenderTarget.Name = "ShadowEngine Shadow Render Target";
            ShadowRenderTarget.Tag = null;

            _gameTimer = new Stopwatch();
            _gameTimer.Start();
        }
        protected sealed override void Draw(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool F11 = keyboardState.IsKeyDown(Keys.F11);
            bool F2 = keyboardState.IsKeyDown(Keys.F2);
            bool F1 = keyboardState.IsKeyDown(Keys.F1);
            if (F11 && !_F11LastFrame)
            {
                MainGraphicsDeviceManager.ToggleFullScreen();
            }
            if (F2 && !_F2LastFrame)
            {
                ProfilerEnabled = !ProfilerEnabled;
            }

            long ticksNow = _gameTimer.ElapsedTicks;
            long TPF = ticksNow - _ticksLastFrame;
            double FPS = 10000000.0 / TPF;
            if (ProfilerEnabled)
            {
                Console.WriteLine($"TPF {TPF} FPS {FPS}");
            }
            _ticksLastFrame = ticksNow;

            GraphicsDevice.SetRenderTarget(MainRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            MainSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, null);
            _updateCallback.Invoke();
            MainSpriteBatch.End();

            GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
            GraphicsDevice.Clear(Color.Black);
            MainSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, null);
            _shadowCallback.Invoke();
            MainSpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            MainSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, null);
            MainSpriteBatch.Draw(MainRenderTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Rectangle(0, 0, RenderWidth, RenderHeight), Color.White);
            MainSpriteBatch.Draw(ShadowRenderTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Rectangle(0, 0, RenderWidth, RenderHeight), new Color(Color.White, 0.75f));
            MainSpriteBatch.End();

            if (F1 && !_F1LastFrame)
            {
                string screenshotPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Screenshot.png";
                SavePNG(MainRenderTarget, screenshotPath);
                Console.WriteLine($"Screenshot saved to \"{screenshotPath}\".");
            }

            _F11LastFrame = F11;
            _F2LastFrame = F2;
            _F1LastFrame = F1;
        }
        public void SavePNG(RenderTarget2D target, string path)
        {
            FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            target.SaveAsPng(fileStream, target.Width, target.Height);
            fileStream.Close();
            fileStream.Dispose();
        }
        public void SavePNG(Texture2D target, string path)
        {
            FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            target.SaveAsPng(fileStream, target.Width, target.Height);
            fileStream.Close();
            fileStream.Dispose();
        }
        public void DrawSprite(Texture2D sprite, int xWorld, int yWorld)
        {
            int xScreen = xWorld - CameraX;
            int yScreen = yWorld - CameraY;
            if (xScreen < RenderWidth && yScreen < RenderHeight && xScreen + sprite.Width >= 0 && yScreen + sprite.Height >= 0)
            {
                int yScreenInverted = RenderHeight - (yScreen + sprite.Height);
                MainSpriteBatch.Draw(sprite, new Vector2(xScreen, yScreenInverted), Color.White);
            }
        }
        public Texture2D LoadTexture(string embeddedResourceName)
        {
            Stream embeddedResourceStream = assembly.GetManifestResourceStream(embeddedResourceName);
            Texture2D output = Texture2D.FromStream(GraphicsDevice, embeddedResourceStream);
            embeddedResourceStream.Dispose();
            return output;
        }
    }
}