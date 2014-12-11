#version 330

layout(std140) uniform PerObject
{
  mat4 WorldViewProjection;
  vec3 CameraPosition;
};

layout(location = 0) in vec3 inPosition;

out vec3 Texcoord;

void main(void)
{
  gl_Position = WorldViewProjection * vec4(inPosition+CameraPosition, 1.0);
  gl_Position.z = gl_Position.w-0.0001;
  Texcoord = inPosition;
}
