/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola@ologix.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

// line 1 "src/org/jvyamlb/resolver_scanner.rl"

using System.Diagnostics;
using System.Globalization;
namespace IronRuby.StandardLibrary.Yaml {

    public static class ResolverScanner {
        // line 52 "src/org/jvyamlb/resolver_scanner.rl"



        // line 13 "src/org/jvyamlb/ResolverScanner.java"
        private static void init__resolver_scanner_actions_0(byte[] r) {
            r[0] = 0; r[1] = 1; r[2] = 0; r[3] = 1; r[4] = 1; r[5] = 1; r[6] = 2; r[7] = 1;
            r[8] = 3; r[9] = 1; r[10] = 4; r[11] = 1; r[12] = 5; r[13] = 1; r[14] = 6; r[15] = 1;
            r[16] = 7;
        }

        private static byte[] create__resolver_scanner_actions() {
            byte[] r = new byte[17];
            init__resolver_scanner_actions_0(r);
            return r;
        }

        private static byte[] _resolver_scanner_actions = create__resolver_scanner_actions();


        private static void init__resolver_scanner_key_offsets_0(short[] r) {
            r[0] = 0; r[1] = 0; r[2] = 21; r[3] = 26; r[4] = 30; r[5] = 32; r[6] = 34; r[7] = 38;
            r[8] = 40; r[9] = 41; r[10] = 42; r[11] = 43; r[12] = 48; r[13] = 52; r[14] = 56; r[15] = 58;
            r[16] = 61; r[17] = 69; r[18] = 73; r[19] = 77; r[20] = 83; r[21] = 85; r[22] = 86; r[23] = 87;
            r[24] = 88; r[25] = 90; r[26] = 93; r[27] = 95; r[28] = 101; r[29] = 105; r[30] = 108; r[31] = 109;
            r[32] = 111; r[33] = 113; r[34] = 114; r[35] = 116; r[36] = 118; r[37] = 124; r[38] = 129; r[39] = 131;
            r[40] = 133; r[41] = 135; r[42] = 142; r[43] = 146; r[44] = 148; r[45] = 149; r[46] = 151; r[47] = 157;
            r[48] = 163; r[49] = 168; r[50] = 173; r[51] = 174; r[52] = 176; r[53] = 177; r[54] = 178; r[55] = 179;
            r[56] = 180; r[57] = 181; r[58] = 182; r[59] = 186; r[60] = 187; r[61] = 188; r[62] = 189; r[63] = 190;
            r[64] = 194; r[65] = 195; r[66] = 196; r[67] = 198; r[68] = 199; r[69] = 200; r[70] = 202; r[71] = 203;
            r[72] = 204; r[73] = 205; r[74] = 207; r[75] = 209; r[76] = 210; r[77] = 211; r[78] = 211; r[79] = 215;
            r[80] = 217; r[81] = 217; r[82] = 227; r[83] = 235; r[84] = 238; r[85] = 241; r[86] = 249; r[87] = 255;
            r[88] = 261; r[89] = 264; r[90] = 265; r[91] = 270; r[92] = 274; r[93] = 276; r[94] = 286; r[95] = 294;
            r[96] = 302; r[97] = 311; r[98] = 314; r[99] = 315; r[100] = 315; r[101] = 319; r[102] = 325; r[103] = 331;
            r[104] = 337; r[105] = 344; r[106] = 344; r[107] = 344;
        }

        private static short[] create__resolver_scanner_key_offsets() {
            short[] r = new short[108];
            init__resolver_scanner_key_offsets_0(r);
            return r;
        }

        private static short[] _resolver_scanner_key_offsets = create__resolver_scanner_key_offsets();


