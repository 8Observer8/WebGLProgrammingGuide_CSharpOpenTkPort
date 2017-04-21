using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utils;
using OpenTK.Graphics;

namespace _13_Shadow
{
    class MainWindow : GameWindow
    {
        private const int OFFSCREEN_WIDTH = 2048;
        private const int OFFSCREEN_HEIGHT = 2048;

        private const float fovyAngle = 45f * (float)Math.PI / 180f;
        private const float fovyAngleFromLight = 70f * (float)Math.PI / 180f;
        private const float planeAngle = -45f * (float)Math.PI / 180f;

        private const int LIGHT_X = 0, LIGHT_Y = 7, LIGHT_Z = 2; // Position of the light source

        private bool canDraw = false;

        private float ANGLE_STEP = 40f; // Rotation angle (degress/second)

        // Prepare a view and projection matrix for generating a shadow map
        private Matrix4 viewMatrixFromLight;
        private Matrix4 projMatrixFromLight;

        // Prepare a view projection matrix for regular drawing
        private Matrix4 modelMatrix;
        private Matrix4 viewMatrix;
        private Matrix4 projMatrix;
        private Matrix4 mvpMatrix;

        private float currentAngle = 0f; // Current rotation angle (degrees)

        // A model view projection matrix from light source (for triangle)
        private Matrix4 mvpMatrixFromLight_t;

        // A model view projection matrix from light source (for plane)
        private Matrix4 mvpMatrixFromLight_p;

        private class FrameBuffer
        {
            public int id;
            public int texture;
        }
        private FrameBuffer fbo;

        private class ShaderProgram
        {
            public int id;
            public int a_Position;
            public int u_MvpMatrix;
        }

        private class ShadowProgram : ShaderProgram
        {
        }
        private ShadowProgram shadowProgram = new ShadowProgram();

        private class NormalProgram : ShaderProgram
        {
            public int a_Color;
            public int u_MvpMatrixFromLight;
            public int u_ShadowMap;
        }
        private NormalProgram normalProgram = new NormalProgram();

        private class Obj
        {
            public VertexBufferObject vertexBuffer;
            public VertexBufferObject colorBuffer;
            public ElementBufferObject indexBuffer;
            public int numIndices;
        }

        private class Triangle : Obj { }
        private Triangle triangle;

        private class Plane : Obj { }
        private Plane plane;

        private class VertexBufferObject
        {
            public int id;
            public int num;
            public VertexAttribPointerType type;
        }

        private class ElementBufferObject
        {
            public int id;
            public DrawElementsType type;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Shadow";

            // Load shader from files
            string vShadowShaderSource = null;
            string fShadowShaderSource = null;
            string vShaderSource = null;
            string fShaderSource = null;
            ShaderLoader.LoadShader("./Shaders/ShadowVertexShader.glsl", out vShadowShaderSource);
            ShaderLoader.LoadShader("./Shaders/ShadowFragmentShader.glsl", out fShadowShaderSource);
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out fShaderSource);
            if (vShadowShaderSource == null || fShadowShaderSource == null ||
                vShaderSource == null || fShaderSource == null)
            {
                Logger.Append("Failed to load vertex or framgment shader");
                return;
            }

            // Initialize shaders for generating a shadow map
            shadowProgram.id = ShaderLoader.CreateProgram(vShadowShaderSource, fShadowShaderSource);
            if (shadowProgram.id == 0)
            {
                Logger.Append("Failed to crate a shadow program");
                return;
            }
            shadowProgram.a_Position = GL.GetAttribLocation(shadowProgram.id, "a_Position");
            shadowProgram.u_MvpMatrix = GL.GetUniformLocation(shadowProgram.id, "u_MvpMatrix");
            if (shadowProgram.a_Position < 0 || shadowProgram.u_MvpMatrix < 0)
            {
                Logger.Append("Failed to get the storage location of attribute or uniform variable from shadowProgram");
                return;
            }

            // Initialize shaders for regular drawing
            normalProgram.id = ShaderLoader.CreateProgram(vShaderSource, fShaderSource);
            if (normalProgram.id == 0)
            {
                Logger.Append("Failed to crate a normal program");
                return;
            }
            normalProgram.a_Position = GL.GetAttribLocation(normalProgram.id, "a_Position");
            normalProgram.a_Color = GL.GetAttribLocation(normalProgram.id, "a_Color");
            normalProgram.u_MvpMatrix = GL.GetUniformLocation(normalProgram.id, "u_MvpMatrix");
            normalProgram.u_MvpMatrixFromLight = GL.GetUniformLocation(normalProgram.id, "u_MvpMatrixFromLight");
            normalProgram.u_ShadowMap = GL.GetUniformLocation(normalProgram.id, "u_ShadowMap");
            if (normalProgram.a_Position < 0 || normalProgram.a_Color < 0 ||
                 normalProgram.u_MvpMatrix < 0 || normalProgram.u_MvpMatrixFromLight < 0 ||
                 normalProgram.u_ShadowMap < 0)
            {
                Logger.Append("Failed to get the storage location of attribute or uniform variable from normalProgram");
                return;
            }

