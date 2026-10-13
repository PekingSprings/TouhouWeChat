using System.Collections.ObjectModel;
using Microsoft.VisualBasic.CompilerServices;
using System.Windows.Media;
using Newtonsoft.Json;


namespace TouhouWeChat.Models;

public class Character
{
    public string _name{get;set;}
    public string Displayname{get;set;}
    public string persondescription{get;set;}
    public int Affection{get;set;}
    public ObservableCollection<ChatMessage> Messagehistory { get; set; } = new();
    [JsonIgnore]
    public ImageSource? AvatarImage { get; set; }
    
}