        private static void init__resolver_scanner_trans_keys_0(char[] r) {
            r[0] = (char)32; r[1] = (char)44; r[2] = (char)46; r[3] = (char)48; r[4] = (char)60; r[5] = (char)61; r[6] = (char)70; r[7] = (char)78;
            r[8] = (char)79; r[9] = (char)84; r[10] = (char)89; r[11] = (char)102; r[12] = (char)110; r[13] = (char)111; r[14] = (char)116; r[15] = (char)121;
            r[16] = (char)126; r[17] = (char)43; r[18] = (char)45; r[19] = (char)49; r[20] = (char)57; r[21] = (char)44; r[22] = (char)46; r[23] = (char)48;
            r[24] = (char)49; r[25] = (char)57; r[26] = (char)44; r[27] = (char)46; r[28] = (char)48; r[29] = (char)57; r[30] = (char)43; r[31] = (char)45;
            r[32] = (char)48; r[33] = (char)57; r[34] = (char)73; r[35] = (char)105; r[36] = (char)48; r[37] = (char)57; r[38] = (char)78; r[39] = (char)110;
            r[40] = (char)70; r[41] = (char)102; r[42] = (char)110; r[43] = (char)44; r[44] = (char)46; r[45] = (char)58; r[46] = (char)48; r[47] = (char)57;
            r[48] = (char)48; r[49] = (char)53; r[50] = (char)54; r[51] = (char)57; r[52] = (char)46; r[53] = (char)58; r[54] = (char)48; r[55] = (char)57;
            r[56] = (char)46; r[57] = (char)58; r[58] = (char)95; r[59] = (char)48; r[60] = (char)49; r[61] = (char)44; r[62] = (char)95; r[63] = (char)48;
            r[64] = (char)57; r[65] = (char)65; r[66] = (char)70; r[67] = (char)97; r[68] = (char)102; r[69] = (char)48; r[70] = (char)53; r[71] = (char)54;
            r[72] = (char)57; r[73] = (char)48; r[74] = (char)53; r[75] = (char)54; r[76] = (char)57; r[77] = (char)73; r[78] = (char)78; r[79] = (char)105;
            r[80] = (char)110; r[81] = (char)48; r[82] = (char)57; r[83] = (char)65; r[84] = (char)97; r[85] = (char)78; r[86] = (char)97; r[87] = (char)110;
            r[88] = (char)48; r[89] = (char)57; r[90] = (char)45; r[91] = (char)48; r[92] = (char)57; r[93] = (char)48; r[94] = (char)57; r[95] = (char)9;
            r[96] = (char)32; r[97] = (char)84; r[98] = (char)116; r[99] = (char)48; r[100] = (char)57; r[101] = (char)9; r[102] = (char)32; r[103] = (char)48;
            r[104] = (char)57; r[105] = (char)58; r[106] = (char)48; r[107] = (char)57; r[108] = (char)58; r[109] = (char)48; r[110] = (char)57; r[111] = (char)48;
            r[112] = (char)57; r[113] = (char)58; r[114] = (char)48; r[115] = (char)57; r[116] = (char)48; r[117] = (char)57; r[118] = (char)9; r[119] = (char)32;
            r[120] = (char)43; r[121] = (char)45; r[122] = (char)46; r[123] = (char)90; r[124] = (char)9; r[125] = (char)32; r[126] = (char)43; r[127] = (char)45;
            r[128] = (char)90; r[129] = (char)48; r[130] = (char)57; r[131] = (char)48; r[132] = (char)57; r[133] = (char)48; r[134] = (char)57; r[135] = (char)9;
            r[136] = (char)32; r[137] = (char)43; r[138] = (char)45; r[139] = (char)90; r[140] = (char)48; r[141] = (char)57; r[142] = (char)9; r[143] = (char)32;
            r[144] = (char)84; r[145] = (char)116; r[146] = (char)48; r[147] = (char)57; r[148] = (char)45; r[149] = (char)48; r[150] = (char)57; r[151] = (char)9;
            r[152] = (char)32; r[153] = (char)84; r[154] = (char)116; r[155] = (char)48; r[156] = (char)57; r[157] = (char)44; r[158] = (char)45; r[159] = (char)46;
            r[160] = (char)58; r[161] = (char)48; r[162] = (char)57; r[163] = (char)44; r[164] = (char)46; r[165] = (char)58; r[166] = (char)48; r[167] = (char)57;
            r[168] = (char)44; r[169] = (char)46; r[170] = (char)58; r[171] = (char)48; r[172] = (char)57; r[173] = (char)60; r[174] = (char)65; r[175] = (char)97;
            r[176] = (char)76; r[177] = (char)83; r[178] = (char)69; r[179] = (char)108; r[180] = (char)115; r[181] = (char)101; r[182] = (char)79; r[183] = (char)85;
            r[184] = (char)111; r[185] = (char)117; r[186] = (char)76; r[187] = (char)76; r[188] = (char)108; r[189] = (char)108; r[190] = (char)70; r[191] = (char)78;
            r[192] = (char)102; r[193] = (char)110; r[194] = (char)70; r[195] = (char)102; r[196] = (char)82; r[197] = (char)114; r[198] = (char)85; r[199] = (char)117;
            r[200] = (char)69; r[201] = (char)101; r[202] = (char)83; r[203] = (char)115; r[204] = (char)97; r[205] = (char)111; r[206] = (char)117; r[207] = (char)102;
            r[208] = (char)110; r[209] = (char)114; r[210] = (char)101; r[211] = (char)69; r[212] = (char)101; r[213] = (char)48; r[214] = (char)57; r[215] = (char)48;
            r[216] = (char)57; r[217] = (char)44; r[218] = (char)46; r[219] = (char)58; r[220] = (char)95; r[221] = (char)98; r[222] = (char)120; r[223] = (char)48;
            r[224] = (char)55; r[225] = (char)56; r[226] = (char)57; r[227] = (char)44; r[228] = (char)46; r[229] = (char)58; r[230] = (char)95; r[231] = (char)48;
            r[232] = (char)55; r[233] = (char)56; r[234] = (char)57; r[235] = (char)95; r[236] = (char)48; r[237] = (char)55; r[238] = (char)95; r[239] = (char)48;
            r[240] = (char)49; r[241] = (char)44; r[242] = (char)95; r[243] = (char)48; r[244] = (char)57; r[245] = (char)65; r[246] = (char)70; r[247] = (char)97;
            r[248] = (char)102; r[249] = (char)44; r[250] = (char)46; r[251] = (char)58; r[252] = (char)95; r[253] = (char)48; r[254] = (char)57; r[255] = (char)44;
            r[256] = (char)46; r[257] = (char)58; r[258] = (char)95; r[259] = (char)48; r[260] = (char)57; r[261] = (char)58; r[262] = (char)48; r[263] = (char)57;
            r[264] = (char)58; r[265] = (char)44; r[266] = (char)58; r[267] = (char)95; r[268] = (char)48; r[269] = (char)57; r[270] = (char)46; r[271] = (char)58;
            r[272] = (char)48; r[273] = (char)57; r[274] = (char)46; r[275] = (char)58; r[276] = (char)44; r[277] = (char)46; r[278] = (char)58; r[279] = (char)95;
            r[280] = (char)98; r[281] = (char)120; r[282] = (char)48; r[283] = (char)55; r[284] = (char)56; r[285] = (char)57; r[286] = (char)44; r[287] = (char)46;
            r[288] = (char)58; r[289] = (char)95; r[290] = (char)48; r[291] = (char)55; r[292] = (char)56; r[293] = (char)57; r[294] = (char)44; r[295] = (char)46;
            r[296] = (char)58; r[297] = (char)95; r[298] = (char)48; r[299] = (char)55; r[300] = (char)56; r[301] = (char)57; r[302] = (char)44; r[303] = (char)45;
            r[304] = (char)46; r[305] = (char)58; r[306] = (char)95; r[307] = (char)48; r[308] = (char)55; r[309] = (char)56; r[310] = (char)57; r[311] = (char)58;
            r[312] = (char)48; r[313] = (char)57; r[314] = (char)58; r[315] = (char)9; r[316] = (char)32; r[317] = (char)84; r[318] = (char)116; r[319] = (char)44;
            r[320] = (char)46; r[321] = (char)58; r[322] = (char)95; r[323] = (char)48; r[324] = (char)57; r[325] = (char)44; r[326] = (char)46; r[327] = (char)58;
            r[328] = (char)95; r[329] = (char)48; r[330] = (char)57; r[331] = (char)44; r[332] = (char)46; r[333] = (char)58; r[334] = (char)95; r[335] = (char)48;
            r[336] = (char)57; r[337] = (char)44; r[338] = (char)45; r[339] = (char)46; r[340] = (char)58; r[341] = (char)95; r[342] = (char)48; r[343] = (char)57;
            r[344] = (char)0;
        }

