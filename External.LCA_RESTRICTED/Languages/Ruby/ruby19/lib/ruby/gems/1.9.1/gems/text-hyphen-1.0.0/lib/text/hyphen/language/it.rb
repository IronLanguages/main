# Hyphenation patterns for Text::Hyphen in Ruby: Italian
#   Converted from the TeX hyphenation/bahyph.tex file, by Claudio Beccari
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
#%%%%%%%%%%%%%%%%%%%%%%%%%%  file ithyph.tex  %%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#
# Prepared by Claudio Beccari   e-mail  claudio.beccari@polito.it
#
#                                       Dipartimento di Elettronica
#                                       Politecnico di Torino
#                                       Corso Duca degli Abruzzi, 24
#                                       10129 TORINO
#
# Copyright  1998, 2004 Claudio Beccari
#
# This program can be redistributed and/or modified under the terms
# of the LaTeX Project Public License Distributed from CTAN
# archives in directory macros/latex/base/lppl.txt; either
# version 1 of the License, or any later version.
#
# \versionnumber{4.8e}   \versiondate{2004/02/21}
#
# These hyphenation patterns for the Italian language are supposed to comply
# with the Recommendation UNI 6461 on hyphenation issued by the Italian
# Standards Institution (Ente Nazionale di Unificazione UNI).  No guarantee
# or declaration of fitness to any particular purpose is given and any
# liability is disclaimed.
#
# See comments at the end of the file after the \endinput line
#++
require 'text/hyphen/language'

Text::Hyphen::Language::IT = Text::Hyphen::Language.new do |lang|
  lang.patterns <<-PATTERNS
.a3p2n % After the Garzanti dictionary: a-pnea, a-pnoi-co,...
.anti1 .anti3m2n .bio1 .ca4p3s .circu2m1 .contro1 .di2s3cine .e2x1eu
.fran2k3 .free3 .narco1 .opto1 .orto3p2 .para1 .poli3p2 .pre1 .p2s .re1i2scr
.sha2re3 .tran2s3c .tran2s3d .tran2s3l .tran2s3n .tran2s3p .tran2s3r .tran2s3t
.su2b3lu .su2b3r .wa2g3n .wel2t1 '2 a1ia a1ie a1io a1iu a1uo a1ya 2at. e1iu
e2w o1ia o1ie o1io o1iu 1b 2bb 2bc 2bd 2bf 2bm 2bn 2bp 2bs 2bt 2bv b2l b2r
2b. 2b' 1c 2cb 2cc 2cd 2cf 2ck 2cm 2cn 2cq 2cs 2ct 2cz 2chh c2h 2chb ch2r
2chn c2l c2r 2c. 2c' .c2 1d 2db 2dd 2dg 2dl 2dm 2dn 2dp d2r 2ds 2dt 2dv 2dw
2d. 2d' .d2 1f 2fb 2fg 2ff 2fn f2l f2r 2fs 2ft 2f. 2f' 1g 2gb 2gd 2gf 2gg
g2h g2l 2gm g2n 2gp g2r 2gs 2gt 2gv 2gw 2gz 2gh2t 2g. 2g' 1h 2hb 2hd 2hh
hi3p2n h2l 2hm 2hn 2hr 2hv 2h. 2h' 1j 2j. 2j' 1k 2kg 2kf k2h 2kk k2l 2km k2r
2ks 2kt 2k. 2k' 1l 2lb 2lc 2ld 2l3f2 2lg l2h 2lk 2ll 2lm 2ln 2lp 2lq 2lr 2ls
2lt 2lv 2lw 2lz 2l. 2l'. 2l'' 1m 2mb 2mc 2mf 2ml 2mm 2mn 2mp 2mq 2mr 2ms 2mt
2mv 2mw 2m. 2m' 1n 2nb 2nc 2nd 2nf 2ng 2nk 2nl 2nm 2nn 2np 2nq 2nr 2ns
n2s3fer 2nt 2nv 2nz n2g3n 2nheit 2n. 2n' 1p 2pd p2h p2l 2pn 3p2ne 2pp p2r
2ps 3p2sic 2pt 2pz 2p. 2p' 1q 2qq 2q. 2q' 1r 2rb 2rc 2rd 2rf r2h 2rg 2rk 2rl
2rm 2rn 2rp 2rq 2rr 2rs 2rt r2t2s3 2rv 2rx 2rw 2rz 2r. 2r' 1s2 2shm 2s3s
s4s3m 2s3p2n 2stb 2stc 2std 2stf 2stg 2stm 2stn 2stp 2sts 2stt 2stv 2sz 4s.
4s'. 4s'' 1t 2tb 2tc 2td 2tf 2tg t2h t2l 2tm 2tn 2tp t2r t2s 3t2sch 2tt
t2t3s 2tv 2tw t2z 2tzk tz2s 2t. 2t'. 2t'' 1v 2vc v2l v2r 2vv 2v. 2v'. 2v''
1w w2h wa2r 2w1y 2w. 2w' 1x 2xb 2xc 2xf 2xh 2xm 2xp 2xt 2xw 2x. 2x' y1ou y1i
1z 2zb 2zd 2zl 2zn 2zp 2zt 2zs 2zv 2zz 2z. 2z'. 2z'' .z2
  PATTERNS
