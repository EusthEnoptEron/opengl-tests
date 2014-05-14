#version 150

out vec4 outColor;
in vec3 Color;
in vec2 Texcoord;

uniform float time;

uniform sampler2D tex1;
uniform sampler2D tex2;
uniform float opacity;


void main()
{
	vec2 coords = Texcoord;
	if(fract(Texcoord.y) > 0.5) {
		coords.y = 1 - Texcoord.y;
		coords.x += sin(2*time + coords.y * 10) * 0.1;
	}

	vec4 texCol1 = texture(tex1, coords);
	vec4 texCol2 = texture(tex2, coords);
	vec4 color   = vec4(Color, 1.0);

	// Interpolate based on alpha values (0  will entirely use the color gradient, 1 will entirely use the texture)
	outColor = ((mix(color, texCol1, texCol1.a) + mix(color, texCol2, texCol2.a)) / 2) * vec4(1, 1, 1, opacity);
	//outColor = color;
	
}