using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using Utils;

namespace _05_ColoredPoints_OptimusEnablement
{
    class MainWindow : GameWindow
    {
        private bool canDraw = false;
        private int program;
        private int a_Position;

        private List<Vector2> points = new List<Vector2>();
        private int u_FragColor;

        private List<Color4> colors = new List<Color4>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Change a point color";
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
                Logger.Append("Failed to load shaders from files");
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

            // Get the storage location of u_FragColor
            u_FragColor = GL.GetUniformLocation(program, "u_FragColor");
            if (u_FragColor < 0)
            {
                Logger.Append("Failed to get the storage location of u_FragColor");
                return;
            }

            // Enable point size
            GL.Enable(EnableCap.ProgramPointSize);

            // Specify the color for clearing the canvas
            GL.ClearColor(Color.Black);

            canDraw = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            // Clear the canvas with the current color
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (canDraw)
            {
                if (points.Count == colors.Count)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        // Pass the position of a point to a_Position variable
                        GL.VertexAttrib2(a_Position, points[i].X, points[i].Y);

                        // Pass the color of a point to u_FragColor variable
                        GL.Uniform4(u_FragColor, colors[i]);

                        // Draw a point
                        GL.DrawArrays(PrimitiveType.Points, 0, 1);
                    }
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
                float x = (e.Mouse.X - Width / 2f) / (Width / 2f);
                float y = -(e.Mouse.Y - Height / 2f) / (Height / 2f);

                // Store the coordinates to points array
                points.Add(new Vector2(x, y));

                // Store the colors to colors array
                if (x >= 0 && y >= 0) // First quadrant
                {
                    colors.Add(new Color4(1f, 0f, 0f, 1f)); // Red
                }
                else if (x < 0 && y < 0) // Third quadrant
                {
                    colors.Add(new Color4(0f, 1f, 0f, 1f)); // Green
                }
                else // Others
                {
                    colors.Add(new Color4(1f, 1f, 1f, 1f)); // White
                }
            }
        }
    }
}
