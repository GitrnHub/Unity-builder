using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ChineseUiLocalizer : MonoBehaviour
{
    private static readonly Dictionary<string, string> Translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"Play", "开始游戏"}, {"Start", "开始"}, {"Start Game", "开始游戏"},
        {"New Game", "新游戏"}, {"Continue", "继续"}, {"Resume", "继续游戏"},
        {"Main Menu", "主菜单"}, {"Menu", "菜单"}, {"Settings", "设置"},
        {"Options", "选项"}, {"Credits", "制作人员"}, {"Exit", "退出"},
        {"Quit", "退出"}, {"Quit Game", "退出游戏"}, {"Back", "返回"},
        {"Apply", "应用"}, {"Save", "保存"}, {"Load", "读取"},
        {"Delete", "删除"}, {"Cancel", "取消"}, {"Confirm", "确认"},
        {"Yes", "是"}, {"No", "否"}, {"OK", "确定"},
        {"Graphics", "画面"}, {"Video", "画面"}, {"Audio", "音频"},
        {"Controls", "控制"}, {"Language", "语言"}, {"Quality", "画质"},
        {"Resolution", "分辨率"}, {"Fullscreen", "全屏"}, {"Windowed", "窗口模式"},
        {"VSync", "垂直同步"}, {"Volume", "音量"}, {"Master Volume", "主音量"},
        {"Music", "音乐"}, {"Sound", "声音"}, {"SFX", "音效"},
        {"Sensitivity", "灵敏度"}, {"Mouse Sensitivity", "鼠标灵敏度"},
        {"Field of View", "视野"}, {"FOV", "视野"}, {"Render Distance", "渲染距离"},
        {"View Distance", "视距"}, {"Brightness", "亮度"}, {"Gamma", "伽马"},
        {"Inventory", "物品栏"}, {"Crafting", "合成"}, {"Health", "生命值"},
        {"Seed", "种子"}, {"World", "世界"}, {"World Name", "世界名称"},
        {"Create World", "创建世界"}, {"Load World", "加载世界"},
        {"Singleplayer", "单人游戏"}, {"Multiplayer", "多人游戏"},
        {"Forward", "前进"}, {"Backward", "后退"}, {"Left", "左"}, {"Right", "右"},
        {"Jump", "跳跃"}, {"Run", "奔跑"}, {"Sprint", "疾跑"}, {"Crouch", "蹲下"},
        {"Generating...", "正在生成..."}, {"Loading...", "正在加载..."},
        {"Paused", "已暂停"}, {"Pause", "暂停"}, {"Game Over", "游戏结束"}
    };

    private Font legacyFont;
    private TMP_FontAsset tmpFont;
    private float nextScan;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<ChineseUiLocalizer>() != null) return;
        GameObject go = new GameObject("[AI] Chinese UI Localizer");
        DontDestroyOnLoad(go);
        go.AddComponent<ChineseUiLocalizer>();
    }

    private void Awake()
    {
        LoadFonts();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start() => LocalizeAllLoadedScenes();

    private void Update()
    {
        if (Time.unscaledTime < nextScan) return;
        nextScan = Time.unscaledTime + 0.75f;
        LocalizeAllLoadedScenes();
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => LocalizeScene(scene);

    private void LoadFonts()
    {
        legacyFont = Resources.Load<Font>("AIEnhancements/NotoSansCJKsc-Regular");
        if (legacyFont == null)
        {
            Debug.LogWarning("[ChineseUiLocalizer] Noto Sans CJK SC font was not found.");
            return;
        }

        tmpFont = TMP_FontAsset.CreateFontAsset(legacyFont);
        if (tmpFont != null)
        {
            tmpFont.name = "Noto Sans CJK SC Dynamic";
            tmpFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            tmpFont.isMultiAtlasTexturesEnabled = true;
        }
    }

    private void LocalizeAllLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded) LocalizeScene(scene);
        }
    }

    private void LocalizeScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            TMP_Text[] tmpTexts = roots[i].GetComponentsInChildren<TMP_Text>(true);
            for (int j = 0; j < tmpTexts.Length; j++)
            {
                if (tmpFont != null && tmpTexts[j].font != tmpFont) tmpTexts[j].font = tmpFont;
                tmpTexts[j].text = Translate(tmpTexts[j].text);
            }

            Text[] legacyTexts = roots[i].GetComponentsInChildren<Text>(true);
            for (int j = 0; j < legacyTexts.Length; j++)
            {
                if (legacyFont != null && legacyTexts[j].font != legacyFont) legacyTexts[j].font = legacyFont;
                legacyTexts[j].text = Translate(legacyTexts[j].text);
            }
        }
    }

    private static string Translate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        string trimmed = value.Trim();
        if (!Translations.TryGetValue(trimmed, out string translated)) return value;
        return value.Replace(trimmed, translated);
    }
}
