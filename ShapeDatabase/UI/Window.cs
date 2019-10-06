using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.UI {

	public class Window : GameWindow
	{

		// Here we now have added the normals of the vertices
		// Remember to define the layouts to the VAO's
		private float[] _vertices;

		private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

		//private int _elementBufferObject;
		private int _vertexBufferObject;
		private int _vaoModel;

		private Shader _lightingShader;

		private readonly KeyController keybindings;
		private Camera _camera;
		private double _angleY;
		private double _angleX;

		public Window(int width, int height, string title, IMesh mesh) : base(width, height, GraphicsMode.Default, title) {

			keybindings = new KeyController();
			RegisterKeyBinds();

			LoadMesh(mesh);
		}

		protected void LoadMesh(IMesh mesh)
		{
			_vertices = new float[mesh.FaceCount * 18];
			int i = 0;

			foreach (Vector3 face in mesh.Faces)
			{

				Vector3[] vertices = new Vector3[3] {
					mesh.GetVertex((uint)face.X),
					mesh.GetVertex((uint) face.Y),
					mesh.GetVertex((uint)face.Z)
				};

				Vector3 Normal = GetNormal(vertices);

				for (int j = 0; j < 3; j++)
				{
					_vertices[(i * 6) + (j * 6)]	 = vertices[j].X;
					_vertices[(i * 6) + (j * 6) + 1] = vertices[j].Y;
					_vertices[(i * 6) + (j * 6) + 2] = vertices[j].Z;
					_vertices[(i * 6) + (j * 6) + 3] = Normal.X;
					_vertices[(i * 6) + (j * 6) + 4] = Normal.Y;
					_vertices[(i * 6) + (j * 6) + 5] = Normal.Z;
				}

				i += 3;
			}
		}

		protected virtual void RegisterKeyBinds() {

			const float cameraSpeed = 1.5f;

			// Application Management
			keybindings.RegisterDown(Key.Escape, Exit);
			keybindings.RegisterDown(Key.Space, // Reset
				() => {
					_camera.Reset(this);
					_angleY = 0;
					_angleX = 0;
				});

			// Movement
			keybindings.RegisterHold(Key.W,     // Forward
				(FrameEventArgs e) => _camera.Position += _camera.Front * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.S,     // Backward
				(FrameEventArgs e) => _camera.Position -= _camera.Front * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.A,     // Left
				(FrameEventArgs e) => _camera.Position -= _camera.Right * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.D,     // Right
				(FrameEventArgs e) => _camera.Position += _camera.Right * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.E,     // Up
				(FrameEventArgs e) => _camera.Position += _camera.Up * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.Q,     // Down
				(FrameEventArgs e) => _camera.Position -= _camera.Up * cameraSpeed * (float) e.Time);

			// Rotations
			keybindings.RegisterHold(Key.Left, () => _angleY -= 1.5);
			keybindings.RegisterHold(Key.Right, () => _angleY += 1.5);
			keybindings.RegisterHold(Key.Up, () => _angleX -= 1.5);
			keybindings.RegisterHold(Key.Down, () => _angleX += 1.5);
		}

		protected Vector3 GetNormal(Vector3[] verts)
		{
			if (verts.Length != 3)
				throw new ArgumentException("Input should have 3 vertices.");

			Vector3 v1 = verts[0];
			Vector3 v2 = verts[1];
			Vector3 v3 = verts[2];
			
			return Vector3.Cross(v2 - v1, v3 - v1).Normalized();
		}

		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

			GL.Enable(EnableCap.DepthTest);

			_vertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

			_lightingShader = new Shader("UI/shader.vert", "UI/lighting.frag");
			
			_vaoModel = GL.GenVertexArray();
			GL.BindVertexArray(_vaoModel);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

			int positionLocation = _lightingShader.GetAttribLocation("aPos");
			GL.EnableVertexAttribArray(positionLocation);
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

			int normalLocation = _lightingShader.GetAttribLocation("aNormal");
			GL.EnableVertexAttribArray(normalLocation);
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

			_camera = new Camera(Vector3.UnitZ * 2, Width / (float)Height);

			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.BindVertexArray(_vaoModel);

			_lightingShader.Use();

			var model = Matrix4.Identity * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_angleY)) * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_angleX));
			_lightingShader.SetMatrix4("model", model);
			_lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
			_lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

			_lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.6f, 0.31f));
			_lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
			_lightingShader.SetVector3("lightPos", _lightPos);
			_lightingShader.SetVector3("viewPos", _camera.Position);

			GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length);
			GL.BindVertexArray(_vaoModel);

			SwapBuffers();

			base.OnRenderFrame(e);
		}


		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (!Focused)
				return;

			keybindings.OnKeyPress(Keyboard.GetState(), e);
			base.OnUpdateFrame(e);
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			_camera.Fov -= e.DeltaPrecise;
			base.OnMouseWheel(e);
		}


		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			_camera.AspectRatio = Width / (float)Height;
			base.OnResize(e);
		}


		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.UseProgram(0);

			GL.DeleteBuffer(_vertexBufferObject);
			GL.DeleteVertexArray(_vaoModel);

			GL.DeleteProgram(_lightingShader.Handle);

			base.OnUnload(e);
		}

		public override void Exit() {
			Settings.Active = false;
			base.Exit();
		}
	}
}
