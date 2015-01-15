#version 330

uniform sampler2D DiffuseTexture;

layout(std140) uniform GlobalUniformbuffer
{
  mat4 WorldViewProjection;
  vec3 LightPosition; float padding0;
  vec3 LightColor; float padding1;
  vec3 MaterialColor;
};

// Input = output from vertex shader.
in vec3 Position;
in vec3 Normal;
in vec2 Texcoord;

out vec4 OutputColor;

const float Ambient = 0.1;

void main()  
{     
  vec3 toLight = LightPosition - Position;
  float lightDistSq = dot(toLight, toLight);
  toLight *= inversesqrt(lightDistSq);

  float lighting = clamp(dot(Normal, toLight), Ambient, 1.0) / lightDistSq;

  vec3 diffuseColor = texture(DiffuseTexture, Texcoord).rgb * MaterialColor;
  OutputColor = vec4(diffuseColor * lighting * LightColor, 1.0);  
}