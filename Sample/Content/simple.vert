#version 330

layout(std140) uniform PerObject
{
  mat4 WorldViewProjection;
};

// Vertex input.
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inNormal;
layout(location = 2) in vec2 inTexcoord;

// Output = input for fragment shader.
out vec3 Normal;
out vec2 Texcoord;

void main(void)
{
  gl_Position = WorldViewProjection * vec4(inPosition, 1.0);

  // Simple pass through
  Normal = inNormal;
  Texcoord = inTexcoord;
}  