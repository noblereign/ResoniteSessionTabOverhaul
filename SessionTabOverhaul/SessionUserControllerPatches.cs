using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Renderite.Shared;
using SkyFrost.Base;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using User = FrooxEngine.User;

namespace SessionTabOverhaul
{
    [HarmonyPatch(typeof(SessionUserController))]
    internal static class SessionUserControllerPatches
    {
        private const string headless = "headless";
        private const string headlessSprite = $"<sprite name=\"{headless}\">";

        private const string screen = "screen";
        private const string screenSprite = $"<sprite name=\"{screen}\">";

        private const string vr = "vr";
        private const string vrSprite = $"<sprite name=\"{vr}\">";

        private const string muteSprite = $"<sprite name=\"{nameof(VoiceMode.Mute)}\">";
        private const string whisperSprite = $"<sprite name=\"{nameof(VoiceMode.Whisper)}\">";
        private const string normalSprite = $"<sprite name=\"{nameof(VoiceMode.Normal)}\">";
        private const string shoutSprite = $"<sprite name=\"{nameof(VoiceMode.Shout)}\">";
        private const string broadcastSprite = $"<sprite name=\"{nameof(VoiceMode.Broadcast)}\">";

        private static readonly colorX HostColor = new colorX(1, .678f, .169f);
        private static colorX? _initialColor;

