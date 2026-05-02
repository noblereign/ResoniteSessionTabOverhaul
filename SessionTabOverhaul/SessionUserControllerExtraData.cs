using FrooxEngine;
using FrooxEngine.UIX;
using System;

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


        public ValueTag<float>? WaveformGraphTag { get; set; }
        public ValueTag<int>? WaveformGraphOffset { get; set; }
        public LineGraphMesh? WaveformLineGraphMesh { get; set; }
        public WeakReference<VolumeMeter>? WorldSpaceVolumeMeter { get; set; }
    }
}