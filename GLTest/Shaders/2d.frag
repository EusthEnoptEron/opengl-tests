#version 150

in vec2 vt_texcoord;
out vec4 color;

uniform sampler2D texFramebuffer;

void main() {
	color = texture(texFramebuffer, vt_texcoord) * 0.9;
}