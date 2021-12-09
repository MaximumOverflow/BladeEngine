using BladeEngine.Core.Rendering.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Numerics;

namespace BladeEngine.Core.Rendering.OpenGL;

public unsafe class OpenGL : GraphicsApi
{
	public static GL Api { get; private set; } = null!;
	public static IWindow Window { get; private set; }  = null!;
	private static uint _renderWidth, _renderHeight;

	public override bool Running => !Window.IsClosing;

	public static bool Initialize(WindowOptions options)
	{
		Debug.Log("Initializing OpenGL context...");
		if (Graphics.CurrentApi != Graphics.Api.None)
		{
			Debug.LogError($"Could switch to OpenGL: {Graphics.CurrentApi} is already in use.");
			return false;
		}

		options.API = GraphicsAPI.Default;
		options.IsEventDriven = false;
		options.ShouldSwapAutomatically = false;
		Window = Silk.NET.Windowing.Window.Create(options);
		Window.Initialize();
		if (!options.IsVisible)
		{
			Window.IsVisible = false;
			var maxSize = Silk.NET.Windowing.Monitor.GetMonitors(Window).MaxBy(d => d.Bounds.Size.Length);
			Window.Size = maxSize!.Bounds.Size;
		}
		
		Api = GL.GetApi(Window);
		Graphics.Current = new OpenGL();

		Api.Enable(EnableCap.DebugOutput);
		Api.DebugMessageCallback((source, type, id, severity, length, message, param) =>
		{
			var str = new string((sbyte*) message, 0, length);
			switch (severity)
			{
				case GLEnum.DebugSeverityLow:
				case GLEnum.DebugSeverityNotification:
					Debug.Log($"[OpenGL] {str}");
					break;
				
				case GLEnum.DebugSeverityMedium when id != 131154: //Ignore Nvidia framebuffer performance warning
					Debug.LogWarning($"[OpenGL] {str}");
					break;
				
				case GLEnum.DebugSeverityHigh:
					Debug.LogError($"[OpenGL] {str}");
					break;
			}
		}, null);

		Graphics.CurrentApi = Graphics.Api.OpenGL;
		Graphics.Current.SetRenderingResolution((uint) options.Size.X, (uint) options.Size.Y);
		Debug.Log("OpenGL context initialized");
		return true;
	}

	public static bool Terminate()
	{
		Debug.Log("Destroying OpenGL context...");
		if (Graphics.CurrentApi != Graphics.Api.OpenGL)
		{
			Debug.LogError($"Could terminate OpenGL: {Graphics.CurrentApi} currently in use.");
			return false;
		}
		
		Window.Dispose();
		Graphics.Current = null!;
		Graphics.CurrentApi = Graphics.Api.None;

		Debug.Log("OpenGL context destroyed...");
		return true;
	}

	public override void Clear()
	{
		uint mask = 0;
		mask |= (uint) GLEnum.ColorBufferBit;
		mask |= (uint) GLEnum.DepthBufferBit;
		Api.Clear(mask);
	}

	public override void SwapBuffers()
	{
		Window.DoEvents();
		Window.SwapBuffers();
	}

	public override void SetClearColor(float r, float g, float b)
		=> Api.ClearColor(r, g, b, 1);

	public override void GetRenderingResolution(out uint width, out uint height)
		=> (width, height) = (_renderWidth, _renderHeight);
	
	public override void SetRenderingResolution(uint width, uint height)
	{
		Api.Scissor(0, 0, width, height);
		Api.Viewport(0, 0, width, height);
		(_renderWidth, _renderHeight) = (width, height);
		Debug.Log($"OpenGL render resolution set to {_renderWidth}x{_renderHeight}...");
	}

	public uint MeshDrawQueueSize { get; private set; }
	private readonly Stack<List<Matrix4x4>> _transformListPool = new();
	private readonly Dictionary<(Mesh, Material), List<Matrix4x4>> _meshDrawQueue = new();

	public override void DrawMesh(Common.Mesh m, Material material, in Matrix4x4 transform)
	{
		var mesh = Unsafe.As<Common.Mesh, Mesh>(ref m);
		if (!_meshDrawQueue.TryGetValue((mesh, material), out var queue))
		{
			if (!_transformListPool.TryPop(out queue)) queue = new List<Matrix4x4>();
			_meshDrawQueue.Add((mesh, material), queue);
		}
		
		queue.Add(transform);
		MeshDrawQueueSize++;
	}

	public override void DrawMesh(Common.Mesh m, Material material, IEnumerable<Matrix4x4> transforms)
	{
		var mesh = Unsafe.As<Common.Mesh, Mesh>(ref m);
		if (!_meshDrawQueue.TryGetValue((mesh, material), out var queue))
		{
			if (!_transformListPool.TryPop(out queue)) queue = new List<Matrix4x4>();
			_meshDrawQueue.Add((mesh, material), queue);
		}

		var count = queue.Count;
		queue.AddRange(transforms);
		MeshDrawQueueSize += (uint) (queue.Count - count);
	}

	public override void ReadPixels(Span<byte> bytes)
	{
		Api.ReadnPixels(0, 0, _renderWidth, _renderHeight, GLEnum.Rgba, PixelType.UnsignedByte, (uint) bytes.Length, Unsafe.AsPointer(ref bytes[0]));
	}

	public override void FlushRenderBuffer()
	{
		var gl = Api;
		foreach (var (data, transforms) in _meshDrawQueue)
		{
			var (mesh, material) = data;
			gl.BindVertexArray(mesh.Vao);
			material.Use();
			gl.BindBuffer(GLEnum.ArrayBuffer, mesh.Ebo);
			
			ref var capacity = ref mesh.EboSize;
			var size = transforms.Count * sizeof(Matrix4x4);

			if (size > capacity)
			{
				gl.BufferData<Matrix4x4>(GLEnum.ArrayBuffer, CollectionsMarshal.AsSpan(transforms).Slice(0, transforms.Count), GLEnum.DynamicDraw);
				capacity = size;
			}
			else gl.BufferSubData<Matrix4x4>(GLEnum.ArrayBuffer, 0, CollectionsMarshal.AsSpan(transforms).Slice(0, transforms.Count));
			
			gl.DrawElementsInstanced(PrimitiveType.Triangles, (uint) mesh.Indices.Length, DrawElementsType.UnsignedInt, null, (uint) transforms.Count);
			
			transforms.Clear();
			_transformListPool.Push(transforms);
		}
		
		_meshDrawQueue.Clear();
	}
}