        private static char[] create__resolver_scanner_trans_keys() {
            char[] r = new char[345];
            init__resolver_scanner_trans_keys_0(r);
            return r;
        }

        private static char[] _resolver_scanner_trans_keys = create__resolver_scanner_trans_keys();


        private static void init__resolver_scanner_single_lengths_0(byte[] r) {
            r[0] = 0; r[1] = 17; r[2] = 3; r[3] = 2; r[4] = 2; r[5] = 0; r[6] = 2; r[7] = 2;
            r[8] = 1; r[9] = 1; r[10] = 1; r[11] = 3; r[12] = 0; r[13] = 2; r[14] = 2; r[15] = 1;
            r[16] = 2; r[17] = 0; r[18] = 0; r[19] = 4; r[20] = 2; r[21] = 1; r[22] = 1; r[23] = 1;
            r[24] = 0; r[25] = 1; r[26] = 0; r[27] = 4; r[28] = 2; r[29] = 1; r[30] = 1; r[31] = 0;
            r[32] = 0; r[33] = 1; r[34] = 0; r[35] = 0; r[36] = 6; r[37] = 5; r[38] = 0; r[39] = 0;
            r[40] = 0; r[41] = 5; r[42] = 4; r[43] = 0; r[44] = 1; r[45] = 0; r[46] = 4; r[47] = 4;
            r[48] = 3; r[49] = 3; r[50] = 1; r[51] = 2; r[52] = 1; r[53] = 1; r[54] = 1; r[55] = 1;
            r[56] = 1; r[57] = 1; r[58] = 4; r[59] = 1; r[60] = 1; r[61] = 1; r[62] = 1; r[63] = 4;
            r[64] = 1; r[65] = 1; r[66] = 2; r[67] = 1; r[68] = 1; r[69] = 2; r[70] = 1; r[71] = 1;
            r[72] = 1; r[73] = 2; r[74] = 2; r[75] = 1; r[76] = 1; r[77] = 0; r[78] = 2; r[79] = 0;
            r[80] = 0; r[81] = 6; r[82] = 4; r[83] = 1; r[84] = 1; r[85] = 2; r[86] = 4; r[87] = 4;
            r[88] = 1; r[89] = 1; r[90] = 3; r[91] = 2; r[92] = 2; r[93] = 6; r[94] = 4; r[95] = 4;
            r[96] = 5; r[97] = 1; r[98] = 1; r[99] = 0; r[100] = 4; r[101] = 4; r[102] = 4; r[103] = 4;
            r[104] = 5; r[105] = 0; r[106] = 0; r[107] = 0;
        }

