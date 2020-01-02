﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameCreator.Plugins.MonoGame
{
    internal sealed class GameCreatorXnaGame : Game
    {
        public GraphicsDeviceManager Graphics { get; }
        public BasicEffect BasicEffect { get; private set; }

        public double Fps = 0;
        public double SleepTime = 0;

        public KeyboardState KeyboardState { get; set; }

        public event Action<Keys> KeyPressed;

        public GameCreatorXnaGame()
        {
            Graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            BasicEffect = new BasicEffect(GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Load?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> Load;
        public event EventHandler<EventArgs> GameCreatorDraw;

        protected override void Update(GameTime gameTime)
        {
            UpdateInput();
            
            base.Update(gameTime);
        }

        private void UpdateInput()
        {
            var oldState = KeyboardState;
            KeyboardState = Keyboard.GetState();

            foreach (var key in KeyboardState.GetPressedKeys())
            {
                // oldState is a struct, so by default, no keys are pressed.
                if (!oldState.IsKeyDown(key))
                {
                    KeyPressed?.Invoke(key);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (gameTime.ElapsedGameTime.TotalSeconds > 0.0)
                Fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
            else
                Fps = 0.0;

            SleepTime = Math.Max(0.0, SleepTime - gameTime.ElapsedGameTime.TotalSeconds);

            if (SleepTime == 0.0)
            {
                GameCreatorDraw?.Invoke(this, EventArgs.Empty);
            }
            base.Draw(gameTime);
        }
        
        public readonly List<Texture2D> ManagedTextures = new List<Texture2D>();

        protected override void UnloadContent()
        {
            ManagedTextures.ForEach(tex => tex.Dispose());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BasicEffect?.Dispose();
                Graphics?.Dispose();
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            // Workaround for hanging on mac: http://community.monogame.net/t/window-freezes-on-exit-for-linux/8391
            Process.GetCurrentProcess().Kill();
            base.OnExiting(sender, args);
        }
    }
}