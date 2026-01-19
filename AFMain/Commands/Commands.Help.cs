using System.Text;
using Terraria;
using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     指令帮助展示逻辑。
/// </summary>
public partial class Commands
{
    private static void HelpCmd(TSPlayer player)
    {
        var isConsole = !player.RealPlayer;
        var helpMessage = new StringBuilder();
        helpMessage.Append("          [i:3455][c/AD89D5:自][c/D68ACA:动][c/DF909A:钓][c/E5A894:鱼][i:3454]");

        if (!isConsole)
            AppendPlayerHelp(player, helpMessage);

        player.SendMessage(helpMessage.ToString(), 193, 223, 186);
    }

    /// <summary>
    ///     仅输出管理员帮助，用于 /afa 和控制台提示。
    /// </summary>
    private static void SendAdminHelpOnly(TSPlayer player)
    {
        var sb = new StringBuilder();
        sb.Append("[自动钓鱼 - 管理员命令]");
        AppendAdminHelp(player, sb);
        player.SendMessage(sb.ToString(), 193, 223, 186);
    }
}