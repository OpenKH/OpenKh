using System.Collections.Generic;

namespace OpenKh.Kh2.Messages.Internals
{
    internal class JapaneseSystemDecode : IMessageDecode
    {
        public static readonly Dictionary<byte, BaseCmdModel> _table = new Dictionary<byte, BaseCmdModel>
        {
            [0x00] = new SimpleCmdModel(MessageCommand.End),
            [0x01] = new TextCmdModel(' '),
            [0x02] = new TextCmdModel('\n'),
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
            [0x17] = new DataCmdModel(MessageCommand.DelayAndFade, 2),
            [0x18] = new DataCmdModel(MessageCommand.Unknown18, 2),
            [0x19] = new TableCmdModel(MessageCommand.Table2, JapaneseSystemTable._table2),
            [0x1a] = new TableCmdModel(MessageCommand.Table3, JapaneseSystemTable._table3),
            [0x1b] = new TableCmdModel(MessageCommand.Table4, JapaneseSystemTable._table4),
            [0x1c] = new TableCmdModel(MessageCommand.Table5, JapaneseSystemTable._table5),
            [0x1d] = new TableCmdModel(MessageCommand.Table6, JapaneseSystemTable._table6),
            [0x1e] = new TableCmdModel(MessageCommand.Table7, JapaneseSystemTable._table7),
            [0x1f] = new TableCmdModel(MessageCommand.Table8, JapaneseSystemTable._table8),
            [0x20] = new TextCmdModel('⬛'),
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
            [0x69] = new TextCmdModel('⇾'), // Used only in EVT
            [0x6a] = new TextCmdModel('⇽'), // Used only in EVT
            [0x6b] = new TextCmdModel('♩'),
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
            [0x84] = new TextCmdModel("III"),
            [0x85] = new TextCmdModel("VII"),
            [0x86] = new TextCmdModel("VIII"),
            [0x87] = new TextCmdModel("X"),
            [0x88] = new TextCmdModel("(R)"),
            [0x89] = new TextCmdModel("o"),
            [0x8a] = new TextCmdModel("n"),
            [0x8b] = new TextCmdModel("r"),
            [0x8c] = new UnsupportedCmdModel(0x8c),
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
                    case 0x19:
                        if (parameter == 0xb2)
                            return AppendComplex(decoder, "XIII");
                        break;
                    case 0x1b:
                        switch (parameter)
                        {
                            case 0x54: return AppendComplex(decoder, "I");
                            case 0x55: return AppendComplex(decoder, "II");
                            case 0x56: return AppendComplex(decoder, "IV");
                            case 0x57: return AppendComplex(decoder, "V");
                            case 0x58: return AppendComplex(decoder, "VI");
                            case 0x59: return AppendComplex(decoder, "IX");
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

    internal static class JapaneseSystemTable
    {
        public static readonly char[] _table2 = new char[0x100]
        {
            'ム', 'メ', 'モ', 'ヤ', 'ユ', 'ヨ', 'ラ', 'リ', 'ル', 'レ', 'ロ', 'ワ', 'ヲ', 'ン', 'ガ', 'ギ', 'グ', 'ゲ', 'ゴ', 'ザ', 'ジ', 'ズ', 'ゼ', 'ゾ', 'ダ', 'ヂ', 'ヅ', 'デ',
            'ド', 'バ', 'ビ', 'ブ', 'ベ', 'ボ', 'ヴ', 'パ', 'ピ', 'プ', 'ペ', 'ポ', 'ァ', 'ィ', 'ゥ', 'ェ', 'ォ', 'ャ', 'ュ', 'ョ', 'ッ', '端', '子', '接', '続', '正', '常', '発',
            '生', '使', '用', '専', '付', '属', '取', '扱', '説', '明', '書', '指', '示', '従', '修', '復', '下', '空', '容', '量', '不', '足', '以', '上', '必', '要', '開', '始',
            '本', '魔', '石', '晶', '結', '大', '紋', '盾', '凍', '透', '燃', '水', '気', '炎', '士', '守', '証', '吹', '闘', '力', '木', '人', '飲', '過', '丸', '騎', '去', '魚',
            '剣', '荒', '氏', '杖', '束', '太', '鳥', '導', '布', '風', '満', '約', '源', '出', '実', '海', '思', '王', '自', '名', '喚', '召', '地', '家', '火', '森', '皆', '許',
            '恐', '古', '轟', '魂', '臭', '章', '雲', '雪', '然', '達', '嘆', '潰', '瓜', '伝', '怒', '悲', '怖', '鳴', '免', '雷', '林', '様', '巻', '片', '翼', '天', '?', '?',

            /*sys1-2 _*/
            '亡', '者', '囚', '封', '印', '迷', '東', '棟', '西', '冥', '_', '?', '?', '?', '事', '典', '匹', '貓', '姬', '規', '失', '敗', '神', '箱', '兵', '教', '跡', '率',
            '組', '造', '図', '情', '多', '報', '分', '由', '立', '具', '星', '質', '流', '替', '役', '優', '雄', '連', '判', '斷', '共', '有', '工', '改', '考', '強', '好', '消',
            '求', '捨', '收', '順', '助', '身', '轉', '同', '渡', '錄', '的', '直', '運', '英', '距', '驚', '惠', '系', '効', '鉱', '三', '止', '字', '狀', '心', '振', '絕', '線',
            '像', '打', '態', '彈',
        };

        public static readonly char[] _table3 = new char[0x100]
        {
            '珍', '投', '当', '內', '配', '白', '半', '費', '文', '枚', '在', '通', '特', '択', '動', '備', '武', '類', '覽', '技', '陸', '押', '換', '起', '经', '裝', '種', '殊',
            '作', '初', '主', '視', '員', '引', '泳', '活', '寄', '材', '全', '增', '速', '短', '倒', '落', '料', '形', '中', '表', '間', '敵', '來', '丘', '見', '橋', '黑', '原',
            '船', '長', '道', '秘', '闇', '数', '变', '化', '時', '高', '型', '段', '食', '登', '場', '賊', '入', '飛', '少', '最', '行', '回', '持', '成', '合', '手', '防', '御',
            '擊', '攻', '法', '耐', '久', '一', '度', '低', '超', '操', '縦', '性', '調', '整', '遠', '複', '機', '能', '追', '加','前', '方', '向', '部', '小', '定', '屋', '居',
            '隱', '奧', '遺', '暗', '安', '淵', '園', '胃', '江', '館', '犬', '客', '験', '穴', '会', '口', '窟', '広', '宮', '庫', '峽', '研', '究', '棺', '鬼', '計', '巻', '虛',
            '基', '果', '球', '界', '交', '休', '街', '議', '群', '岩', '月', '拷', '逆', '獄', '後', '所', '宿', '赤', '私', '室', '斎', '層', '草', '砂', '深', '倉', '集', '敷',
            '世', '息', '想', '女', '城', '蔵', '乗', '島', '竹', '滝', '通', '置', '底', '沈', '庭', '潮', '吊', '袋', '腸', '扉', '堂', '洞', '殿', '台', '沼', '難', '浜', '辺',
            '壁', '宝', '破', '腹', '氷', '拝', '噴', '崩', '番', '漠', '物', '墓', '密', '門', '默', '問', '遊', '谷', '離', '路', '裏', '綠', '廊', '楼', '牢', '礼', '巨', '樹',
            '意', '俺', '降', '壊', '今', '急', '光', '怪', '凶', '舵', '近', '確', '旗', '減', '画', '昇', '青', '左', '色', '捜', '先', '勝', '選', '受', '次', '丈', '点', '注',
            '他', '知', '体', '駄',
        };

        public static readonly char[] _table4 = new char[0x100]
        {
            '何', '吞', '返', '夫', '僕', '暴', '面', '目', '無', '待', '戾', '認', '電', '切', '完', '了', '保', '既', '可', '込',
            '決', '新', '差', '商', '設', '削', '除', '獣', '值', '抜', '仲', '品', '未', '読', '野', '右', '素', '個', '移', '位', '憶', '器', '更', '記', '期', '観', '級', '現',

            /*sys2-1*/
            '外', '送', '梱', '包', '謁', '礎', '格', '納', '盤', '信', '枢', '港', '号', '死', '山', '薬', '砦', '市', '店', '護', '住', '処', '廃', '墟', '試', '練', '埋', '営',
            '関', '村', '尾', '根', '頂', '場', '玉', '座', '_', '_', '_', '_', '_', '_', 'α', 'β', 'γ', '二', '四', '五', '六', '七', '几', '九', '零', '壱', '弐', '參',
            '百', '式', '號', '駅', '宅', '町', '車', '塔', '階', '絡', '狭', '象', '存', '渓', '建', '製', '施', '忘', '却', '桟', '停', '泊', '摩', '災', '床', '滅', '危', '波',
            '岸', '話', '再', '彼', '姿', '聞', '言', '戦', '隊', '着', '国', '進', '訪', '服', '救', '残', '帰', '元', '呪', '幻', '得', '逃', '解', '貸', '金', '囲', '探', '脱',
            '協', '男', '到', '帝', '都', '相', '頼', '盗', '影', '代', '宰', '緒', '皇', '血', '妃', '任', '放', '誰', '財', '務', '捕', '声', '終', '越', '閉', '聾', '平', '杯',
            '窓', '騒', '音', '償', 'ヶ', '準', '葉', '告', '父', '喜', '習', '々', '犯', '勢', '吉', '違', '真', '胸', '険', '催', '途', '日', '届', '友', '狙', '和', '突', '奪',
            '軍', '棒', '娘', '抱', '景', '異', '恋', '応', '映', '略', '支', '顔', '命', '別', '呼', '散', '叩', '感', '提', '揺', '荷', '挑', '駆', '談', '銅', '味', '慣', '謎',
            '笑', '博', '輝', '畑', '宣', '況', '泥', '早', '演', '悩', '川', '弱',
        };

        public static readonly char[] _table5 = new char[0x100]
        {
            '係', '資', '張', '希', '非', '如', '迎', '軽', '暮', '派', '旅', '責', '督', '退', '縮', '頭',
            '謝', '貸', '曲', '偉', '並', '借', '継', '絵', '伏', '治', '功', '与', '楽', '望', '殺', '親', '美', '勇', '染', '眠', '走', '契', '隙', '拠', '討', '拒', '族', '歌',
            '良', '夢', '預', '覚', '価', '予', '争', '惑', '儀', '贈', '為', '崖', '周', '板', '穏', '割', '術', '誕', '伐', '疲', '渉', '狼', '互', '煙', '夜', '因', '爆', '嵐',
            '漂', '固', '砲', '側', '看', '抑', '絆', '末', '紡', '誘', '歪', '貫', '柱', '黄', '昏', '旋', '律', '辿', '混', '沌', '創', '祭', '壇', '精', '妖', '買', '吸', '克',
            '歴', '歩', '北', '対', '司', '夏', '静', '帯', '師', '反', '総', '限', '売', '利', '秒', '写', '響', '委', '両', '角', '列', '菓', '補', '販', '各', '桜', '虹', '紫',
            '年', '禁', '灰', '超', '矢', '触', '重', '十',
            '充', '仮', '霊', '幽', '拾', '符', '冒', '第', '件', '背', '綱', '負', '花', '匠', '標', '頑', '射', '紙', '柄', '制',
            '峠', '困', '範', '陣', '君', '倍', '奏', '躍', '昨', '簡', '網', '願', '奇', '便', '浮', '郊', '単', '侵', '域', '叫', '詳', '索', '検', '渦', '幕', '官', '項', '念',

            /*sys2-2*/
            '還', '題', '没', '描', '令', '→', '←', '♪', '艦', '聖', '竜', '龍', '忍', '苦', '評', '仕', '賢', '距', '&', '採', '鉱', '坑', '積', '夕', '錬', '斬', '霸', '一',
            '編', '語', '条', '理', '拡', '央', '冠', '極', '烈', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
            't', 'u', 'v', 'w', 'x', 'y', 'z', '欲', '兜', '限', '臨', '威', '乱', '故', '郷', '悪'
        };

        public static readonly char[] _table6 = new char[0x100]
        {
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
        };

        public static readonly char[] _table7 = new char[0x40]
        {
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
            '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?',
        };

        public static readonly char[] _table8 = new char[0]
        {
        };
    }
}
