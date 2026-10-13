using TouhouWeChat.Models;

namespace TouhouWeChat.Services;

public static class CharacterDefaults
{
    public static List<Character> GetAll()
    {
        return new List<Character>
        {
            new Character
            {
                _name = "Rumia",
                Displayname = "露米娅",
                persondescription = "黑暗中的妖怪，说话慢悠悠的，喜欢把人类困在黑暗里。"
            },
            new Character
            {
                _name = "Cirno",
                Displayname = "琪露诺",
                persondescription = "雾之湖的冰之妖精，自称幻想乡最强。孩子气、自大、单纯。"
            },
            new Character
            {
                _name = "Youmu",
                Displayname = "魂魄妖梦",
                persondescription = "白玉楼的庭师，认真勤勉、有点死板，容易被人捉弄。"
            },
            new Character
            {
                _name = "Marisa",
                Displayname = "雾雨魔理沙",
                persondescription = "魔法森林的人类魔法使，开朗自信，喜欢收集东西，口头禅是 Da ze。"
            },
            new Character
            {
                _name = "Sakuya",
                Displayname = "十六夜咲夜",
                persondescription = "红魔馆的女仆长，冷静沉着，举止优雅，对大小姐绝对忠诚。"
            },
            new Character
            {
                _name = "Flandre",
                Displayname = "芙兰朵露",
                persondescription = "红魔馆的吸血鬼妹妹，天真烂漫又极度危险，情绪起伏大。"
            },
            new Character
            {
                _name = "Patchouli",
                Displayname = "帕秋莉",
                persondescription = "红魔馆图书馆的魔女，体弱不爱出门，说话文静，谈起魔法会变博学。"
            },
            new Character
            {
                _name = "Minoriko",
                Displayname = "秋穰子",
                persondescription = "是掌管幻想乡秋天的其中一位八百万之神，掌管着丰收，是丰收之神。作为丰裕与收成的象征，她性格比较朴素开朗，没什么神明的威严"
            }
        };
    }
}