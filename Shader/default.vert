#version 120
attribute vec3 aPosition;
attribute vec2 aTexCoord;
attribute float aRandom1;
attribute float aRandom2;
attribute float aRandom3;
varying float random1;
varying float random2;
varying float random3;
varying vec2 pos;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main(){
	pos = aTexCoord;
	random1 = aRandom1;
	random2 = aRandom2;
	random3 = aRandom3;
	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}