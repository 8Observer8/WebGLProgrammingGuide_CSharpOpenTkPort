using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using Utils;

namespace _04_ClickedPoints_OptimusEnablement
{
    class MainWindow : GameWindow
    {
        private bool canDraw = false;
        private int program;
        private int a_Position;

        private List<Vector2> points = new List<Vector2>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Draw a point with a mouse click";
            Width = 400;
            Height = 400;

            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("Video Adapter: " + GL.GetString(StringName.Renderer));

            // Load the shaders from the files
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

            // Get the storage location of a_Position
            a_Position = GL.GetAttribLocation(program, "a_Position");
            if (a_Position < 0)
            {
                Logger.Append("Failed to get the storage location of a_Position");
                return;
            }

            // Enable Point Size
            GL.Enable(EnableCap.ProgramPointSize);

            // Specify the color for clearing the render canvas
            GL.ClearColor(Color.Black);

            canDraw = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            // Clear the render canvas
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (canDraw)
            {
                foreach (var p in points)
                {
                    // Pass coordinates of the point to a_Position
                    GL.VertexAttrib2(a_Position, p.X, p.Y);

                    // Draw the point
                    GL.DrawArrays(PrimitiveType.Points, 0, 1);
                }
            }

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left)
            {
                float x = (e.X - Width / 2f) / (Width / 2f);
                float y = -(e.Y - Height / 2f) / (Height / 2f);

                // Store the coordinates to point list
                points.Add(new Vector2(x, y));
            }
        }
    }
}
