#version 150

out vec4 outColor;
in vec3 Color;
in vec2 Texcoord;

uniform float time;

uniform sampler2D tex;

void main()
{
	vec4 text = texture(tex, Texcoord);

	outColor = text.a * text  + (1-text.a) * vec4(Color, 1.0);
	
}