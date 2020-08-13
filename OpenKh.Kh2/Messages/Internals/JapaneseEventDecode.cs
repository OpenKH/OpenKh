using System.Collections.Generic;

namespace OpenKh.Kh2.Messages.Internals
{
    internal class JapaneseEventDecode : IMessageDecode
    {
        public static readonly Dictionary<byte, BaseCmdModel> _table = new Dictionary<byte, BaseCmdModel>
        {
            [0x00] = new SimpleCmdModel(MessageCommand.End),
            [0x01] = new TextCmdModel(' '),
            [0x02] = new SimpleCmdModel(MessageCommand.NewLine),
            [0x03] = new SimpleCmdModel(MessageCommand.Reset),
            [0x04] = new SingleDataCmdModel(MessageCommand.Theme),
            [0x05] = new DataCmdModel(MessageCommand.Unknown05, 6),
            [0x06] = new SingleDataCmdModel(MessageCommand.Unknown06),
            [0x07] = new DataCmdModel(MessageCommand.Color, 4),
            [0x08] = new DataCmdModel(MessageCommand.Unknown08, 3),
            [0x09] = new SingleDataCmdModel(MessageCommand.PrintIcon),
            [0x0a] = new SingleDataCmdModel(MessageCommand.TextScale),
            [0x0b] = new SingleDataCmdModel(MessageCommand.TextWidth),
            [0x0c] = new SingleDataCmdModel(MessageCommand.LineSpacing),
            [0x0d] = new SimpleCmdModel(MessageCommand.Unknown0d),
            [0x0e] = new SingleDataCmdModel(MessageCommand.Unknown0e),
            [0x0f] = new DataCmdModel(MessageCommand.Unknown0f, 5),
            [0x10] = new SimpleCmdModel(MessageCommand.Clear),
            [0x11] = new DataCmdModel(MessageCommand.Position, 4),
            [0x12] = new DataCmdModel(MessageCommand.Unknown12, 2),
            [0x13] = new DataCmdModel(MessageCommand.Unknown13, 4),
            [0x14] = new DataCmdModel(MessageCommand.Delay, 2),
            [0x15] = new DataCmdModel(MessageCommand.CharDelay, 2),
            [0x16] = new SingleDataCmdModel(MessageCommand.Unknown16),
            [0x17] = null,
            [0x18] = new DataCmdModel(MessageCommand.Unknown18, 2),
            [0x19] = new TableCmdModel(MessageCommand.Table2, JapaneseEventTable._table2),
            [0x1a] = new TableCmdModel(MessageCommand.Table3, JapaneseEventTable._table3),
            [0x1b] = new TableCmdModel(MessageCommand.Table4, JapaneseEventTable._table4),
            [0x1c] = new TableCmdModel(MessageCommand.Table5, JapaneseEventTable._table5),
            [0x1d] = new TableCmdModel(MessageCommand.Table6, JapaneseEventTable._table6),
            [0x1e] = new TableCmdModel(MessageCommand.Table7, JapaneseEventTable._table7),
            [0x1f] = new TableCmdModel(MessageCommand.Table8, JapaneseEventTable._table8),
            [0x20] = new TextCmdModel(' '),
            [0x21] = new TextCmdModel('0'),
            [0x22] = new TextCmdModel('1'),
            [0x23] = new TextCmdModel('2'),
            [0x24] = new TextCmdModel('3'),
            [0x25] = new TextCmdModel('4'),
            [0x26] = new TextCmdModel('5'),
            [0x27] = new TextCmdModel('6'),
            [0x28] = new TextCmdModel('7'),
            [0x29] = new TextCmdModel('8'),
            [0x2a] = new TextCmdModel('9'),
            [0x2b] = new TextCmdModel('+'),
            [0x2c] = new TextCmdModel('−'),
            [0x2d] = new TextCmdModel('ₓ'),
            [0x2e] = new TextCmdModel('A'),
            [0x2f] = new TextCmdModel('B'),
            [0x30] = new TextCmdModel('C'),
            [0x31] = new TextCmdModel('D'),
            [0x32] = new TextCmdModel('E'),
            [0x33] = new TextCmdModel('F'),
            [0x34] = new TextCmdModel('G'),
            [0x35] = new TextCmdModel('H'),
            [0x36] = new TextCmdModel('I'),
            [0x37] = new TextCmdModel('J'),
            [0x38] = new TextCmdModel('K'),
            [0x39] = new TextCmdModel('L'),
            [0x3a] = new TextCmdModel('M'),
            [0x3b] = new TextCmdModel('N'),
            [0x3c] = new TextCmdModel('O'),
            [0x3d] = new TextCmdModel('P'),
            [0x3e] = new TextCmdModel('Q'),
            [0x3f] = new TextCmdModel('R'),
            [0x40] = new TextCmdModel('S'),
            [0x41] = new TextCmdModel('T'),
            [0x42] = new TextCmdModel('U'),
            [0x43] = new TextCmdModel('V'),
            [0x44] = new TextCmdModel('W'),
            [0x45] = new TextCmdModel('X'),
            [0x46] = new TextCmdModel('Y'),
            [0x47] = new TextCmdModel('Z'),
            [0x48] = new TextCmdModel('!'),
            [0x49] = new TextCmdModel('?'),
            [0x4a] = new TextCmdModel('%'),
            [0x4b] = new TextCmdModel('/'),
            [0x4c] = new TextCmdModel('※'),
            [0x4d] = new TextCmdModel('、'),
            [0x4e] = new TextCmdModel('。'),
            [0x4f] = new TextCmdModel('.'),
            [0x50] = new TextCmdModel(','),
            [0x51] = new TextCmdModel('·'),
            [0x52] = new TextCmdModel(':'),
            [0x53] = new TextCmdModel('…'),
            [0x54] = new TextCmdModel("-"),
            [0x55] = new TextCmdModel('ー'),
            [0x56] = new TextCmdModel('〜'),
            [0x57] = new TextCmdModel("'"),
            [0x58] = new TextCmdModel("‟"),
            [0x59] = new TextCmdModel("„"),
            [0x5a] = new TextCmdModel('('),
            [0x5b] = new TextCmdModel(')'),
            [0x5c] = new TextCmdModel('「'),
            [0x5d] = new TextCmdModel('」'),
            [0x5e] = new TextCmdModel('『'),
            [0x5f] = new TextCmdModel('』'),
            [0x60] = new TextCmdModel('“'),
            [0x61] = new TextCmdModel('”'),
            [0x62] = new TextCmdModel('['),
            [0x63] = new TextCmdModel(']'),
            [0x64] = new TextCmdModel('<'),
            [0x65] = new TextCmdModel('>'),
            [0x66] = new TextCmdModel('-'),
            [0x67] = new TextCmdModel("–"),
            [0x68] = new TextCmdModel('⤷'), // Used only in EVT
            [0x69] = new TextCmdModel('♪'),
            [0x6a] = new TextCmdModel('⇾'), // Used only in EVT
            [0x6b] = new TextCmdModel('⇽'), // Used only in EVT
            [0x6c] = new TextCmdModel("全"),
            [0x6d] = new TextCmdModel("合"),
            [0x6e] = new TextCmdModel("成"),
            [0x6f] = new TextCmdModel("半"),
            [0x70] = new TextCmdModel('◯'),
            [0x71] = new TextCmdModel('✕'),
            [0x72] = new TextCmdModel('△'),
            [0x73] = new TextCmdModel('☐'),
            [0x74] = new TextCmdModel('▴'),
            [0x75] = new TextCmdModel('▾'),
            [0x76] = new TextCmdModel('▸'),
            [0x77] = new TextCmdModel('◂'),
            [0x78] = null,
            [0x79] = null,
            [0x7a] = null,
            [0x7b] = null,
            [0x7c] = null,
            [0x7d] = null,
            [0x7e] = null,
            [0x7f] = null,
            [0x80] = null,
            [0x81] = null,
            [0x82] = new TextCmdModel('⭑'),
            [0x83] = new TextCmdModel('⭒'),
            [0x84] = new TextCmdModel('l'),
            [0x85] = new TextCmdModel('a'),
            [0x86] = new TextCmdModel('y'),
            [0x87] = new TextCmdModel('t'),
            [0x88] = new TextCmdModel('i'),
            [0x89] = new TextCmdModel('o'),
            [0x8a] = new TextCmdModel('n'),
            [0x8b] = new TextCmdModel('r'),
            [0x8c] = new TextCmdModel('⬛'),
            [0x8d] = new TextCmdModel('前'),
            [0x8e] = new TextCmdModel('選'),
            [0x8f] = new TextCmdModel('一'),
            [0x90] = new TextCmdModel('あ'),
            [0x91] = new TextCmdModel('い'),
            [0x92] = new TextCmdModel('う'),
            [0x93] = new TextCmdModel('え'),
            [0x94] = new TextCmdModel('お'),
            [0x95] = new TextCmdModel('か'),
            [0x96] = new TextCmdModel('き'),
            [0x97] = new TextCmdModel('く'),
            [0x98] = new TextCmdModel('け'),
            [0x99] = new TextCmdModel('こ'),
            [0x9a] = new TextCmdModel('さ'),
            [0x9b] = new TextCmdModel('し'),
            [0x9c] = new TextCmdModel('す'),
            [0x9d] = new TextCmdModel('せ'),
            [0x9e] = new TextCmdModel('そ'),
            [0x9f] = new TextCmdModel('た'),
            [0xa0] = new TextCmdModel('ち'),
            [0xa1] = new TextCmdModel('つ'),
            [0xa2] = new TextCmdModel('て'),
            [0xa3] = new TextCmdModel('と'),
            [0xa4] = new TextCmdModel('な'),
            [0xa5] = new TextCmdModel('に'),
            [0xa6] = new TextCmdModel('ぬ'),
            [0xa7] = new TextCmdModel('ね'),
            [0xa8] = new TextCmdModel('の'),
            [0xa9] = new TextCmdModel('は'),
            [0xaa] = new TextCmdModel('ひ'),
            [0xab] = new TextCmdModel('ふ'),
            [0xac] = new TextCmdModel('へ'),
            [0xad] = new TextCmdModel('ほ'),
            [0xae] = new TextCmdModel('ま'),
            [0xaf] = new TextCmdModel('み'),
            [0xb0] = new TextCmdModel('む'),
            [0xb1] = new TextCmdModel('め'),
            [0xb2] = new TextCmdModel('も'),
            [0xb3] = new TextCmdModel('や'),
            [0xb4] = new TextCmdModel('ゆ'),
            [0xb5] = new TextCmdModel('よ'),
            [0xb6] = new TextCmdModel('ら'),
            [0xb7] = new TextCmdModel('り'),
            [0xb8] = new TextCmdModel('る'),
            [0xb9] = new TextCmdModel('れ'),
            [0xba] = new TextCmdModel('ろ'),
            [0xbb] = new TextCmdModel('わ'),
            [0xbc] = new TextCmdModel('を'),
            [0xbd] = new TextCmdModel('ん'),
            [0xbe] = new TextCmdModel('が'),
            [0xbf] = new TextCmdModel('ぎ'),
            [0xc0] = new TextCmdModel('ぐ'),
            [0xc1] = new TextCmdModel('げ'),
            [0xc2] = new TextCmdModel('ご'),
            [0xc3] = new TextCmdModel('ざ'),
            [0xc4] = new TextCmdModel('じ'),
            [0xc5] = new TextCmdModel('ず'),
            [0xc6] = new TextCmdModel('ぜ'),
            [0xc7] = new TextCmdModel('ぞ'),
            [0xc8] = new TextCmdModel('だ'),
            [0xc9] = new TextCmdModel('ぢ'),
            [0xca] = new TextCmdModel('づ'),
            [0xcb] = new TextCmdModel('で'),
            [0xcc] = new TextCmdModel('ど'),
            [0xcd] = new TextCmdModel('ば'),
            [0xce] = new TextCmdModel('び'),
            [0xcf] = new TextCmdModel('ぶ'),
            [0xd0] = new TextCmdModel('べ'),
            [0xd1] = new TextCmdModel('ぼ'),
            [0xd2] = new TextCmdModel('ぱ'),
            [0xd3] = new TextCmdModel('ぴ'),
            [0xd4] = new TextCmdModel('ぷ'),
            [0xd5] = new TextCmdModel('ぺ'),
            [0xd6] = new TextCmdModel('ぽ'),
            [0xd7] = new TextCmdModel('ぁ'),
            [0xd8] = new TextCmdModel('ぃ'),
            [0xd9] = new TextCmdModel('ぅ'),
            [0xda] = new TextCmdModel('ぇ'),
            [0xdb] = new TextCmdModel('ぉ'),
            [0xdc] = new TextCmdModel('ゃ'),
            [0xdd] = new TextCmdModel('ゅ'),
            [0xde] = new TextCmdModel('ょ'),
            [0xdf] = new TextCmdModel('っ'),
            [0xe0] = new TextCmdModel('ア'),
            [0xe1] = new TextCmdModel('イ'),
            [0xe2] = new TextCmdModel('ウ'),
            [0xe3] = new TextCmdModel('エ'),
            [0xe4] = new TextCmdModel('オ'),
            [0xe5] = new TextCmdModel('カ'),
            [0xe6] = new TextCmdModel('キ'),
            [0xe7] = new TextCmdModel('ク'),
            [0xe8] = new TextCmdModel('ケ'),
            [0xe9] = new TextCmdModel('コ'),
            [0xea] = new TextCmdModel('サ'),
            [0xeb] = new TextCmdModel('シ'),
            [0xec] = new TextCmdModel('ス'),
            [0xed] = new TextCmdModel('セ'),
            [0xee] = new TextCmdModel('ソ'),
            [0xef] = new TextCmdModel('タ'),
            [0xf0] = new TextCmdModel('チ'),
            [0xf1] = new TextCmdModel('ツ'),
            [0xf2] = new TextCmdModel('テ'),
            [0xf3] = new TextCmdModel('ト'),
            [0xf4] = new TextCmdModel('ナ'),
            [0xf5] = new TextCmdModel('ニ'),
            [0xf6] = new TextCmdModel('ヌ'),
            [0xf7] = new TextCmdModel('ネ'),
            [0xf8] = new TextCmdModel('ノ'),
            [0xf9] = new TextCmdModel('ハ'),
            [0xfa] = new TextCmdModel('ヒ'),
            [0xfb] = new TextCmdModel('フ'),
            [0xfc] = new TextCmdModel('ヘ'),
            [0xfd] = new TextCmdModel('ホ'),
            [0xfe] = new TextCmdModel('マ'),
            [0xff] = new TextCmdModel('ミ'),
        };

