using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace ShapeDatabase.UI {

	public class Window : GameWindow
	{

		// Here we now have added the normals of the vertices
		// Remember to define the layouts to the VAO's
		private readonly float[] _vertices =
		{
             // Position          Normal
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, // Front face
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
			 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
			 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
			-0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
			-0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

			-0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, // Back face
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
			 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
			 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
			-0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
			-0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

			-0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, // Left face
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
			-0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
			-0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
			-0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
			-0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

			 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, // Right face
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
			 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
			 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
			 0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
			 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

			-0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, // Bottom face
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
			 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
			 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
			-0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
			-0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

			-0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, // Top face
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
			 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
			 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
			-0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
			-0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
		};

		/*
		uint[] indices = {		// note that we start from 0!
			0, 1, 2,			// first triangle
			3, 4, 5,			// second triangle
			6, 7, 8,			// etc.
			9, 10, 11,
			12, 13, 14,
			15, 16, 17,
			18, 19, 20,
			21, 22, 23,
			24, 25, 26,
			27, 28, 29,
			30, 31, 32,
			33, 34, 35,
		};*/

		private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

		//private int _elementBufferObject;
		private int _vertexBufferObject;
		private int _vaoModel;

		private Shader _lightingShader;

		private KeyController keybindings;
		private Camera _camera;
		private double _angleY;
		private double _angleX;

		public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) {

			const float cameraSpeed = 1.5f;
			keybindings = new KeyController();

			keybindings.RegisterDown(Key.Escape, Exit);
			keybindings.RegisterDown(Key.Space, // Reset
				() => {
					_camera.Reset(this);
					_angleY = 0;
					_angleX = 0;
				});

			keybindings.RegisterHold(Key.W,		// Forward
				(FrameEventArgs e) => _camera.Position += _camera.Front * cameraSpeed * (float) e.Time );
			keybindings.RegisterHold(Key.S,		// Backward
				(FrameEventArgs e) => _camera.Position -= _camera.Front * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.A,		// Left
				(FrameEventArgs e) => _camera.Position -= _camera.Right * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.D,		// Right
				(FrameEventArgs e) => _camera.Position += _camera.Right * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.Q,     // Up
				(FrameEventArgs e) => _camera.Position += _camera.Up * cameraSpeed * (float) e.Time);
			keybindings.RegisterHold(Key.E,     // Down
				(FrameEventArgs e) => _camera.Position -= _camera.Up * cameraSpeed * (float) e.Time);

			keybindings.RegisterHold(Key.Left,	() => _angleY -= 1.5);
			keybindings.RegisterHold(Key.Right, () => _angleY += 1.5);
			keybindings.RegisterHold(Key.Up,	() => _angleX -= 1.5);
			keybindings.RegisterHold(Key.Down,	() => _angleX += 1.5);

			/*
			Vector3[] test = new Vector3[3] { new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f) };

			Vector3 result = GetNormal(test);*/
		}

		protected Vector3 GetNormal(Vector3[] verts)
		{
			if (verts.Length != 3)
				throw new ArgumentException("Input should have 3 vertices");

			Vector3 v1 = verts[0];
			Vector3 v2 = verts[1];
			Vector3 v3 = verts[2];
			
			return Vector3.Cross(v2 - v1, v3 - v1).Normalized();
		}

		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

			GL.Enable(EnableCap.DepthTest);

			_vertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

			_lightingShader = new Shader("UI/shader.vert", "UI/lighting.frag");
			
			_vaoModel = GL.GenVertexArray();
			GL.BindVertexArray(_vaoModel);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

			var positionLocation = _lightingShader.GetAttribLocation("aPos");
			GL.EnableVertexAttribArray(positionLocation);
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

			var normalLocation = _lightingShader.GetAttribLocation("aNormal");
			GL.EnableVertexAttribArray(normalLocation);
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

			//GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

			_camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);

			CursorVisible = false;

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

			_lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
			_lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
			_lightingShader.SetVector3("lightPos", _lightPos);
			_lightingShader.SetVector3("viewPos", _camera.Position);

			/*
			_elementBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
			GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);*/

			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

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
			if (Focused)
			{
				Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
			}

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
	}
}
