#version 430

uniform mat4 model;
uniform mat4 projection;

layout(std430, binding = 0) buffer particles
{
  vec4 Particles[];
};

layout(location = 0) in vec3 position;

void main() {
	uint id = gl_InstanceID;
	vec4 particle = Particles[id];
	vec2 position = particle.xy;
	gl_Position = vec4(position, 0.0, 1.0);
}