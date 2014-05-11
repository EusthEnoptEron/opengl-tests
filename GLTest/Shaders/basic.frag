#version 150

out vec4 outColor;
in vec3 Color;
uniform float time;

void main()
{
    outColor = vec4(Color, 1.0);
}