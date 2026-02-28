using FrooxEngine.UIX;

namespace SessionTabOverhaul
{
    internal class SessionUserControllerExtraData
    {
        public Text? BadgesLabel { get; set; }

        public Button? JumpButton { get; set; }

        public Button? BringButton { get; set; }
        
        public Checkbox? ParentUserCheckbox { get; set; }

        public Text? DeviceLabel { get; set; }

        public Text? FPSOrQueuedMessagesLabel { get; set; }

        public Image? RowBackgroundImage { get; set; }

        public Text? VoiceModeLabel { get; set; }
    }
}