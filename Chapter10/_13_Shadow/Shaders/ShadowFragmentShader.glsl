#version 120

precision mediump float;

void main()
{
    // Write the z-value in R
    gl_FragColor = vec4(gl_FragCoord.z, 0.0, 0.0, 0.0);
}