        public List<MessageCommandModel> Decode(byte[] data) =>
            new BaseMessageDecoder(_table, data).Decode(decoder =>
            {
                if (decoder.IsEof(1))
                    return false;

                var ch = decoder.Peek(0);
                var parameter = decoder.Peek(1);
                decoder.WrapTable(ref ch, ref parameter);

                switch (ch)
                {
                    case 0x1d:
                        switch (parameter)
                        {
                            case 0x1a: return AppendComplex(decoder, "III");
                            case 0x1b: return AppendComplex(decoder, "VII");
                            case 0x1c: return AppendComplex(decoder, "VIII");
                            case 0x1d: return AppendComplex(decoder, "X");
                        }
                        break;
                    case 0x1e:
                        switch (parameter)
                        {
                            case 0x50: return AppendComplex(decoder, "XIII");
                            case 0xb6: return AppendComplex(decoder, "VI");
                            case 0xb7: return AppendComplex(decoder, "IX");
                        }
                        break;
                }

                return false;
            });

        private bool AppendComplex(IDecoder decoder, string value)
        {
            decoder.Next();
            decoder.Next();
            decoder.AppendComplex(value);
            return true;
        }
    }

    internal static class JapaneseEventTable
    {
        public static readonly char[] _table2 = new char[0x100]
        {
            'ム', 'メ', 'モ', 'ヤ', 'ユ', 'ヨ', 'ラ',
            'リ', 'ル', 'レ', 'ロ', 'ワ', 'ヲ', 'ン', 'ガ', 'ギ', 'グ', 'ゲ', 'ゴ', 'ザ', 'ジ', 'ズ', 'ゼ', 'ゾ', 'ダ', 'ヂ', 'ヅ', 'デ',
            'ド', 'バ', 'ビ', 'ブ', 'ベ', 'ボ', 'ヴ', 'パ', 'ピ', 'プ', 'ペ', 'ポ', 'ァ', 'ィ', 'ゥ', 'ェ', 'ォ', 'ャ', 'ュ', 'ョ', 'ッ',
            '端', '子', '接', '続', '正', '常', '発', '生', '使', '用', '専', '付', '属', '取', '扱', '説', '明', '書', '指', '示', '従',
            '修', '復', '下', '空', '容', '量', '不', '足', '以', '上', '必', '要', '開', '始', '本', '魔', '石', '晶', '結', '大', '紋',
            '盾', '凍', '透', '燃', '水', '気', '炎', '士', '守', '証', '吹', '闘', '力', '木', '人', '飲', '過', '丸', '騎', '去', '魚',
            '剣', '荒', '氏', '杖', '束', '太', '鳥', '導', '布', '風', '満', '約', '源', '出', '実', '海', '思', '王', '自', '名', '喚',
            '召', '地', '家', '火', '森', '皆', '許', '恐', '古', '轟', '魂', '臭', '章', '雲', '雪', '然', '達', '嘆', '潰', '瓜', '伝',
            '怒', '悲', '怖', '鳴', '免', '雷', '林', '様', '巻', '片', '翼', '天', '則', '四', '掲', '板', '登', '孫', '⬛', '⬛', '⬛',
            '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛',
            '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛',
            
            //evt1-2
            '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛',
            '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛',
        };

