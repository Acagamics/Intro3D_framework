#version 330 core

layout(std140) uniform PerFrame
{
  mat4 ViewProjection;
};
layout(std140) uniform PerBlock
{
  vec3 Position;
  float scale;
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
  gl_Position = ViewProjection * vec4(inPosition * scale + Position, 1.0);

  // Simple pass through
  Normal = inNormal;
  Texcoord = inTexcoord;
}  