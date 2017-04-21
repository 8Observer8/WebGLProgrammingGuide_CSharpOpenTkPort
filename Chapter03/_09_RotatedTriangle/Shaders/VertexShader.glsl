#version 110

// x' = x cosβ - y sinβ
// y' = x sinβ + y cosβ　Equation 3.3
// z' = z

attribute vec4 a_Position;
uniform float u_CosB;
uniform float u_SinB;

void main()
{
    gl_Position.x = a_Position.x * u_CosB - a_Position.y * u_SinB;
    gl_Position.y = a_Position.x * u_SinB + a_Position.y * u_CosB;
    gl_Position.z = a_Position.z;
    gl_Position.w = 1.0;
}