        private static byte[] create__resolver_scanner_single_lengths() {
            byte[] r = new byte[108];
            init__resolver_scanner_single_lengths_0(r);
            return r;
        }

        private static byte[] _resolver_scanner_single_lengths = create__resolver_scanner_single_lengths();


        private static void init__resolver_scanner_range_lengths_0(byte[] r) {
            r[0] = 0; r[1] = 2; r[2] = 1; r[3] = 1; r[4] = 0; r[5] = 1; r[6] = 1; r[7] = 0;
            r[8] = 0; r[9] = 0; r[10] = 0; r[11] = 1; r[12] = 2; r[13] = 1; r[14] = 0; r[15] = 1;
            r[16] = 3; r[17] = 2; r[18] = 2; r[19] = 1; r[20] = 0; r[21] = 0; r[22] = 0; r[23] = 0;
            r[24] = 1; r[25] = 1; r[26] = 1; r[27] = 1; r[28] = 1; r[29] = 1; r[30] = 0; r[31] = 1;
            r[32] = 1; r[33] = 0; r[34] = 1; r[35] = 1; r[36] = 0; r[37] = 0; r[38] = 1; r[39] = 1;
            r[40] = 1; r[41] = 1; r[42] = 0; r[43] = 1; r[44] = 0; r[45] = 1; r[46] = 1; r[47] = 1;
            r[48] = 1; r[49] = 1; r[50] = 0; r[51] = 0; r[52] = 0; r[53] = 0; r[54] = 0; r[55] = 0;
            r[56] = 0; r[57] = 0; r[58] = 0; r[59] = 0; r[60] = 0; r[61] = 0; r[62] = 0; r[63] = 0;
            r[64] = 0; r[65] = 0; r[66] = 0; r[67] = 0; r[68] = 0; r[69] = 0; r[70] = 0; r[71] = 0;
            r[72] = 0; r[73] = 0; r[74] = 0; r[75] = 0; r[76] = 0; r[77] = 0; r[78] = 1; r[79] = 1;
            r[80] = 0; r[81] = 2; r[82] = 2; r[83] = 1; r[84] = 1; r[85] = 3; r[86] = 1; r[87] = 1;
            r[88] = 1; r[89] = 0; r[90] = 1; r[91] = 1; r[92] = 0; r[93] = 2; r[94] = 2; r[95] = 2;
            r[96] = 2; r[97] = 1; r[98] = 0; r[99] = 0; r[100] = 0; r[101] = 1; r[102] = 1; r[103] = 1;
            r[104] = 1; r[105] = 0; r[106] = 0; r[107] = 0;
        }

        private static byte[] create__resolver_scanner_range_lengths() {
            byte[] r = new byte[108];
            init__resolver_scanner_range_lengths_0(r);
            return r;
        }

        private static byte[] _resolver_scanner_range_lengths = create__resolver_scanner_range_lengths();


        private static void init__resolver_scanner_index_offsets_0(short[] r) {
            r[0] = 0; r[1] = 0; r[2] = 20; r[3] = 25; r[4] = 29; r[5] = 32; r[6] = 34; r[7] = 38;
            r[8] = 41; r[9] = 43; r[10] = 45; r[11] = 47; r[12] = 52; r[13] = 55; r[14] = 59; r[15] = 62;
            r[16] = 65; r[17] = 71; r[18] = 74; r[19] = 77; r[20] = 83; r[21] = 86; r[22] = 88; r[23] = 90;
            r[24] = 92; r[25] = 94; r[26] = 97; r[27] = 99; r[28] = 105; r[29] = 109; r[30] = 112; r[31] = 114;
            r[32] = 116; r[33] = 118; r[34] = 120; r[35] = 122; r[36] = 124; r[37] = 131; r[38] = 137; r[39] = 139;
            r[40] = 141; r[41] = 143; r[42] = 150; r[43] = 155; r[44] = 157; r[45] = 159; r[46] = 161; r[47] = 167;
            r[48] = 173; r[49] = 178; r[50] = 183; r[51] = 185; r[52] = 188; r[53] = 190; r[54] = 192; r[55] = 194;
            r[56] = 196; r[57] = 198; r[58] = 200; r[59] = 205; r[60] = 207; r[61] = 209; r[62] = 211; r[63] = 213;
            r[64] = 218; r[65] = 220; r[66] = 222; r[67] = 225; r[68] = 227; r[69] = 229; r[70] = 232; r[71] = 234;
            r[72] = 236; r[73] = 238; r[74] = 241; r[75] = 244; r[76] = 246; r[77] = 248; r[78] = 249; r[79] = 253;
            r[80] = 255; r[81] = 256; r[82] = 265; r[83] = 272; r[84] = 275; r[85] = 278; r[86] = 284; r[87] = 290;
            r[88] = 296; r[89] = 299; r[90] = 301; r[91] = 306; r[92] = 310; r[93] = 313; r[94] = 322; r[95] = 329;
            r[96] = 336; r[97] = 344; r[98] = 347; r[99] = 349; r[100] = 350; r[101] = 355; r[102] = 361; r[103] = 367;
            r[104] = 373; r[105] = 380; r[106] = 381; r[107] = 382;
        }

