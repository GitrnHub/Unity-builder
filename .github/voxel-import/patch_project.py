from pathlib import Path
import re

# Prefer the upstream project's existing High Fidelity URP quality level for Standalone.
quality = Path('ProjectSettings/QualitySettings.asset')
text = quality.read_text(encoding='utf-8')
text = text.replace('  m_CurrentQuality: 1', '  m_CurrentQuality: 2', 1)
text = text.replace('    Standalone: 1', '    Standalone: 2', 1)
quality.write_text(text, encoding='utf-8')

# SSAO uses depth information. Explicitly request a depth texture in the High Fidelity URP asset.
urp = Path('Assets/Settings/URP-HighFidelity.asset')
text = urp.read_text(encoding='utf-8')
text = text.replace('  m_RequireDepthTexture: 0', '  m_RequireDepthTexture: 1', 1)
urp.write_text(text, encoding='utf-8')

# Strengthen the upstream SSAO renderer feature slightly for stronger voxel contact shading.
renderer = Path('Assets/Settings/URP-HighFidelity-Renderer.asset')
text = renderer.read_text(encoding='utf-8')
text = text.replace('    Intensity: 0.5', '    Intensity: 0.72', 1)
text = text.replace('    Radius: 0.25', '    Radius: 0.35', 1)
renderer.write_text(text, encoding='utf-8')

translations = {
    'Play': '开始游戏', 'Start': '开始', 'Start Game': '开始游戏', 'New Game': '新游戏',
    'Continue': '继续', 'Resume': '继续游戏', 'Main Menu': '主菜单', 'Menu': '菜单',
    'Settings': '设置', 'Options': '选项', 'Credits': '制作人员', 'Exit': '退出',
    'Quit': '退出', 'Quit Game': '退出游戏', 'Back': '返回', 'Apply': '应用',
    'Save': '保存', 'Load': '读取', 'Delete': '删除', 'Cancel': '取消',
    'Confirm': '确认', 'Yes': '是', 'No': '否', 'OK': '确定',
    'Graphics': '画面', 'Video': '画面', 'Audio': '音频', 'Controls': '控制',
    'Language': '语言', 'Quality': '画质', 'Resolution': '分辨率', 'Fullscreen': '全屏',
    'Windowed': '窗口模式', 'VSync': '垂直同步', 'Volume': '音量',
    'Master Volume': '主音量', 'Music': '音乐', 'Sound': '声音', 'SFX': '音效',
    'Sensitivity': '灵敏度', 'Mouse Sensitivity': '鼠标灵敏度',
    'Field of View': '视野', 'FOV': '视野', 'Render Distance': '渲染距离',
    'View Distance': '视距', 'Brightness': '亮度', 'Gamma': '伽马',
    'Inventory': '物品栏', 'Crafting': '合成', 'Health': '生命值',
    'Seed': '种子', 'World': '世界', 'World Name': '世界名称',
    'Create World': '创建世界', 'Load World': '加载世界',
    'Singleplayer': '单人游戏', 'Multiplayer': '多人游戏',
    'Forward': '前进', 'Backward': '后退', 'Left': '左', 'Right': '右',
    'Jump': '跳跃', 'Run': '奔跑', 'Sprint': '疾跑', 'Crouch': '蹲下',
    'Generating...': '正在生成...', 'Loading...': '正在加载...',
    'Paused': '已暂停', 'Pause': '暂停', 'Game Over': '游戏结束'
}

pattern = re.compile(r'^(\s*m_[Tt]ext:\s*)(.*)$')
changed = 0
for suffix in ('*.prefab', '*.unity'):
    for path in Path('Assets').rglob(suffix):
        try:
            lines = path.read_text(encoding='utf-8').splitlines(True)
        except UnicodeDecodeError:
            continue
        dirty = False
        output = []
        for line in lines:
            raw = line.rstrip('\r\n')
            ending = line[len(raw):]
            match = pattern.match(raw)
            if match:
                value = match.group(2).strip()
                quoted = value.startswith('"') and value.endswith('"')
                key = value[1:-1] if quoted else value
                if key in translations:
                    translated = translations[key]
                    if quoted:
                        translated = '"' + translated + '"'
                    line = match.group(1) + translated + ending
                    dirty = True
                    changed += 1
            output.append(line)
        if dirty:
            path.write_text(''.join(output), encoding='utf-8')

Path('AI_MODIFICATIONS.md').write_text(
    '# AI modification summary\n\n'
    '- Base project: savatkinv/VoxelGame (MIT).\n'
    '- Standalone defaults to the upstream High Fidelity URP quality level.\n'
    '- High Fidelity already provides HDR, 4x MSAA, 4096 shadow maps, four cascades, soft shadows and SSAO.\n'
    '- Depth texture enabled and SSAO strengthened for stronger voxel contact shading.\n'
    '- Runtime ACES tonemapping, Bloom, color grading, atmospheric fog and procedural god rays added.\n'
    '- Simplified Chinese UI localization and Noto Sans CJK SC dynamic font added.\n'
    f'- Serialized UI labels translated: {changed}.\n',
    encoding='utf-8'
)
print(f'Translated serialized UI labels: {changed}')