        private static readonly ConditionalWeakTable<SessionUserController, SessionUserControllerExtraData> controllerExtraData = new ConditionalWeakTable<SessionUserController, SessionUserControllerExtraData>();

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SessionUserController.Create))]
        private static bool CreatePrefix(out SessionUserController __result, User user, UIBuilder ui)
        {
            ui.Style.MinHeight = SessionUserController.HEIGHT + 8;
            HorizontalLayout horizontal = ui.HorizontalLayout(4, 4, Alignment.MiddleCenter);
            horizontal.ForceExpandHeight.Value = false;
            horizontal.ForceExpandWidth.Value = false;

            SessionUserController controller = horizontal.Slot.AttachComponent<SessionUserController>();
            controller._cachedUserName = user.UserName;
            controller.TargetUser = user;

            SessionUserControllerExtraData extraData = controllerExtraData.GetOrCreateValue(controller);
            FontCollection badgeFont = controller.GetBadgeFontCollection();

            extraData.RowBackgroundImage = horizontal.Slot.AttachComponent<Image>();
            extraData.RowBackgroundImage.Tint.Value = (horizontal.Slot.ChildIndex & 1) == 0 ? SessionTabOverhaul.FirstRowColor : SessionTabOverhaul.SecondRowColor;

            if (SessionTabOverhaul.ShowFPSOrQueuedMessages)
            {
                ui.Style.MinHeight = SessionUserController.HEIGHT;
                ui.Style.MinWidth = 2.5f * SessionUserController.HEIGHT;

                ui.Panel();
                extraData.FPSOrQueuedMessagesLabel = ui.Text(GetUserFPSOrQueuedMessages(user), alignment: Alignment.MiddleCenter);
                extraData.FPSOrQueuedMessagesLabel.Font.Target = badgeFont;
                ui.NestOut();
            }

            if (SessionTabOverhaul.ShowDeviceLabel)
            {
                ui.Style.MinWidth = 1.5f * SessionUserController.HEIGHT;
                ui.Style.MinHeight = 0.8f * SessionUserController.HEIGHT;

                ui.Panel();
                extraData.DeviceLabel = ui.Text(GetUserDevice(user), alignment: Alignment.MiddleCenter);
                extraData.DeviceLabel.Font.Target = badgeFont;
                extraData.DeviceLabel.Color.Value = colorX.Red.SetValue(.7f);
                ui.NestOut();
            }

            ui.Style.MinWidth = -1;
            ui.Style.FlexibleWidth = 1;
            ui.Style.MinHeight = SessionUserController.HEIGHT;

            ui.Panel();
            controller._name.Target = ui.Text(controller._cachedUserName, alignment: Alignment.MiddleLeft);

            _initialColor ??= controller._name.Target.Color.Value;

            if (user.IsHost && SessionTabOverhaul.ColorHostName)
                controller._name.Target.Color.Value = HostColor;

            if (user.IsLocalUser && SessionTabOverhaul.ColorLocalUserName)
                controller._name.Target.Color.Value = SessionTabOverhaul.LocalUserColor;

            if (user.UserID != null)
            {
                // In LocalHome or for anonymous users, there is no id
                Button button = controller._name.Target.Slot.AttachComponent<Button>();
                button.SetupAction(SessionUserController.OpenUserProfile, user.UserID);
            }

            ui.NestOut();
            ui.Style.FlexibleWidth = -1;

            ui.Style.MinWidth = 224;
            ui.Style.MinHeight = 0.8f * SessionUserController.HEIGHT;

            ui.Panel();
            ui.OverlappingLayout();
            RectMesh<LineGraphMesh> audioSourceWaveForm = ui.RectMesh<LineGraphMesh>();
            LineGraphMesh wav = audioSourceWaveForm.Mesh;
            wav.Width.Value = 1f;

            ValueGraphRecorder valueGraphRecorder = audioSourceWaveForm.Slot.AttachComponent<ValueGraphRecorder>();
            ValueTag<float> field = audioSourceWaveForm.Slot.AttachComponent<ValueTag<float>>();
            valueGraphRecorder.SourceValue.Target = field.Value;
            valueGraphRecorder.TargetArray.Target = wav.Values;
            valueGraphRecorder.TargetArrayOffset.Target = wav.StartIndex;
            valueGraphRecorder.RangeMin.Target = wav.MinValue;
            valueGraphRecorder.RangeMax.Target = wav.MaxValue;
            valueGraphRecorder.Points.Value = 256;

            ValueTag<int> graphRecorderBugFixTag = audioSourceWaveForm.Slot.AttachComponent<ValueTag<int>>();
            valueGraphRecorder.TargetArrayOffset.Target = graphRecorderBugFixTag.Value;

            extraData.WaveformLineGraphMesh = wav;
            extraData.WaveformGraphOffset = graphRecorderBugFixTag;
            extraData.WaveformGraphTag = field;
            // once this is set, it'll keep waiting for the voice stream until we get it later on
            // no need to set it right here

            wav.Color.DriveFrom(controller._name.Target.Color);

            if (!SessionTabOverhaul.HideAllBadges)
            {
                extraData.BadgesLabel = ui.Text("", alignment: Alignment.MiddleLeft);
                extraData.BadgesLabel.Font.Target = badgeFont;
            }
            ui.NestOut();
            ui.NestOut();

            ui.Style.MinWidth = 192;
            ui.Style.MinHeight = SessionUserController.HEIGHT;
            controller._slider.Target = ui.Slider(SessionUserController.HEIGHT, 1f, 0f, 2f);
            controller._slider.Target.BaseColor.Value = GetUserVoiceModeColor(user);

            InteractionElement.ColorDriver colorXDrive = controller._slider.Target.ColorDrivers[0];
            colorXDrive.TintColorMode.Value = InteractionElement.ColorMode.Explicit;
            colorXDrive.NormalColor.Value = colorX.LightGray;
            colorXDrive.HighlightColor.Value = colorX.White;
            colorXDrive.PressColor.Value = colorX.Gray;
            colorXDrive.DisabledColor.Value = colorX.DarkGray;

            if (SessionTabOverhaul.ShowVoiceMode)
            {
                ui.Style.MinWidth = SessionUserController.HEIGHT;
                Button voiceModeButton = ui.Button(GetUserVoiceModeLabel(user));
                voiceModeButton.BaseColor.Value = new colorX(1, 0);
                voiceModeButton.LocalPressed += (_, _) =>
                {
                    if (!controller.IsDestroyed)
                        controller._slider.Target.Value.Value = 1;
                };

                extraData.VoiceModeLabel = voiceModeButton.Slot.GetComponentInChildren<Text>();
                extraData.VoiceModeLabel.Font.Target = badgeFont;
            }

            ui.Style.MinWidth = 64;
            ui.Style.MinHeight = SessionUserController.HEIGHT;
            controller._mute.Target = ui.Button("User.Actions.Mute".AsLocaleKey(), controller.OnMute);
            controller._jump.Target = ui.World.RootSlot.FindChildOrAdd("__TEMP").GetComponentOrAttach<Button>();

            extraData.JumpButton = ui.Button("User.Actions.Jump".AsLocaleKey());
            extraData.JumpButton.LocalPressed += (_, _) =>
            {
                if (controller.World != Userspace.UserspaceWorld)
                    return;

                user.World.RunSynchronously(() =>
                {
                    if (user.Root == null)
                        return;

                    UserRoot? root = user.World.LocalUser.Root;
                    root?.JumpToPoint(user.Root.HeadPosition);
                    if (extraData.ParentUserCheckbox?.State.Value == true) root?.Slot.SetParent(user.Root.Slot.Parent);
                    CharacterController? charControl = (root?.GetRegisteredComponent<LocomotionController>()?.ActiveModule as IPhysicalLocomotion)?.CharacterController;
                    if (charControl != null)
                    {
                        charControl.LinearVelocity = float3.Zero;
                    }
                });
            };

            if (SessionTabOverhaul.ShowBringButton)
            {
                extraData.BringButton = ui.Button("Bring");
                extraData.BringButton.LocalPressed += (_, _) =>
                {
                    if (controller.World != Userspace.UserspaceWorld)
                        return;

                    user.World.RunSynchronously(() =>
                    {
                        if (user.World.LocalUser.Root == null)
                            return;

                        UserRoot? root = user.Root;
                        CharacterController? charControl = (root?.GetRegisteredComponent<LocomotionController>()?.ActiveModule as IPhysicalLocomotion)?.CharacterController;
                        if (charControl != null)
                        {
                            float oldval = charControl.LinearDamping.Value;
                            charControl.LinearDamping.Value = float.MaxValue;
                            user.StartTask(async () =>
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    await default(NextUpdate);
                                }

                                charControl.RunSynchronously(() =>
                                {
                                    root?.JumpToPoint(user.World.LocalUser.Root.HeadPosition);
                                    if (extraData.ParentUserCheckbox?.State.Value == true) root?.Slot.SetParent(user.World.LocalUser.Root.Slot.Parent);
                                    charControl.LinearDamping.Value = oldval;
                                });
                            });
                        }
                    });
                };
            }
            
            if (SessionTabOverhaul.ShowParentUserCheckbox)
                extraData.ParentUserCheckbox = ui.Checkbox();

            if (SessionTabOverhaul.ShowSteamButton)
            {
                ui.Style.MinWidth = 80;
                Button steamButton = ui.Button("Steam");
                steamButton.Enabled = false;
                if (user.Metadata.TryGetElement("SteamID", out SyncVar value) && value.TryGetValue(out ulong steamID))
                {
                    steamButton.Enabled = true;
                    steamButton.LocalPressed += (_, _) => Process.Start($"https://steamcommunity.com/profiles/{steamID}");
                }
            }

            ui.Style.MinWidth = 108;
            controller._respawn.Target = ui.Button("User.Actions.Respawn".AsLocaleKey(), controller.OnRespawn);

            ui.Style.MinWidth = 80;
            controller._silence.Target = ui.Button("User.Actions.Silence".AsLocaleKey(), controller.OnSilence);

            ui.Style.MinWidth = 48;
            controller._kick.Target = ui.Button("User.Actions.Kick".AsLocaleKey(), controller.OnKick);
            controller._ban.Target = ui.Button("User.Actions.Ban".AsLocaleKey(), controller.OnBan);

            ui.NestOut();

            if (user.IsHost)
                controller.AddBadge("host");

            if (user.Platform.IsMobilePlatform())
                controller.AddBadge("mobile");

            if (user.Platform == Platform.Linux)
                controller.AddBadge("linux");

            if (user.HeadDevice == HeadOutputDevice.Headless)
                controller.AddBadge("headless");

            if (user.UserID != null)
            {
                controller.StartTask(async delegate
                {
                    CloudResult<SkyFrost.Base.User> cloudResult = await controller.Cloud.Users.GetUserCached(user.UserID);
                    if (cloudResult.IsOK)
                    {
                        controller.SetCloudData(cloudResult.Entity);
                    }
                });
            }

            __result = controller;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SessionUserController.AddBadge), new[] { typeof(string) })]
        private static bool AddStandardBadgePrefix(SessionUserController __instance, string spriteName)
        {
            if (!controllerExtraData.TryGetValue(__instance, out SessionUserControllerExtraData? extraData) || extraData?.BadgesLabel == null)
                return false;

            if (SessionTabOverhaul.HidePatreonBadge && (spriteName == "patreon" || spriteName == "stripe" || spriteName == "supporter"))
                return false;

            Sync<string> text = extraData.BadgesLabel.Content;

            if (text.Value.Length > 0)
                text.Value += " ";

            text.Value += $"<sprite name=\"{spriteName}\">";

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SessionUserController.AddBadge), new[] { typeof(Uri), typeof(string), typeof(bool) })]
        private static bool AddCustomBadgePrefix(SessionUserController __instance, Uri badge, string key)
        {
            DynamicSpriteFont spriteFont = (DynamicSpriteFont)__instance.GetBadgeFontCollection().FontSets[1];

            if (!spriteFont.HasSprite(key))
                spriteFont.AddSprite(key, badge, 1.25f);

            if (SessionTabOverhaul.HideCustomBadges)
                return false;

            AddStandardBadgePrefix(__instance, key);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SessionUserController.GetBadgeFontCollection))]
        private static void GetBadgeFontCollectionPostfix(FontCollection __result)
        {
            if (SessionTabOverhaul.SpritesInjected)
                return;

            DynamicSpriteFont spriteFont = (DynamicSpriteFont)__result.FontSets[1];

            if (!spriteFont.HasSprite(screen))
                spriteFont.AddSprite(screen, new Uri("resdb:///1c88a45653f60a9b29eefc5e3adc4659f3021a85debd5b4f3425ff29d4564794"));

            if (!spriteFont.HasSprite(vr))
                spriteFont.AddSprite(vr, new Uri("resdb:///1d2dc53aa1b44d8a21aaaa3ce41b695ae724eb0553e7ee08d50fc0c7922ae149"));

            foreach (VoiceMode voiceMode in Enum.GetValues<VoiceMode>())
            {
                string name = voiceMode.ToString();

                if (!spriteFont.HasSprite(name))
                    spriteFont.AddSprite(name, VoiceHelper.GetIcon(voiceMode));
            }

            SessionTabOverhaul.SpritesInjected = true;
        }

        private static string GetUserDevice(User user)
        {
            if (user.HeadDevice == HeadOutputDevice.Headless)
                return headlessSprite;

            if (user.VR_Active)
                return vrSprite;

            return screenSprite;
        }

        private static VoiceMode GetUserVoiceMode(User user) => user.isMuted ? VoiceMode.Mute : user.VoiceMode;

        private static string GetUserVoiceModeLabel(User user) => GetUserVoiceMode(user) switch
        {
            VoiceMode.Mute => muteSprite,
            VoiceMode.Whisper => whisperSprite,
            VoiceMode.Normal => normalSprite,
            VoiceMode.Shout => shoutSprite,
            VoiceMode.Broadcast => broadcastSprite,
            _ => ""
        };

        private static colorX GetUserVoiceModeColor(User user) => VoiceHelper.GetColor(GetUserVoiceMode(user)).SetSaturation(.5f);

        private static string GetUserFPSOrQueuedMessages(User user) => user.QueuedMessages > 10 ? $"<color={colorX.Red.SetValue(.7f).ToHexString()}>{user.QueuedMessages} <size=60%>Q'd" : $"<color=#F0F0F0>{MathX.RoundToInt(user.FPS)} <size=60%>FPS";

        private static void AssignVoiceStream(User user, OpusStream<MonoSample>? voiceStream, SessionUserControllerExtraData extraData)
        {
            if (voiceStream == null)
            {
                voiceStream = user.Streams.OfType<OpusStream<MonoSample>>().FirstOrDefault(s => s.Name == "Voice");
            }
            if (voiceStream == null)
            {
                return;
            }

            user.World.RunSynchronously(() =>
            {
                if (user.World.LocalUser.Root == null)
                    return;

                UserRoot? root = user.Root;

                if (root == null) return;

                Slot rootSlot = root.Slot;
                string slotName = $"{user.UserName}'s Voice Stream VolumeMeter";

                VolumeMeter volMeter = rootSlot.FindLocalChildOrAdd(slotName).GetComponentOrAttach<VolumeMeter>();
                volMeter.Source.Target = voiceStream;
                volMeter.Power.Value = .25f;
                volMeter.Smoothing.Value = 0;

                extraData.WorldSpaceVolumeMeter = new WeakReference<VolumeMeter>(volMeter);
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnCommonUpdate")]
        private static void OnCommonUpdatePostfix(SessionUserController __instance)
        {
            if (!controllerExtraData.TryGetValue(__instance, out SessionUserControllerExtraData? extraData) || extraData == null)
                return;

            if (extraData.RowBackgroundImage != null)
                extraData.RowBackgroundImage.Tint.Value = (__instance.Slot.ChildIndex & 1) == 0 ? SessionTabOverhaul.FirstRowColor : SessionTabOverhaul.SecondRowColor;

            User user = __instance.TargetUser;

            if (__instance._name.Target != null && user.HeadDevice != HeadOutputDevice.Headless)
            {
                if (!user.IsPresentInWorld)
                {
                    __instance._name.Target.Color.Value = colorX.Red;
                }
                else if (!user.IsPresentInHeadset)
                {
                    __instance._name.Target.Color.Value = colorX.Red.SetSaturation(0.5f);
                }
                else
                {
                    __instance._name.Target.Color.Value = _initialColor ?? colorX.White;

                    if (user.IsHost && SessionTabOverhaul.ColorHostName)
                    {
                        __instance._name.Target.Color.Value = HostColor;
                    }

                    if (user.IsLocalUser && SessionTabOverhaul.ColorLocalUserName)
                    {
                        __instance._name.Target.Color.Value = SessionTabOverhaul.LocalUserColor;
                    }
                }
            }

            if (extraData.FPSOrQueuedMessagesLabel != null)
                extraData.FPSOrQueuedMessagesLabel.Content.Value = GetUserFPSOrQueuedMessages(user);

            if (extraData.DeviceLabel != null)
                extraData.DeviceLabel.Content.Value = GetUserDevice(user);

            __instance._slider.Target.BaseColor.Value = GetUserVoiceModeColor(user);

            if (extraData.VoiceModeLabel != null)
                extraData.VoiceModeLabel.Content.Value = GetUserVoiceModeLabel(user);

            if (extraData.JumpButton != null)
                extraData.JumpButton.Enabled = !user.IsLocalUser;

            if (extraData.BringButton != null)
                extraData.BringButton.Enabled = !user.IsLocalUser;

            if (extraData.ParentUserCheckbox != null)
                extraData.ParentUserCheckbox.Enabled = !user.IsLocalUser;

            if (extraData.WaveformGraphTag != null && extraData.WaveformLineGraphMesh != null && extraData.WaveformGraphOffset != null)
            {
                if (extraData.WorldSpaceVolumeMeter != null && extraData.WorldSpaceVolumeMeter.TryGetTarget(out VolumeMeter worldSpaceVolumeMeter))
                {
                    // creating a faux-waveform...
                    // using the actual waveform mesh would've been nice, but
                    // getting that to work between worldspace and userspace is incredibly complicated :(
                    extraData.WaveformGraphTag.Value.Value = .5f + ((worldSpaceVolumeMeter.Volume.Value / 2) * (__instance.World.Time.LocalUpdateIndex % 2 == 0 ? 1 : -1));
                }
                else
                {
                    extraData.WaveformGraphTag.Value.Value = .5f;
                    AssignVoiceStream(user, null, extraData);
                }
                extraData.WaveformLineGraphMesh.StartIndex.Value = extraData.WaveformGraphOffset.Value - 1;
            }
        }
    }
}