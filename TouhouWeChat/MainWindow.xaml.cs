using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
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
    private const string ApiKey = "sk-8c17b514cef045048add99b97ba03161";
    public MainWindow()
    {
        InitializeComponent();
        var store = new ChatStore();
        _characters = store.LoadAllCharacters();

        if (_characters.Count == 0)
        {
            // 第一次运行，没有任何记录，用默认角色
            _characters = CharacterDefaults.GetAll();
        }
        foreach (var ch in _characters)
        {
            ch.AvatarImage = LoadAvatar(ch._name);
        }

        CharacterList.ItemsSource = _characters;
    }
    

    public void Selected_Click(object sender, RoutedEventArgs e)
    {
        _currentCharacter=CharacterList.SelectedItem as Character;
        DisplayNameText.Text=_currentCharacter?.Displayname??"";
        
        if (_currentCharacter != null)
        {
            MessageList.ItemsSource = _currentCharacter.Messagehistory;
        }
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
            var result = await service.ChatAsync(_currentCharacter.persondescription,_currentCharacter.Affection,_currentCharacter.Messagehistory);
            Console.WriteLine($"[原始文本] {result.Text}");
            Console.WriteLine($"[好感度变化] {result.AffectionDelta}");
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
    
}