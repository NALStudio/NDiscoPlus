using HomagGroup.Blazor3D.Cameras;
using HomagGroup.Blazor3D.Controls;
using HomagGroup.Blazor3D.Core;
using HomagGroup.Blazor3D.Geometires;
using HomagGroup.Blazor3D.Lights;
using HomagGroup.Blazor3D.Materials;
using HomagGroup.Blazor3D.Maths;
using HomagGroup.Blazor3D.Objects;
using HomagGroup.Blazor3D.Scenes;
using HomagGroup.Blazor3D.Settings;
using HomagGroup.Blazor3D.Viewers;
using NDiscoPlus.Shared.Models;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoPlus.Components.Elements;
public partial class LightPositionViewer : IDisposable
{
    public delegate void OnLightSectedCallback(string? objectType, LightId? light);

    private class LightSignal(string color)
    {
        public string Color { get; set; } = color;
        public int RefCount { get; set; }
    }

    private Viewer? viewer;
    private Scene Scene { get; set; }

    private ViewerSettings ViewerSettings { get; }
    private Camera Camera { get; }
    private OrbitControls Controls { get; }

    private readonly record struct SceneObjects(Mesh Mesh, PointLight Light);
    private record LightData
    {
        public required SceneObjects SceneObjects { get; init; }

        public required Vector3 Position { get; set; }
        public required string Color { get; set; }
        public AnimationData? Animation { get; set; } = null;
    }

    private record AnimationData
    {
        public required string Color { get; init; }
    }

    private readonly Dictionary<LightId, LightData> lightDatas;
    private Dictionary<Guid, string> objectTypes;
    private Mesh? monitorMesh;

    public LightPositionViewer()
    {
        Camera = new OrthographicCamera
        {
            Position = new(3d, 1d, 3d),
            Zoom = 0.65d,
            AnimateRotationSettings = new(true, 0.05, 0, 0.05, radius: 3)
            {
                StopAnimationOnOrbitControlMove = true
            }
        };

        Controls = new OrbitControls
        {
            EnableDamping = true
        };

        ViewerSettings = new()
        {
            ShowViewHelper = false,
            CanSelect = true
        };

        lightDatas = new();

        Scene = new();
        objectTypes = new SceneBuilder(Scene).Build();
    }

    public bool Initialized { get; private set; } = false;
    private readonly TaskCompletionSource initializedTaskSource = new();

    public Task WaitUntilInitializedAsync() => initializedTaskSource.Task;

