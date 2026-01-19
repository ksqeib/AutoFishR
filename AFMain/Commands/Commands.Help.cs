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

        // 管理员附加指令（末尾追加，避免遮挡玩家指令）
        if (player.HasPermission("autofish.admin"))
            AppendAdminHelp(player, helpMessage);

        player.SendMessage(helpMessage.ToString(), 193, 223, 186);
    }
}