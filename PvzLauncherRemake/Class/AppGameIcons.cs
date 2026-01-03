using PvzLauncherRemake.Controls.Icons;
using System.Windows.Controls;

namespace PvzLauncherRemake.Class
{
    public enum GameIcons
    {
        Unknown,//unknown

        Origin,//origin
        GOTY,//goty
        Steam,//steam
        Test,//test
        Beta,//beta
        Ghtr,//ghtr
        Dream,//dream
        NineFive,//ninefive
        Hybrid,//he
        Fusion,//fe
        Tat,//tat
        Eagrace,//eagrace
        Unnamed,//unnamed
        Ultimate,//ultimate
        Random,//random
        Guidance,//guidance

        PvzToolkit,//pvztoolkit
        CheatEngine//ce
    }

    public class GameIconConverter
    {
        /// <summary>
        /// 将图标字符串转换为枚举类型
        /// </summary>
        /// <param name="iconName">图标字符串，一般是下载索引获取的</param>
        /// <returns></returns>
        public static GameIcons ParseToGameIcons(string iconName)
        {
            switch (iconName)
            {
                case "origin": return GameIcons.Origin;
                case "goty": return GameIcons.GOTY;
                case "steam": return GameIcons.Steam;
                case "test": return GameIcons.Test;
                case "beta": return GameIcons.Beta;
                case "ghtr": return GameIcons.Ghtr;
                case "dream": return GameIcons.Dream;
                case "ninefive": return GameIcons.NineFive;
                case "he": return GameIcons.Hybrid;
                case "fe": return GameIcons.Fusion;
                case "tat": return GameIcons.Tat;
                case "eagrace": return GameIcons.Eagrace;
                case "unnamed": return GameIcons.Unnamed;
                case "ultimate": return GameIcons.Ultimate;
                case "random": return GameIcons.Random;
                case "guidance": return GameIcons.Guidance;

                case "pvztoolkit": return GameIcons.PvzToolkit;
                case "ce": return GameIcons.CheatEngine;

                case "unknown": return GameIcons.Unknown;


                default: return GameIcons.Unknown;
            }
        }

        /// <summary>
        /// 将图标类型转换为UserControl
        /// </summary>
        /// <param name="gameIcons"></param>
        /// <returns></returns>
        public static UserControl ParseToUserControl(GameIcons gameIcons)
        {
            switch (gameIcons)
            {
                case GameIcons.Unknown: return new GameIconUnknown();

                case GameIcons.Origin: return new GameIconOrigin();
                case GameIcons.GOTY: return new GameIconGoty();
                case GameIcons.Steam: return new GameIconSteam();
                case GameIcons.Test: return new GameIconTest();
                case GameIcons.Beta: return new GameIconBeta();
                case GameIcons.Ghtr: return new GameIconGhtr();
                case GameIcons.Dream: return new GameIconDream();
                case GameIcons.NineFive: return new GameIconNineFive();
                case GameIcons.Hybrid: return new GameIconHybrid();
                case GameIcons.Fusion: return new GameIconFusion();
                case GameIcons.Tat: return new GameIconTat();
                case GameIcons.Eagrace: return new GameIconEagrace();
                case GameIcons.Unnamed: return new GameIconUnnamed();
                case GameIcons.Ultimate: return new GameIconUltimate();
                case GameIcons.Random: return new GameIconRandom();
                case GameIcons.Guidance: return new GameIconGuidance();

                case GameIcons.PvzToolkit: return new GameIconPvzToolkit();
                case GameIcons.CheatEngine: return new GameIconCheatEngine();

                default: return new GameIconUnknown();
            }
        }
    }
}
