﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using System.Windows;
using Microsoft.Xna.Framework.Content;
using C3DE.Inputs;
using Microsoft.Xna.Framework;
using C3DE.Components.Renderers;
using C3DE.Prefabs;
using C3DE.Materials;
using C3DE.Utils;
using C3DE.Components.Lights;
using System.Diagnostics;
using C3DE.Editor.Components;

namespace C3DE.Editor.MonoGameBridge
{
    public class C3DEGameHost : D3D11Host, IServiceProvider
    {
        private GraphicsDeviceService graphicsService;

        private GameTime _gameTime;
        private GameServiceContainer services;
        private List<IUpdateable> updateableComponents;
        private SpriteBatch spriteBatch;
        private Renderer renderer;
        private ContentManager content;
        private Scene scene;

        protected override void Initialize()
        {
            base.Initialize();

            _gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
            
            updateableComponents = new List<IUpdateable>();
            graphicsService = new GraphicsDeviceService(graphicsDevice);

            services = new GameServiceContainer();
            services.AddService<IGraphicsDeviceService>(graphicsService);

            content = new ContentManager(this);
            content.RootDirectory = "Content";
            scene = new Scene(content);

            var mouseComponent = new MouseComponent(null);
            updateableComponents.Add(mouseComponent);

            var timerComponent = new Time(null);
            updateableComponents.Add(timerComponent);

            Application.Content = content;
            Application.GraphicsDevice = GraphicsDevice;

            Input.Mouse = mouseComponent;

            Screen.Setup((int)ActualWidth, (int)ActualHeight, null, null);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderer = new Renderer(GraphicsDevice);
            renderer.LoadContent(content);
            scene.Initialize();

            PopulateSceneWithThings();
        }

        private void PopulateSceneWithThings()
        {
            var camera = new CameraPrefab("camera", scene);
            camera.AddComponent<EditorOrbitController>();

            var lightPrefab = new LightPrefab("lightPrefab", LightType.Directional, scene);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.DiffuseColor = Color.Violet;
            lightPrefab.Light.Direction = new Vector3(-1, 1, -1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 1);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain", scene);
            terrain.Randomize();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            camera.Transform.Position = new Vector3(-terrain.Width >> 1, 2, -terrain.Depth / 2);

            renderer.Skybox.Generate(GraphicsDevice, content, new string[6] 
            {
                "Textures/Skybox/bluesky/px",   
                "Textures/Skybox/bluesky/nx",
                "Textures/Skybox/bluesky/py",
                "Textures/Skybox/bluesky/ny",
                "Textures/Skybox/bluesky/pz",
                "Textures/Skybox/bluesky/nz"
            });
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Screen.Setup((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height, null, null);
            renderer.NeedsBufferUpdate = true;
        }

        protected override void Update(Stopwatch timer)
        {
            base.Update(timer);

            _gameTime.ElapsedGameTime = timer.Elapsed;
            _gameTime.TotalGameTime += timer.Elapsed;

            foreach (var component in updateableComponents)
                component.Update(_gameTime);

            scene.Update();
        }

        protected override void Draw(RenderTarget2D renderTarget)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);
            renderer.RenderEditor(scene, scene.MainCamera, renderTarget);
        }

        public object GetService(Type serviceType)
        {
            return services.GetService(serviceType);
        }
    }
}