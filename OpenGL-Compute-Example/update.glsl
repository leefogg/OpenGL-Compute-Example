#version 430

layout(std430, binding = 0) buffer particles
{
  vec4 Particles[];
};

uniform vec2 cursor;
uniform float deltaTime = 0;

layout(local_size_x = 1024, local_size_y = 1, local_size_z = 1) in;

const float G = 0.0001;

void main() {
	// Get easier references to positions/velocities
	uint id = gl_GlobalInvocationID.x;
	vec4 particle = Particles[id];
	vec2 position = particle.xy;
	vec2 velocity = particle.zw;

	//Build our 2D acceleration using universal gravitation
	vec2 r = cursor - position;
	float magnitude = length(r);
	r = normalize(r);
	vec2 accel2D = r * G / (magnitude * magnitude);

	//Update our velocity and position
	velocity += accel2D;

	position += velocity * deltaTime;

	//Save our position and velocities for displaying
	Particles[id] = vec4(position, velocity);
}