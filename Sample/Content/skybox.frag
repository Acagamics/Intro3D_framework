#version 330

uniform samplerCube Cubemap;

in vec3 Texcoord;

out vec4 OutputColor;


void main()  
{     
  OutputColor = texture(Cubemap, Texcoord);  
}
