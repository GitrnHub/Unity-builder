# AI modification summary

- Base project: savatkinv/VoxelGame (MIT).
- Standalone defaults to the upstream High Fidelity URP quality level.
- High Fidelity uses HDR, 4x MSAA, 4096 shadow maps, four cascades, soft shadows and SSAO.
- Voxel chunk Shader Graphs are upgraded from URP Unlit to URP Lit during Editor load and Cloud Build pre-export, enabling real directional lighting, shadow caster/depth passes and proper SSAO participation.
- Chunk MeshRenderers explicitly cast and receive shadows.
- High Fidelity now requests depth + opaque color, renders at 1.1 scale, uses 180 m shadow distance, HDR color grading and a 64-size grading LUT.
- SSAO intensity/radius/quality increased for stronger voxel contact shading.
- Runtime cinematic stack uses ACES tonemapping, stronger Bloom, color grading, atmospheric fog, reduced ambient fill, warm high-intensity sunlight, film grain and soft high-resolution shadows.
- Added a depth-occluded URP ScriptableRendererFeature for screen-space sun shafts / god rays; the old translucent-geometry approximation is retired.
- Simplified Chinese UI localization and Noto Sans CJK SC dynamic font are enabled.
- Graphy monitoring UI is now included in persistent-object localization: avg/fps/ms/reserved/allocated/mono/ram are translated to compact Chinese labels.
- Serialized menu UI labels translated: 17.