end
Text::Hyphen::Language::ITA = Text::Hyphen::Language::IT

  # Information
  #                           ON ITALIAN HYPHENATION
  #
  # I have been working on patterns for the Italian language since 1987; in
  # 1992 I published
  #
  #   C. Beccari, "Computer aided hyphenation for Italian and Modern Latin",
  #               TUG vol. 13, n. 1, pp. 23-33 (1992)
  #
  # which contained a set of patterns that allowed hyphenation for both
  # Italian and Latin; a slightly modified version of the patterns published
  # in the above paper is contained in LAHYPH.TEX available on the CTAN
  # archives.
  #
  # This minor revision has been tested with an enlarged set of difficult
  # Italian words so as to comply with a larger number of technical words
  # with foreign roots. The overall number of patterns is slightly reduced,
  # but its strength is increased. As with the previous release hyathi are
  # not hyphenated in order to cope with the habits of Italian readers.
  # Similarly single vowel internal syllables are avoided.
  #
  # As the previous versions, this new set of patterns does not contain any
  # accented character so that the hyphenation algorithm behaves properly in
  # both cases, that is with OT1 and T1 encodings. With the former encoding
  # fonts do not contain accented characters, while with the latter accented
  # characters are present and sequences such as à map directly to slot "E0
  # that contains "agrave".
  #
  # Of course if you use T1 encoded fonts you get the full power of the
  # hyphenation algorithm, while if you use OT1 encoded fonts you miss some
  # possible break points; this is not a big inconvenience in Italian
  # because:
  #
  # 1) The Regulation UNI 6015 on accents specifies that compulsory accents
  #    appear only on the ending vowel of oxitone words; this means that it
  #    is almost indifferent to have or to miss the T1 encoded fonts because
  #    the only difference consists in how TeX evaluates the end of the
  #    word; in practice if you have these special facilities you get
  #    "qua-li-tà", while if you miss them, you get "qua-lità" (assuming
  #    that \righthyphenmin > 1).
  #
  # 2) Optional accents are so rare in Italian, that if you absolutely want
  #    to use them in those rare instances, and you miss the T1 encoding
  #    facilities, you should also provide explicit discretionary hyphens as
  #    in "sé\-gui\-to".
  #
  # There is no explicit hyphenation exception list because these patterns
  # proved to hyphenate correctly a very large set of words suitably chosen
  # in order to test them in the most heavy circumstances; these patterns
  # were used in the preparation of a number of books and no errors were
  # discovered.
  #
  # Nevertheless if you frequently use technical terms that you want
  # hyphenated differently from what is normally done (for example if you
  # prefer etymological hyphenation of prefixed and/or suffixed words) you
  # should insert a specific hyphenation list in the preamble of your
  # document, for example:
  #
  # \hyphenation{su-per-in-dut-to-re su-per-in-dut-to-ri}
  #
  # If you use, as you should, the italan option of the babel package, then
  # you have available the active charater " that allows you to put
  # a discretionary break at a word boundary of a compound word while
  # maintaning the hyphenation algorithm on the rest of the word.
  #
  # Please, read the babel package documentation.
  #
  # Should you find any word that gets hyphenated in a wrong way, please,
  # AFTER CHECKING ON A RELIABLE MODERN DICTIONARY, report to the author,
  # preferably by e-mail.
  #
  # Happy multilingual typesetting!