        private static short[] create__resolver_scanner_index_offsets() {
            short[] r = new short[108];
            init__resolver_scanner_index_offsets_0(r);
            return r;
        }

        private static short[] _resolver_scanner_index_offsets = create__resolver_scanner_index_offsets();


        private static void init__resolver_scanner_indicies_0(byte[] r) {
            r[0] = 0; r[1] = 3; r[2] = 4; r[3] = 5; r[4] = 7; r[5] = 8; r[6] = 9; r[7] = 10;
            r[8] = 11; r[9] = 12; r[10] = 13; r[11] = 14; r[12] = 15; r[13] = 16; r[14] = 17; r[15] = 18;
            r[16] = 0; r[17] = 2; r[18] = 6; r[19] = 1; r[20] = 3; r[21] = 19; r[22] = 20; r[23] = 21;
            r[24] = 1; r[25] = 3; r[26] = 22; r[27] = 3; r[28] = 1; r[29] = 23; r[30] = 23; r[31] = 1;
            r[32] = 24; r[33] = 1; r[34] = 25; r[35] = 26; r[36] = 22; r[37] = 1; r[38] = 27; r[39] = 28;
            r[40] = 1; r[41] = 29; r[42] = 1; r[43] = 29; r[44] = 1; r[45] = 28; r[46] = 1; r[47] = 3;
            r[48] = 22; r[49] = 31; r[50] = 30; r[51] = 1; r[52] = 32; r[53] = 33; r[54] = 1; r[55] = 24;
            r[56] = 31; r[57] = 33; r[58] = 1; r[59] = 24; r[60] = 31; r[61] = 1; r[62] = 34; r[63] = 34;
            r[64] = 1; r[65] = 35; r[66] = 35; r[67] = 35; r[68] = 35; r[69] = 35; r[70] = 1; r[71] = 36;
            r[72] = 37; r[73] = 1; r[74] = 38; r[75] = 39; r[76] = 1; r[77] = 25; r[78] = 40; r[79] = 26;
            r[80] = 41; r[81] = 22; r[82] = 1; r[83] = 42; r[84] = 42; r[85] = 1; r[86] = 29; r[87] = 1;
            r[88] = 43; r[89] = 1; r[90] = 29; r[91] = 1; r[92] = 44; r[93] = 1; r[94] = 45; r[95] = 46;
            r[96] = 1; r[97] = 47; r[98] = 1; r[99] = 48; r[100] = 48; r[101] = 50; r[102] = 50; r[103] = 49;
            r[104] = 1; r[105] = 48; r[106] = 48; r[107] = 51; r[108] = 1; r[109] = 53; r[110] = 52; r[111] = 1;
            r[112] = 53; r[113] = 1; r[114] = 54; r[115] = 1; r[116] = 55; r[117] = 1; r[118] = 56; r[119] = 1;
            r[120] = 57; r[121] = 1; r[122] = 58; r[123] = 1; r[124] = 59; r[125] = 59; r[126] = 60; r[127] = 60;
            r[128] = 61; r[129] = 62; r[130] = 1; r[131] = 59; r[132] = 59; r[133] = 60; r[134] = 60; r[135] = 62;
            r[136] = 1; r[137] = 63; r[138] = 1; r[139] = 64; r[140] = 1; r[141] = 62; r[142] = 1; r[143] = 59;
            r[144] = 59; r[145] = 60; r[146] = 60; r[147] = 62; r[148] = 61; r[149] = 1; r[150] = 48; r[151] = 48;
            r[152] = 50; r[153] = 50; r[154] = 1; r[155] = 51; r[156] = 1; r[157] = 65; r[158] = 1; r[159] = 66;
            r[160] = 1; r[161] = 48; r[162] = 48; r[163] = 50; r[164] = 50; r[165] = 67; r[166] = 1; r[167] = 3;
            r[168] = 68; r[169] = 22; r[170] = 31; r[171] = 30; r[172] = 1; r[173] = 3; r[174] = 22; r[175] = 31;
            r[176] = 69; r[177] = 1; r[178] = 3; r[179] = 22; r[180] = 31; r[181] = 70; r[182] = 1; r[183] = 71;
            r[184] = 1; r[185] = 72; r[186] = 73; r[187] = 1; r[188] = 74; r[189] = 1; r[190] = 75; r[191] = 1;
            r[192] = 76; r[193] = 1; r[194] = 77; r[195] = 1; r[196] = 78; r[197] = 1; r[198] = 76; r[199] = 1;
            r[200] = 76; r[201] = 79; r[202] = 76; r[203] = 80; r[204] = 1; r[205] = 81; r[206] = 1; r[207] = 0;
            r[208] = 1; r[209] = 82; r[210] = 1; r[211] = 0; r[212] = 1; r[213] = 83; r[214] = 76; r[215] = 84;
            r[216] = 76; r[217] = 1; r[218] = 76; r[219] = 1; r[220] = 76; r[221] = 1; r[222] = 85; r[223] = 86;
            r[224] = 1; r[225] = 75; r[226] = 1; r[227] = 78; r[228] = 1; r[229] = 87; r[230] = 88; r[231] = 1;
            r[232] = 76; r[233] = 1; r[234] = 76; r[235] = 1; r[236] = 73; r[237] = 1; r[238] = 76; r[239] = 80;
            r[240] = 1; r[241] = 84; r[242] = 76; r[243] = 1; r[244] = 86; r[245] = 1; r[246] = 88; r[247] = 1;
            r[248] = 1; r[249] = 89; r[250] = 89; r[251] = 22; r[252] = 1; r[253] = 24; r[254] = 1; r[255] = 1;
            r[256] = 3; r[257] = 22; r[258] = 31; r[259] = 91; r[260] = 92; r[261] = 93; r[262] = 90; r[263] = 30;
            r[264] = 1; r[265] = 3; r[266] = 22; r[267] = 31; r[268] = 91; r[269] = 90; r[270] = 30; r[271] = 1;
            r[272] = 91; r[273] = 91; r[274] = 1; r[275] = 34; r[276] = 34; r[277] = 1; r[278] = 35; r[279] = 35;
            r[280] = 35; r[281] = 35; r[282] = 35; r[283] = 1; r[284] = 94; r[285] = 22; r[286] = 95; r[287] = 96;
            r[288] = 21; r[289] = 1; r[290] = 94; r[291] = 22; r[292] = 97; r[293] = 96; r[294] = 94; r[295] = 1;
            r[296] = 97; r[297] = 37; r[298] = 1; r[299] = 97; r[300] = 1; r[301] = 96; r[302] = 97; r[303] = 96;
            r[304] = 96; r[305] = 1; r[306] = 24; r[307] = 95; r[308] = 39; r[309] = 1; r[310] = 24; r[311] = 95;
            r[312] = 1; r[313] = 3; r[314] = 22; r[315] = 31; r[316] = 91; r[317] = 92; r[318] = 93; r[319] = 98;
            r[320] = 99; r[321] = 1; r[322] = 3; r[323] = 22; r[324] = 31; r[325] = 91; r[326] = 100; r[327] = 70;
            r[328] = 1; r[329] = 3; r[330] = 22; r[331] = 31; r[332] = 91; r[333] = 101; r[334] = 69; r[335] = 1;
            r[336] = 3; r[337] = 68; r[338] = 22; r[339] = 31; r[340] = 91; r[341] = 90; r[342] = 30; r[343] = 1;
            r[344] = 103; r[345] = 102; r[346] = 1; r[347] = 103; r[348] = 1; r[349] = 1; r[350] = 48; r[351] = 48;
            r[352] = 50; r[353] = 50; r[354] = 1; r[355] = 94; r[356] = 22; r[357] = 95; r[358] = 96; r[359] = 104;
            r[360] = 1; r[361] = 94; r[362] = 22; r[363] = 95; r[364] = 96; r[365] = 105; r[366] = 1; r[367] = 94;
            r[368] = 22; r[369] = 95; r[370] = 96; r[371] = 106; r[372] = 1; r[373] = 94; r[374] = 68; r[375] = 22;
            r[376] = 95; r[377] = 96; r[378] = 21; r[379] = 1; r[380] = 1; r[381] = 1; r[382] = 1; r[383] = 0;
        }

