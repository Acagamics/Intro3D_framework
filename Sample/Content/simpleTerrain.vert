#version 330

layout(std140) uniform PerObject
{
  mat4 WorldViewProjection;
  vec3 CameraPosition;
  float Time;
};

// Vertex input.
layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inTexcoord;
layout(location = 2) in float inHeight;

// Output = input for fragment shader.
out vec2 Texcoord;
out float Height;

void main(void)
{
  gl_Position = WorldViewProjection * vec4(inPosition.x, inHeight, inPosition.y, 1.0);

  // Simply pass through
  Texcoord = inTexcoord;
  Height = inHeight;
}  