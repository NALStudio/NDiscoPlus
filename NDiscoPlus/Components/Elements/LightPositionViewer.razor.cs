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
        BuildScene(Scene);
    }

    public bool Initialized { get; private set; } = false;
    private readonly TaskCompletionSource initializedTaskSource = new();

    public Task WaitUntilInitializedAsync() => initializedTaskSource.Task;

    public event Action<LightId?>? OnLightSelected;

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

        OnLightSelected?.Invoke(light);
    }

    public async Task ClearAsync()
    {
        ThrowIfNotInitialized();

        viewer.Scene.Children.Clear();
        lightDatas.Clear();

        BuildScene(Scene);

        await viewer.UpdateScene();
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

    private static void BuildScene(Scene scene)
    {
        const double _180deg = Math.PI;
        const double _90deg = _180deg / 2;

        // Light
        scene.Add(new AmbientLight() { Intensity = 0.8d });

        // Room
        scene.Add(CreateWall(new Vector3(0, -1, 0), new Euler() { X = -_90deg }, 2, 2, colorOverride: "#806060")); // floor
        scene.Add(CreateWall(new Vector3(0, -1, 0), new Euler() { X = _90deg }, 2, 2, colorOverride: "#806060")); // floor inverted (so that you can't see through the floor when looking upwards)
        scene.Add(CreateWall(new Vector3(-1, 0, 0), new Euler() { Y = _90deg }, 2, 2)); // left wall
        scene.Add(CreateWall(new Vector3(1, 0, 0), new Euler() { Y = -_90deg }, 2, 2)); // right wall
        scene.Add(CreateWall(new Vector3(0, 0, -1), new Euler(), 2, 2)); // back wall
        scene.Add(CreateWall(new Vector3(0, 0, 1), new Euler() { X = _180deg }, 2, 2)); // front wall
        // scene.Add(CreateWall(new Vector3(0, 1, 0), new Euler() { X = _90deg }, 2, 2, colorOverride: "#C0C0C0")); // ceiling
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

    private static Mesh CreateWall(Vector3 pos, Euler rotation, double width, double height, string? colorOverride = null)
    {
        return new Mesh()
        {
            Position = pos,
            Rotation = rotation,
            Geometry = new PlaneGeometry(width: width, height: height),
            Material = new MeshStandardMaterial()
            {
                Color = colorOverride ?? "E0E0E0"
            }
        };
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

    private Vector3 LightPositionToObjectPosition(LightPosition pos)
    {
        return new(
            x: pos.X,
            y: pos.Z, // Blazor3D doesn't allow setting Z axis as up
            z: -pos.Y // Z grows away from camera, which is inverse of Philips Hue
        );
    }
}
