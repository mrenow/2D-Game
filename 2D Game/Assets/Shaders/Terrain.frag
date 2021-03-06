﻿#version 400 core

in vec2 uv;
in float lighting;

uniform sampler2D texture;

const float ambientLighting = 0.1;

out vec4 fragment;

void main(void) {
	vec4 tex = texture2D(texture, uv);
	if (tex.w == 0) discard;
	fragment = max(lighting, ambientLighting) * tex;
}