attribute vec3 aPosition;
attribute vec2 aTexCoord;
varying out vec3 pos;
varying out vec2 tex;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main(){
	pos = aPosition;
	tex = aTexCoord;
	gl_Position = vec4(aPosition*0.5, 1) * model * view * projection ;
}