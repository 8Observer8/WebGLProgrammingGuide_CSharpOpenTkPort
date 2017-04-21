#version 330

precision mediump float;

uniform vec4 u_FragColor;
out vec4 fragColor;

void main()
{
    fragColor = u_FragColor;
}