        public static readonly char[] _table3 = new char[0x100]
        {
            '⬛', '誠', '納',
            '泡', '程', '析', '遮', '戒', '井', '及', '模', '算', '霧', '熟', '＃', '鈍', '賭', '欺', '鹿', '式', '炸', '裂', '索', '詮',
            '眩', '罠', '千', '塗', '羽', '柵', '緊', '粗', '何', '俺', '行', '世', '界', '紀', '事', '前', '符', '来', '見', '一', '絡',
            '分', '寂', '心', '賃', '言', '物', '知', '外', '君', '私', '宛', '今', '男', '島', '老', '緒', '幕', '処', '菓', '場', '鎖',
            '食', '夢', '女', '賞', '所', '者', '時', '消', '探', '当', '無', '儀', '話', '鍵', '捜', '中', '目', '公', '声', '謎', '間',
            '為', '鍵', '密', '闇', '掌', '僕', '留', '句', '般', '誰', '聞', '連', '夫', '扉', '統', '顔', '丈', '少', '我', '他', '立',
            '合', '助', '対', '刀', '侮', '辱', '秘', '通', '頼', '冒', '帰', '刑', '夜', '絶', '船', '即', '方', '込', '全', '良', '忘',
            '凱', '追', '旅', '起', '険', '供', '抜', '遠', '最', '感', '役', '各', '入', '旋', '遂', '日', '欠', '堅', '主', '会', '向',
            '年', '早', '別', '褒', '宝', '変', '度', '後', '星', '授', '矢', '配', '現', '覚', '選', '急', '二', '居', '座', '願', '勝',
            '怪', '漁', '鼻', '影', '違', '離', '遅', '訣', '繋', '視', '勇', '笑', '奪', '馳', '々', '切', '厄', '父', '犬', '白', '跳',
            '祝', '仮', '強', '押', '仲', '面', '毛', '回', '倒', '拒', '持', '画', '近', '戻', '望', '負', '傷', '刀', '拐', '優', '内',
            '赦', '秒', '数', '敵', '槍', '珍', '姿', '歩', '左', '英', '雄', '破', '求', '鷹', '周', '蹴', '費', '衝', '狙', '経', '験',
            '値', '便', '表', '歳', '元', '災', '先', '宙', '残', '閉', '移', '動', '走', '眠', '与', '敢', '壊', '臆', '病', '犠', '牲',
            '滅',

        };

