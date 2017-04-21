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

namespace _04_RotatingTriangle
{
    class MainWindow : GameWindow
    {
        private bool canDraw = false;
        private int nVertices;
        private int program;
        private float currentAngle = 0f;
        Matrix4 modelMatrix;
        int u_ModelMatrix;

        // Rotation angle (degress/second)
        float ANGLE_STEP = 1f;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Continually Rotate A Triangle";
            Width = 400;
            Height = 400;

            // Load shaders from files
            string vShaderSource = null;
            string fShaderSource = null;
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out fShaderSource);
            if (vShaderSource == null)
            {
                Logger.Append("Load the vertex shader from a file");
                return;
            }
            if (fShaderSource == null)
            {
                Logger.Append("Load the fragment shader from a file");
                return;
            }

            // Initialize shaders
            if (!ShaderLoader.InitShaders(vShaderSource, fShaderSource, out program))
            {
                Logger.Append("Failed to initialize shaders");
                return;
            }

            // Set vertex information
            nVertices = InitVertexBuffers();
            if (nVertices < 0)
            {
                Logger.Append("Failed to set vertex information");
                return;
            }

            // Get the storage location of u_ModelMatrix
            u_ModelMatrix = GL.GetUniformLocation(program, "u_ModelMatrix");
            if (u_ModelMatrix < 0)
            {
                Logger.Append("Failed to get the storage location of u_ModelMatrix");
                return;
            }

            // Set the color for clearing a canvas
            GL.ClearColor(Color.BlueViolet);

            canDraw = true;
        }

        private int InitVertexBuffers()
        {
            float[] vertices = new float[]
            {
                0f, 0.3f, -0.3f, -0.3f, 0.3f, -0.3f
            };
            int n = 3;

            // Create a buffer object
            int vertexBuffer;
            GL.GenBuffers(1, out vertexBuffer);
            if (vertexBuffer < 0)
            {
                Logger.Append("Failed to create the buffer object");
                return -1;
            }

            // Bind buffer to the target
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            // Write data to the target
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Get the storage location of a_Position
            int a_Position = GL.GetAttribLocation(program, "a_Position");
            if (a_Position < 0)
            {
                Logger.Append("Failed to get the storage location of a_Position");
                return -1;
            }

            // Assign the buffer to a_Position variable
            GL.VertexAttribPointer(a_Position, 2, VertexAttribPointerType.Float, false, 0, 0);
            // Enable assignment
            GL.EnableVertexAttribArray(a_Position);

            return n;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            currentAngle = Animate(currentAngle, e.Time);

            // Set the rotation matrix
            modelMatrix = Matrix4.CreateRotationZ(currentAngle);

            // Pass the model matrix to the vertex shader
            GL.UniformMatrix4(u_ModelMatrix, false, ref modelMatrix);
        }

        private float Animate(float angle, double elapsed)
        {
            float newAngle = angle + (ANGLE_STEP * (float)elapsed);
            return newAngle % 360;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnRenderFrame(e);

            // Clear canvas with current color
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
