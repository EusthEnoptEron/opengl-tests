#version 150

in vec2 vt_texcoord;
out vec4 color;

uniform sampler2D texFramebuffer;

const float[] mask = float[] 
	(1, 4, 1,
	 4, 6, 4,
	 1, 4, 1);

const float blurSizeH = 1.0 / 300.0;
const float blurSizeV = 1.0 / 200.0;

vec4 blur() {
	vec4 col = vec4(0,0,0,0);
	float divisor = 0;
	int index = 0;

	for( int x = -1; x <= 1; x++ ) {
		for( int y = -1; y <= 1; y++) {
			index = 4 + (y * 3 + x);
			if(index < 0 || index >= 9) continue;
			
			divisor  += mask[index];
			// Old:
			//col += mask[index] * texture(texFramebuffer, vt_texcoord + vec2(x * blurSizeH, y * blurSizeV));		
			// New:
			col += mask[index] * textureOffset(texFramebuffer, vt_texcoord, ivec2(x*5, y*5));		

		}
	}

	if(divisor > 0)
		return col / divisor;
	else 
		return vec4(0,0,0,1);
}

vec4 sobel() {
	// Old:
	//vec4 s1 = texture(texFramebuffer, vt_texcoord - blurSizeH - blurSizeV);
	//vec4 s2 = texture(texFramebuffer, vt_texcoord + blurSizeH - blurSizeV);
	//vec4 s3 = texture(texFramebuffer, vt_texcoord - blurSizeH + blurSizeV);
	//vec4 s4 = texture(texFramebuffer, vt_texcoord + blurSizeH + blurSizeV);
	// New:
	vec4 s1 = textureOffset(texFramebuffer, vt_texcoord, ivec2(-1, -1) );
	vec4 s2 = textureOffset(texFramebuffer, vt_texcoord, ivec2(+1, -1) );
	vec4 s3 = textureOffset(texFramebuffer, vt_texcoord, ivec2(-1, +1) );
	vec4 s4 = textureOffset(texFramebuffer, vt_texcoord, ivec2(+1, +1) );

	vec4 sx = 4.0 * ((s4 + s3) - (s2 + s1));
	vec4 sy = 4.0 * ((s2 + s4) - (s1 + s3));

	return sqrt(sx * sx + sy * sy) + vec4(0,0,0,1);
}


void main() {
	//color = blur()- sobel() + vec4(0,0,0,1);
	color = texture(texFramebuffer, vt_texcoord) -sobel() + vec4(0,0,0,1);
}