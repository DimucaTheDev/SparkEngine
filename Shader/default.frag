//#version 330 core
//in vec2 texCoord;
//out vec4 FragColor;
//precision mediump float;
//varying vec2 pos;
//uniform sampler2D texture0;
//void main(){
//	gl_FragColor = vec4(pos.x,0.,1.,1.);//texture(texture0, texCoord);	
//	
//}
#version 120
precision mediump float;
varying vec2 pos;
varying in float random1;
varying in float random2;
varying in float random3;
uniform vec2 u_resolution;
void main(){
	//fract(sin(dot(pos.xy/0.001+1,vec2(12.9898,78.233)))*43758.5453123)
	gl_FragColor = vec4(random1, random2, random3, 1.);
}