            // Set the vertex information
            triangle = InitVertexBuffersForTriangle();
            plane = InitVertexBuffersForPlane();
            if (triangle == null || plane == null)
            {
                Logger.Append("Failed to set the vertex information");
                return;
            }

            // Initialize framebuffer object (FBO)
            fbo = InitFramebufferObject();
            if (fbo == null)
            {
                Logger.Append("Failed to intialize the framebuffer object (FBO)");
                return;
            }
            GL.ActiveTexture(TextureUnit.Texture0); // Set a texture object to the texture unit
            GL.BindTexture(TextureTarget.Texture2D, fbo.texture);

            // Set the clear color and enable the depth test
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest); // GL.Enable(EnableCap.CullFace);

            viewMatrixFromLight = Matrix4.LookAt(LIGHT_X, LIGHT_Y, LIGHT_Z, 0f, 0f, 0f, 0f, 1f, 0f);
            viewMatrix = Matrix4.LookAt(0f, 7f, 9f, 0f, 0f, 0f, 0f, 1f, 0f);
            SetProjMatrixes();

            canDraw = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (canDraw)
            {
                currentAngle = Animate(currentAngle, e.Time);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo.id); // Change the drawing destination to FBO
                GL.Viewport(0, 0, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT); // Set viewport for FBO
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Clear FBO

