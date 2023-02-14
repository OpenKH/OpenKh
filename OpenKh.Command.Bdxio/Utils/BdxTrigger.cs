using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.Bdxio.Utils
{
    public record BdxTrigger(string Label, int Key)
    {
        public static BdxTrigger[] GetKnownItems() => new BdxTrigger[]
        {
            new BdxTrigger("OBJ_INIT", 0x0),
            new BdxTrigger("OBJ_MAIN", 0x1),
            new BdxTrigger("UNIT_INIT", 0x2),
            new BdxTrigger("UNIT_MAIN", 0x3),
            new BdxTrigger("UNIT_FINALIZER", 0x4),
            new BdxTrigger("TEST", 0x5),
            new BdxTrigger("OBJ_DAMAGE", 0x6),
            new BdxTrigger("NEWGAME", 0x7),
            new BdxTrigger("PROGRESS_CALLBACK", 0x8),
            new BdxTrigger("GAMEOVER", 0x9),
            new BdxTrigger("SIGNAL_CALLBACK", 0xA),
            new BdxTrigger("REACTION_CALLBACK", 0xB),
            new BdxTrigger("SECOND_SET_CALLBACK", 0xC),
            new BdxTrigger("OBJ_FALL", 0xD),
            new BdxTrigger("OBJ_LAND", 0xE),
            new BdxTrigger("OBJ_ATTACK", 0xF),
            new BdxTrigger("OBJ_ANMATR_EFFECT", 0x10),
            new BdxTrigger("OBJ_ANMATR_CALLBACK", 0x11),
            new BdxTrigger("OBJ_CHANGE_ACT", 0x12),
            new BdxTrigger("OBJ_REFLECT", 0x13),
            new BdxTrigger("MAGIC_START", 0x14),
            new BdxTrigger("MAGIC_SHOT", 0x15),
            new BdxTrigger("LIMIT_START", 0x16),
            new BdxTrigger("LIMIT_CALL", 0x17),
            new BdxTrigger("LIMIT_ANMATR_EFFECT", 0x18),
            new BdxTrigger("LIMIT_ANMATR_CALLBACK", 0x19),
            new BdxTrigger("LIMIT_ATTACK", 0x1A),
            new BdxTrigger("OBJ_IK_ADJUST_CALLBACK", 0x1B),
        };
    }
}
