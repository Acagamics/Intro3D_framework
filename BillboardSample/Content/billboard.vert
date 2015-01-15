#version 330

layout(std140) uniform GlobalUniformbuffer
{
  mat4 WorldViewProjection;
  vec3 LightPosition; float padding0;
  vec3 LightColor; float padding1;
  vec4 MaterialColor;
};

// Vertex input.
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inTexcoord;
layout(location = 2) in vec4 inColor;

// Output = input for fragment shader.
out vec2 Texcoord;
out vec4 Color;

void main(void)
{
  gl_Position = WorldViewProjection * vec4(inPosition, 1.0);

  // Simple pass through
  Texcoord = inTexcoord;
  Color = inColor;
}  