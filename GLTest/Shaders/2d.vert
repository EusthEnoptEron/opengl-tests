#version 150

in vec2 position;
in vec2 texcoord;

out vec2 vt_texcoord;
void main() {
	vt_texcoord = texcoord;
	gl_Position = vec4(position, 0, 1);
}