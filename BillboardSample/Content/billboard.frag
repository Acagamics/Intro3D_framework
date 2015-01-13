#version 330

uniform sampler2D DiffuseTexture;

// Input = output from vertex shader.
in vec2 Texcoord;
in vec4 Color;

out vec4 OutputColor;

void main()  
{     
  OutputColor = texture(DiffuseTexture, Texcoord) * Color;  
}