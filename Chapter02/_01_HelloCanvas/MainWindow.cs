using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _01_HelloCanvas
{
    class MainWindow : GameWindow
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Clear canvas";
            Width = 400;
            Height = 400;

            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("Video Adapter: " + GL.GetString(StringName.Renderer));

            // Specify the color for clearing the canvas
            GL.ClearColor(Color.Black);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            // Clear the canvas with the current color
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Flush();
            SwapBuffers();
        }
    }
}
