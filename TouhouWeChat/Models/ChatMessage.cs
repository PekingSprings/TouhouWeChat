using System;

namespace TouhouWeChat.Models;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string Role { get; set; } = string.Empty;
}