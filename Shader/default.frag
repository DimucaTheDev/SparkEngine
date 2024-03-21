#version 410
#extension GL_ARB_shading_language_include : require
uniform vec2 u_resolution;
in vec2 tex;
in vec3 pos;
in vec3 lightColor;
out vec4 color;
in vec3 FragPos;
uniform sampler2D _texture;
void main(){
  float strength = 0.1;
  color = vec4(1 * strength * pos,1);
}