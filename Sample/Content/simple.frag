#version 330

// Input = output from vertex shader.
in vec3 inNormal;
in vec2 inTexcoord;

out vec4 outputColor;

void main()  
{     
  outputColor = vec4(abs(inNormal), 1.0);  
}