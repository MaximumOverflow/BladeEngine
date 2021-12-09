using Silk.NET.Assimp;
using Silk.NET.OpenGL;

namespace BladeEngine.Core.Rendering.OpenGL;

internal sealed unsafe class Mesh : Common.Mesh
{
	internal int EboSize;
	public readonly uint Vao, Vbo, Ibo, Ebo;
	public override uint[] Indices { get; set; }
	public override Vector3[] Normals { get; set; }
	public override Vector3[] Vertices { get; set; }

	public Mesh()
	{
		var gl = OpenGL.Api;
		EboSize = 0;
		Vbo = gl.CreateBuffer();
		Ibo = gl.CreateBuffer();
		Ebo = gl.CreateBuffer();
		Vao = gl.CreateVertexArray();
		Indices = Array.Empty<uint>();
		Normals = Array.Empty<Vector3>();
		Vertices = Array.Empty<Vector3>();

		var stride = (uint) sizeof(Vector3) * 2;
		
		gl.BindVertexArray(Vao);
		//Model data
		gl.BindBuffer(GLEnum.ArrayBuffer, Vbo);
		gl.BindBuffer(GLEnum.ElementArrayBuffer, Ibo);
		gl.VertexAttribPointer(0, 3, GLEnum.Float, false, stride, null); //Positions
		gl.VertexAttribPointer(1, 3, GLEnum.Float, false, stride, (void*) sizeof(Vector3)); //Normals
		gl.EnableVertexAttribArray(0);
		gl.EnableVertexAttribArray(1);
		//Instance data
		gl.BindBuffer(GLEnum.ArrayBuffer, Ebo);
		gl.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, (uint) sizeof(Matrix4x4), (void*) (sizeof(Vector4) * 0));
		gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (uint) sizeof(Matrix4x4), (void*) (sizeof(Vector4) * 1));
		gl.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, (uint) sizeof(Matrix4x4), (void*) (sizeof(Vector4) * 2));
		gl.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, (uint) sizeof(Matrix4x4), (void*) (sizeof(Vector4) * 3));
		gl.EnableVertexAttribArray(3); gl.EnableVertexAttribArray(4); gl.EnableVertexAttribArray(5); gl.EnableVertexAttribArray(6);
		gl.VertexAttribDivisor(3, 1); gl.VertexAttribDivisor(4, 1); gl.VertexAttribDivisor(5, 1); gl.VertexAttribDivisor(6, 1);
	}

	~Mesh() => Dispose();

	public override void Update()
	{
		if (Vertices.Length != Normals.Length)
		{
			Debug.LogError("Could not update texture, the number of vertices and normals does not match.");
			return;
		}
		
		var gl = OpenGL.Api;
		var count = Normals.Length + Vertices.Length;
		var vBytes = (nuint) (sizeof(Vector3) * count);
		var buffer = (Vector3*) NativeMemory.Alloc(vBytes);
		var vertices = new Span<Vector3>(buffer, count);
		for (int i = 0, j = 0; i < count; j++)
		{
			vertices[i++] = Vertices[j];
			vertices[i++] = Normals[j];
		}
		
		gl.BindVertexArray(Vao);
		gl.BindBuffer(GLEnum.ArrayBuffer, Vbo);
		gl.BufferData(GLEnum.ArrayBuffer, vBytes, buffer, GLEnum.StaticDraw);
		gl.BufferData<uint>(GLEnum.ElementArrayBuffer, Indices.AsSpan(), GLEnum.StaticDraw);
		NativeMemory.Free(buffer);
	}

	public override void Dispose()
	{
		GC.SuppressFinalize(this);
		OpenGL.Api.DeleteBuffer(Vbo);
		OpenGL.Api.DeleteBuffer(Ibo);
		OpenGL.Api.DeleteBuffer(Ebo);
		OpenGL.Api.DeleteVertexArray(Vao);
	}

	public new static Mesh? FromFile(string path)
	{
		var assimp = Assimp.GetApi();
		var scene = assimp.ImportFile(path, 0x2 | 0x8 | 0x20 | 0x200000);
		var meshes = new Span<Silk.NET.Assimp.Mesh>(*scene->MMeshes, (int) scene->MNumMeshes);
		if (meshes.Length != 1)
		{
			Debug.LogError("Could not load mesh, mesh count != 1.");
			return null;
		}

		var data = meshes[0];

		var normals = new Vector3[data.MNumVertices];
		var vertices = new Vector3[data.MNumVertices];
		new Span<Vector3>(data.MNormals, (int) data.MNumVertices).CopyTo(normals);
		new Span<Vector3>(data.MVertices, (int) data.MNumVertices).CopyTo(vertices);

		var indices = new List<uint>();
		for (var i = 0u; i < data.MNumFaces; i++)
		{
			var face = data.MFaces[i];
			var span = new Span<uint>(face.MIndices, (int) face.MNumIndices);
			foreach (var index in span) indices.Add(index);
		}

		assimp.FreeScene(scene);
		var mesh = new Mesh {Vertices = vertices, Indices = indices.ToArray(), Normals = normals};
		mesh.Update();

		Debug.Log($"Loaded mesh \"{path}\".  Vertices: {vertices.Length}  Indices: {indices.Count}.");
		return mesh;
	}
}