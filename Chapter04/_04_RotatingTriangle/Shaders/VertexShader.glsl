#version 110

uniform mat4 u_ModelMatrix;
attribute vec4 a_Position;

void main()
{
    gl_Position = u_ModelMatrix * a_Position;
}
