# Hyphenation patterns for Text::Hyphen in Ruby: Portuguese
#   Converted from the TeX hyphenation/pthyphen.tex file, by Pedro J. de
#   Rezende, copyright 1987.
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
# Tabela TeX de separacao de silabas para portugues.
# The Portuguese TeX hyphenation table.
# (C) 1987 by Pedro J. de Rezende
# Release date: 02/13/87
#
#  Pedro de Rezende
#  College of Computer Science
#  Northeastern University
#  360 Huntington Ave
#  Boston MA 02115
#
#  (617) 437-2078
#  CSnet: derezende@northeastern.edu
#
# Permission is hereby granted to copy and distribute this material provided
# that the copies are not made or  distributed for  commercial or  lucrative
# purpose. 
#
# FURTHERMORE, THE CONTENTS OF THIS TABLE ARE NOT TO BE CHANGED IN ANY WAY!
#
# ----
#    WHY TEX DOESN'T DO PORTUGUESE HYPHENATION ANY FURTHER (YET):
# 1. TeX stops  hyphenating at the  syllable  preceding an accent,  a
#    cedilla or any other macros (see Appendix H of The TeXbook).
# 2. For aesthetic reasons any hyphenated word must have at least two
#    characters on the line containing the hyphen and  at least three
#    characters on the following line (ava-reza but not a-vare-za).
#++
$LOAD_PATH.unshift("../../..")
require 'text/hyphen/language'

Text::Hyphen::Language::PT = Text::Hyphen::Language.new do |lang|
  lang.patterns <<-PATTERNS
1ba 1be 1bi 1bo 1bu 1by 1b2l 1b2r o2b3long 1ca 1ce 1ci 1co 1cu 1cy 1c2k 1ch
1c2l 1c2r 1da 1de 1di 1do 1du 1dy 1d2l 1d2r e1e 1fa 1fe 1fi 1fo 1fu 1fy 1f2l
1f2r 1ga 1ge 1gi 1go 1gu 1gy 1g2l 1g2r ba1hia 1j 1ka 1ke 1ki 1ko 1ku 1ky
1k2l 1k2r 1la 1le 1li 1lo 1lu 1ly 1lh 1ma 1me 1mi 1mo 1mu 1my m2n m1h 1na
1ne 1ni 1no 1nu 1ny 1nh o1o 1pa 1pe 1pi 1po 1pu 1py 1ph 1p2l 1p2r 1p2neu
1p2sic 1qu 1ra 1re 1ri 1ro 1ru 1ry 1sa 1se 1si 1so 1su 1sy 1ta 1te 1ti 1to
1tu 1ty 1th 1t2l 1t2r 1va 1ve 1vi 1vo 1vu 1vy 1v2l 1v2r w2 1xa 1xe 1xi 1xo
1xu 1xy 1z
  PATTERNS

  lang.exceptions <<-EXCEPTIONS
hard-ware soft-ware
  EXCEPTIONS
end
Text::Hyphen::Language::POR = Text::Hyphen::Language::PT
