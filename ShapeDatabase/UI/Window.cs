using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;

namespace ShapeDatabase.UI {

	public class Window : GameWindow {

		private KeyController keybinds;

		private int VertexArrayObject;
		public Shader shader;

		int VertexBufferObject;
		int ElementBufferObject;

		float[] vertices = {
			0.5f,  0.5f, 0.0f,		// top right
			0.5f, -0.5f, 0.0f,		// bottom right
			-0.5f, -0.5f, 0.0f,		// bottom left
			-0.5f,  0.5f, 0.0f		// top left
		};

		uint[] indices = {			// note that we start from 0!
			0, 1, 3,				// first triangle
			1, 2, 3					// second triangle
		};

		public Window(int width, int height, string title)
			: base(width, height, GraphicsMode.Default, title) {
			
			keybinds = new KeyController();
			keybinds.RegisterDown(Key.Escape, Exit);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			KeyboardState input = Keyboard.GetState();
			keybinds.OnKeyPress(input);

			base.OnUpdateFrame(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

			VertexBufferObject = GL.GenBuffer();	
			ElementBufferObject = GL.GenBuffer();

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			shader = new Shader("UI/shader.vert", "UI/shader.frag");
			shader.Use();

			// 3. now draw the object
			VertexArrayObject = GL.GenVertexArray();

			// ..:: Initialization code (done once (unless your object frequently changes)) :: ..
			// 1. bind Vertex Array Object
			GL.BindVertexArray(VertexArrayObject);

			// 2. copy our vertices array in a buffer for OpenGL to use
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			// 3. then set our vertex attributes pointers
			GL.VertexAttribPointer(shader.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			base.OnLoad(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);

			shader.Use();

			GL.BindVertexArray(VertexArrayObject);
			GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

			Context.SwapBuffers();

			base.OnRenderFrame(e);
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);

			base.OnResize(e);
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.DeleteBuffer(VertexBufferObject);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.DeleteBuffer(ElementBufferObject);

			shader.Dispose();

			base.OnUnload(e);
		}
	}
}
