#version 150

out vec4 outColor;
in vec3 Color;
in vec2 Texcoord;

uniform sampler2D tex;

void main()
{
	// Interpolate based on alpha values (0  will entirely use the color gradient, 1 will entirely use the texture)
	outColor = texture(tex, Texcoord) + vec4(0,0,0,1); //
	//outColor = color;
	
}