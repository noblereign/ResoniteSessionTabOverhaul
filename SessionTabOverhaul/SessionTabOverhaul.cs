using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace SessionTabOverhaul
{
    public class SessionTabOverhaul : ResoniteMod
    {
        public static ModConfiguration Config;

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ColorHostNameKey = new ModConfigurationKey<bool>("ColorHostName", "Color the Host's username like the host icon.", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ColorLocalUserNameKey = new ModConfigurationKey<bool>("ColorLocalUserName", "Color the Local Users's username?", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<colorX> LocalUserColorKey = new ModConfigurationKey<colorX>("LocalUserColor", "Color of the Local User in the Session user list.", () => RadiantUI_Constants.Hero.PURPLE);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<colorX> FirstRowColorKey = new ModConfigurationKey<colorX>("FirstRowColor", "Background color of the first row in the Session user lists.", () => new colorX(0, .85f));
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<colorX> SecondRowColorKey = new ModConfigurationKey<colorX>("SecondRowColor", "Background color of the second row in the Session user lists.", () => new colorX(1, .15f));

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> HideAllBadgesKey = new ModConfigurationKey<bool>("HideAllBadges", "Hide all Badges in the Session Users list.", () => false);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> HideCustomBadgesKey = new ModConfigurationKey<bool>("HideCustomBadges", "Hide Custom Badges in the Session Users list.", () => false);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> HidePatreonBadgeKey = new ModConfigurationKey<bool>("HidePatreonBadge", "Hide the Patreon Badge in the Session Users list.", () => false);
        
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ShowParentUserCheckboxKey = new ModConfigurationKey<bool>("ShowParentUserCheckbox", "Show the Parent User checkbox in the Session Users list.", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ShowBringButtonKey = new ModConfigurationKey<bool>("ShowBringButton", "Show the Bring button in the Session Users list.", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ShowDeviceLabelKey = new ModConfigurationKey<bool>("ShowDeviceLabel", "Show the Device label in the Session Users list.", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ShowFPSOrQueuedMessagesKey = new ModConfigurationKey<bool>("ShowFPSOrQueuedMessages", "Show the FPS / Queued messages in the Session Users list.", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ShowSteamButtonKey = new ModConfigurationKey<bool>("ShowSteamButton", "Show the Steam button in the Session Users list.", () => false);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ShowVoiceModeKey = new ModConfigurationKey<bool>("ShowVoiceMode", "Show the Voice mode in the Session Users list.", () => true);

        public override string Author => "Banane9, NepuShiro";
        public override string Link => "https://github.com/NepuShiro/ResoniteSessionTabOverhaul";
        public override string Name => "SessionTabOverhaul";
        public override string Version => "2.1.1";

        internal static bool ColorHostName => Config.GetValue(ColorHostNameKey);
        internal static bool ColorLocalUserName => Config.GetValue(ColorLocalUserNameKey);
        internal static colorX LocalUserColor => Config.GetValue(LocalUserColorKey);
        internal static colorX FirstRowColor => Config.GetValue(FirstRowColorKey);
        internal static colorX SecondRowColor => Config.GetValue(SecondRowColorKey);
        internal static bool HideAllBadges => Config.GetValue(HideAllBadgesKey);
        internal static bool HideCustomBadges => Config.GetValue(HideCustomBadgesKey);
        internal static bool HidePatreonBadge => Config.GetValue(HidePatreonBadgeKey);
        internal static bool ShowParentUserCheckbox => Config.GetValue(ShowParentUserCheckboxKey);
        internal static bool ShowBringButton => Config.GetValue(ShowBringButtonKey);
        internal static bool ShowDeviceLabel => Config.GetValue(ShowDeviceLabelKey);
        internal static bool ShowFPSOrQueuedMessages => Config.GetValue(ShowFPSOrQueuedMessagesKey);
        internal static bool ShowSteamButton => Config.GetValue(ShowSteamButtonKey);
        internal static bool ShowVoiceMode => Config.GetValue(ShowVoiceModeKey);
        internal static bool SpritesInjected { get; set; }

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.NepuShiro.SessionTabOverhaul");
            Config = GetConfiguration()!;
            Config.Save(true);
            harmony.PatchAll();
        }
    }
}