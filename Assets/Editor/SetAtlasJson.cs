using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 要求制作的Atlas预制体名字包含图片的名字
/// 对应的Atlas和打包生成的.png和.txt放置在同一目录下
/// 如果你想修改，也可以修改代码
/// </summary>
public class SetAtlasJson : Editor
{
    //Atlas目录
    private static readonly List<string> jsonPath = new List<string>()
    {
        "Assets/Atlas/",
    };

    [MenuItem("CTool/ResetAtlasJson")]
    static void SetAtlasRotate()
    {
        for (int i = 0; i < jsonPath.Count; i++)
        {
            DirectoryInfo dir = new DirectoryInfo(jsonPath[i]);
            //只搜索当前目录下面的txt文本
            FileInfo[] inf = dir.GetFiles("*.txt");
            for (int j = 0; j < inf.Length; j++)
            {
                SetAtlasJsonFileInfo(jsonPath[i] + inf[j].Name);
                //将对应的json放置在UIAtlas上
                _putJsonToAtlas(jsonPath[i], inf[j].Name);
            }
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 将对应的json文件放置在对应的Atlas上
    /// </summary>
    private static void _putJsonToAtlas(string parentPath, string jsonFileName)
    {
        DirectoryInfo dir = new DirectoryInfo(parentPath);
        //string pattern = string.Format(".*{0}.*.prefab", jsonFileName.Split('.')[0]);
        //FileInfo[] inf = dir.GetFiles(pattern);
        FileInfo[] inf = dir.GetFiles();
        for (int i = 0; i < inf.Length; i++)
        {
            if (inf[i].Name.EndsWith(".prefab") && inf[i].Name.Contains(jsonFileName.Split('.')[0]))
            {
                UIAtlas atlas = AssetDatabase.LoadAssetAtPath(parentPath + inf[i].Name, typeof(UIAtlas)) as UIAtlas;
                TextAsset ta = AssetDatabase.LoadAssetAtPath(parentPath + jsonFileName, typeof(TextAsset)) as TextAsset;
                NGUIJson.LoadSpriteData(atlas, ta);
                break;
            }
        }
    }

    private static void SetAtlasJsonFileInfo(string path)
    {
        int alterSpriteCount = 0;
        TextAsset ta = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
        object jsonObj;
        try
        {
            jsonObj = NGUIJson.jsonDecode(ta.text);
        }
        catch
        {
            Debug.LogError(path + " is not available json file.");
            return;
        }
        Hashtable decodeHash = jsonObj as Hashtable;
        Hashtable frames = decodeHash["frames"] as Hashtable;
        Hashtable hashResult = new Hashtable();
        foreach (DictionaryEntry item in frames)
        {
            Hashtable hashTemp = item.Value as Hashtable;
            if ((bool)(hashTemp["rotated"]))
            {
                //如果发生了旋转，则将对应的宽，高调换位置
                _alterHashtable(hashTemp);
                alterSpriteCount++;
            }
        }
        string strTemp = NGUIJson.jsonEncode(decodeHash);
        //格式化下json串，也可以不用格式化
        strTemp = JsonTree(strTemp);
        StreamWriter sw = new StreamWriter(path, false);
        sw.Write(strTemp);
        sw.Close();
        Debug.Log(string.Format("{0} complete the operation,{1} sprites finish revision.", path, alterSpriteCount));
    }

    private static void _alterHashtable(Hashtable table)
    {
        table["rotated"] = false;
        List<Hashtable> tableList = new List<Hashtable>();
        tableList.Add(table["frame"] as Hashtable);
        tableList.Add(table["spriteSourceSize"] as Hashtable);
        tableList.Add(table["sourceSize"] as Hashtable);
        for (int i = 0; i < tableList.Count; i++)
        {
            int temp = Convert.ToInt32(tableList[i]["w"]);
            tableList[i]["w"] = tableList[i]["h"];
            tableList[i]["h"] = temp;
        }
    }

    public static string JsonTree(string json)
    {
        int level = 0;
        var jsonArr = json.ToArray(); 
        string jsonTree = string.Empty;
        for (int i = 0; i < json.Length; i++)
        {
            char c = jsonArr[i];
            if (level > 0 && '\n' == jsonTree.ToArray()[jsonTree.Length - 1])
            {
                jsonTree += TreeLevel(level);
            }
            switch (c)
            {
                case '{':
                    jsonTree += c + "\n";
                    level++;

                    break;
                case ',':
                    jsonTree += c + "\n";
                    break;
                case '}':
                    jsonTree += "\n";
                    level--;
                    jsonTree += TreeLevel(level);
                    jsonTree += c;
                    break;
                default:
                    jsonTree += c;
                    break;
            }
        }
        return jsonTree;
    }

    private static string TreeLevel(int level)
    {
        string leaf = string.Empty;
        for (int t = 0; t < level; t++)
        {
            leaf += "\t";
        }
        return leaf;
    }
}
