#version 410
#extension GL_ARB_shading_language_include : require
uniform vec2 u_resolution;
in vec2 tex;
in vec3 pos;
out vec4 color;
uniform sampler2D _texture;
void main(){
  color = vec4(pos,1);//vec3(rand(pos.xy),rand(pos.xy),rand(pos.xy));
  if(tex != vec2(0,0))
  color = texture(_texture,tex);
}