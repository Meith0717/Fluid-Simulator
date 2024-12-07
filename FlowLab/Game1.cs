﻿// Game1.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Objects.Layers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const string Title = "Flow Lab";

        private bool _active;
        private bool _safeToStart;

        public LayerManager LayerManager { get; private set; }
        public readonly GraphicsDeviceManager GraphicsManager;
        public readonly PersistenceManager PersistenceManager = new(Title);
        public readonly ConfigsManager ConfigsManager = new();
        private SpriteBatch _spriteBatch;
        private readonly ContentLoader _contentLoader;
        private readonly InputManager _inputManager = new();
        private readonly FrameCounter _frameCounter = new(200);
        private bool _ResolutionWasResized;

        public Game1()
        {
            Content.RootDirectory = "Content";
            GraphicsManager = new(this) { GraphicsProfile = GraphicsProfile.HiDef };
            _contentLoader = new(Content);

            // Manage if Window is selected or not
            Activated += delegate { _active = true; };
            Deactivated += delegate { _active = false; };

            // Window properties
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Title = Title;
            Window.ClientSizeChanged += delegate { _ResolutionWasResized = true; };
        }

        protected override void Initialize()
        {
            base.Initialize();
            LayerManager = new(this);
            IsFixedTimeStep = false;
            GraphicsManager.PreferMultiSampling = false;
            GraphicsManager.SynchronizeWithVerticalRetrace = false;
            GraphicsManager.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TextureManager.Instance.SetSpriteBatch(_spriteBatch);
            TextureManager.Instance.SetGraphicsDevice(GraphicsDevice);

            _contentLoader.LoadEssenzialContentSerial();
            _contentLoader.LoadContentAsync(ConfigsManager, () => _safeToStart = true, (ex) => throw ex);
        }

        private void StartMainMenu()
        {

            LayerManager.PopLayer();
            LayerManager.AddLayer(new SimulationLayer(this, _frameCounter));

            _safeToStart = false;
        }

        protected override void Update(GameTime gameTime)
        {
            // if (!_active) return;
            if (_safeToStart)
                StartMainMenu();

            MusicManager.Instance.Update();
            if (_ResolutionWasResized)
            {
                _ResolutionWasResized = false;
                LayerManager.OnResolutionChanged(gameTime);
            }

            _frameCounter.Update(gameTime);
            base.Update(gameTime);

            InputState inputState = _active ? _inputManager.Update(gameTime) : new([], "", Vector2.Zero);
            LayerManager.Update(gameTime, inputState);
            Exiting += delegate { LayerManager.Exit(); };
        }

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCounting();
            GraphicsDevice.Clear(Color.Black);
            LayerManager.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}