        private static byte[] create__resolver_scanner_indicies() {
            byte[] r = new byte[384];
            init__resolver_scanner_indicies_0(r);
            return r;
        }

        private static byte[] _resolver_scanner_indicies = create__resolver_scanner_indicies();


        private static void init__resolver_scanner_trans_targs_wi_0(byte[] r) {
            r[0] = 77; r[1] = 0; r[2] = 2; r[3] = 3; r[4] = 19; r[5] = 93; r[6] = 101; r[7] = 50;
            r[8] = 106; r[9] = 51; r[10] = 58; r[11] = 63; r[12] = 66; r[13] = 69; r[14] = 72; r[15] = 73;
            r[16] = 74; r[17] = 75; r[18] = 76; r[19] = 6; r[20] = 81; r[21] = 86; r[22] = 78; r[23] = 5;
            r[24] = 79; r[25] = 7; r[26] = 10; r[27] = 8; r[28] = 9; r[29] = 80; r[30] = 11; r[31] = 12;
            r[32] = 13; r[33] = 14; r[34] = 84; r[35] = 85; r[36] = 88; r[37] = 89; r[38] = 91; r[39] = 92;
            r[40] = 20; r[41] = 22; r[42] = 21; r[43] = 23; r[44] = 25; r[45] = 26; r[46] = 44; r[47] = 27;
            r[48] = 28; r[49] = 42; r[50] = 43; r[51] = 29; r[52] = 30; r[53] = 31; r[54] = 32; r[55] = 33;
            r[56] = 34; r[57] = 35; r[58] = 36; r[59] = 37; r[60] = 38; r[61] = 41; r[62] = 99; r[63] = 97;
            r[64] = 40; r[65] = 45; r[66] = 46; r[67] = 100; r[68] = 24; r[69] = 47; r[70] = 48; r[71] = 105;
            r[72] = 52; r[73] = 55; r[74] = 53; r[75] = 54; r[76] = 107; r[77] = 56; r[78] = 57; r[79] = 59;
            r[80] = 61; r[81] = 60; r[82] = 62; r[83] = 64; r[84] = 65; r[85] = 67; r[86] = 68; r[87] = 70;
            r[88] = 71; r[89] = 4; r[90] = 82; r[91] = 83; r[92] = 15; r[93] = 16; r[94] = 87; r[95] = 18;
            r[96] = 90; r[97] = 17; r[98] = 94; r[99] = 49; r[100] = 95; r[101] = 96; r[102] = 98; r[103] = 39;
            r[104] = 102; r[105] = 103; r[106] = 104;
        }