    public event OnLightSectedCallback? OnLightSelected;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            viewer!.JsModuleLoaded += Viewer_JsModuleLoaded;
            viewer.ObjectSelected += Viewer_ObjectSelected;
        }
    }

    private Task Viewer_JsModuleLoaded()
    {
        Initialized = true;
        initializedTaskSource.SetResult();

        return Task.CompletedTask;
    }

    private void Viewer_ObjectSelected(HomagGroup.Blazor3D.Events.Object3DArgs e)
    {
        LightId? light = null;
        foreach ((LightId id, LightData data) in lightDatas)
        {
            if (e.UUID == data.SceneObjects.Mesh.Uuid)
            {
                light = id;
                break;
            }
        }

        string? objectType = objectTypes.GetValueOrDefault(e.UUID);
        OnLightSelected?.Invoke(objectType, light);
    }

    public async Task ClearAsync()
    {
        ThrowIfNotInitialized();

        viewer.Scene.Children.Clear();
        lightDatas.Clear();
        objectTypes.Clear();

        objectTypes = new SceneBuilder(Scene).Build();

        if (monitorMesh is not null) // add monitor back (was removed during clear)
            AddMonitorMesh(monitorMesh);

        await viewer.UpdateScene();
    }

    public async Task RemoveMonitorRect()
    {
        ThrowIfNotInitialized();

        if (monitorMesh is not null)
        {
            viewer.Scene.Children.Remove(monitorMesh);

            bool removed = objectTypes.Remove(monitorMesh.Uuid);
            Debug.Assert(removed);

            await viewer.UpdateScene();
        }
    }

    /// <summary>
    /// Monitor depth is controlled by the viewer.
    /// </summary>
    public async Task SetMonitorRect(double x, double y, double z, double width, double height)
    {
        ThrowIfNotInitialized();

        if (monitorMesh is null)
        {
            monitorMesh = new Mesh()
            {
                Geometry = new BoxGeometry(depth: 0.05),
                Material = new MeshStandardMaterial()
                {
                    Color = "#000000"
                }
            };

            AddMonitorMesh(monitorMesh);
        }

        monitorMesh.Position = LightPositionToObjectPosition(x, y, z);

        BoxGeometry geometry = (BoxGeometry)monitorMesh.Geometry;
        geometry.Width = width;
        geometry.Height = height;

        await viewer.UpdateScene();
    }

    private void AddMonitorMesh(Mesh mesh)
    {
        Scene.Add(mesh);
        objectTypes.Add(mesh.Uuid, "Monitor");
    }

    public async Task AddOrUpdateLightAsync(NDPLight light, string color)
    {
        ThrowIfNotInitialized();

        if (lightDatas.TryGetValue(light.Id, out LightData? data))
        {
            data.Color = color;
            data.Position = LightPositionToObjectPosition(light.Position);
            UpdateLight(data);
        }
        else
        {
            AddLight(light, color);
        }

        await viewer.UpdateScene();
    }

    public async Task Signal(NDPLight light, TimeSpan duration, string color)
    {
        ThrowIfNotInitialized();

        if (!lightDatas.TryGetValue(light.Id, out LightData? data))
            throw new InvalidOperationException("Light not added.");

        if (data.Animation is not null)
            throw new InvalidOperationException("Light is already animating another animation.");

        data.Animation = new()
        {
            Color = color
        };
        UpdateLight(data);
        await viewer.UpdateScene();

        await Task.Delay(duration);

        data.Animation = null;
        UpdateLight(data);
        await viewer.UpdateScene();
    }

    [MemberNotNull(nameof(viewer))]
    public void ThrowIfNotInitialized()
    {
        if (!Initialized)
            throw new InvalidOperationException("Viewer has not yet been initialized.");
        Debug.Assert(viewer is not null);
    }

    private static void UpdateLight(LightData data)
    {
        Mesh mesh = data.SceneObjects.Mesh;
        PointLight pointLight = data.SceneObjects.Light;


        mesh.Position = data.Position;
        pointLight.Position = data.Position;

        string color = data.Animation?.Color ?? data.Color;
        mesh.Material.Color = color;
        pointLight.Color = color;
    }

    private void AddLight(NDPLight light, string color)
    {
        Mesh mesh = new()
        {
            Geometry = new SphereGeometry(radius: 0.05),
            Material = new MeshStandardMaterial()
        };
        Scene.Add(mesh);

        PointLight pointLight = new()
        {
            Intensity = 0.8d,
            Distance = 1d
        };
        Scene.Add(pointLight);

        SceneObjects sceneObjects = new(mesh, pointLight);

        LightData data = new()
        {
            SceneObjects = sceneObjects,
            Position = LightPositionToObjectPosition(light.Position),
            Color = color
        };
        lightDatas.Add(light.Id, data);

        UpdateLight(data);
    }

    private class SceneBuilder
    {
        public Scene Scene { get; }
        private Dictionary<Guid, string>? objectTypes;

        public SceneBuilder(Scene scene)
        {
            Scene = scene;
            objectTypes = null;
        }

        public Dictionary<Guid, string> Build()
        {
            if (objectTypes is not null)
                throw new InvalidOperationException("Scene builder has already been built.");

            objectTypes = new Dictionary<Guid, string>();
            BuildScene();
            return objectTypes;
        }

        private void BuildScene()
        {
            const double _180deg = Math.PI;
            const double _90deg = _180deg / 2;

            // Light
            Scene.Add(new AmbientLight() { Intensity = 0.8d });

            // Room
            CreateFloor(new Vector3(0, -1, 0), 2, 2);
            CreateWall(new Vector3(-1, 0, 0), new Euler() { Y = _90deg }, 2, 2); // left wall
            CreateWall(new Vector3(1, 0, 0), new Euler() { Y = -_90deg }, 2, 2); // right wall
            CreateWall(new Vector3(0, 0, -1), new Euler(), 2, 2); // back wall
            CreateWall(new Vector3(0, 0, 1), new Euler() { X = _180deg }, 2, 2); // front wall
                                                                                 // scene.Add(CreateWall(new Vector3(0, 1, 0), new Euler() { X = _90deg }, 2, 2, colorOverride: "#C0C0C0")); // ceiling
        }

        private void CreateFloor(Vector3 pos, double sizeX, double sizeY)
        {
            const double _90deg = Math.PI / 2d;
            CreateWall(pos, new Euler() { X = -_90deg }, sizeX, sizeY, colorOverride: "#806060", typeOverride: "Floor"); // floor
            CreateWall(pos, new Euler() { X = _90deg }, sizeX, sizeY, colorOverride: "#806060", typeOverride: "Floor"); // floor inverted (so that you can't see through the floor when looking upwards)
        }

        private void CreateWall(Vector3 pos, Euler rotation, double width, double height, string? colorOverride = null, string? typeOverride = null)
        {
            Mesh mesh = new()
            {
                Position = pos,
                Rotation = rotation,
                Geometry = new PlaneGeometry(width: width, height: height),
                Material = new MeshStandardMaterial()
                {
                    Color = colorOverride ?? "E0E0E0"
                }
            };

            Scene.Add(mesh);
            objectTypes!.Add(mesh.Uuid, typeOverride ?? "Wall");
        }
    }

    public void Dispose()
    {
        if (viewer is not null)
        {
            viewer.JsModuleLoaded -= Viewer_JsModuleLoaded;
            viewer.ObjectSelected -= Viewer_ObjectSelected;
            viewer.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private Vector3 LightPositionToObjectPosition(LightPosition pos) => LightPositionToObjectPosition(pos.X, pos.Y, pos.Z);
    private Vector3 LightPositionToObjectPosition(double x, double y, double z)
    {
        return new(
            x: x,
            y: z, // Blazor3D doesn't allow setting Z axis as up
            z: -y // Z grows away from camera, which is inverse of Philips Hue
        );
    }
}
