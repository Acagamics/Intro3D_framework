#version 330

uniform sampler2D DiffuseTexture;

// Input = output from vertex shader.
in vec3 Position;
in vec3 Normal;
in vec2 Texcoord;

out vec4 OutputColor;

const vec3 LightPosition = vec3(8.0, -8.0, 0.0);
const vec3 LightColor = vec3(20, 20, 17);
const float Ambient = 0.1;

void main()  
{     
  vec3 toLight = LightPosition - Position;
  float lightDistSq = dot(toLight, toLight);
  toLight *= inversesqrt(lightDistSq);

  float lighting = clamp(dot(Normal, toLight), Ambient, 1.0) / lightDistSq;

  vec3 textureColor = texture(DiffuseTexture, Texcoord).rgb;
  OutputColor = vec4(textureColor * lighting * LightColor, 1.0);  
}