        public static readonly char[] _table4 = new char[0x100]
        {
            '竜', '床', '落', '思', '踏', '特', '加', '停', '応', '化', '右', '死', '襲', '由', '進', '滑', '陽', '頭', '済', '限',
            '昼', '五', '楽', '道', '昇', '朝', '照', '確', '甲', '更', '辿', '着', '断', '途', '光', '身', '番', '武', '器', '考', '傑',
            '性', '意', '味', '同', '景', '色', '長', '待', '彼', '悪', '穴', '飛', '友', '集', '植', '逃', '雅', '料', '街', '幸', '危',
            '決', '房', '記', '判', '掃', '部', '備', '完', '隠', '試', '遊', '姫', '形', '撫', '規', '材', '成', '置', '尾', '計', '作',
            '準', '卵', '仕', '命', '真', '音', '宮', '裁', '娘', '聴', '嘘', '殿', '憶', '信', '城', '奴', '福', '返', '己', '拠', '戦',
            '冗', '談', '康', '答', '弱', '黙', '呑', '欲', '理', '調', '犯', '予', '止', '博', '件', '解', '削', '舎', '渡', '深', '策',
            '屈', '装', '努', '挽', '捕', '失', '刻', '例', '息', '航', '敗', '得', '訳', '放', '矛', '告', '罪', '次', '参', '機', '巣',

            //evt2-1
            '金', '情', '令', '貸', '突', '円', '妙', '録', '笛', '境', '田', '秀', '根', '相', '終', '広', '乗', '駄', '的', '宇', '邪',
            '冷', '投', '呼', '葉', '暴', '百', '黄', '新', '町', '喰', '捨', '避', '妃', '安', '頃', '枚', '尻', '没', '嵐', '季', '節',
            '困', '悔', '汽', '暗', '排', '体', '状', '況', '流', '条', '希', '店', '被', '再', '第', '僧', '練', '習', '末', '契', '果',
            '可', '族', '滝', '反', '組', '多', '協', '支', '念', '了', '誘', '卒', '業', '泳', '嬢', '灯', '賊', '張', '嫌', '員', '隊',
            '争', '服', '黒', '→', '←', '口', '忙', '映', '写', '砂', '漠', '妖', '精', '粉', '存', '在', '♪', '野', '郎', '降', '想',
            '像', '数', '激', '国', '務',
        };

