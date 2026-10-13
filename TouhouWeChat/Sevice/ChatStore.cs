using System.IO;
using Newtonsoft.Json;
using TouhouWeChat.Models;

namespace TouhouWeChat;

public class ChatStore
{
    private static string DataFolder =>
        Path.Combine(AppContext.BaseDirectory, "Data");

    public void SaveCharacter(Character ch)
    {
        Directory.CreateDirectory(DataFolder);
        string file = Path.Combine(DataFolder, ch._name + ".json");
        File.WriteAllText(file, JsonConvert.SerializeObject(ch));
    }

    public Character? Load(string name)
    {
        string file = Path.Combine(DataFolder, name + ".json");
        if (!File.Exists(file)) return null;
        string json = File.ReadAllText(file);
        return JsonConvert.DeserializeObject<Character>(json);
    }
    public List<Character> LoadAllCharacters()
    {
        var result = new List<Character>();

        if (!Directory.Exists(DataFolder)) return result;

        foreach (var file in Directory.GetFiles(DataFolder, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                var ch = JsonConvert.DeserializeObject<Character>(json);
                if (ch != null) result.Add(ch);
            }
            catch
            {
                // 单个文件坏了跳过，不影响其他
            }
        }

        return result;
    }
}