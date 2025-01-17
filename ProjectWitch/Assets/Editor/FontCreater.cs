﻿#if UNITY_5
#pragma warning disable 0618
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class BitmapFontCreater : MonoBehaviour
{

    // XML形式の.fntファイルを読み込むため、ツリー構造をマッピングするためのクラスを宣言する
    // （ここでは必要最小限なデータ構造のクラスを宣言している）
    [XmlRoot("font")]
    public class FontData
    {
        [XmlElementAttribute("common")]
        public FontCommon common;
        [XmlArray("chars")]
        [XmlArrayItem("char")]
        public List<FontChar> chars;
    }

    public class FontCommon
    {
        [XmlAttribute("lineHeight")]
        public float lineHeight;
        [XmlAttribute("scaleW")]
        public float scaleW;
        [XmlAttribute("scaleH")]
        public float scaleH;
    }

    public class FontChar
    {
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("x")]
        public float x;
        [XmlAttribute("y")]
        public float y;
        [XmlAttribute("width")]
        public float width;
        [XmlAttribute("height")]
        public float height;
        [XmlAttribute("xoffset")]
        public float xoffset;
        [XmlAttribute("yoffset")]
        public float yoffset;
        [XmlAttribute("xadvance")]
        public float xadvance;
    }

    // Assetsメニュー→「Create」に「Bitmap Font」の項目を追加する
    [MenuItem("Assets/Create/Bitmap Font")]
    public static void Create()
    {
        // Projectビューで選択されているテキストファイルとテクスチャを取得する
        Object[] selectedTextAssets =
            Selection.GetFiltered(typeof(TextAsset), SelectionMode.DeepAssets);
        Object[] selectedTextures =
            Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

        // テキストファイルが選択されていなければエラー
        if (selectedTextAssets.Length < 1)
        {
            Debug.LogWarning("No text asset selected.");
            return;
        }

        // テクスチャが選択されていなければエラー
        if (selectedTextures.Length < 1)
        {
            Debug.LogWarning("No texture selected.");
            return;
        }

        // テキストファイルのあるフォルダに後でアセットを保存する
        string baseDir =
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedTextAssets[0]));
        // フォントの名前はテキストファイルの名前にする
        string fontName = selectedTextAssets[0].name;
        // テキストファイルの中身を取得する
        string xml = ((TextAsset)selectedTextAssets[0]).text;

        // XMLを読み込み、ツリー構造をFontDataクラスにマッピングする
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(FontData));
        FontData fontData = null;
        using (StringReader reader = new StringReader(xml))
        {
            fontData = (FontData)xmlSerializer.Deserialize(reader);
        }

        // データが不正だったらエラー
        if (fontData == null || fontData.chars.Count < 1)
        {
            Debug.LogWarning("Invalid data.");
            return;
        }

        // カスタムフォント用のマテリアルを作成して、選択されたテクスチャを割り当てる
        Material fontMaterial = new Material(Shader.Find("UI/Default"));
        fontMaterial.mainTexture = (Texture2D)selectedTextures[0];

        // カスタムフォントを作成して、マテリアルを割り当てる
        Font font = new Font(fontName);
        font.material = fontMaterial;

        // カスタムフォントに文字を追加する
        float textureWidth = fontData.common.scaleW;
        float textureHeight = fontData.common.scaleH;
        CharacterInfo[] characterInfos = new CharacterInfo[fontData.chars.Count];
        for (int i = 0; i < fontData.chars.Count; i++)
        {
            FontChar fontChar = fontData.chars[i];
            float charX = fontChar.x;
            float charY = fontChar.y;
            float charWidth = fontChar.width;
            float charHeight = fontChar.height;

            // 文字情報の設定[^5]
            characterInfos[i] = new CharacterInfo();
            characterInfos[i].index = fontChar.id;
            characterInfos[i].uv = new Rect(
                charX / textureWidth, (textureHeight - charY - charHeight) / textureHeight,
                charWidth / textureWidth, charHeight / textureHeight);
            characterInfos[i].vert = new Rect(
                fontChar.xoffset, -fontChar.yoffset,
                charWidth, -charHeight);
            characterInfos[i].width = fontChar.xadvance;
        }
        font.characterInfo = characterInfos;

        // Line Spacingプロパティはスクリプトから直接設定することができないため、
        // SerializedPropertyを使って設定する
        // （この方法はUnityの将来のバージョンで使えなくなる可能性があります）
        SerializedObject serializedFont = new SerializedObject(font);
        SerializedProperty serializedLineSpacing =
            serializedFont.FindProperty("m_LineSpacing");
        serializedLineSpacing.floatValue = fontData.common.lineHeight;
        serializedFont.ApplyModifiedProperties();

        // 作成したマテリアルとフォントをアセットとして保存する
        SaveAsset(fontMaterial, baseDir + "/" + fontName + ".mat");
        SaveAsset(font, baseDir + "/" + fontName + ".fontsettings");
    }

    // アセットとして保存するための関数
    // 既存のものがある場合は、上書きして更新する
    private static void SaveAsset(Object obj, string path)
    {
        Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
        if (existingAsset != null)
        {
            EditorUtility.CopySerialized(obj, existingAsset);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(obj, path);
        }
    }
}

#if UNITY_5
#pragma warning restore 0618
#endif
