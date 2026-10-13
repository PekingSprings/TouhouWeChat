using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Input;
using TouhouWeChat.Services;
using TouhouWeChat.Models;
namespace TouhouWeChat;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<Character> _characters = new();
    private Character? _currentCharacter;
    private  string ApiKey = "";
    private string apifilepath=>Path.Combine(AppContext.BaseDirectory,"Data","APIKEY","Apikey.txt");
    private UserProfile user = new UserProfile();
    public MainWindow()
    {
        InitializeComponent();
        LoadApikey(apifilepath);
        var userinfo = new ChatStore();
        user = userinfo.LoadUserConfig() ?? new UserProfile();
        var store = new ChatStore();
        var saved = store.LoadAllCharacters();          // Data/ 里的（有聊天记录的）
        var defaults = CharacterDefaults.GetAll();      // 全部默认角色

// 按名字索引已保存的角色
        var savedDict = saved.ToDictionary(c => c._name);

// 遍历默认列表：有记录的用记录，没记录的用默认
        _characters = new List<Character>();
        foreach (var def in defaults)
        {
            if (savedDict.TryGetValue(def._name, out var savedChar))
                _characters.Add(savedChar);
            else
                _characters.Add(def);
        }

// 加载头像
        foreach (var ch in _characters)
        {
            ch.AvatarImage = LoadAvatar(ch._name);
        }

        CharacterList.ItemsSource = _characters;
    }

    public void LoadApikey(string path)
    {
        var file = File.ReadAllText(path);
        ApiKey= file;
    }

    public void Selected_Click(object sender, RoutedEventArgs e)
    {
        _currentCharacter=CharacterList.SelectedItem as Character;
        DisplayNameText.Text=_currentCharacter?.Displayname??"";
        
        if (_currentCharacter != null)
        {
            MessageList.ItemsSource = _currentCharacter.Messagehistory;
        }
        ChatScroll.ScrollToEnd();
    }
    
    private async void SendButton_ClickAndAIreply(object sender, RoutedEventArgs e)
    {
        if (_currentCharacter == null) return;

        string text = InputBox.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        InputBox.Clear();

        _currentCharacter.Messagehistory.Add(new ChatMessage
        {
            Role = "user",
            Content = text
        });
        SendButton.IsEnabled = false;
        try
        {
            var service = new DeepSeekService(ApiKey);
            var result = await service.ChatAsync(_currentCharacter.persondescription,_currentCharacter.Affection,_currentCharacter.Messagehistory,user);
            // Console.WriteLine($"[原始文本] {result.Text}");
            // Console.WriteLine($"[好感度变化] {result.AffectionDelta}");
            _currentCharacter.Affection += result.AffectionDelta;
            if (_currentCharacter.Affection > 100) _currentCharacter.Affection = 100;
            if (_currentCharacter.Affection < 0) _currentCharacter.Affection = 0;

            _currentCharacter.Messagehistory.Add(new ChatMessage
            {
                Role = "assistant",
                Content = result.Text
            });
            new ChatStore().SaveCharacter(_currentCharacter);
        }
        catch (Exception exception)
        {
            _currentCharacter.Messagehistory.Add(new ChatMessage
            {
                Role = "assistant",
                Content = $"[出错] {exception.Message}"
            });
        }
        finally
        {
            SendButton.IsEnabled = true;
        }

        

        ChatScroll.ScrollToEnd();
    }
    
    
    private static ImageSource? LoadAvatar(string name)
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Avatar",
            name + ".png");

        if (!File.Exists(path)) return null;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new Uri(path);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    public void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCharacter == null) return;

        var confirm = MessageBox.Show(
            $"确定要清空与「{_currentCharacter.Displayname}」的聊天记录吗？\n好感度也会归零。",
            "确认清除",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.OK) return;

        _currentCharacter.Messagehistory.Clear();
        _currentCharacter.Affection = 0;

        new ChatStore().SaveCharacter(_currentCharacter);
    }
    
    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            e.Handled = true;
            SendButton_ClickAndAIreply(sender, e);
            ChatScroll.ScrollToEnd();
        }
    }
}