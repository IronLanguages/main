# Hyphenation patterns for Text::Hyphen in Ruby: Mongolian
#   Converted from the TeX hyphenation/bahyph.tex file, by Oliver Corff and
#   Dorjpalam Dorj (1999).
#
# WARNING: This is probably wrongly encoded into UTF-8.
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#        File: mnhyphen.tex
#      Author: Oliver Corff and Dorjpalam Dorj
#        Date: February 26th, 1999		% mls.sty prevails
#     Version: \VersionRelease			% see mls.sty!
#   Copyright: Ulaanbaatar, Beijing, Berlin
#
# Description: The Mongolian Hyphenation Pattern File
#	             to be used together with LMC encoding.
#	             Hyphenation exceptions should be stored
#              in mnhyphex.tex.
#
#              It may well be possible that the hyphenation
#              patterns given below are incomplete or plainly
#              wrong. It should also be mentioned that TeX
#              sometimes ignores correct hyphenation information
#              and makes up its own mind. Anyway, please con-
#              sider all hyphenation data strictly experimental
#              and *not yet stable*.
#
#              This file is mostly based on Cäwäl's Mongol
#              Xälniï Towq Taïlbar Tol' (MXTTT for short;
#              ``Short Explanatory Dictionary of Mongolian)
#              but contains a few other sources as well.
#
#              Comments, corrections and suggestions are
#              highly appreciated and should be directed to
#              the authors at corff@zedat.fu-berlin.de
#
#              U/B/B, February 1999
#
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# -------------------     identification     -------------------
#
#message{mnhyphen.tex - Hyphenation Patterns for
# 		Xalx Mongolian, LMC Encoding}
#
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#
# The following code is closely modelled after russian.sty and
# its accompanying hyphenation file.
# 
# We first must make some of the non-ASCII range characters known
# as characters to TeX, and include case mapping information.
#++
require 'text/hyphen/language'

Text::Hyphen::Language::MN = Text::Hyphen::Language.new do |lang|
  encoding "UTF-8"
  lang.patterns <<-PATTERNS
.aa2 .in2 .oë2 .oo2 .öö2 .uu2 .üü2 .ää2 aa2j a1d a2di
aï2b a1p asaa3 a1t a1f a1x a1c a3c2d a1q a1š a1ţ a2űal 1ba
baï2du ba3mi 2b1g 1bi 2bl 2b1r bu3j 1wa w1b 2w1g 2w1d 1we we2d w1j w1z
1wi 2w1l 2w1n 1wo 1wö 2w1r w1s 2w1t 1wu 1wü w1c 2w1š 1wä 1wű
1ga g1b g1w 2g1g 2g1d 2g1j 1gi 2g1l 2g1m 2g1n 1go go2di 1gö g1r g2ram
3gre 2gs 2g1t 1gu guuliu2 1gü g1x g1c 2g1q 2g1š g3ši 1gy 1gä
gä2nü gűl3 da1wy d1b 2d1w 2d1g 2d1d 2dek 2d1j dia1 2dit 2d1l 2d1m
2d1n 2d1r 2d1s 2d1t 3düü. 2d1x 2d1c 2d1q 2dş 2dť e1b e1w e1g2
e1d e1z e1i e1l e1m eo1 e1p e1re e3t2ru e1x e1c eci1 e1š ë1d ë1z
ëo2q ëx2 1ja 2j1w 2j1g 2j1d 2j1j 1ji 3jig 3jin 2j1l 2j1m 2j1n 1jö
2j1r 2j1s 2j1t 1ju 1jü 1jy 1jä 1za 2z1w 2z1g 2z1d 2z1j 1zi 2z1l 2z1m
2z1n 1zo 1zö 2z1r 2z1s 2z1t 1zu 1zü z1x z1c z1q 2z1š 1zy 2zť
1zä ig2ra i1d i3dal i2dy i1j i1z il2di is3p i1t i1x i1c i1š ï1b
ï1w ï1g ï2gr ï1d ï1j ï1p ï2pl ï1r ï1s ï1t
ï1x ï1c ï1q 1ka 1ke k1j 1ki k1k k1l k1n ko1o ks3p k1t 1ku 2k1c
1kä 1la 2l1b l1w 2l1g 2l1d 1le 2l1j 2l1z 1li 2l1l lli1 2l1m 2ln 1lo lo2d
1lö 2l1r 2l1s 2l1t 1lu 1lü 2l1x 2l1c 2lq 2lš 2lş 1ly 2lť
1lä l1ţ 1ma m1b m1g m1d 1me 1mi 2min m1k2 m1l m1n 1mo 1mö 2m1p m1r
1mu 1mü m1f m1x m1c m1š 1my 1mä 1na n1b n1w n1g n2gr ng2re n1d
1nëwrl	% The occasional Russian loan 1ni n1k n1l n1m 1no 1nö n1p n1s
n3s2d n1t 1nu 1nü n1x n1c 1ny 1nä n1ű o1a o1b o1g2r o1d o1e o1j
o1ne on3st on3t o1p2 o2pe o1sp o1t o1f o1x o1c o1ä ö1d ö1j ö2ri
ö1x ö1c ö1q 2p1d po1 po3s 2p1p 2pra p2ro 2p1t 1ra 2rab r1b r1w 2r1g
2r1d 1ri 2r1l r1m 2r1n 1ro 1rö r1p r1r 2r1s 2r1t 1ru 2ruk 1rü 2r1x r1c
2rq 2r1š 1ry 1rä 1sa s1b 2s1w 2s1g 2s1d 1se 2s1j s1z 1si 1sk2 2skw
2s1l 2s1m 2s1n 1so 1sö s1p s2pe s2pi 2s1r 2s1s 2s1t 1su 1sü s1f 2s1x
s1c 2s1q s3š2t 1sy 1sä 1ta 2t1b 2t1w 2t1g 2t1d 2t1j 2t1z 1ti 2t1l 2t1m
2t1n 1to 1tö 2t1r t2ro 1tru 2t1s 2t1t 1tü 2t1x 2t1c 2t1q 2t1š 1ty
1tä u1d u2ji u1z u1l u1t u1f u1x u1c u1š ü1d ü1z ü2zä
ü1l ü1p üs2d ü1x ü1c ü1š f1d f1m 1fo 1xa xaa2dy xa2ţ
2x1b 2x1w 2x1g 2x1d 2x1j 2x1z 1xi xi2da 2xiť 2x1l 2x1m 2x1n 1xo 1xö
2x1r 2x1s 2x1t 1xu xu3j 1xü 2x1x 2x1c 2xq 2x1š 1xy 2xť 1xä 1ca
2c1w 2c1g 2c1d 2c1j 2c1l 2c1m 2c1n 2c1r 2c1s 2c1t 2c1x 2c1q 2cş 1qa q1w
2q1g 2q1d 1qi 2q1l 2q1m 2q1n 1qo 2q1r 2q1s 2q1t 1qu 1qü 2q1x 1qä
1ša š1b 2š1w 2š1g 2š1d 2š1j 1ši 2š1k 2š1l 2š1m
2š1n 1šo 1šö 2š1r 2š1s 2š1t 1šu 1šü
šüü3lť 2š1x 2š1q 1šä ş1e2 ş1ë2 ş1ű2
y1g y1s y1x ť1b ť1d ť1k ť1t ť1x ť1c ť1q ť1š
ť1ű2 ä1d ä1j ä1z ä2näxi ä1x ä1c 2ţd 1űa
ű1d űnš2d ű1t ű1x ű1š
  PATTERNS
end
Text::Hyphen::Language::MON = Text::Hyphen::Language::MN
