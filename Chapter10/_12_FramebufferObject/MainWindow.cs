using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Utils;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace _12_FramebufferObject
{
    class MainWindow : GameWindow
    {
        // Size of off screen
        private const int OFFSCREEN_WIDTH = 256;
        private const int OFFSCREEN_HEIGHT = 256;

        private bool canDraw = false;

        private float currentAngle = 0f;
        float ANGLE_STEP = 30f; // Rotation angle (degress/second)

        private int texture;

        private Matrix4 modelMatrix;
        private Matrix4 viewMatrix;
        private Matrix4 projMatrix;
        private Matrix4 mvpMatrix;

        private Matrix4 modelMatrixFBO;
        private Matrix4 viewMatrixFBO;
        private Matrix4 projMatrixFBO;
        private Matrix4 mvpMatrixFBO;

        private class FrameBuffer
        {
            public int id;
            public int texture;
        }
        private FrameBuffer frameBuffer;

        private class ShaderProgram
        {
            public int id;
            public int a_Position;
            public int a_TexCoord;
            public int u_MvpMatrix;
        }
        private ShaderProgram program = new ShaderProgram();

        private class Obj
        {
            public VertexBufferObject vertexBuffer;
            public VertexBufferObject texCoordBuffer;
            public ElementBufferObject indexBuffer;
            public int numIndices;
        }

        private class Cube : Obj { }
        private Cube cube;

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

            // Load shader from files
            string vShaderSource = null;
            string fShaderSource = null;
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out fShaderSource);
            if (vShaderSource == null || fShaderSource == null)
            {
                Logger.Append("Failed to load vertex or framgment shader");
                return;
            }

            // Initialize the shaders
            if (!ShaderLoader.InitShaders(vShaderSource, fShaderSource, out program.id))
            {
                Logger.Append("Failed to initialize the shaders");
                return;
            }

            // Get the storage location of attribute variables and uniform variables
            program.a_Position = GL.GetAttribLocation(program.id, "a_Position");
            program.a_TexCoord = GL.GetAttribLocation(program.id, "a_TexCoord");
            program.u_MvpMatrix = GL.GetUniformLocation(program.id, "u_MvpMatrix");
            if (program.a_Position < 0 || program.a_TexCoord < 0 || program.u_MvpMatrix < 0)
            {
                Logger.Append("Failed to get the storage location of a_Position, a_TexCoord, u_MvpMatrix");
                return;
            }

            // Set the vertex information
            cube = InitVertexBuffersForCube();
            plane = InitVertexBuffersForPlane();
            if (cube == null || plane == null)
            {
                Logger.Append("Failed to set the vertex information");
                return;
            }

            // Set texture
            texture = InitTextures();
            if (texture < 0)
            {
                Logger.Append("Failed to intialize the texture");
                return;
            }

            // Initialize framebuffer object (FBO)
            frameBuffer = InitFramebufferObject();
            if (frameBuffer == null)
            {
                Logger.Append("Failed to intialize the framebuffer object (FBO)");
                return;
            }

            // Enable depth test
            GL.Enable(EnableCap.DepthTest); // GL.Enable(EnableCap.CullFace);

            modelMatrix = Matrix4.Identity;
            viewMatrix = Matrix4.LookAt(0f, 0f, 5f, 0f, 0f, 0f, 0f, 1f, 0f); // Prepare view projection matrix for color buffer
            SetProjMatrix();

            projMatrixFBO = Matrix4.CreatePerspectiveFieldOfView(0.8f, OFFSCREEN_WIDTH / (float)OFFSCREEN_HEIGHT, 0.1f, 100f); // The projection matrix
            viewMatrixFBO = Matrix4.LookAt(0f, 2f, 7f, 0f, 0f, 0f, 0f, 1f, 0f); // The view matrix
            modelMatrixFBO = Matrix4.Identity;
            mvpMatrixFBO = modelMatrixFBO * viewMatrixFBO * projMatrixFBO;

            canDraw = true;
        }

        private Cube InitVertexBuffersForCube()
        {
            // Create a cube
            //    v6----- v5
            //   /|      /|
            //  v1------v0|
            //  | |     | |
            //  | |v7---|-|v4
            //  |/      |/
            //  v2------v3

            // Vertex coordinates
            float[] vertices = new float[]
            {
                1f, 1f, 1f, -1f, 1f, 1f, -1f, -1f, 1f, 1f, -1f, 1f,     // v0-v1-v2-v3 front
                1f, 1f, 1f, 1f, -1f, 1f, 1f, -1f, -1f, 1f, 1f, -1f,     // v0-v3-v4-v5 right
                1f, 1f, 1f, 1f, 1f, -1f, -1f, 1f, -1f, -1f, 1f, 1f,     // v0-v5-v6-v1 up
                -1f, 1f, 1f, -1f, 1f, -1f, -1f, -1f, -1f, -1f, -1f, 1f, // v1-v6-v7-v2 left
                -1f, -1f, -1f, 1f, -1f, -1f, 1f, -1f, 1f, -1f, -1f, 1f, // v7-v4-v3-v2 down
                1f, -1f, -1f, -1f, -1f, -1f, -1f, 1f, -1f, 1f, 1f, -1f  // v4-v7-v6-v5 back
            };

            // Texture coordinates
            float[] texCoords = new float[]
            {
                1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f,    // v0-v1-v2-v3 front
                0f, 1f, 0f, 0f, 1f, 0f, 1f, 1f,    // v0-v3-v4-v5 right
                1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f,    // v0-v5-v6-v1 up
                1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f,    // v1-v6-v7-v2 left
                0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f,    // v7-v4-v3-v2 down
                0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f     // v4-v7-v6-v5 back
            };

            // Indices of the vertices
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3,       // front
                4, 5, 6, 4, 6, 7,       // right
                8, 9, 10, 8, 10, 11,    // up
                12, 13, 14, 12, 14, 15, // left
                16, 17, 18, 16, 18, 19, // down
                20, 21, 22, 20, 22, 23  // back
            };

            Cube o = new Cube(); // Create the "Object" object to return multiple objects

            // Write vertex information to buffer object
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.texCoordBuffer = InitArrayBufferForLaterUse(texCoords, 2, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            if (o.vertexBuffer == null || o.texCoordBuffer == null || o.indexBuffer == null)
            {
                return null;
            }

            o.numIndices = indices.Length;

            // Unbind the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return o;
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
                1f, 1f, 0f, -1f, 1f, 0f, -1f, -1f, 0f, 1f, -1f, 0f // v0-v1-v2-v3
            };

            // Texture coordinates
            float[] texCoords = new float[]
            {
                1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f
            };

            // Indices of the vertices
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            Plane o = new Plane(); // Create the "Object" object to return multiple objects

            // Write vertex information to buffer object
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.texCoordBuffer = InitArrayBufferForLaterUse(texCoords, 2, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            if (o.vertexBuffer == null || o.texCoordBuffer == null || o.indexBuffer == null)
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

        private int InitTextures()
        {
            // Create a texture object
            int texture;
            GL.GenBuffers(1, out texture);
            if (texture < 0)
            {
                Logger.Append("Failed to create the Texture object");
                return -1;
            }

            // Get storage location of u_Sampler
            int u_Sampler = GL.GetUniformLocation(program.id, "u_Sampler");
            if (u_Sampler < 0)
            {
                Logger.Append("Failed to get the storage location of u_Sampler");
                return -1;
            }

            string fileName = "./Textures/sky_cloud.jpg";
            Bitmap image = null;
            try
            {
                image = new Bitmap(fileName);
            }
            catch (FileNotFoundException e)
            {
                Logger.Append("Failed to find the texture: " + fileName);
                return -1;
            }
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);
            // Pass the texure unit 0 to u_Sampler
            GL.Uniform1(u_Sampler, 0);

            return texture;
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
            GL.BindTexture(TextureTarget.Texture2D, texture); // Bind the object to target
            // Set the texture image
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            frameBuffer.texture = texture;

            // Create a renderbuffer object and Set its size and parameters
            GL.GenRenderbuffers(1, out depthBuffer); // Create a renderbuffer object
            if (depthBuffer < 0)
            {
                Logger.Append("Failed to create renderbuffer object");
                return null;
            }
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer); // Bind the object to target
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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            currentAngle = Animate(currentAngle, e.Time);

            if (canDraw)
            {
                Draw();
            }

            SwapBuffers();
        }

        private void Draw()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer.id); // Change the drawing destination to FBO
            GL.Viewport(0, 0, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT); // Set a viewport for FBO

            GL.ClearColor(new Color4(0.2f, 0.2f, 0.4f, 1f)); // Set clear color (the color is slightly changed)
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Clear FBO

            DrawTexturedCube(cube, texture); // Draw the cube

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // Change the drawing destination to color buffer
            GL.Viewport(0, 0, Width, Height); // Set the size of viewport back to that of render canvas

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Clear the color buffer

            DrawTexturedPlane(plane, frameBuffer.texture); // Draw the plane
        }

        private float cubeAngleX = 20f * (float)Math.PI / 180f;
        private void DrawTexturedCube(Obj o, int texture)
        {
            // Calculate a model matrix
            modelMatrixFBO =
                Matrix4.CreateRotationY(currentAngle * (float)Math.PI / 180f) *
                Matrix4.CreateRotationX(cubeAngleX);

            // Calculate the model view project matrix and pass it to u_MvpMatrix
            mvpMatrixFBO = modelMatrixFBO * viewMatrixFBO * projMatrixFBO;
            GL.UniformMatrix4(program.u_MvpMatrix, false, ref mvpMatrixFBO);

            DrawTexturedObject(o, texture);
        }

        private float planeAngleX = 20f * (float)Math.PI / 180f;
        private void DrawTexturedPlane(Obj o, int texture)
        {
            // Calculate a model matrix
            modelMatrix =
                Matrix4.CreateRotationY(currentAngle * (float)Math.PI / 180f) *
                Matrix4.CreateRotationX(planeAngleX) *
                Matrix4.CreateTranslation(0f, 0f, 1f);

            // Calculate the model view project matrix and pass it to u_MvpMatrix
            SetProjMatrix();
            mvpMatrix = modelMatrix * viewMatrix * projMatrix;
            GL.UniformMatrix4(program.u_MvpMatrix, false, ref mvpMatrix);

            DrawTexturedObject(o, texture);
        }

        private void DrawTexturedObject(Obj o, int texture)
        {
            // Assign the buffer objects and enable the assignment
            InitAttributeVariable(program.a_Position, o.vertexBuffer);      // Vertex coordinates
            InitAttributeVariable(program.a_TexCoord, o.texCoordBuffer);    // Texture coordinates

            // Bind the texture object to the target
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Draw
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, o.indexBuffer.id);
            GL.DrawElements(PrimitiveType.Triangles, o.numIndices, o.indexBuffer.type, 0);
        }

        // Assign the buffer objects and enable the assignment
        private void InitAttributeVariable(int a_attribute, VertexBufferObject buffer)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.id);
            GL.VertexAttribPointer(a_attribute, buffer.num, buffer.type, false, 0, 0);
            GL.EnableVertexAttribArray(a_attribute);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetProjMatrix();
        }

        private void SetProjMatrix()
        {
            projMatrix = Matrix4.CreatePerspectiveFieldOfView(0.8f, Width / (float)Height, 0.1f, 100f); // The projection matrix
        }
    }
}