        private static byte[] create__resolver_scanner_trans_targs_wi() {
            byte[] r = new byte[107];
            init__resolver_scanner_trans_targs_wi_0(r);
            return r;
        }

        private static byte[] _resolver_scanner_trans_targs_wi = create__resolver_scanner_trans_targs_wi();


        private static void init__resolver_scanner_eof_actions_0(byte[] r) {
            r[0] = 0; r[1] = 0; r[2] = 0; r[3] = 0; r[4] = 0; r[5] = 0; r[6] = 0; r[7] = 0;
            r[8] = 0; r[9] = 0; r[10] = 0; r[11] = 0; r[12] = 0; r[13] = 0; r[14] = 0; r[15] = 0;
            r[16] = 0; r[17] = 0; r[18] = 0; r[19] = 0; r[20] = 0; r[21] = 0; r[22] = 0; r[23] = 0;
            r[24] = 0; r[25] = 0; r[26] = 0; r[27] = 0; r[28] = 0; r[29] = 0; r[30] = 0; r[31] = 0;
            r[32] = 0; r[33] = 0; r[34] = 0; r[35] = 0; r[36] = 0; r[37] = 0; r[38] = 0; r[39] = 0;
            r[40] = 0; r[41] = 0; r[42] = 0; r[43] = 0; r[44] = 0; r[45] = 0; r[46] = 0; r[47] = 0;
            r[48] = 0; r[49] = 0; r[50] = 0; r[51] = 0; r[52] = 0; r[53] = 0; r[54] = 0; r[55] = 0;
            r[56] = 0; r[57] = 0; r[58] = 0; r[59] = 0; r[60] = 0; r[61] = 0; r[62] = 0; r[63] = 0;
            r[64] = 0; r[65] = 0; r[66] = 0; r[67] = 0; r[68] = 0; r[69] = 0; r[70] = 0; r[71] = 0;
            r[72] = 0; r[73] = 0; r[74] = 0; r[75] = 0; r[76] = 0; r[77] = 5; r[78] = 13; r[79] = 13;
            r[80] = 13; r[81] = 15; r[82] = 15; r[83] = 15; r[84] = 15; r[85] = 15; r[86] = 15; r[87] = 15;
            r[88] = 15; r[89] = 15; r[90] = 15; r[91] = 15; r[92] = 15; r[93] = 15; r[94] = 15; r[95] = 15;
            r[96] = 15; r[97] = 9; r[98] = 9; r[99] = 9; r[100] = 7; r[101] = 15; r[102] = 15; r[103] = 15;
            r[104] = 15; r[105] = 3; r[106] = 11; r[107] = 1;
        }

