#version 430

layout(std430, binding = 0) buffer pos
{
  vec4 positions[];
};

layout(std430, binding = 1) buffer vel
{
  vec4 velocities[];
};

uniform vec2 cursor;
uniform float deltaTime = 0;

layout(local_size_x = 1024, local_size_y = 1, local_size_z = 1) in;

const float G = 0.0001;

void main() {
  // Get easier references to positions/velocities
  vec4 position = positions[gl_GlobalInvocationID.x];
  vec4 velocity = velocities[gl_GlobalInvocationID.x];

  //Build our 2D acceleration using universal gravitation
  vec2 r;
  r.x = cursor.x - position.x;
  r.y = cursor.y - position.y;
  float magnitude = length(r);
  r = normalize(r);
  vec2 accel2D = r * (G) / (magnitude * magnitude);

  //Update our velocity and position
  velocity.x += accel2D.x;
  velocity.y += accel2D.y;

  position = position + velocity * deltaTime;

  //Save our position and velocities for displaying
  positions[gl_GlobalInvocationID.x] = position;
  velocities[gl_GlobalInvocationID.x] = velocity;
}