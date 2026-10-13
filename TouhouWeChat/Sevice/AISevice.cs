using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TouhouWeChat.Models;
using System.Text.RegularExpressions;

namespace TouhouWeChat
{
    public class DeepSeekService
    {
        private static readonly HttpClient _http = new HttpClient();
        private const string ApiUrl = "https://api.deepseek.com/chat/completions";
        private const string Model = "deepseek-flash";

        public DeepSeekService(string apiKey)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }
        

        /// <summary>
        /// 发一条消息给 AI，带上角色人设和历史记录，返回 AI 回复。
        /// </summary>
        public async Task<ChatResult> ChatAsync(string persona, int affection, IEnumerable<ChatMessage> history)
        {
            try
            {
                var messages = new List<object>();
                messages.Add(new { role = "system", content = BuildSystemPrompt(persona, affection) });

                foreach (var msg in history)
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }

                var requestBody = new { model = Model, messages = messages, stream = false };
                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(ApiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ChatResult { Text = $"[请求失败 {response.StatusCode}]", AffectionDelta = 0 };
                }

                var obj = JObject.Parse(responseText);
                var raw = obj["choices"]?[0]?["message"]?["content"]?.ToString() ?? "……";

                return ParseReply(raw);
            }
            catch (Exception ex)
            {
                return new ChatResult { Text = $"[网络错误] {ex.Message}", AffectionDelta = 0 };
            }
        }

        private static ChatResult ParseReply(string raw)
        {
            var regex = new Regex(@"\[affection:\s*([+-]?\d+)\s*\]");
            var match = regex.Match(raw);

            int delta = 0;
            string clean = raw;

            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out delta);
                clean = raw.Remove(match.Index, match.Length).Trim();
            }

            return new ChatResult { Text = clean, AffectionDelta = delta };
        }

        /// <summary>
        /// 拼 system 提示词：角色人设 + 好感度描述。
        /// </summary>
        private string BuildSystemPrompt(string persona, int affection)
        {
            string affectionDesc = affection switch
            {
                < 20 => "你和对方刚刚认识，态度比较生疏。",
                < 50 => "你和对方已经聊过一阵子了，态度自然友好。",
                < 80 => "你把对方当作熟人朋友，说话比较放松。",
                _ => "对方是你非常亲近的人，说话亲昵自然。"
            };

            return $"{persona}\n\n" +
                   $"当前你与对方的好感度为 {affection}。{affectionDesc}\n\n" +
                   "【行为准则】\n" +
                   "1. 始终以第一人称、以角色的身份和口吻说话，绝不跳出角色。\n" +
                   "2. 回复要口语化、自然，像日常闲聊。默认 1~2 句话。\n" +
                   "3. 绝对不要自称 AI、助手、语言模型。\n" +
                   "4. 可以自然地提到幻想乡的日常、符卡、妖怪、宴会等话题。\n" +
                   "5. 更具性格和好感度来回答，好感度>60时谈及恋爱结婚可接受，<60则会推脱和转移话题\n"+
                   "6. 每次回复的最后，必须单独一行输出你对用户这次发言的好感度变化，\n" +
                   "   格式为 [affection:+N] 或 [affection:-N] 或 [affection:0]。\n" +
                   "   +1~+3 表示用户的话让你开心、温暖、有好感；\n" +
                   "   -1~-3 表示用户的话让你不舒服、生气、讨厌；\n" +
                   "   0 表示没有明显情绪波动。\n" +
                   "   这一行必须存在，且放在回复的最后。";
        }
    }
}