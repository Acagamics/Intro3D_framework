#version 330

// Input = output from vertex shader.
in vec3 Normal;
in vec2 Texcoord;

out vec4 OutputColor;

const vec3 GlobalLightDirection = vec3(0.333, -0.333, 0.333);

void main()  
{     
  float lighting = clamp(dot(Normal, GlobalLightDirection), 0.0f, 1.0f);
  OutputColor = vec4(lighting, lighting, lighting, 1.0);  
}