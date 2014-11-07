#version 330

uniform sampler2D DiffuseTexture;

// Input = output from vertex shader.
in vec2 Texcoord;

out vec4 OutputColor;

void main()  
{     
  OutputColor = vec4(Texcoord, 0,1) + texture(DiffuseTexture, Texcoord);  
}