                GL.UseProgram(shadowProgram.id); // Set shaders for generating a shadow map
                // Draw the triangle and the plane (for generating a shadow map)
                DrawTriangle(shadowProgram, triangle, viewMatrixFromLight, projMatrixFromLight);
                mvpMatrixFromLight_t = mvpMatrix; // Used later
                DrawPlane(shadowProgram, plane, viewMatrixFromLight, projMatrixFromLight);
                mvpMatrixFromLight_p = mvpMatrix; // Used later

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // Change the drawing destination to color buffer
                GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.UseProgram(normalProgram.id); // Set the shader for regular drawing
                GL.Uniform1(normalProgram.u_ShadowMap, 0); // Pass 0 because gl.TEXTURE0 is enabled
                // Draw the triangle and plane (for regular drawing)
                GL.UniformMatrix4(normalProgram.u_MvpMatrixFromLight, false, ref mvpMatrixFromLight_t);
                DrawTriangle(normalProgram, triangle, viewMatrix, projMatrix);
                GL.UniformMatrix4(normalProgram.u_MvpMatrixFromLight, false, ref mvpMatrixFromLight_p);
                DrawPlane(normalProgram, plane, viewMatrix, projMatrix);
            }

            SwapBuffers();
        }

        private void DrawTriangle(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            // Set rotate angle to model matrix and draw triangle
            modelMatrix = Matrix4.CreateRotationY(currentAngle * (float)Math.PI / 180f);
            Draw(program, o, viewMatrix, projMatrix);
        }

        private void DrawPlane(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            modelMatrix =
                Matrix4.CreateRotationY(planeAngle) *
                Matrix4.CreateRotationZ(planeAngle);
            Draw(program, o, viewMatrix, projMatrix);
        }

        private void Draw(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            InitAttributeVariable(program.a_Position, o.vertexBuffer);
            if (ReferenceEquals(program.GetType(), typeof(NormalProgram)))
            {
                InitAttributeVariable((program as NormalProgram).a_Color, o.colorBuffer);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, o.indexBuffer.id);

            // Calculate the model view project matrix and pass it to u_MvpMatrix
            mvpMatrix = modelMatrix * viewMatrix * projMatrix;
            GL.UniformMatrix4(program.u_MvpMatrix, false, ref mvpMatrix);

            GL.DrawElements(PrimitiveType.Triangles, o.numIndices, DrawElementsType.UnsignedInt, 0);
        }

        // Assign the buffer objects and enable the assignment
        private void InitAttributeVariable(int a_attribute, VertexBufferObject buffer)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.id);
            GL.VertexAttribPointer(a_attribute, buffer.num, buffer.type, false, 0, 0);
            GL.EnableVertexAttribArray(a_attribute);
        }

        private Plane InitVertexBuffersForPlane()
        {
            // Create face
            //  v1------v0
            //  |        | 
            //  |        |
            //  |        |
            //  v2------v3

            // Vertex coordinates
            float[] vertices = new float[]
            {
                3.0f, -1.7f, 2.5f, -3.0f, -1.7f, 2.5f, -3.0f, -1.7f, -2.5f, 3.0f, -1.7f, -2.5f // v0-v1-v2-v3
            };

            // Colors
            float[] colors = new float[]
            {
                1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f
            };

            // Indices of the vertices
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            Plane o = new Plane(); // Utilize Object object to return multiple buffer objects together

            // Write vertex information to buffer object
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.colorBuffer = InitArrayBufferForLaterUse(colors, 3, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            if (o.vertexBuffer == null || o.colorBuffer == null || o.indexBuffer == null)
            {
                return null;
            }

            o.numIndices = indices.Length;

            // Unbind the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return o;
        }

        private Triangle InitVertexBuffersForTriangle()
        {
            // Create a triangle
            //       v2
            //      / | 
            //     /  |
            //    /   |
            //  v0----v1

            // Vertex coordinates
            float[] vertices = new float[]
            {
                -0.8f, 3.5f, 0f, 0.8f, 3.5f, 0f, 0f, 3.5f, 1.8f
            };

            // Colors
            float[] colors = new float[]
            {
                1f, 0.5f, 0f, 1f, 0.5f, 0f, 1f, 0f, 0f
            };

            // Indices of the vertices
            int[] indices = new int[]
            {
                0, 1, 2
            };

            Triangle o = new Triangle(); // Utilize Object object to return multiple buffer objects together

            // Write vertex information to buffer object
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.colorBuffer = InitArrayBufferForLaterUse(colors, 3, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            if (o.vertexBuffer == null || o.colorBuffer == null || o.indexBuffer == null)
            {
                return null;
            }

            o.numIndices = indices.Length;

            // Unbind the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return o;
        }

        private VertexBufferObject InitArrayBufferForLaterUse(float[] data, int num, VertexAttribPointerType type)
        {
            // Create a buffer object
            VertexBufferObject buffer = new VertexBufferObject();
            GL.GenBuffers(1, out buffer.id);
            if (buffer.id < 0)
            {
                Logger.Append("Failed to create the buffer object");
                return null;
            }
            // Write date into the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.id);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

            // Store the necessary information to assign the object to the attribute variable later
            buffer.num = num;
            buffer.type = type;

            return buffer;
        }

        private ElementBufferObject InitElementArrayBufferForLaterUse(int[] data, DrawElementsType type)
        {
            // Create a buffer object
            ElementBufferObject buffer = new ElementBufferObject();
            GL.GenBuffers(1, out buffer.id);
            if (buffer.id < 0)
            {
                Logger.Append("Failed to create the buffer object");
                return null;
            }
            // Write date into the buffer object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffer.id);
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

            buffer.type = type;

            return buffer;
        }

        private FrameBuffer InitFramebufferObject()
        {
            int texture, depthBuffer;

            // Create a frame buffer object (FBO)
            FrameBuffer frameBuffer = new FrameBuffer();
            GL.GenFramebuffers(1, out frameBuffer.id);
            if (frameBuffer.id < 0)
            {
                Logger.Append("Failed to create frame buffer object");
                return null;
            }

            // Create a texture object and set its size and parameters
            GL.GenTextures(1, out texture);
            if (texture < 0)
            {
                Logger.Append("Failed to create texture object");
                return null;
            }
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

            // Create a renderbuffer object and Set its size and parameters
            GL.GenRenderbuffers(1, out depthBuffer); // Create a renderbuffer object
            if (depthBuffer < 0)
            {
                Logger.Append("Failed to create renderbuffer object");
                return null;
            }
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT);

            // Attach the texture and the renderbuffer object to the FBO
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer.id);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

            // Check if FBO is configured correctly
            FramebufferErrorCode e = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (e != FramebufferErrorCode.FramebufferComplete)
            {
                Logger.Append("Frame buffer object is incomplete: " + e.ToString());
                return null;
            }

            frameBuffer.texture = texture; // keep the required object

            // Unbind the buffer object
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            return frameBuffer;
        }

        private float Animate(float angle, double elapsed)
        {
            float newAngle = angle + (ANGLE_STEP * (float)elapsed);
            return (newAngle % 360);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetProjMatrixes();
        }

        private void SetProjMatrixes()
        {
            projMatrixFromLight = Matrix4.CreatePerspectiveFieldOfView(fovyAngleFromLight, OFFSCREEN_WIDTH / (float)OFFSCREEN_HEIGHT, 1f, 100f);
            projMatrix = Matrix4.CreatePerspectiveFieldOfView(fovyAngle, ClientSize.Width / (float)ClientSize.Height, 1f, 100f);
        }
    }
}
