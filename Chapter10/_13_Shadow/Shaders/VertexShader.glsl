attribute vec4 a_Position;
attribute vec4 a_Color;
uniform mat4 u_MvpMatrix;
uniform mat4 u_MvpMatrixFromLight;
varying vec4 v_PositionFromLight;
varying vec4 v_Color;

void main()
{
    gl_Position = u_MvpMatrix * a_Position;
    v_PositionFromLight = u_MvpMatrixFromLight * a_Position;
    v_Color = a_Color;
}
