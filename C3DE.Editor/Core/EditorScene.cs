﻿using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Editor.Core.Components;
using C3DE.Editor.GameComponents;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Editor
{
    public class EditorScene : Scene
    {
        private StandardMaterial m_DefaultMaterial;
        private List<string> m_AddList;
        private List<GameObject> m_RemoveList;

        public event Action<GameObject[]> SceneInitialized = null;

        public EditorScene() : base("3D Editor")
        {
            m_AddList = new List<string>();
            m_RemoveList = new List<GameObject>();
        }

        public void Reset()
        {
            Unload();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            // Add a camera with a FPS controller
            var cameraGo = AddGameObject("Camera"); GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            var camera = cameraGo.GetComponent<Camera>();
            camera.Setup(new Vector3(0, 2, 5), Vector3.Forward, Vector3.Up);
            camera.Far = 10000;
            camera.AddComponent<EditorController>();

            // And a light
            var lightGo = AddGameObject("Directional");
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
            var directionalLight = lightGo.GetComponent<Light>();

            // Sun Flares
            var content = Application.Content;
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };

            var direction = directionalLight.Direction;
            var sunflares = directionalLight.AddComponent<LensFlare>();
            sunflares.Setup(glowTexture, flareTextures);

            // Skybox
            var blueSkybox = new string[6]
            {
                "Textures/Skybox/bluesky/px",
                "Textures/Skybox/bluesky/nx",
                "Textures/Skybox/bluesky/py",
                "Textures/Skybox/bluesky/ny",
                "Textures/Skybox/bluesky/pz",
                "Textures/Skybox/bluesky/nz"
            };

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, blueSkybox);

            // Grid
            var grid = new GameObject("Grid");
            grid.Tag = EditorGame.EditorTag;
            grid.AddComponent<Grid>();

            m_DefaultMaterial = new StandardMaterial();
            m_DefaultMaterial.MainTexture = GraphicsHelper.CreateTexture(Color.WhiteSmoke, 1, 1);

            SceneInitialized?.Invoke(new[] { cameraGo, lightGo });
        }

        public override void Update()
        {
            base.Update();

            if (m_RemoveList.Count > 0)
            {
                foreach (var sceneObject in m_RemoveList)
                    RemoveGameObject(sceneObject);

                m_RemoveList.Clear();
            }

            if (m_AddList.Count > 0)
            {
                foreach (var type in m_AddList)
                    AddGameObject(type);

                m_AddList.Clear();
            }
        }

        #region Add

        public void AddComponent(string name)
        {
        }

        public GameObject AddGameObject(string type)
        {
            GameObject gameObject = null;

            switch (type)
            {
                case "Cube": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Cube, m_DefaultMaterial); break;
                case "Cylinder": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Cylinder, m_DefaultMaterial); break;
                case "Quad": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Quad, m_DefaultMaterial); break;
                case "Plane": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Plane, m_DefaultMaterial); break;
                case "Pyramid": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Pyramid, m_DefaultMaterial); break;
                case "Sphere": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Sphere, m_DefaultMaterial); break;
                case "Torus": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Torus, m_DefaultMaterial); break;

                case "Terrain":
                    gameObject = GameObjectFactory.CreateTerrain();
                    var terrain = gameObject.GetComponent<Terrain>();
                    terrain.Flatten();
                    terrain.Renderer.Material = m_DefaultMaterial;
                    break;

                case "Water":
                    gameObject = GameObjectFactory.CreateWater(GraphicsHelper.CreateTexture(Color.AliceBlue, 1, 1), GraphicsHelper.CreateRandomTexture(32), Vector3.One);
                    break;

                case "Directional": gameObject = CreateLight(type, LightType.Directional); break;
                case "Point": gameObject = CreateLight(type, LightType.Point); break;
                case "Spot": gameObject = CreateLight(type, LightType.Spot); break;

                case "Camera":
                    gameObject = GameObjectFactory.CreateCamera();
                    var cameraRenderer = gameObject.AddComponent<EditorIconRenderer>();
                    cameraRenderer.Setup("Camera_Icon");
                    cameraRenderer.AddComponent<SphereCollider>();
                    break;
                default: break;
            }

            gameObject.Name = type;

            AddGameObject(gameObject);

            return gameObject;
        }

        public void AddGameObject(GameObject sceneObject)
        {
            if (sceneObject == null)
                return;

            var collider = sceneObject.GetComponent<Collider>();
            if (collider != null)
                collider.IsPickable = true;

            Add(sceneObject, true);
        }

        private GameObject CreateLight(string name, LightType type)
        {
            var gameObject = new GameObject(name);
            gameObject.AddComponent<SphereCollider>();

            var light = gameObject.AddComponent<Light>();
            light.TypeLight = type;

            var lightRenderer = light.AddComponent<EditorIconRenderer>();
            lightRenderer.Setup("Light_Icon");
            lightRenderer.AddComponent<SphereCollider>();
            return gameObject;
        }

        public void RemoveGameObject(GameObject sceneObject)
        {
            Remove(sceneObject);
        }

        #endregion

        public GameObject[] GetGameObjects()
        {
            var list = new List<GameObject>();

            foreach (var go in gameObjects)
                if (go.Tag != EditorGame.EditorTag)
                    list.Add(go);

            return list.ToArray();
        }
    }
}
