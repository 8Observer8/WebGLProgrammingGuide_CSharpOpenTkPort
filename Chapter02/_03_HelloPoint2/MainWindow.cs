using System;
using Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace _03_HelloPoint2
{
    class MainWindow : GameWindow
    {
        private int program;

        private bool canDraw = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set window title and canvas size
            Title = "Hello Point (2)";
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


            // Initialize shaders
            if (!ShaderLoader.InitShaders(vShaderSource, fShaderSource, out program))
            {
                Logger.Append("Failed to initialize the shaders");
                return;
            }

            // Get a storage location of a_Position
            int a_Position = GL.GetAttribLocation(program, "a_Position");
            if (a_Position < 0)
            {
                Logger.Append("Failed to get the storage location of a_Position");
                return;
            }

            // Pass vertex position to attribute variable
            GL.VertexAttrib3(a_Position, 0f, 0f, 0f);

            // Specify the color for clearing "canvas"
            GL.ClearColor(Color4.Black);

            GL.Enable(EnableCap.ProgramPointSize);

            canDraw = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            if (!canDraw) return;

            // Clear "canvas"
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw a point
            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            GL.Flush();
            SwapBuffers();
        }
    }
}
