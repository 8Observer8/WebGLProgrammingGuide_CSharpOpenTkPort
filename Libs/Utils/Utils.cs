using System;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Utils
{
    public class ShaderLoader
    {
        ///<summary>
        ///Create a program object and make current
        ///</summary>
        ///<param name="vShader">a vertex shader program</param>
        ///<param name="fShader">a fragment shader program</param>
        ///<param name="program">created program</param>
        ///<returns>
        ///return true, if the program object was created and successfully made current
        ///</returns>
        public static bool InitShaders(string vShaderSource, string fShaderSource, out int program)
        {
            program = CreateProgram(vShaderSource, fShaderSource);
            if (program == 0)
            {
                Logger.Append("Failed to create program");
                return false;
            }

            GL.UseProgram(program);

            return true;
        }

        ///<summary>
        ///Load a shader from a file
        ///</summary>
        ///<param name="errorOutputFileName">a file name for error messages</param>
        ///<param name="fileName">a file name to a shader</param>
        ///<param name="shaderSource">a shader source string</param>
        public static void LoadShader(string shaderFileName, out string shaderSource)
        {
            if (File.Exists(Logger.logFileName))
            {
                // Clear File
                File.WriteAllText(Logger.logFileName, "");
            }

            shaderSource = null;

            using (StreamReader sr = new StreamReader(shaderFileName))
            {
                shaderSource = sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Create a program
        /// </summary>
        /// <param name="vShader">Vertex Shader Source</param>
        /// <param name="fShader">Fragment Shader Source</param>
        /// <returns>a program Id or 0 if an error occurs creating the program object</returns>
        public static int CreateProgram(string vShader, string fShader)
        {
            // Create shader object
            int vertexShader = LoadShader(ShaderType.VertexShader, vShader);
            int fragmentShader = LoadShader(ShaderType.FragmentShader, fShader);
            if (vertexShader == 0 || fragmentShader == 0)
            {
                return 0;
            }

            // Create a program object
            int program = GL.CreateProgram();
            if (program == 0)
            {
                return 0;
            }

            // Attach the shader objects
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            // Link the program object
            GL.LinkProgram(program);

            // Check the result of linking
            int status;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
            {
                string errorString = string.Format("Failed to link program: {0}" + Environment.NewLine, GL.GetProgramInfoLog(program));
                Logger.Append(errorString);
                GL.DeleteProgram(program);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                return 0;
            }

            return program;
        }

        private static int LoadShader(ShaderType shaderType, string shaderSource)
        {
            // Create shader object
            int shader = GL.CreateShader(shaderType);
            if (shader == 0)
            {
                Logger.Append("Unable to create shader");
                return 0;
            }

            // Set the shader program
            GL.ShaderSource(shader, shaderSource);

            // Compile the shader
            GL.CompileShader(shader);

            // Check the result of compilation
            int status;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                string errorString = string.Format("Failed to compile {0} shader: {1}", shaderType.ToString(), GL.GetShaderInfoLog(shader));
                Logger.Append(errorString);
                GL.DeleteShader(shader);
                return 0;
            }

            return shader;
        }
    }

    public class Logger
    {
        public static string logFileName = "info.txt";

        /// <summary>
        /// Write a message to a log file
        /// </summary>
        /// <param name="message">a message that will append to a log file</param>
        public static void Append(string message)
        {
            File.AppendAllText(logFileName, message + Environment.NewLine);
            Console.WriteLine(message);
        }
    }
}
