using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utils;

namespace _04_HelloTriangle_LINE_STRIP
{
    class MainWindow : GameWindow
    {
        private bool canDraw = false;
        private int program;
        private int nVertices;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Draw a triangle (gl.LINE_STRIP)";
            Width = 400;
            Height = 400;

            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("Video Adapter: " + GL.GetString(StringName.Renderer));

            // Load shaders from files
            string vShaderSource = null;
            string fShaderSource = null;
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out fShaderSource);
            if (vShaderSource == null)
            {
                Logger.Append("Failed to load the vertex shader from a file");
                return;
            }
            if (fShaderSource == null)
            {
                Logger.Append("Failed to load the fragment shader from a file");
                return;
            }

            // Initialize the shaders
            if (!ShaderLoader.InitShaders(vShaderSource, fShaderSource, out program))
            {
                Logger.Append("Failed to initialize the shaders");
                return;
            }

            // Write the positions of vertices to a vertex shader
            nVertices = InitVertexBuffers();
            if (nVertices < 0)
            {
                Logger.Append("Failed to write the positions of vertices to a vertex shader");
                return;
            }

            // Specify the color for clearing the canvas
            GL.ClearColor(Color.Black);

            canDraw = true;
        }

        private int InitVertexBuffers()
        {
            float[] vertices = new float[] { 0f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f };

            // Create a buffer object
            int vertexBuffer;
            GL.GenBuffers(1, out vertexBuffer);

            // Bind the buffer object to target
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            // Write data into the buffer object
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Get the storage location of a_Position
            int a_Position = GL.GetAttribLocation(program, "a_Position");
            if (a_Position < 0)
            {
                Logger.Append("Failed to get the storage location of a_Position");
                return -1;
            }

            // Assign the buffer object to a_Position variable
            GL.VertexAttribPointer(a_Position, 2, VertexAttribPointerType.Float, false, 0, 0);

            // Enable the assignment to a_Position variable
            GL.EnableVertexAttribArray(a_Position);

            return vertices.Length / 2;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            if (canDraw)
            {
                GL.DrawArrays(PrimitiveType.LineStrip, 0, nVertices);
            }

            GL.Flush();
            SwapBuffers();
        }
    }
}