        public static readonly char[] _table5 = new char[0x100]
        {
            '操', '縦', '洞', '整', '鼓', '室', '屋', '貝', '騒', '問', '題', '簡', '単', '関', '係', '祈',
            '辺', '昔', '匹', '母', '礼', '任', '腹', '輩', '侵', '非', '距', '学', '愚', '点', '活', '躍', '臣', '喜', '板', '岩', '茶',
            '異', '線', '沈', '迷', '働', '秩', '序', '規', '＃', '貴', '送', '静', '初', '六', '払', '客', '薬', '暮', '青', '諸', '舵',
            '旗', '廃', '墟', '陰', '奇', '懐', '染', '横', '響', '逆', '苦', '威', '廷', '育', '群', '悟', '奥', '査', '慎', '好', '利',
            '有', '住', '平', '和', '壁', '瞬', '難', '干', '渉', '乱', '窟', '窓', '研', '究', '香', '台', 'ヶ', '救', '態', '村', '驚',
            '伸', '咲', '繁', '価', '甘', '獲', '案', '誤', '賛', '舞', '迎', '保', '識', '庫', '慣', '冠', '魅', '増', '狐', '握', '班',
            '散', '転', '弁', '至', '律', '亡', '万', '油', '等', '席', '興', '局', '位', '図', '宿', '申', '憎', '焼', '惑', '凶', '北',
            '謝', '議', '紹', '介', '複', '雑', '才', '虜', '恩', '順', '格', '皮', '質', '雨', '直', '胸', '近', '媒', '路', '未', '吸',
            '志', '封', '土', '牢', '獄', '溶', '哀', '届', '徒', '択', '勢', '踊', '祭', '叩', '叱', '厳', '報', '寸', '似', '引', '受',
            '囲', '振', '＃', '嬉', '背', '曲', '芽', '偶', '借', '休', '伏', '陛', '除', '緑', '換', '舟', '覆', '叶', 'a', '詳', '焦',
            '惜', '久', '企', '互', '招', '汚', '差', '塔', '吐', '渦', 'e', '狭', '尽', '構', '固', '紙', '読', 'i', '競', '姉', '検',
            '審', '担', '売', '買', '率', 'k', '美', '獣', '兵', 'n', 'o', '冥', '短', '忠', '盗', '赤', '耳', '故', '郷', '刺', '勉',
            '印', '功', '鬼', 'r', '蒸', '愛', '婚', '恋', '賢',
        };

