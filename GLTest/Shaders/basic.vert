#version 150

in vec2 position;
in vec3 color;

out vec3 Color;

uniform float time;


float rand(vec2 co) {
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main()
{
	Color = color;
	float r = rand(position * time) - 0.5;
    gl_Position = vec4(position + vec2(0.01 * r, 0.01 * r), 0.0, 1.0);
}


