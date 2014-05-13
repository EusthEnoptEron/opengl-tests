#version 150

in vec2 position;
in vec3 color;
in vec2 texcoord;

out vec3 Color;
out vec2 Texcoord;


uniform float time;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;


float rand(vec2 co) {
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main()
{
	Texcoord = texcoord;
	Color = color;
	float r = rand(position * time) - 0.5;

    gl_Position = projection * view * model * vec4(position + vec2(0.01 * r, 0.01 * r), (sin(3 * time + (position.x - position.y)) + cos(2 * time + (position.x + position.y))) / 2 , 1.0);
}