        public static readonly char[] _table6 = new char[0x100]
        {
            '猫', '責', '熱', '若', '市', '歌', '袋', '虫', '裏', '義', '畑', '西',
            '囚', '弟', '妻', 's', '善', '禁', '占', '芸', '幽', '霊', '清', '個', 'l', 'p', 'Ⅲ', 'Ⅶ', 'Ⅷ', 'Ⅹ', '高', '工', '敷',
            '科', '恵', '痛', '毎', '注', '軽', '橋', '殖', '球', '陣', '打', '崩', '品', '退', '催', '永', '遺', '脱', '疑', '民', '尊',
            '敬', '純', '純', '治', '潜', '種', '類', '抑', '制', '幅', '領', '域', '療', '施', '具', '称', '収', '象', '跡', '察', '糧',
            
            //evt2-2
            '統', '観', '触', '揺', '標', '到', '拐', '超', '測', '弾', '富', '留', '易', '文', '献', '採', '漂', '語', '造', '稼', '改',
            '能', '比', '較', '区', '訪', '往', '混', '沌', '底', '原', '墓', '普', '絵', '額', '又', '暇', '縮', '呪', '掛', '慮', '＃',
            '癒', '階', '鐘', '描', '秋', '冬', '夏', '泊', '東', '展', '戸', '舷', '浮', '毒', '氷', '幻', '胃', '腸', '誕', '椅', '館',
            '廊', '堂', '斎', '玉', '械', '砲', '聖', '針', '春', '冬', '南', '山', '川', '谷', '渓', '崖', '岸', 't', '棺', '門', '月',
            '丘', '吊', '拷', '倉', '蔵', '浜', '江', '淵', '溝', '庭', '園', '峡', '潮', '段', '柱', '基', 'h', '汝', '楼', '拝', '草',
            '竹', '沼', '層', '虚', '噴', '交', '花', '輝', '護', '巧', '師', '首', '術', '飾', '神', '箱', '輪', '腕', '手', '防', '御',
            '撃', '攻', '小', '定', '認', '法', '技', '巨', '樹', '効', '系', '運', '半', '字', '鉱', '三', '型', '耐', '速', '設', '低',
            '共', 'g', '側', '阻', '両', '級', '殻', '圧', '亜', '劣', '胴', '脚', '独', '射', '偉', '勘', '頂', '戴', '忍', '素', 'b',
            '＆', '則', '四', '掲', '板', '登', '孫', '締', '憐', '鮮', '歪', '叫', '推',
        };

