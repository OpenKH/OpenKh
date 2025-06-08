using CommunityToolkit.Mvvm.ComponentModel;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class M_EffectElements_Wrapper : ObservableObject
    {
        public ushort EffectNumber { get; set; }
        public ushort Id { get; set; }
        public byte Group { get; set; }
        public byte FadeoutFrame { get; set; }
        public short BoneId { get; set; }
        public ulong Category { get; set; }
        //public Pax.Element.ElementFlags Flags { get; set; }
        public bool FlagBind { get; set; }
        public bool FlagBindOnlyStart { get; set; }
        public bool FlagBindOnlyPos { get; set; }
        public bool FlagGetColor { get; set; }
        public bool FlagGetBrightness { get; set; }
        public bool FlagDestroyWhenMotionChange { get; set; }
        public bool FlagDestroyFadeout { get; set; }
        public bool FlagDestroyLoopend { get; set; }
        public bool FlagBindScale { get; set; }
        public bool FlagDestroyWhenObjectLeaves { get; set; }
        public bool FlagDestroyWhenBindEffectOff { get; set; }
        public bool FlagGetObjectFade { get; set; }
        public bool FlagBonePos { get; set; }
        public bool FlagBindCamera { get; set; }
        public float StartWait { get; set; }
        public int SoundEffectNumber { get; set; }
        public float TranslationX { get; set; }
        public float TranslationZ { get; set; }
        public float TranslationY { get; set; }
        public float RotationX { get; set; }
        public float RotationZ { get; set; }
        public float RotationY { get; set; }
        public float ScaleX { get; set; }
        public float ScaleZ { get; set; }
        public float ScaleY { get; set; }
        public Pax.Element.BonePos BonePosition { get; set; } = new Pax.Element.BonePos();

        public static M_EffectElements_Wrapper Wrap(Pax.Element entry)
        {
            M_EffectElements_Wrapper wrapper = new M_EffectElements_Wrapper();
            Property_Util.CopyProperties(entry, wrapper);
            Property_Util.CopyProperties(entry.BonePosition, wrapper.BonePosition);
            return wrapper;
        }
        public Pax.Element Unwrap()
        {
            Pax.Element entry = new Pax.Element();
            Property_Util.CopyProperties(this, entry);
            Property_Util.CopyProperties(this.BonePosition, entry.BonePosition);
            return entry;
        }
    }
}
