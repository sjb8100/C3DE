﻿using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// The renderer will render a scene to the screen according to a camera view.
    /// </summary>
    public class Renderer
    {
        private GraphicsDevice graphicsDevice;
        private RenderTarget2D _sceneRT;
        //private RenderTarget2D _shadowRT;
        private List<ModelRenderer> _renderList;
        //private float _shadowMapSize;
        private Light _light;
        private Effect _objectFx;
        //private Effect _shadowFx;
        private ShadowGenerator _shadowGenerator;
        private SpriteBatch _spriteBatch;
        private Color _ambientColor;

        public Color AmbientColor
        {
            get { return _ambientColor; }
            set { _ambientColor = value; }
        }

        public Light Light
        {
            get { return _light; }
        }

        public Renderer(GraphicsDevice device)
        {
            graphicsDevice = device;
            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _light = new Light();
            _ambientColor = Color.White;
            _shadowGenerator = new ShadowGenerator(device);
        }

        /// <summary>
        /// Load shaders.
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            _objectFx = content.Load<Effect>("fx/StandardEffect");
            _shadowGenerator.LoadContent(content);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        private void renderObjects(Camera camera)
        {
            graphicsDevice.SetRenderTarget(_sceneRT);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            _objectFx.Parameters["View"].SetValue(camera.view);
            _objectFx.Parameters["Projection"].SetValue(camera.projection);

            // Shadows
            if (_shadowGenerator.Enabled)
            {
                _objectFx.Parameters["shadowTexture"].SetValue(_shadowGenerator.ShadowRT);
                //_objectFx.Parameters["shadowMapSize"].SetValue(_shadowGenerator.ShadowMapSize);
            }
            else
                _objectFx.Parameters["shadowMapSize"].SetValue(0.0f);

            // Light
            _objectFx.Parameters["lightView"].SetValue(_light.viewMatrix);
            _objectFx.Parameters["lightProjection"].SetValue(_light.projectionMatrix);
            _objectFx.Parameters["lightPosition"].SetValue(_light.Transform.Position);
            _objectFx.Parameters["lightRadius"].SetValue(Light.radius);
            _objectFx.Parameters["ambientColor"].SetValue(_ambientColor.ToVector4());
            

            for (int i = 0; i < _renderList.Count; i++)
            {
                _objectFx.Parameters["World"].SetValue(_renderList[i].SceneObject.Transform.world);
                _objectFx.Parameters["mainTexture"].SetValue(_renderList[i].MainTexture);
                _objectFx.CurrentTechnique.Passes[0].Apply();
                _renderList[i].DrawMesh(graphicsDevice);
            }

            graphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        private void renderBuffers()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public void render(Scene scene, Camera camera)
        {
            _renderList = scene.RenderList;

            if (_shadowGenerator.Enabled)
            {
                _shadowGenerator.Light = _light;
                _shadowGenerator.RenderList = _renderList;
                _shadowGenerator.renderShadows(camera);
            }

            renderObjects(camera);
            renderBuffers();
        }
    }
}
