#version 150

in vec2 vt_texcoord;
uniform sampler2D image;

out vec4 color;

void main() {
	//color = blur()- sobel() + vec4(0,0,0,1);
	color = texture(image, vt_texcoord);
	//color = vec4(1,0,0,1);
}