#version 330

uniform sampler2D DiffuseTexture;

// Input = output from vertex shader.
in vec2 Texcoord;
in float Height;

out vec4 OutputColor;

void main()  
{ 
  OutputColor = (Height*0.25+0.5)*2.0 * texture(DiffuseTexture, Texcoord*10.0) + (Height*0.25)*2.0 * vec4(1,1,1,1);  
}