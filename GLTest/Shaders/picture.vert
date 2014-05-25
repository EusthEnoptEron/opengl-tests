#version 150

in vec2 position;
in vec2 texcoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform sampler2D image;
out vec2 vt_texcoord;

void main() {
	vt_texcoord = texcoord;

	vec4 color = texture(image, texcoord);
	float avg = (color.r + color.b + color.g) / 3;
	avg = -length(color);

	gl_Position = projection * view * model * vec4(position.x, avg*100, position.y, 1);
}