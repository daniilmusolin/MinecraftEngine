#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float timeOfDay;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;
out float Height;

void main() {
    vec4 worldPos = model * vec4(aPosition, 1.0);
    gl_Position = projection * view * worldPos;
    TexCoord = aTexCoord;
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(model))) * aNormal;
    Height = aPosition.y;
}