        private static byte[] create__resolver_scanner_eof_actions() {
            byte[] r = new byte[108];
            init__resolver_scanner_eof_actions_0(r);
            return r;
        }

        private static byte[] _resolver_scanner_eof_actions = create__resolver_scanner_eof_actions();


        static int resolver_scanner_start = 1;

        // line 55 "src/org/jvyamlb/resolver_scanner.rl"

        internal static string Recognize(string/*!*/ str) {
            // TODO: scanner should be char based
            byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
            string tag = null;
            int cs;
            int p = 0;
            int pe = data.Length;

            if (pe == 0) {
                data = new byte[] { (byte)'~' };
                pe = 1;
            }

            // line 372 "src/org/jvyamlb/ResolverScanner.java"
            {
                cs = resolver_scanner_start;
            }
            // line 74 "src/org/jvyamlb/resolver_scanner.rl"


            // line 379 "src/org/jvyamlb/ResolverScanner.java"
            {
                int _klen;
                int _trans;
                int _keys;

                if (p != pe) {
                    if (cs != 0) {
                        while (true) {
                            do {
                                do {
                                    _keys = _resolver_scanner_key_offsets[cs];
                                    _trans = _resolver_scanner_index_offsets[cs];
                                    _klen = _resolver_scanner_single_lengths[cs];
                                    if (_klen > 0) {
                                        int _lower = _keys;
                                        int _mid;
                                        int _upper = _keys + _klen - 1;
                                        while (true) {
                                            if (_upper < _lower)
                                                break;

                                            _mid = _lower + ((_upper - _lower) >> 1);
                                            if (data[p] < _resolver_scanner_trans_keys[_mid])
                                                _upper = _mid - 1;
                                            else if (data[p] > _resolver_scanner_trans_keys[_mid])
                                                _lower = _mid + 1;
                                            else {
                                                _trans += (_mid - _keys);
                                                goto _match;
                                            }
                                        }
                                        _keys += _klen;
                                        _trans += _klen;
                                    }

                                    _klen = _resolver_scanner_range_lengths[cs];
                                    if (_klen > 0) {
                                        int _lower = _keys;
                                        int _mid;
                                        int _upper = _keys + (_klen << 1) - 2;
                                        while (true) {
                                            if (_upper < _lower)
                                                break;

                                            _mid = _lower + (((_upper - _lower) >> 1) & ~1);
                                            if (data[p] < _resolver_scanner_trans_keys[_mid])
                                                _upper = _mid - 2;
                                            else if (data[p] > _resolver_scanner_trans_keys[_mid + 1])
                                                _lower = _mid + 2;
                                            else {
                                                _trans += ((_mid - _keys) >> 1);
                                                goto _match;
                                            }
                                        }
                                        _trans += _klen;
                                    }
                                } while (false);
                            _match:
                                _trans = _resolver_scanner_indicies[_trans];
                                cs = _resolver_scanner_trans_targs_wi[_trans];

                            } while (false);
                            if (cs == 0)
                                goto _resume;
                            if (++p == pe)
                                goto _resume;
                        }
                    _resume:
                        ;
                    }
                }
            }
            // line 76 "src/org/jvyamlb/resolver_scanner.rl"


            // line 452 "src/org/jvyamlb/ResolverScanner.java"
            int _acts = _resolver_scanner_eof_actions[cs];
            int _nacts = (int)_resolver_scanner_actions[_acts++];
            while (_nacts-- > 0) {
                switch (_resolver_scanner_actions[_acts++]) {
                    case 0:
                        // line 10 "src/org/jvyamlb/resolver_scanner.rl"
                        { 
                            tag = ToBool(str).Value ? Tags.True : Tags.False;
                        }
                        break;
                    case 1:
                        // line 11 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:merge"; }
                        break;
                    case 2:
                        // line 12 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:null"; }
                        break;
                    case 3:
                        // line 13 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:timestamp#ymd"; }
                        break;
                    case 4:
                        // line 14 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:timestamp"; }
                        break;
                    case 5:
                        // line 15 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:value"; }
                        break;
                    case 6:
                        // line 16 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:float"; }
                        break;
                    case 7:
                        // line 17 "src/org/jvyamlb/resolver_scanner.rl" 
                        { tag = "tag:yaml.org,2002:int"; }
                        break;
                    // line 489 "src/org/jvyamlb/ResolverScanner.java"
                }
            }

            // line 78 "src/org/jvyamlb/resolver_scanner.rl"
            return tag;
        }

        // TODO: thsi should be distinguished by the parser:
        internal static bool? ToBool(string value) {
            switch (value.ToUpper(CultureInfo.InvariantCulture)) {
                case "YES":
                case "TRUE":
                case "ON":
                    return true;

                case "NO":
                case "FALSE":
                case "OFF":
                    return false;

                default:
                    return null;
            }
        }
    }
}