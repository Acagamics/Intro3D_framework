#version 330

// Vertex input.
layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inTexcoord;

// Output = input for fragment shader.
out vec2 Texcoord;

void main(void)
{
  gl_Position = vec4(inPosition, 0.0, 1.0);

  // Simply pass through
  Texcoord = inTexcoord;
}  