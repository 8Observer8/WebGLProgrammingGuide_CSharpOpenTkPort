// This is an example from the book "WebGL Programming Guide"
// by Kouichi Matsuda and Rodger Lea

// Author of this port to OpenTK
// Full Name: Ivan Enzhaev
// Nick Name: 8Observer8
// Email: 8observer8@gmail.com

using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utils;

namespace _03_TranslatedRotatedTriangle
{
    class MainWindow : GameWindow
    {
        int nVertices;
        bool canDraw = false;
        private int program;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Rotate And Then Translate A Triangle";
            Width = 400;
            Height = 400;

            Console.WriteLine("Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("Video Adapter: " + GL.GetString(StringName.Renderer));

            // Load shaders from files
            string vShaderSource = null;
            string fShaderSource = null;
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out fShaderSource);
            if (vShaderSource == null)
            {
                Logger.Append("Failed to load vertex shader from file");
                return;
            }
            if (fShaderSource == null)
            {
                Logger.Append("Failed to load fragment shader from file");
                return;
            }

            // Initialize shaders
            if (!ShaderLoader.InitShaders(vShaderSource, fShaderSource, out program))
            {
                Logger.Append("Failed to initialize shaders");
                return;
            }

            // Write the positions of vertices to a vertex shader
            nVertices = InitVertexBuffers();
            if (nVertices < 0)
            {
                Logger.Append("Failed to set the positions of the vertices");
                return;
            }

            float ANGLE = 60f;  // The rotation angle
            var Tx = 0.5f;       // Translation distance

            // Create Matrix4 object for model transformation
            // And calculate a model matrix
            Matrix4 modelMatrix =
                Matrix4.CreateRotationZ(ANGLE * (float)Math.PI / 180f) *
                Matrix4.CreateTranslation(Tx, 0f, 0f);

            // Pass the model matrix to the vertex shader
            int u_ModelMatrix = GL.GetUniformLocation(program, "u_ModelMatrix");
            if (u_ModelMatrix < 0)
            {
                Logger.Append("Failed to get the storage location of u_ModelMatrix");
                return;
            }
            GL.UniformMatrix4(u_ModelMatrix, false, ref modelMatrix);

            // Specify color for clearing canvas
            GL.ClearColor(Color.Black);

            canDraw = true;
        }

        private int InitVertexBuffers()
        {
            float[] vertices = new float[]
            {
                0f, 0.3f, -0.3f, -0.3f, 0.3f, -0.3f
            };
            int n = 3; // The number of vertices

            // Create a buffer object
            int vertexBuffer;
            GL.GenBuffers(1, out vertexBuffer);
            if (vertexBuffer < 0)
            {
                Logger.Append("Failed to create the buffer object");
                return -1;
            }

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

            return n;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            // Clear canvas
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (canDraw)
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, nVertices);
            }

            GL.Flush();
            SwapBuffers();
        }
    }
}
