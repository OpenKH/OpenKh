uniform sampler2D Texture : hint_default_white, source_color;
uniform vec2 UVScrollSpeed = vec2(0);

vec4 Run(vec2 uv)
{
	vec2 scrolledPos = UVScrollSpeed * TIME;
	return texture(Texture, uv + scrolledPos);
}
