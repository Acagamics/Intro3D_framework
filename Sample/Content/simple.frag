#version 330

uniform sampler2D DiffuseTexture;

// Input = output from vertex shader.
in vec3 Normal;
in vec2 Texcoord;

out vec4 OutputColor;

const vec3 GlobalLightDirection = vec3(0.333, -0.333, 0.333);
const float Ambient = 0.2f;

void main()  
{     
  float lighting = clamp(dot(Normal, GlobalLightDirection), Ambient, 1.0f);
  vec3 textureColor = texture(DiffuseTexture, Texcoord).rgb;
  OutputColor = vec4(textureColor * lighting, 1.0);  
}