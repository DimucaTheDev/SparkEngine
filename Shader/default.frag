#version 410
#extension GL_ARB_shading_language_include : require
uniform vec2 u_resolution;
in vec2 tex;
in vec3 pos;
in vec3 lightColor;
 out vec4 color;
uniform sampler2D _texture;
void main(){
  color = vec4( pos,1);
}