        public static readonly char[] _table7 = new char[0x100]
        {
            '旧', '臨', '奈', '摩', 'y', '細', '肉', '因',
            '財', '疲', '婆', '省', '寒', '沢', '商', '却', '派', '盛', '泥', '棄', '棒', '代', '晩', '枯', '期', '譲', '晴', '卑', '越',
            '縁', '権', '極', '磨', '替', '宣', '歓', '績', '繰', '栄', '淑', '幾', '杯', '紳', '候', '壁', '杭', '補', '懲', '酬', '勤',
            '怯', '余', '労', '匂', '罰', '糸', '辛', '棟', '課', '荷', '頑', '恥', '酷', '随', '醜', '詞', '償', '棒', '隣', '励', '倍',
            '充', '＃', '雰', '涙', '脅', '泣', '泥', '党', '洪', '_', '討', '伐', '皇', '帝', '殺', '編', '窮', '恨', '腰', '飯', '貨',
            '港', '軍', '号', '血', '掟', '浴', '範', '謹', '寄', '豪', '詰', '慢', '損', '史', '略', '抱', '遭', '慈', '際', '誉', '縄',
            '銘', '肝', '並', '殴', '利', '誇', '謁', '憩', '祖', '官', '都', '司', '駆', '柄', '挑', '偽', '吉', '堤', '建', '銅', '車',
            '贈', '唯', '資', '預', '委', '埋', '督', '謀', '穏', '筋', '適', '迫', '殊', '絆', '狼', '割', '砦', '徐', '煙', '肖', '慌',
            '訓', '芝', '桟', '灰', '障', '砕', '幼', '製', '鋭', '骨', '馬', '拾', '掘', '温', '微', '歯', '将', '兄', '駅', '忌', '坊',
            '慕', '執', '寝', '酒', '管', '華', 'Ⅵ', 'Ⅸ', '狂', '網', '雲', '征', '総', '衆', '冶', '雇', '髪', '燭', '鉄', '監', '団',
            '継', '肩', '電', '給', '評', '営', '依', '職', '憧', '麗', '症', '衣', '眼', '揮', '鋼', '瞳', '匠', '貫', '克', '歴', '養',
            '織', '礎', '讐', '演', '衛', '狩', '減', '波', '丁', '積', '環', '銃', '警', '龍', '峠', '脈', '宅', '創', '社', '論', '枢',
            '否', '列', '包', '奮', '七', '噂', '坂', '貼', '逮', '幕', '帯', '銀', '援', '悩', '凄', '昨', '妬',
        };

        public static readonly char[] _table8 = new char[0x100]
        {
            '捧', '怠', '裕', '十', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊', '＊',
            '＊', '＊', '＊', '＊',
        };
    }
}
