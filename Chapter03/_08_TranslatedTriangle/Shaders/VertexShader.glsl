#version 110

attribute vec4 a_Position;
uniform vec4 u_Translation;

void main()
{
    gl_Position = a_Position + u_Translation;
}
