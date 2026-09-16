#version 460 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;
in float Height;

out vec4 FragColor;

uniform sampler2D textureAtlas;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform float ambientStrength;
uniform float timeOfDay;

void main() {
    vec4 texColor = texture(textureAtlas, TexCoord);
    if (texColor.a < 0.1) discard;
    
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;
    
    vec3 ambient = ambientStrength * ambientColor;
    
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    vec3 specular = spec * lightColor * 0.5;
    
    float fogDistance = length(FragPos - viewPos);
    float fogFactor = 1.0 / exp(fogDistance * 0.01);
    fogFactor = clamp(fogFactor, 0.0, 1.0);
    
    vec3 result = (ambient + diffuse + specular) * texColor.rgb;
    vec3 fogColor = vec3(0.5, 0.6, 0.7);
    result = mix(fogColor, result, fogFactor);
    
    FragColor = vec4(result, texColor.a);
}