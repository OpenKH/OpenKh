using OpenKh.Engine.Renderers;
using OpenKh.Common;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using OpenKh.Engine.Input;

namespace OpenKh.Game.Menu
{
    public class MenuStatus : MenuBase
    {
        const int OptionHover = 0;
        const int OptionStart = 1;
        const int OptionSel = 2;
        const int OptionEnd = 3;
        private readonly int[] OptionNormal = new int[]
        {
            132,
            133,
            134,
            135
        };
        private readonly int[] OptionSub = new int[]
        {
            128,
            129,
            130,
            131
        };

        private const int CharacterSora = 0;
        private const int CharacterRoxas = 14;
        private const int CharacterDonald = 1;
        private const int CharacterGoofy = 2;
        private const int CharacterMickey = 3;
        private const int CharacterAuron = 4;
        private const int CharacterMulan = 5;
        private const int CharacterAladdin = 6;
        private const int CharacterJack = 7;
        private const int CharacterBeast = 8;
        private const int CharacterSparrow = 9;
        private const int CharacterSimba = 10;
        private const int CharacterTron = 11;
        private const int CharacterRiku = 12;

        private const int FormVanillaValor = 0;
        private const int FormVanillaWisdom = 1;
        private const int FormVanillaMaster = 2;
        private const int FormVanillaFinal = 3;
        private const int FormVanillaSummon = 4;
        private const int FormFinalMixValor = 0;
        private const int FormFinalMixWisdom = 1;
        private const int FormFinalMixLimit = 2;
        private const int FormFinalMixMaster = 3;
        private const int FormFinalMixFinal = 4;
        private const int FormFinalMixSummon = 5;

        private const int SeqResistance = 139;
        private const int SeqLevel = 137;
        private const int SeqBonus = 144;
        private const int SeqBattle = 150;
        private const int SeqStats = 143;
        private const int WindowStart = 140;
        private const int WindowLoop = 141;
        private const int WindowEnd = 142;
        private const int SeqValueText = 124;

        private static readonly int[] CharacterMsgIds = new int[]
        {
            0x851F,
            0x8520,
            0x8521,
            0x8522,
            0x8523,
            0x8525, // 0x8524 for Ping
            0x8526,
            0x8527,
            0x8528,
            0x8529,
            0x852A,
            0x852B,
            0x852C,
            0x852D,
        };

        private static readonly int[] FormVanillaMsgIds = new int[]
        {
            0xC32A,
            0xC32B,
            0xC32C,
            0xC32D,
            //0xC5B1, // no summon here
            0x864A, // antiform
        };

        private static readonly int[] FormFinalMixMsgIds = new int[]
        {
            0xC32A,
            0xC32B,
            0xCF0B,
            0xC32C,
            0xC32D,
            //0xC5B1, // no summon here
            0x864A, // antiform
        };

        private static readonly int[] CharacterSoraVanilla = new int[]
        {
            CharacterSora,
            FormVanillaValor,
            FormVanillaWisdom,
            FormVanillaMaster,
            FormVanillaFinal,
            FormVanillaSummon
        };
        private static readonly int[] CharacterSoraFinalMix = new int[]
        {
            CharacterSora,
            FormFinalMixValor,
            FormFinalMixWisdom,
            FormFinalMixLimit,
            FormFinalMixMaster,
            FormFinalMixFinal,
            FormFinalMixSummon
        };

        private readonly int[][] Characters;
        private readonly int[] FormMsgIds;
        private readonly int[] CurrentCharacters;
        private IAnimatedSequence _menuSeq;
        private IAnimatedSequence _subMenuSeq;
        private List<(int charaterIndex, int formIndex)> _partyList;
        private int OptionCount => _partyList.Count;
        private int _selectedOption;

        public int SelectedOption
        {
            get => _selectedOption;
            set
            {
                value = value < 0 ? OptionCount - 1 : value % OptionCount;
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    _menuSeq = InitializeMenu(true);
                    _menuSeq.Begin();

                    _subMenuSeq = InitializeSubMenu(true);
                    _subMenuSeq.Begin();
                }
            }
        }

        public override ushort MenuNameId => 0xBEB7;

        protected override bool IsEnd => _menuSeq.IsEnd;

