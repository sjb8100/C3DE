C3DE : Cool 3D Engine
=====================

### What is it ?
C3DE is a research project to create a small but powerfull 3D engine powered by MonoGame.

![](http://38.media.tumblr.com/88d6831c96fbdc0dcac7e90654f193ae/tumblr_naltfbT5uf1s15knro1_1280.jpg)

### Some features

- Scene (parent/child)
- Component based
- Cameras
- Model (FBX/X with content pipeline)
- Custom Mesh geometry
- Terrain: Flat, Random, Heightmap, multi textured
- Materials: Standard, Simple, Reflective, Water, Lava, Custom
- Shadow mapping
- Input management: Keyboard, Mouse, Gamepad 
- Procedural texture generation

### Sample

```C#
public class SuperCoolGame : Engine
{
	public SuperCoolGame()
	    : base()
	{
	}

	protected override void Initialize()
    {
        base.Initialize();

        // Create a camera node with an orbit controller.
        var camera = new CameraPrefab("camera", scene);
        camera.AddComponent<OrbitController>();

        // Add a light with shadows
        var lightPrefab = new LightPrefab("light", LightType.Directional, scene);
        lightPrefab.EnableShadows = true;

        // Add a terrain generated with Pelrin Noise.
        var terrain = new TerrainPrefab("terrain", scene);
        terrain.TextureRepeat = new Vector2(16);
        terrain.Randomize();
        terrain.Renderer.Material = new StandardMaterial(scene);
        terrain.Renderer.Material.MainTexture = Content.Load<Texture2D>("Textures/terrain");
    }
}
```

### What's next ?
- Post processing
- More light types (Spot, Area)
- Multipass lighting
- Instancing
- Collision management
- Editor

### Requirement
You need a fresh copy of MonoGame assembly (OpenGL or DirectX).

MIT License