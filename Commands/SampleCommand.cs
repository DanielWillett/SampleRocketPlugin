using Rocket.Core.Steam;

namespace SampleRocketPlugin.Commands;
public class EchoCommand : IRocketCommand
{
    public AllowedCaller AllowedCaller => AllowedCaller.Both;
    public string Name => "echo";
    public string Help => "Echos text back to you.";
    public string Syntax => "/echo <text...>";
    public List<string> Aliases { get; } = new List<string> { "e" };
    public List<string> Permissions { get; } = new List<string> { "src.echo" };

    public void Execute(IRocketPlayer caller, string[] command)
    {
        if (caller is UnturnedPlayer player)
        {
            ThreadEx.RunTask(async () =>
            {
                Profile profile = await Players.GetProfileAsync(player.CSteamID.m_SteamID);

                await ThreadEx.ToMainThread();

                caller.SendChat("echo_response_with_profile", string.Join(" ", command), profile.RealName);
            });
        }
        else
            caller.SendChat("echo_response", string.Join(" ", command));
    }
}
