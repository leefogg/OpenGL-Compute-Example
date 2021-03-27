#version 430

uniform mat4 model;
uniform mat4 projection;

layout(std430, binding = 0) buffer positions
{
  vec4 data[];
};

layout(location = 0) in vec3 position;

void main() {
	gl_Position = vec4(data[gl_InstanceID].xyz, 1.0);
}