
namespace HostingTest {
    /// <summary>
    /// This is a simple lib with special string types to tests interfaces.
    /// At this point it is just a brain dump of all different string types
    /// that I can think of that might cause problems.
    ///
    /// This probably should be a FileStream base collection that uses byte value
    /// data thus allowing us to test every kind of string and encoding type.
    /// </summary>
    internal class StandardTestStrings { 

        internal static string[] AllStrings {
            get {
                return _data;
            }
        }

        //Make sure these are all InValid Source Code (i.e., Junk)
        private static string[] _data = {
                          // UTF8 BOM with escape syntax
                          "\xEF\xBB\xBF blah bblah",
                          // HTML entity small y, acute accent
                          "&#253;",
                          // é 
                          "U+0065_____----22.0000000000010000000000000001",
                          // é single char get converted int something... 
                          "é99999s0s0s0s))_____",
                          // Japanese
                          "‚±‚Ì–{‚Ì‚¨‚©‚°‚Ål¶‚ª‚©‚í‚Á‚½I ŠCŠO’“ÝŒoŒ±‚Ì‚È‚¢’´ƒhƒƒXƒeƒBƒbƒN‚ÈŽ„‚",
                          // Turkish
                          "BİRLEŞMİŞ MİLLETLER BİYOLOJİK ÇEŞİTLİLİK SÖZLEŞMESİ TARAFLAR KONFERANSI SIRASINDA DÜZENLENECEK BASINI BİLGİLENDİRME ÇALIŞTAYI",
                          // Hex?
                          "0xFE 0xFF",
                          // GUID Examples
                          "{3F2504E0-4F89-11D3-9A0C-0305E82C3301}",
                          // GUID Ex 
                          "3F2504E0-4F89-11D3-9A0C-0305E82C3301",
                          // XML Eg 1
                          "<stuff dt:dt=\"binary.base64\">84592gv8Z53815Zb82bA68g</stuff>",
                          // XML Eg 2
                          "<Blank><Tab><CR><LF>,;:./(){}[]<>+-~#*&%$§!=\'",
                          // XML Eg 3
                          "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=ISO-8859-1\">",
                          // XML Eg 4
                          "_<ID>MARK&#x20;&#x26;&#x20;SCAN</ID>",
                          "----000__l&apos;",
                          "Like ÂÃ to Ẫ? character with diacritic does encoded character. i.e, the character Ẫ can be encoded as x1EAA, as x0041x02C2x0303 (A + ^ + ~), as x00C3x0303 (Ã̃ Ã + ~) or as x00C2x02C2 (Â˂ + ^), where the ~ and ^ are not the characters on your keyboard, but the combining diacritical marks. Also, depending on how you look at it, Æ· (x00C6) equals (is compatible with) AE",
                          "[#x0300-#x0345] | [#x0360-#x0361] |[#x0483-#x0486] | [#x0591-#x05A1] | [#x05A3-#x05B9] | [#x05BB-#x05BD] |#x05BF | [#x05C1-#x05C2] | #x05C4 | [#x064B-#x0652] | #x0670 |[#x06D6-#x06DC] | [#x06DD-#x06DF] | [#x06E0-#x06E4] | [#x06E7-#x06E8] |[#x06EA-#x06ED] | [#x0901-#x0903] | #x093C | [#x093E-#x094C] | #x094D |[#x0951-#x0954] | [#x0962-#x0963] | [#x0981-#x0983] | #x09BC | #x09BE |#x09BF | [#x09C0-#x09C4] | [#x09C7-#x09C8] | [#x09CB-#x09CD] | #x09D7 |[#x09E2-#x09E3] | #x0A02 | #x0A3C | #x0A3E | #x0A3F | [#x0A40-#x0A42] |[#x0A47-#x0A48] | [#x0A4B-#x0A4D] | [#x0A70-#x0A71] | [#x0A81-#x0A83] |#x0ABC | [#x0ABE-#x0AC5] | [#x0AC7-#x0AC9] | [#x0ACB-#x0ACD] |[#x0B01-#x0B03] | #x0B3C | [#x0B3E-#x0B43] | [#x0B47-#x0B48] |[#x0B4B-#x0B4D] | [#x0B56-#x0B57] | [#x0B82-#x0B83] | [#x0BBE-#x0BC2] |[#x0BC6-#x0BC8] | [#x0BCA-#x0BCD] | #x0BD7 | [#x0C01-#x0C03] |[#x0C3E-#x0C44] | [#x0C46-#x0C48] | [#x0C4A-#x0C4D] | [#x0C55-#x0C56] |[#x0C82-#x0C83] | [#x0CBE-#x0CC4] | [#x0CC6-#x0CC8] | [#x0CCA-#x0CCD] |[#x0CD5-#x0CD6] | [#x0D02-#x0D03] | [#x0D3E-#x0D43] | [#x0D46-#x0D48] |[#x0D4A-#x0D4D] | #x0D57 | #x0E31 | [#x0E34-#x0E3A] | [#x0E47-#x0E4E] |#x0EB1 | [#x0EB4-#x0EB9] | [#x0EBB-#x0EBC] | [#x0EC8-#x0ECD] |[#x0F18-#x0F19] | #x0F35 | #x0F37 | #x0F39 | #x0F3E | #x0F3F |[#x0F71-#x0F84] | [#x0F86-#x0F8B] | [#x0F90-#x0F95] | #x0F97 |[#x0F99-#x0FAD] | [#x0FB1-#x0FB7] | #x0FB9 | [#x20D0-#x20DC] | #x20E1 |[#x302A-#x302F] | #x3099 | #x309A",
                          "we have ü, ö, ä, õ, ž and š.",
                          // Hing
                          "You can change the chinese words to unicode format.For example,  繁體中文字  that is  &#x7E41;&#x9AD4;&#x4E2D;&#x6587;&#x5B57;",
                          // Arabic
                          "<p xml:id=\"p3\" region=\"r1\"><![CDATA[<p align=\"right\"><font face=\"arial\" size=\"8\">Test-8 قئ خقئهوى تجيسض ؤءقشع وئ</font></p>]]></p>",
                          // Esperanto
                          "Dek tri mil ĉinoj ĉe lingva festivalo – La unua Lingva Festivalo en Ĉinio allogis pli ol 13.500 ĉeestantojn, kaj post la angla Esperanto estis la plej populara el la 70 lingvoj prezentitaj kaj instruitaj ĉe la festivalo.Legi pli » Sendita de joevinilo Publikigita antaŭ 4 tagoj 3 horoj – Anoncita antaŭ 1 tago 3 horoj Kategorio: kulturo   Etikedoj: ĉinio kulturo lingvo lingvoj",
                          // XML Special chars...
                          // ANSI
                          // MIME 
                          "Subject: =?utf-8?Q?=C2=A1Hola,_se=C3=B1or!?=",
                          "This is the body of the message. --frontier Content-type: application/octet-stream Content-transfer-encoding: base64 PGh0bWw+CiAgPGhlYWQ+CiAgPC9oZWFkPgogIDxib2R5PgogICAgPHA+VGhpcyBpcyB0aGUg Ym9keSBvZiB0aGUgbWVzc2FnZS48L3A+CiAgPC9ib2R5Pgo8L2h0bWw+Cg== --frontier--",
                          // MIME base64
                          "________ SVNBKjAwKiAgICAgICAgICAqMDAqICAgICAgICAgICowMSo5ODc2NTQzMjEgICAgICAqMTIq\n"
                          + "ODAwNTU1MTIzNCAgICAgKjkxMDYwNyowMTExKlUqMDAyMDAqMTEwMDAwNzc3KjAqVCo+CkdT\n"
                          + "KlBPKjk4NzY1NDMyMSo4MDA1NTUxMjM0KjkyMDUwMSoyMDMyKjc3MjEqWCowMDIwMDMKU1Qq\n"
                          + "ODUwKjAwMDAwMDAwMQpCRUcqMDAqTkUqTVMxMTEyKio5MjA1MDEqKkNPTlRSQUNUIwpSRUYq\n"
                          + "SVQqODEyODgyNzc2MwpOMSpTVCpNQVZFUklDSyBTWVNURU1TCk4zKjMzMTIgTkVXIEhBTVBT\n"
                          + "SElSRSBTVFJFRVQKTjQqU0FOIEpPU0UqQ0EqOTQ4MTEKUE8xKjEqMjUqRUEqKipWQypUUDhN\n"
                          + "TSpDQipUQVBFOE1NClBPMSoyKjMwKkVBKioqVkMqVFAxLzQqQ0IqVEFQRTEvNElOQ0gKUE8x\n"
                          + "KjMqMTI1KkVBKioqVkMqRFNLMzEvMipDQipESVNLMzUKQ1RUKjMKU0UqMTEqMDAwMDAwMDAx\n"
                          + "CkdFKjEqNzcyMQpJRUEqMSoxMTAwMDA3NzcK"

                         };

        
        /// <summary>
        /// In the future read from a file stream perhaps.
        /// </summary>
        public StandardTestStrings() {
            //Do something like populate the JunkStringCollection data.
        }

      

    }
}
