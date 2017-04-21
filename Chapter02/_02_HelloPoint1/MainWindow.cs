using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utils;

namespace _02_HelloPoint1
{
    class MainWindow : GameWindow
    {
        int program;
        bool canDraw = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Draw a point (1)";
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

            // Specify the color for clearing
            GL.ClearColor(Color.Black);

            // Enable Point Size
            GL.Enable(EnableCap.ProgramPointSize);

            canDraw = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(0, 0, Width, Height);

            if (!canDraw) return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw a point
            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            GL.Flush();

            SwapBuffers();
        }
    }
}