        public MenuStatus(IMenuManager menuManager) : base(menuManager)
        {
            var isFinalMix = menuManager.GameContext.Kernel.IsFinalMix;
            Characters = new int[][]
            {
                isFinalMix ? CharacterSoraFinalMix : CharacterSoraVanilla,
                new int[] { CharacterDonald },
                new int[] { CharacterGoofy },
                new int[] { CharacterMickey },
                new int[] { CharacterAuron },
                new int[] { CharacterMulan },
                new int[] { CharacterAladdin },
                new int[] { CharacterJack },
                new int[] { CharacterBeast },
                new int[] { CharacterSparrow },
                new int[] { CharacterSimba },
                new int[] { CharacterTron },
                new int[] { CharacterRiku },
                new int[] { CharacterRoxas },
            };
            FormMsgIds = isFinalMix ? FormFinalMixMsgIds : FormVanillaMsgIds;

            CurrentCharacters = new int[] // TODO: inhert this from the save data
            {
                CharacterSora,
                CharacterRiku,
                CharacterGoofy,
                CharacterDonald
            };

            // Generates the menu option list
            _partyList = new List<(int charaterIndex, int formIndex)>();
            for (var i = 0; i < CurrentCharacters.Length; i++)
            {
                var characterId = CurrentCharacters[i];
                var characterDesc = Characters[characterId];
                _partyList.Add((characterDesc[0], -1));
                for (var j = 1; j < characterDesc.Length; j++)
                    _partyList.Add((characterId, characterDesc[j]));
            }

            _menuSeq = InitializeMenu(false);
            _menuSeq.Begin();

            _subMenuSeq = InitializeSubMenu(false);
            _subMenuSeq.Begin();
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            AnimatedSequenceDesc createSequenceDesc(int index, int msgId, int[] optionSeq) => new AnimatedSequenceDesc
            {
                SequenceIndexStart = skipIntro ? -1 : optionSeq[OptionStart],
                SequenceIndexLoop = SelectedOption == index ? optionSeq[OptionHover] : optionSeq[OptionSel],
                SequenceIndexEnd = optionSeq[OptionEnd],
                StackIndex = index,
                StackWidth = 0,
                StackHeight = AnimatedSequenceDesc.DefaultStacking,
                Flags = AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                MessageId = (ushort)msgId,
                Children = SelectedOption == index ? new List<AnimatedSequenceDesc>
                    {
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexLoop = 25,
                            TextAnchor = TextAnchor.BottomLeft,
                            Flags = AnimationFlags.NoChildTranslationX,
                        },
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexStart = skipIntro ? -1 : 27,
                            SequenceIndexLoop = 28,
                            SequenceIndexEnd = 29,
                        }
                    } : new List<AnimatedSequenceDesc>()
            };

            var animDescs = new List<AnimatedSequenceDesc>();
            animDescs = _partyList.Select((x, i) =>
            {
                var characterDesc = Characters[x.charaterIndex];
                if (x.formIndex >= 0)
                    return createSequenceDesc(i, FormMsgIds[x.formIndex], OptionSub);
                else
                    return createSequenceDesc(i, CharacterMsgIds[characterDesc[0]], OptionNormal);
            }).ToList();

            return SequenceFactory.Create(animDescs);
        }
        private IAnimatedSequence InitializeSubMenu(bool skipIntro)
        {
            AnimatedSequenceDesc createSequenceDesc(int x, int y, int msgId, int seqId, object value) => new AnimatedSequenceDesc
            {
                X = x * 170,
                Y = y * 24,
                SequenceIndexStart = -1,
                SequenceIndexLoop = seqId,
                SequenceIndexEnd = -1,
                StackIndex = 0,
                StackWidth = 0,
                StackHeight = 0,
                Flags = AnimationFlags.TextIgnoreScaling |
                        AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                MessageId = (ushort)msgId,
                TextAnchor = TextAnchor.BottomLeft,
                Children = new List<AnimatedSequenceDesc>()
                {
                    new AnimatedSequenceDesc
                    {
                        SequenceIndexStart = -1,
                        SequenceIndexLoop = SeqValueText,
                        SequenceIndexEnd = -1,
                        StackIndex = 0,
                        StackWidth = AnimatedSequenceDesc.DefaultStacking,
                        StackHeight = AnimatedSequenceDesc.DefaultStacking,
                        Flags = AnimationFlags.TextTranslateX,
                        TextAnchor = TextAnchor.BottomRight,
                        MessageText = value.ToString(),
                        Children = new List<AnimatedSequenceDesc>(),
                    }
                }
            };

            IEnumerable<AnimatedSequenceDesc> createMenuForMainPlayer(int characterIndex)
            {
                var saveData = MenuManager.GameContext.Kernel.SaveData;
                if (characterIndex < 0 || characterIndex >= (saveData.Characters?.Length ?? 0))
                {
                    Log.Err("Stats for character '{0}' is out of range", characterIndex);
                    yield break;
                }

                var formIndex = 0;
                var character = saveData.Characters[characterIndex];
                if (character == null)
                {
                    Log.Err("Stats for character '{0}' is null", characterIndex);
                    yield break;
                }

                yield return createSequenceDesc(0, 0, 0xC559, SeqLevel, character.Level);
                yield return createSequenceDesc(0, 1, 0xC55A, SeqLevel, saveData.Experience);
                yield return createSequenceDesc(0, 2, 0xC55B, SeqLevel, -1);
                yield return createSequenceDesc(0, 3, 0xC55C, SeqStats, $"{character.HpCur}/{character.HpMax}");
                yield return createSequenceDesc(0, 4, 0xC55D, SeqStats, $"{character.MpCur}/{character.MpMax}");
                yield return createSequenceDesc(0, 5, 0xC55E, SeqStats, $"-1/{character.ApBoost}");
                yield return createSequenceDesc(0, 6, 0xC55F, SeqStats, -1);
                yield return createSequenceDesc(1, 0, 0xC560, SeqBattle, character.StrengthBoost);
                yield return createSequenceDesc(1, 1, 0xC561, SeqBattle, character.MagicBoost);
                yield return createSequenceDesc(1, 2, 0xC562, SeqBattle, character.DefenseBoost);
                yield return createSequenceDesc(1, 3, 0xC563, SeqResistance, "-1%");
                yield return createSequenceDesc(1, 4, 0xC564, SeqResistance, "-1%");
                yield return createSequenceDesc(1, 5, 0xC565, SeqResistance, "-1%");
                yield return createSequenceDesc(1, 6, 0xC566, SeqResistance, "-1%");
                yield return createSequenceDesc(0, 7, 0xC567, SeqBonus, saveData.BonusLevel);
                yield return createSequenceDesc(1, 7, 0xC5AA, SeqBonus, saveData.DriveForms[formIndex++].Level);
                yield return createSequenceDesc(0, 8, 0xC5AB, SeqBonus, saveData.DriveForms[formIndex++].Level);
                if (MenuManager.GameContext.Kernel.IsFinalMix)
                    yield return createSequenceDesc(1, 8, 0xCE9B, SeqBonus, saveData.DriveForms[formIndex++].Level);
                yield return createSequenceDesc(0, 9, 0xC5AC, SeqBonus, saveData.DriveForms[formIndex++].Level);
                yield return createSequenceDesc(1, 9, 0xC5AD, SeqBonus, saveData.DriveForms[formIndex++].Level);
            }

            IEnumerable<AnimatedSequenceDesc> createMenuForPartyMember(int characterIndex)
            {
                var saveData = MenuManager.GameContext.Kernel.SaveData;
                if (characterIndex < 0 || characterIndex >= (saveData.Characters?.Length ?? 0))
                {
                    Log.Err("Stats for character '{0}' is out of range", characterIndex);
                    yield break;
                }

                var character = saveData.Characters[characterIndex];
                if (character == null)
                {
                    Log.Err("Stats for character '{0}' is null", characterIndex);
                    yield break;
                }

                yield return createSequenceDesc(0, 0, 0xC559, SeqLevel, character.Level);
                yield return createSequenceDesc(0, 1, 0xC55A, SeqLevel, saveData.Experience);
                yield return createSequenceDesc(0, 2, 0xC55B, SeqLevel, -1);
                yield return createSequenceDesc(0, 3, 0xC55C, SeqStats, $"{character.HpCur}/{character.HpMax}");
                yield return createSequenceDesc(0, 4, 0xC55D, SeqStats, $"{character.MpCur}/{character.MpMax}");
                yield return createSequenceDesc(0, 5, 0xC55E, SeqStats, $"-1/{character.ApBoost}");
                yield return createSequenceDesc(1, 0, 0xC560, SeqBattle, character.StrengthBoost);
                yield return createSequenceDesc(1, 1, 0xC561, SeqBattle, character.MagicBoost);
                yield return createSequenceDesc(1, 2, 0xC562, SeqBattle, character.DefenseBoost);
                yield return createSequenceDesc(1, 3, 0xC563, SeqResistance, "-1%");
                yield return createSequenceDesc(1, 4, 0xC564, SeqResistance, "-1%");
                yield return createSequenceDesc(1, 5, 0xC565, SeqResistance, "-1%");
                yield return createSequenceDesc(1, 6, 0xC566, SeqResistance, "-1%");
            }

            IEnumerable<AnimatedSequenceDesc> createMenuForDriveForm(int driveFormIndex)
            {
                var saveData = MenuManager.GameContext.Kernel.SaveData;
                if (driveFormIndex < 0 || driveFormIndex >= (saveData.DriveForms?.Length ?? 0))
                {
                    Log.Err("Stats for drive form '{0}' is out of range", driveFormIndex);
                    yield break;
                }
                var driveForm = saveData.DriveForms[driveFormIndex];
                if (driveForm == null)
                {
                    Log.Err("Stats for drive form '{0}' is null", driveFormIndex);
                    yield break;
                }

                yield return createSequenceDesc(0, 0, 0xC569, SeqLevel, driveForm.Level);
                yield return createSequenceDesc(0, 1, 0xC55A, SeqLevel, driveForm.Experience);
                yield return createSequenceDesc(0, 2, 0xC55B, SeqLevel, -1);
                yield return createSequenceDesc(0, 3, 0xC583, SeqStats, -1);
                yield return createSequenceDesc(1, 0, 0xC560, SeqBattle, -1);
                yield return createSequenceDesc(1, 1, 0xC561, SeqBattle, -1);
                yield return createSequenceDesc(1, 2, 0xC562, SeqBattle, -1);
                yield return createSequenceDesc(1, 3, 0xC563, SeqResistance, -1);
                yield return createSequenceDesc(1, 4, 0xC564, SeqResistance, -1);
                yield return createSequenceDesc(1, 5, 0xC565, SeqResistance, -1);
                yield return createSequenceDesc(1, 6, 0xC566, SeqResistance, -1);
            }

            var animDesc = new AnimatedSequenceDesc
            {
                SequenceIndexStart = skipIntro ? -1 : WindowStart,
                SequenceIndexLoop = WindowLoop,
                SequenceIndexEnd = WindowEnd,
                StackHeight = AnimatedSequenceDesc.DefaultStacking,
                Flags = AnimationFlags.TextTranslateX,
                Children = null
            };

            var partyMemberDesc = _partyList[SelectedOption];
            if (partyMemberDesc.formIndex < 0)
            {
                switch (partyMemberDesc.charaterIndex)
                {
                    case CharacterSora:
                    case CharacterRoxas:
                        animDesc.Children = createMenuForMainPlayer(partyMemberDesc.charaterIndex).ToList();
                        break;
                    default:
                        animDesc.Children = createMenuForPartyMember(partyMemberDesc.charaterIndex).ToList();
                        break;
                }
            }
            else
            {
                animDesc.Children = createMenuForDriveForm(partyMemberDesc.formIndex).ToList();
            }


            return SequenceFactory.Create(animDesc);
        }

        protected override void ProcessInput(IInput input)
        {
            if (input.Repeated.Up)
                SelectedOption--;
            else if (input.Repeated.Down)
                SelectedOption++;
            else if (input.Triggered.Confirm)
            {
                // No selection
            }
            else
                base.ProcessInput(input);
        }

        public override void Open()
        {
            _menuSeq.Begin();
            base.Open();
        }

        public override void Close()
        {
            _menuSeq.End();
            base.Close();
        }

        protected override void MyUpdate(double deltaTime)
        {
            _menuSeq.Update(deltaTime);
            _subMenuSeq.Update(deltaTime);
        }

        protected override void MyDraw()
        {
            _menuSeq.Draw(0, 0);
            _subMenuSeq.Draw(0, 0);
        }

        public override string ToString() => "STATUS";
    }
}
