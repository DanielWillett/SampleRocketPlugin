/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

using Rocket.Core.Steam;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;

namespace SampleRocketPlugin.Util;

internal static class Players
{
    internal static bool IsValidPlayerId(this CSteamID id) => IsValidPlayerId(id.m_SteamID);
    internal static bool IsValidPlayerId(this ulong id) => id / 100000000000000ul == 765;
    internal static async Task<Profile> GetProfileAsync(ulong player)
    {
        Profile profile = (Profile)FormatterServices.GetUninitializedObject(typeof(Profile));
        profile.SteamID64 = player;

        await ReloadProfileAsync(profile).ConfigureAwait(false);
        return profile;
    }
    internal static async Task ReloadProfileAsync(Profile profile)
    {
        // slightly modified Reload() from
        // https://github.com/SmartlyDressedGames/Legally-Distinct-Missile/blob/master/Rocket/Rocket.Core/Steam/Profile.cs#L69

        string field = "OnlineState";
        try
        {
            XmlDocument doc = new XmlDocument();
            using WebClient client = new WebClient();
            string url = "http://steamcommunity.com/profiles/" + profile.SteamID64.ToString(CultureInfo.InvariantCulture) + "?xml=1";
            doc.LoadXml(
                await client.DownloadStringTaskAsync(url).ConfigureAwait(false));
            XmlElement? profileElem = doc["profile"];
            if (profileElem != null)
            {
                profile.SteamID = profileElem["steamID"]?.ParseString(); field = "OnlineState";
                profile.OnlineState = profileElem["onlineState"]?.ParseString(); field = "StateMessage";
                profile.StateMessage = profileElem["stateMessage"]?.ParseString(); field = "PrivacyState";
                profile.PrivacyState = profileElem["privacyState"]?.ParseString(); field = "VisibilityState";
                profile.VisibilityState = profileElem["visibilityState"]?.ParseUInt16(); field = "AvatarIcon";
                profile.AvatarIcon = profileElem["avatarIcon"]?.ParseUri(); field = "AvatarMedium";
                profile.AvatarMedium = profileElem["avatarMedium"]?.ParseUri(); field = "AvatarFull";
                profile.AvatarFull = profileElem["avatarFull"]?.ParseUri(); field = "IsVacBanned";
                profile.IsVacBanned = profileElem["vacBanned"]?.ParseBool(); field = "TradeBanState";
                profile.TradeBanState = profileElem["tradeBanState"]?.ParseString(); field = "IsLimitedAccount";
                profile.IsLimitedAccount = profileElem["isLimitedAccount"]?.ParseBool(); field = "CustomURL";

                profile.CustomURL = profileElem["customURL"]?.ParseString(); field = "MemberSince";
                profile.MemberSince = profileElem["memberSince"]?.ParseDateTime(new CultureInfo("en-US", false)); field = "HoursPlayedLastTwoWeeks";
                profile.HoursPlayedLastTwoWeeks = profileElem["hoursPlayed2Wk"]?.ParseDouble(); field = "Headline";
                profile.Headline = profileElem["headline"]?.ParseString(); field = "Location";
                profile.Location = profileElem["location"]?.ParseString(); field = "RealName";
                profile.RealName = profileElem["realname"]?.ParseString(); field = "Summary";
                profile.Summary = profileElem["summary"]?.ParseString(); field = "MostPlayedGame";

                XmlElement? mostPlayedGameElem = profileElem["mostPlayedGames"];
                profile.MostPlayedGames = new List<Profile.MostPlayedGame>();
                if (mostPlayedGameElem != null)
                {
                    foreach (XmlElement mostPlayedGame in mostPlayedGameElem.ChildNodes)
                    {
                        Profile.MostPlayedGame game = new Profile.MostPlayedGame();
                        field = "MostPlayedGame.Name";
                        game.Name = mostPlayedGame["gameName"]?.ParseString(); field = "MostPlayedGame.Link";
                        game.Link = mostPlayedGame["gameLink"]?.ParseUri(); field = "MostPlayedGame.Icon";
                        game.Icon = mostPlayedGame["gameIcon"]?.ParseUri(); field = "MostPlayedGame.Logo";
                        game.Logo = mostPlayedGame["gameLogo"]?.ParseUri(); field = "MostPlayedGame.LogoSmall";
                        game.LogoSmall = mostPlayedGame["gameLogoSmall"]?.ParseUri(); field = "MostPlayedGame.HoursPlayed";
                        game.HoursPlayed = mostPlayedGame["hoursPlayed"]?.ParseDouble(); field = "MostPlayedGame.HoursOnRecord";
                        game.HoursOnRecord = mostPlayedGame["hoursOnRecord"]?.ParseDouble();
                        profile.MostPlayedGames.Add(game);
                    }
                }

                XmlElement? groupsElem = profileElem["groups"];
                profile.Groups = new List<Profile.Group>();
                if (groupsElem != null)
                {
                    foreach (XmlElement group in groupsElem.ChildNodes)
                    {
                        Profile.Group grp = new Profile.Group();
                        field = "Group.IsPrimary";
                        grp.IsPrimary = group.Attributes["isPrimary"] is { InnerText: "1" }; field = "Group.SteamID64";
                        grp.SteamID64 = group["groupID64"]?.ParseUInt64(); field = "Group.Name";
                        grp.Name = group["groupName"]?.ParseString(); field = "Group.URL";
                        grp.URL = group["groupURL"]?.ParseString(); field = "Group.Headline";
                        grp.Headline = group["headline"]?.ParseString(); field = "Group.Summary";
                        grp.Summary = group["summary"]?.ParseString(); field = "Group.AvatarIcon";
                        grp.AvatarIcon = group["avatarIcon"]?.ParseUri(); field = "Group.AvatarMedium";
                        grp.AvatarMedium = group["avatarMedium"]?.ParseUri(); field = "Group.AvatarFull";
                        grp.AvatarFull = group["avatarFull"]?.ParseUri(); field = "Group.MemberCount";
                        grp.MemberCount = group["memberCount"]?.ParseUInt32(); field = "Group.MembersInChat";
                        grp.MembersInChat = group["membersInChat"]?.ParseUInt32(); field = "Group.MembersInGame";
                        grp.MembersInGame = group["membersInGame"]?.ParseUInt32(); field = "Group.MembersOnline";
                        grp.MembersOnline = group["membersOnline"]?.ParseUInt32();
                        profile.Groups.Add(grp);
                    }
                }
            }
            else
            {
                profile.Groups = new List<Profile.Group>();
                profile.MostPlayedGames = new List<Profile.MostPlayedGame>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, "Error reading Steam Profile, Field: " + field);
        }
    }
}
