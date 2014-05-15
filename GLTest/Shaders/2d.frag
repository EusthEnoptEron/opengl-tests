#version 150

in vec2 vt_texcoord;
out vec4 color;

uniform sampler2D texFramebuffer;

const float[] mask = float[] 
	(-5, 2, 5,
	 -2, 1, 2,
	 -5, 2, 5);

const float blurSizeH = 1.0 / 300.0;
const float blurSizeV = 1.0 / 200.0;

void main() {
	vec4 col;
	float divisor = 0;
	int index = 0;

	for( int x = -1; x <= 1; x++ ) {
		for( int y = -1; y <= 1; y++) {
			index = 4 + (y * 3 + x);
			if(index < 0 || index >= 9) continue;
			
			divisor  += mask[index];
			col += mask[index] * texture(texFramebuffer, vt_texcoord + vec2(x * blurSizeH, y * blurSizeV));		
		}
	}

	if(divisor > 0)
		color = col / divisor;
	else 
		color = vec4(0,0,0,1);
}