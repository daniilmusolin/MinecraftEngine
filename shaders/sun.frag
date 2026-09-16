#version 460 core

in vec2 TexCoord;
out vec4 FragColor;

uniform vec3 sunColor;
uniform float timeOfDay;

void main() {
    vec2 center = TexCoord - 0.5;
    float dist = length(center);
    if (dist > 0.5) discard;
    
    float alpha = 1.0 - smoothstep(0.3, 0.5, dist);
    vec3 color = sunColor * (1.0 + 0.2 * (1.0 - dist * 2.0));
    
    FragColor = vec4(color, alpha);
}