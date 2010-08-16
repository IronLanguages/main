# Hyphenation patterns for Text::Hyphen in Ruby: Hungarian
#   Converted from the TeX hyphenation/huhyphn.tex file, by NAGY Bence
#   <huhyphn@tipogral.hu>.
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
# Huhyphn - hungarian hyphenation patterns v20031107
#
# Copyright (C) 2003, NAGY Bence <huhyphn@tipogral.hu>
# This file can be distributed under the terms of the
# GNU General Public License version 2.
# 
# Encoding: Cork/T1/EC
#++
if defined?(Text::Hyphen::Language::HU)
  raise LoadError, "Text::Hyphen::Language::HU has already been defined."
end

require 'text/hyphen/language'

Text::Hyphen::Language::HU = Text::Hyphen::Language.new do |lang|
  encoding "UTF-8"
  lang.patterns <<-PATTERNS
.a2 .adás1s .ae1 .agyon1 .al1eg .al1e2m .al1e2s .al1ér .al1is .al1os .arc1c
.ar2c3ho .atmo1 .az1a2 .az1ám .aze2 .á2 .ál1a2 .ál1e .ál1é2 .ál1i .ál1ok
.áre2 .ár1em .ár2nyo .áro2 .ár1os .ár1s2 .át1ad. .át1ada .át1á2 .át1e2
.át1é2 .át1i2 .áto2 .át1ol .át1or .át1os .át1ö2 .b2 .ba2l1e2g .ba1ts .bá1th
.be2csé .bei2 .be1k2 .bi2o .bo2rit .bu2szá .c2 .cle2 .cs2 .csőda2rab. .d2
.da2i .di1kr .e2 .eb1u2 .egy1a .egy1ü2l .el1a2d .el1a2k .el1an .ela2s
.el1ass .el1aszo .elá2 .el1ál .el1ás .el1áz .el1e2gyek .el1ejt .el1emel
.el1eng .el1e2p .el1e2r .el1e2se .el1e2sés .el1e2si .el1esn .el1ess .el1est
.el1esv .el1esz .el1ékt .el1éle .elé2n .el1éne .el1ér. .el1é2re .el1ért
.el1érv .el1id .el1in .el1ir .eli2s .el1iss .el1isz .el1itta .el1ít .elo2k
.el1oká .el1os .elö2 .el1öli .el1ölő .el1ölt. .elői2 .el1t2 .el1u2 .elü2
.el1ül. .el1üli .el1ült .el1ülv .es1er .et2h .ex1el .ez1e2l .é2 .égés1s
.ég1o .ék1a2 .ék1i .én1á .én1el .épe2 .ép1es .ér2csi .ér1el .ét1é .f2
.fa2jeg .fe2len .fe2lev .fe2lék .fe2lül. .fé2lér .fé2lév .fé2má .fö2lül. .g2
.ga2z1em .gé2ná .gi1g2 .h2 .ha2d1e2 .hale2 .ha2lev .ha2l1és .há2li .háncs1
.há2tus .he2i .horo1s .i2 .in2d3z .io2ná .í2 .íráskész1 .j2 .jazz1 .je2gy1á
.k2 .ka2ring .kés1s .ké2t1a2 .ké2tá .kiá2 .kié2 .ki1g2 .kr2 .l2 .la2pa
.le2g1 .le3g2esle2g1 .le3g2esle3g2esle2g1 .legé2nyem .le3gy .le2ná .le1t2r
.lé2ta .li2o1 .liszt1á .lókész1 .ló1s2 .lőe2 .ly1o .m2 .malo2 .má2r1is
.me2g1á .me2g1e2s .me2g1é2 .me2gis .me2gő .me2i .mé2szi .mu2e .n2 .na2gya
.na2gyá .na2p1e .na1t .nerc1 .ne2um .né2pí .ny2l .o2 .ok1a2d .ok1ir .ó2 .óé2
.óf2 .ö2 .ön1á .ön1k2 .ön3n .össz1á .öte2 .öt1el .özö2n1ö .ő2 .őr1an .ős1a2
.ős1e2r .ős1í2 .őz3s .p2 .pa2i .pais1 .pe2r1a2 .pé2k1 .ph2 .pier2 .po2re .q2
.r2 .ra2big .rát2 .re2ár .ren2d1ő2 .ru2ma .s2 .sa2li .sc2 .sk2 .so2ki .so1ny
.so2rál .sp2 .st2 .sto2 .su2r .sz2 .sza2kér .sze2szá .szé2t .szk2 .szódás1
.szt2 .t2 .ta2ni .ta2nos .ta2ur .tá2v1i2ratozá .te2j1á .té2nyi .té2rá .to2ké
.tö2k1élv .tölcséres1 .tűz3s .u2 .utas1s .ú2 .úr1a .úr1ist .út1a2 .út1e .ü2
.ük1 .ű2 .űr1i .v2 .va2d1al .va2gyi .vasas1s .va2sol .vé2g1o .w2 .x2 .y2 .z2
.ze2i .zs2 2a. 2a1a aa2d aa2j aa2k aa2l a2am aa2n aa2p aa2u aa2v aa2z 2a1á
aá2b aá2gak aá2gy a2ál. a2áln aá2p aá2r aá2s aá2t1 aáta2 aba2de 2aban 2abar
2abáj 2abáz 2abes 2abir 2abiz 2abom abos3s 2abő 2abú a2b1ü a2ce. ace2l
ac1ele acé2l1e a2c1ép ac1ére a2chá ac3hoz 2ací ac3ság a2c3sü ac3sz ada2le
ada2li ada2l1ú a2dap ad1apr ada2t1e2 ad1ág a2dású ad2del a2d1e2g ade2i
1a2dek ad1e2le ad1elő ad1elv ade2n ad1ene ade2s ad1esé adet2 ad1e2v adé2ke
a2d1ép a2d1é2v ad1ide adi2o1g2 ad1íz a2d1orzá 1a2dód ad1ö2l ad2rót adu2r
ad1ura 2adú 2a1e ae2b ae2c ae2dé aegész1 a2ei ae2k ae2l1a2 ae2le ae2l1o
ae2lő ae2me ae2p ae2re aero1s aerosz2 ae2rő ae2se ae2sé ae2si ae2ső ae2v
ae2z 2a1é aé2g aé2h aé2k aé2l aé2n aé2p aé2r aé2v 2afajn 2afe afe2li 2afés
2afi 2afő af2re 2afü a2g1a2j ag1any ag1arc agán1n agá2nyé a2gár. ag1át1a
ag1edé a2g1eg ag1elm ag1elt ag1elv a2gem ag1emb a2g1ev a2g1é2g a2g1é2l
ag1épí 1agg. ag3gye a1g2hó agi2g ag1igé a2g1i2p a2g1ír agli2 ago2na 2agor
a2g1ors ag1órá a2g1öt a1g2raf a2gut ag1uta ag1u2tá a2g1út a2g1ü2 a2gyad
agy1a2dó a2gyag agy1agy agy1ala agy1alk agy1any a2gyál agy1ári a2gyát agy1el
a2gy1em a2gy1en a2gyep agy1erő a2gy1es a2gy1ez a2gyér agy1ére a2gyét a2gyic
a2gyim a2gy1ip a2gyir agy1is. agy1ist a2gyoko a2gyokt agyo2r agy1oro 2ahar
2ahe 1ahh 2ahi 2ahí aho2l1i 2ahu a1i a2ia. ai2bo 2ai2de ai2dő ai2e 2aig2
ai2gazító ai2gé a2ign ai2j ai2k1as ai2k1e2 ai2ma. 2ai2má ai2nal ai2nas 2aing
2aint ai2o 2ai2p ai2ram ai2rá ai2rod ai2zo 2a1í2 a2j1al aja2n aj1ana 2aje
a2j1e2d aj1egé a2j1elm aj1e2lő a2j1em a2j1er a2jég a2j1ék a2j1ir 1ajkúa
a2j1ola aj1old aj1ül aj2ze aj2zí aj2zsí a2k1akn a2k1alk ak1ann 2akap aka2s1ü
2akav akás1s 2ake a2k1eg ak1els ak1emb a2k1e2rő aké2k a2k1éke 2akép ak1épí
2akérd a2k1é2ret ak1értel a2k1értő 2akés a1khó akiá2 2akin a2k1i2p ak1iro
ak1is. aki2s1a ak1isk ak1ism ak1izm 2akí ak2kór ak2lei ak2lés ak2lin
a2k1o2ly a2k1orv 2akö a2k1örv ak1őr. 2akré 2akri ak2rit a1k2ru ak1ug 2akut
2akú ak1útr 2akü ak1ülé a2k1ü2v 2akv al1abl alag1g ala2gol ala2j1e2 1alakí
1a2lakok 1a2lakú a2lany ala2nye ala2p1a2 ala2pár 1a2lapi 1a2lapí alap1p
a2láf al1ág. al1á2ga al1ágg al1á2gi al1ágk al1á2go 1aláí 2alán a2láp a2le.
al1eln al1elt al1eml al1eny al1e2rő al1esp a2l1est ale2sz al1ez a2léb
a2l1é2ne al1érz a2l1é2te 1alfö alg2 al1gr al1ido a2l1ikr alió2ra a2l1irá
al1isk al1isp al1itt al1í2rá 1aljb 1alji 1aljn 1aljz al2l1aj 1almád 1almák
almás1s 2alogizm aloi2do 2alok al1ola alo2mit alo2m1o2k 2alor alos1s
a2l1oszt 2alógiá 2alógu alókész1 a2l1ön a2l1ös al2t1el al1úr. al1úri al1útr
a2l1ü2 1alvó a2lyál a2ly1e am1a2dó 2amag am1akk am1app a2m1arc am1arr
ama2ter ama2tin 2amá a2m1árj am2b1eg am2b1ő a2me. ame2g1á a2m1elá a2m1e2l1o
am1els am1elt a2m1erd 1a2meri a2m1e2rő a2m1érd am1fr a2mim a2m1i2rá amise2
amis3s am1ír 2amod amos3s a2m1ó2r a2m1ö2 a2mő am1őr am1ős am1p2h 2amun 2amut
a2m1ü2 2amű a2nah an1akk an1alj an1any ana2sze a2n1az anás1s an2ce. ancs1es
an2cső an2dolv an1eb a2ner a2nes anet1o ane2u a2n1e2z 2ané ané2v an1évb
an1éve an1évn an2gad an2g1em an2gí an2g1or an2g1osz an2g1ös 1anim an2k1a2k
ank1ala an2kau an2k1el an2k1ék. ank1osz an2k1u2 an2kü ano1g2 an1old an1onn
an1ott an1órá 2anö an1ö2t ansz1ál an2sz1u2 an2tau a2nü any1ala 1a2nyád
a2nyám. a2ny1á2rak 2anye a2ny1ed a2ny1em any1eső anyé2 any1éh any1ék any1ér
a2nyif a2nyig anyigaz1 a2nyim 2anyo a2nyor a2ny1osz any1ó2r a2nyö a2nyő
anzo2 2a1o ao2la a2olo ao2ro 2a1ó a2ób a2óc a2óf aó2l aó2ra aó2rá aó2ri
2a1ö2 2a1ő2 apa2cs1i ap1a2da 1a2pai 1a2paké ap1a2la ap1alk apa2tér 1a2pád
1a2páé a2p1ág 1a2pái a2páll a2p1áru a2páta 2ape a2p1e2g ape2i ape2l ap1ele
ap1elg ap1elj ap1elő ap1elv a2p1em ape2n ap1ene ap1eny ape2s ap1ese ap1f2
a2p1ide api2g ap1ige a2p1i2rá a2p1ist a2p1ín 2apl a2p1ola ap1ope apos3s
ap1ös ap1öt ap2rém a1p2rés 1aprí ap1szt ap1t2 ap1udv ap1utá ar1ajt ara2nye
ar1ará a2r1ág. a2r1á2ga ará2gáb ará2gán ará2gár ará2gát ará2gáv a2r1ágb
a2r1ágg a2r1ági a2r1á2go a2r1ágr a2r1ágt ar1á2guk ar1á2gun ar1á2gy a2r1áll
ará2nyé ar2car ar2ced ar2ces ar2cö arc3sor 1arcú ar2dél 2are ar1e2dz are2l
ar1elh ar1elo a2r1e2m aren2d1ő a2r1er a2r1e2v ar1é2ne a2r1é2r aré2z1 ar1gh
argon1n a2r1i2d a2r1iga ar1ingb a2r1inge a2rizma. a2rizmán a2rizo ar1khe
ar2k1ő2 ar2k1u2s 2aro arog2r aro2mis a2r1or 2aró a2r1ó2rá a2r1ö2l a2r1ő2
ar1s2h ar1thá arto1g2rá art1old ar2t1ő2r ar1ty 2aru a2r1ü a2r1ű2 ar2v1ér
a1ry a2s1a2g 2asaj as1a2kar 2asal a2s1alk as1alm asa2n a2s1ant a2s1any
as1apr a2s1ál as1áru 2asáv 2ase as1e2d as1e2k as1ell as1emb as1e2s as1ez
as1éhe as1ék. a2s1é2l as1étv a2s1é2v asi2d as1ide a2s1i2p as1isk as1izo
2asík as1old 2asor as1ord as1osz as1órá as1ö2k as1ön as1ös 2asp as1sy
as3szab astil2 2astí as1urn a2s1u2t a2s1ú2t 2asü as1ün a2s1ür as1üs a2s1ü2v
2asű a2sz1ad asz1ág. a2szárad 2aszeg 2aszek asz1e2lő a2sz1év a2szid 2aszkó
asz2kóp asz1ors asz1ön asz2t1és asz2t1ív 2aszü 1aszz at1abl 2atak 2atan
2atar ata2rán a2t1arc ata2tom a2t1ág 2atál atá2nal 2atár a2t1á2ram a2t1átl
a2t1átv atá2v1i2 atá2v1í2 2ate a2t1ef at1e2gé at1e2gye ate2led at1e2lem
at1elh at1elm at1elv at1elz at1emb at1eml at1epo at1e2tet at1e2ve a2t1e2z
2até at1éhe a2t1é2le a2t1ép a2t1érz at1év. 2at1f2 at2hé at2hón atig2 a2t1iga
at1i2gé a2t1inf ati2nó at1iro a2t1isk 2atn a2t1ob ato1g2 ato2me at1opt 2ator
at1orsz a2t1orv atos3s atosz2 ato1szf 2ató a2t1ó2rá ató2riá ató1sz 2atö
a2t1ös at1ö2ve at1ö2vö at2sán attó2 at2tór at1tre at1tré 2atu a2t1uj 1atyj
a1tyl 2atz a1u au2de au2ga aug2h au2go au1k2r au1ly 2aur a2us. au2tal a2utó.
au2zs a1ú aú2r1 aú2s 2a1ü2 a1ű2 av1anh avara2n ava2s1as ava2sze 2avá 2avi
avi2cse avíz1 2ay a1ya a1yá a1ye 2aza aza2teg azaú2 az1áll az1ált azás1s
az2du 2aze az1e2le az1elj az1elő aze2o az1év az1i2d azo2n1á 2azó az1óta
az1p2 az3ság az3st a2zsú a2z3sü az1ut az1új 2á. á1a2 á1á2 áb1áll áb2b1a2d
áb2b1é2 áb2bis áb2bol áb2bos áb2b1ot áb2bö áb2bú ábe2l1a ábe2lér á2b1elő
á2b1e2m á2b1e2n á2b1ér á2b1ikr á2b1in á2b1ir 2ábiz áb1izm áb1izz 2ábí 1ábrá
2ábu ábu2t áb1utá áca1k2 ác1al ácá2ná á2c1e ác3ho ácin2ti áci2ósz á2cs1aj
á2csas ác3ság á2cs1e2l á2csil ácsü2 ács1ül ád1alk áda2n ád1any á2d1apá
á2d1ál á2d1e2l á2d1e2m áde2ros á2d1es á2d1in ád1ira ádo2ge á2d1osz ádö2
á1d2rót ádsza2 á2d1ur á1dy á1e2 á1é áé2he áé2ke áé2l áé2ne áé2p áé2re áé2ri
áé2rő. áf1elm áf1üz á2g1a2d ág1ala á2g1a2r 1á2gaz á2g1ág á2g1álv á2g1áru
á2g1árv ágá2sé ág1e2d á2g1e2g á2g1e2l á2g1e2s á2g1é2g á2g1é2l ág1é2ne á2g1ép
ág1érd ág1érte ág1érté ágg2 ág1gr á2g1id á2g1if á2g1iga á2gigén á2g1ill
á2g1ing á2g1i2p ág1iro ág1ír á2g1ola á2g1old á2g1olv ágos3s ág1óc á2g1ö2
ágsz2 á2g1u2r á2g1u2t á2g1ü2 á2g1ű2 á2gyac á2gyál á2gy1e2 2áha 2áhá 2áhe
áh1ors á1i ái2do ái2g ái2j 2áil ái2má ái2rá ái2s á1í2 á2jí áj2kő áj2lá
á2j1or á2jő áj2teg áj2tel áj2ti á2jul á2jü ák1abr á2k1al á2k1as á2k1ál
á2k1e2l ák1em á2k1é2l ák1é2ve ák1in ák1k2 á2k1oli á2k1oll á2k1ott 2ákö ák1s
á2k1ut ál1abl ála2m ála2n á2l1ana á2l1ang ál1any ála2szel ála2szü ál1áll
ál1árf 1áldá 1áldj 1áldo ál1d2r 1áldu 1áldv ále2l ál1emu á2l1e2r ál1ér.
álé2t ál1éte á2l1id ális3s á2l1í2 ál1k2 ál2liz ál1ob á2l1o2kok á2l1ol
1á2lomb álo2me álo2mit á2l1op á2l1or ál1osz 2álö ál1öl ál1ös ál1öz ál1p2
áls2 ál1st ál2t1e2g ál2tiv á2l1ug ál1u2s ál1u2t ál1út á2lü ál1ű á2lyal
á2lyál á2ly1e á2lyiga á2lyis á2ly1ó2 á2lyö á2lyü á2m1ac á2m1akt á2m1a2l
áma2n ám1any ám1apa ám1a2rá ám1aty ám1ár. ám1bl áme2g ám1elm á2m1e2m á2m1es
ámész1 ám1f2 á2m1id ám1il á2m1ip á2m1i2r ámi2s ám1ism ám1isz á2m1ír ám1ív
á2m1ob á2m1op ámo2rál ám1oszl á2m1ö á2m1ő2 ám1p2 ám1s ámu2n ám1ur á2m1ü2
ám1ű2z ána2d án1ada ánai2 án1ajt á2n1akc á2n1a2la án1alk á2n1a2pa á2n1apá
án1ass án1aut á2n1áll án1áru án2c1ed án2c1es án2c3h án2c1is án2cor án2c1ö
án2cü á2n1e2d án1e2g ánegyez1 án1e2l án1em án1en án1e2t á2n1é2g á2n1é2l
án1é2ne á2n1épü án1érő án1f2 án2g1a2r án2gel án2g1e2s án2g1és án2g1osz án2gö
á2n1i2d á2n1im án1int á2n1ip á2n1ir án1isz ánizs1 á2n1izz án2k1es án1k2l
á2n1oki á2n1or á2n1osz á2n1ot ánó2r á2n1ö án1szf án1sz2l án1szp án2tir
án2t1iz án2tór á2n1uj á2n1u2t ánú2 án1út á2n1ü ány1a2dó ány1a2la ánya2n
ány1any á2nyap ány1ass á2ny1ál á2ny1ár. á2ny1á2rak ánye2 á2nyeg á2ny1el
ány1er á2nyég á2nyél á2nyérz ány1í2r á2ny1ola á2ny1osz á2ny1ö á2nyő ány1us
á2nyü án2z1ag án2zs á1o áo2sze á1ó á1ö á1ő2 áp1a2n ápe2 áp1eg á2p1ér áp1int
á2poló ápora2 ápo2rad ár1abl ár1abr 2árag á2r1agg ára2j ár1ajá ár1ajt
á2r1a2la ár1alá ár1alj ár1alk ára2m1el 1á2ramk 1á2ramú ár1a2p á2r1a2r
á2r1á2g ár1ár. á2r1áro ár1árt árás3s ár1á2t1e2 á2r1átk ár1b2 1árboc árd1el
ár2d1ó2 á2r1e2g á2r1e2l ár1eng á2r1er ár1esé á2r1e2z á2r1é2g á2r1éj á2r1é2n
á2r1ép á2r1é2tű ár2gye ár2gyor ár1ide ári2g á2r1iga ár1inc ár1ind ár1ing
ár1int á2r1i2o á2r1ir ár1isp ár1i2ta ár1izo ár1ír árká2s árkász1 ár1kh
1árkok ármas1s árnás1 áro2koz ár1oll áro2mér áro2m1os á2r1or áros3s á2rostr
árö2 ár1öl ár1ös ár1öv ár1öz á2r1ő2 ár1p2 1árpa. árs3s ár1str árt1akt
árt1áll árt1árn árt1áru ár2teg ár2tig ár2t1okt ár2t1ön ár1tro 1á2ru. á2rud
á2rué 1á2ruf 1á2rui 1áruj á2ruke 1á2ruké á2rum 1á2rup á2rur á2rut ár1uta
1áruü 1áruv ár1úr árú2t ár1úti á2rü 1árvu á1ry á2s1abl á2sad ás1a2dó á2s1a2j
á2s1akt ás1a2la ás1alk ás1alm ás1a2lo ása2n á2s1ana á2s1any á2s1as á2s1a2u
á2s1a2v á2s1a2z á2sábr á2s1ál ás1áru á2sást ás1átl ás1átv ás1e2d á2s1e2l
ás1em á2s1en á2s1e2s ás1ex ás1ez á2s1é2l á2s1ép ás1ére á2s1érte á2s1érté
á2s1és á2s1éve á2s1id á2s1if ási2g ás1iga ás1inf ás1int á2s1i2p ás1i2rá
á2s1is á2s1ín á2s1í2r ás1ív ás1k ás1okta á2s1or ás1osz 1á2só. 1á2sój 1á2sók
1á2són á2sór ás1órá 1á2sót á2s1ö á2s1ő ás1p2 áspis1 áss2 ás1st á1ssy ás3sze
ás3szé ás3szi ás3szor ás3szö ás3szt ás3szü ás1tra á2s1uj ásu2t ás1uta ás1utá
ás1u2z á2s1üg ás1ür ás1üs á2s1ü2v á2sz1aj á2sz1ap á2szas ás3zav 2ászá á2szág
ász1eb ász1el ász1em ász1ep ász1es ász1e2t ászé2 á2sz1éj á2sz1én ász1év
ászi2 á2szid á2sz1in á2sz1ir á2sz1is ász1ors ász1osz ász1ös ászt2 ász1tr
á2szur á2szut ász1út ász1ün á2szüz át1ado át1adt át1alm át1a2lu át1a2u
á2t1ál át1ej át1e2m át1é2g áté2ke á2t1é2l át1é2p át1f2 1átfú 1áthi áti2ag
át1id áti2g át1in á2t1i2r á2t1izm á2t1izo át1í2r át1í2v 1átlé 1átne át1öb
á2t1öm át1ön át1ö2v át1ö2z á2t1ő2 át1t2 át1ug á2t1uj 1á2t1u2t á2t1ú2s átü2
á2t1ül át1üt át1üz 1átvo á1u2 á1ú á1ü2 á1ű áv1el áv1érz á2v1i2rá á2v1iz
á2v1okt á2v1ús áz1abl á2z1aj áz1alk á2z1ann áza2t1e2 áza2tí áz1aty á2z1a2v
á2z1á2l ázás1s áz1ea á2z1eg áz1e2r á2z1es áze2z á2z1eze á2z1ezré á2z1é2p
áz1éve ázhoz1 á2z1igá á2z1ing á2zins ázi2s1em ázis3s áz1izm áz1izo áz1k2
2ázol á2z1o2r 2ázos á2z1osz á2z1ö á2z1ő ázs1aj ázs1ár. ázs1árá ázs1árh
á2zs1e2 á2zsé ázs1é2n á2zsiá á2zs1ig áz3sor áz3spa áz3sug á2zsü ázs1ü2v
á2z3sű 2ázú á2z1ü2 á1zy 1ba baá2 baba1 ba2b1o2l ba2d1ár ba2del ba2der ba2des
ba2d1i2 ba2dot ba2dun badú2 ba2d1ús bae2 bago2 2bajg ba2k1as ba2k1ál ba2k1in
ba1krá ba2l1a2d ba2l1e2s ba2l1í2 ba2l1ol 2bals ban2ch banka2 ban2kad ban2kal
ban2kép ban2kin bart2 bar1th ba2seb ba1sp ba2ue bau2t 1bá bá2bál bá2csü
b1á2gú bá2gyu bákos1 bá2lan 2bálm bá2ne 2b1á2p bá2r1as 2b1á2ru bá2s1é bá2szö
bá2t1a2 2b1átd 2b1átm bb1adh bb1adj bb1adu bb1adv b2b1alk bb1als bb1ar
b2b1ál b2b1e2g bb1ela b2b1e2le bbe2m b2b1eme b2b1esé bbes1s b2b1é2v bb1is.
b2b1í2r bb1old bb1olt bb1olv bb1osz bbó2 bb1ór bbö2 b2b1öl bb1ör bb1ös bb1s2
b2b1u2t 1be bea2 beat1 beá2r beá2z be2cs1á be1f be1gr bei2d bei2g bei2s
be1k2v be2lad be2lár bel1ér. be2lór be2lőa be2n1á ben2ná ben2tét beo2 be1p2
bera2n be2rap be2r1ar be2ras be2r1ár be2r1e2g be2r1e2lő be2r1eml ber1e2pé
be2r1ev be2rid be2r1ist 2b1e2rő ber1s2 be1ska be1s2l besp2 be1spr be1sr
be1sto beu2 1bé bé2d1o bé2du bé2let bé2lo b1é2lő bé2lú bé2lyi 2b1é2p bé2ran
bé2rá bé2r1em bé2r1os 2b1érte 2b1érté bé2ru bé2rú bé2vi 1bi bi2ed bi1f2r
2b1i2gáb 2b1i2gáj bi2k1em bi2kél bi1k2ro bil1ant bi2l1ip 2bime 2b1ind bio1g2
bi2ok bi2ol bi2om bi2or bi2os 2b1i2rá bi1sc bi1sh bi2t1á2r bi2tip bi2tü
2b1i2vad 1bí 2b1ív b1k2 bl2 bla1bla bla2k1e2 bla1pr ble2t1ér ble2ti b1lj
1b2lok blu2 blues1 b2lú b1ly bmeg1á 1bo boc1c bo2ce bo2c3h bo2g1e bok2sz1á
2b1olda bolo1g2 bo2r1ad bo2ral bo2r1e2c bo2rén bo2r1is bor1itt bo2r1iv bo2se
bos3zs bo2t1e bo2ul bo2zé 1bó bó2rá 1bö bö2n1 2b1öv 1bő bő2r1a bő2rá bő2reg
bő2rel bő2rin bő2r1o bő2ru bőr1ü2l b1p2 br2 1b2rig bsé2gel bsz1t2 b1t2 1bu
2b1ujj buk2je bu2n1á bu2sz1ál bu2tó 1bú bú2sz búzás1 1bü 2b1üz 1bű bű2v1e
bvá2nyé bve2g bvegy1 1by by2t 1ca 2c1a2la 2c1alk cam1b ca2ny ca2rán catá2
ca2tem 2c1atl caus2 1cá 2c1á2g cá2l 2c1áll cá2po 2c1árk 2c1á2sá c2c1e2v c2ch
c2c1í2 c2cs c3csap c3csi c3csí cc3sor ccs1ön c3csú cda2lé 1ce ce2dén 2c1e2dz
ceg1ér 2c1e2gy ce2lem ce2lők ceo2 ce1ph 2c1e2rő 2c1eszt 2cetb 2ceton
2c1e2zer 2c1ezr 1cé cé2ga cé2gel cé2g1o cé2lab cé2l1ar cé2l1á2 céle2 cé2leg
cé2lek cé2lem cé2l1er cé2lip cé2lir cé2liz cé2lö cé2lő cé2pí cé2ret 2c1é2ri
cés3s c1év. 2c1é2ven 2c1é2ves 2c1é2vi c2h 1cha cha2e 1chá c3ház 1che 2che.
chel2 chet2t 1ché 1chi 1chí 1cho 1chó 1chö 1chő ch2ro 2chu 2chú 1chü 1chű
1ci ci2ac ci2af ci2a1g2 ci2aku ci2am ci2ap ci2ar ci2av ci2az 2c1i2dő ci1g2r
2cii 2cing ci2o1g2 ci2ol ci2óa ci2óc ci2ófe ci2ófé ciókész1 ci2ól ci2órá
ci2óta 2cispán. ci2szi cito1 2c1izma 2c1izmo ci2zom 1cí cí2ma cí2mi 2c1í2rá
2cív cí2ve cí2zs cké2t1 c2k1í ck1o2pe ck2ré ck2ri 1c2ky 1co co2lig com2biz
co2o 2cori 2cország. 2c1osz co2u 1có 1cö c1ö2le c1öln cö2t c1ötb c1öt1e
c1öté c1ötö 1cő c1p2 c2s 1csa cs1abl 2cs1a2d cs1ass csa2sz 3csat 1csá 2cság
c3ság. cs1ágú 2cs1ásá 2csásó 2csátí 1cse 2cselc 2csevő 1csé 2c3ség 1csi
2cs1ita 1csí cs1í2v cs1k 1cso 2csolaj 3cson 3csop 2cs1orr c3sorv 1csó 1csö
2csönte 1cső cs1p2 cs3s cs1t2 c3s2tí 1csu 1csú 1csü cs1üll cs1ü2té c3süv
2csüz 1csű c3sza c3száz. c3szem c3szes c3szí c3szo c3szö c3sző cs3zs ct2
c1tr 1cu 2c1utc 1cú 1cü 2c1üg c1ünn 2c1ü2r 2c1ü2v 1cű 1cy c2z 1cza 1czá 1cze
2c3zen cze2ö 1czé 1czi 1czí 1czo 1czó 1czö 1cző c3zs 1czu 1czú 1czü 1czű
1czy 1da daá2 2d1abl 2d1add 2d1a2dó dae2 da1g2ro da2hán dai2g da2lab da2l1aj
da2lat da2lág da2l1e2l da2l1im dal1l da2l1or da2l1ó2r da2ma. da2maz da2mel
2d1ann da2nyá daó2 2d1a2pa. 2dapai 2dapak da2pák da2pát da1p2f da1p2l da2pó
dar2c3h dar2cso da2rel da2r1é da1spe dast2 da1str 2d1aszt da2t1ag dat1ala
dat1alk dat1áll 2datc da2t1eg da2t1em da2téh da2t1és da2t1in da2top da2tút
2d1a2ty da2z1á da2zé 1dá dáb2 2dá2g 2d1állá dá2n1iz dára2 dá2r1ag dá2ral
dá2ras dá2r1e2 dá2rij dá2rit 2d1á2ru dá2sal dá2se dást2 dá2sü dá2sz1al
dá2sz1ál dá2szár dá2sz1e dá2sz1ö d1átm dá2z1ak dá2zs dáz3sá dd1elh d2d1i2d
d2d1o2d d2dz 1de de2a1s de2at 2d1eff de1f2r de2g1al de2g1ál de2g1el de2g1em
deg3g de2gin de2g1o de2g1ö2 de2gő 2degy de2gye de2gyi de2isz de2k1a2r
de2k1e2g de2k1ell dek1érv 2dekö de2köz dele2já dele2meg 2d1e2lemz del2lal
dellé2 del2l1én de2lőa d1elz de2m1é2rem de2m1érm de2mú de2n1e2g de2n1ese
de2nol de2of de2ok de2ol de2om de2or de2os de2ot de1pra de1p2re de2r1áz
der1osz de2rö 2d1e2rő de2r1u2 de2s1a2 de2sá de2s1ér de2su de2s1ú2 2d1e2szű
det1é2ré 2deza de2zér dezi2 de2zid de2zil de2zin de2zor 1dé dé2da dé2d1ő
dé2du dé2d1ü dé1f2 dé2gá dég3g dé2go dé2hes 2d1éhs 2d1éjs dé2k1a2n dé2l1a2
dé2l1e2l dé2l1es dé2lir dé2lo dé2lu dé2lyá 2d1é2pí dé2raj dé2rar 2d1érté
2dértő dé2sa dé2s1el dé2s1o dé2sö dé2sza dé2ti dét1is dé2tu 2d1év. 2d1é2vei
2d1é2vek 2d1é2ven 2d1é2ves 2d1é2vet 2dévér 2d1é2vi d1f2 dg2 dgá2z d1gr 1di
di2ac dia2dó di2afo di2ah di2ain di2aj di2am di2ap dia1szk di2at di2av
2d1i2dő die2s di1fl digaz1 di2gén di2gér 2d1i2gét di1k2ro di2k1u2ta 2d1ind
di2ol di2óa di2óc di2ófe di2ósz 2d1i2rod 2d1irt 2d1iste 2d1i2zü 1dí dí2j1al
dí2ji 2d1ín. 2d1í2rá d1írn dí2ró 2díz dí2zi d2j1i dk2 dkész1 d1kr dlás1s
dlő1k2 dme2g1é 1do 2dodú doe2 doge2 do2k1ú do2laj 2d1oml do2n1e do2név
do1p2h do2re do2r1é2 do2rip do2ris do1sz2k 2doszt 1dó dóa2 dóel1 dó1p2
dó2sem 2dósl dó2só 2d1ó2vó 1dö döke2 dö2k1er dö2kí dö2le 2d1ö2v 1dő dőa2
dő1d2r dőe2r dőé2 dői2rá 2dőra 2d1őrb 2d1őrez 2d1őrf 2d1őrh 2d1ő2rig 2dőril
2d1őrj 2d1őrk 2d1őrm 2dőrö dő2rök dő2rön 2dőrőr 2d1őrr 2d1őr1s 2d1őrt 2d1őrv
dő1ská dő1s2p dő2tál dő1tr dőu2 d1p2 1d2rám dri2t1 drog2r drop2 1d2ruk
dsé2gi d1s2p d1s2t2 dsuhanc1 d2tarc 1du du2c3h du2gal du2g1ár 2d1u2no 2d1unt
du2se dus3s du2ta du2tá 1dú dú2cs dú2to 1dü 2d1ü2g 2d1ü2z 1dű 2d1ű2z dv1or
dy1a 1dyk d2z 1dza 1dzá d3zár 1dze 1dzé 1dzi 1dzí 1dzo 1dzó 1dzö 2d3zöld.
1dző 2dzőj 2dzős 1dzsa 1dzsá 1dzse 1dzsé 1dzsi 1dzsí 1dzso 1dzsó 1dzsö 1dzső
1dzsu 1dzsú 1dzsü 1dzsű 1dzu 1dzú 1dzü 1dzű 2e. e1a e2ac ea2da ea2dá e2adi
ea2do ea2dó ea2du e2aga e2ah eai2v ea2j e2aki e2ako ea2la ea2lá e2ale ea2p
e2ar. ea2ra eas3s east2 e2at. e2atk e2atl e2ato e2au ea2z e1á eá2bé eá2cs
eá2f eá2g eá2ke eá2lár e2ále eá2l1é eálu2 e2ámu eá2nyal eá2nyan eá2nyas
eá2nye eá2sa eá2se eá2so e2áte e2áté e2átö eb1a2d e2b1ajk eb1atl 1e2béd
eb1ing e1b2lo eb1ó2r ec1á2r 2ecento ece2ti ecés1 ec3hez ecs1ért edeles1
ede2ri edé2lyo 1e2dény edés3s 2edi edi2g e2d1iga edi2ó ed1iro ed1ír ed1íz
ed1orv e1d2ró edu2r e1dy e1e ee2b ee2c e2e2d1 ee2g ee2l eel1i eel1o ee2m
e2ene ee2r ee2se ee2sé ee2si ee2ső. ee2sőn ee2sü ee2sz ee2tet e2e1th ee2v
e1é2 e2gabá eg1abr e2g1a2d e2g1a2g e2g1a2j e2g1a2k ega2lac ega2lak e2g1a2lá
e2g1alt e2g1alu e2g1alv eg1ann eg1any e2g1a2pa e2g1apr e2g1a2r e2g1a2s e3gat
e2g1au e2g1a2v e3gaz eg1á2cs e2g1ág eg1áld eg1álm e2g1á2r e2g1á2s eg1áta
eg1átk eg1átl eg1á2zi eg1ázt e2g1e2g e2g1eh e2g1ej ege2lem ege2léb e2g1ell
e3g2elői e2g1els eg1emé e3gend e2gerd e2g1esh eg1essz eg1e2sze e2g1e2tet.
e2g1é2g egé2k eg1éke e2g1é2le eg1é2lü egé2nyel e2g1épí eg1épü e2gér. eg1érd
egé2ret eg1érj 1egészs eg3gya eg3gyás eg3gyen eg3gyo eg3gyú e2g1if egi2g
e2g1iga e2g1ige e2g1igé eg1iha e2g1ij eg1ill e2g1ing eg1ino egi2p eg1ira
egi2ro e2g1ist eg1iszi eg1iszo e2g1ita e2g1itta. e2g1ittá egius1 eg1ivá
eg1izg eg1izm eg1ír eg1o2d eg1o2k eg1ola e3gom eg1op e3goro e2g1os eg1órá
e2góv eg1öb eg1ök eg1öz e1g2ráf eg1ug eg1un eg1u2ra eg1u2t eg1u2z e2g1ú2
egü2g eg1ü2li eg1ür eg1üz egy1a2d egya2r egy1as egy1a2t egy1az egy1ál
egy1ára egy1ell 1e2gyenl egyes1s egy1eszt egyköz1 egyo2 egy1ol egy1om egy1ór
egy1öl egy1ös egy1őr e2gyür 1együt ehé2ra ehé2z1 1ehh e2h1ors eh1s e1i e2ia
e2iá e2ibu e2ic e2idá eido1 ei2dő e2ier e2if ei2gá ei2gé ei2gy ei2ha ei2j
e2ima ei2má e2ini e2inr eins2 ei2on ei2pa ei2ram ei2rat ei2rod e2iró ei2ta
e2ito e2itu ei2vó e1í2 ej1a2d ej1al ej1ál ejes1s ejé2k e2j1ék. e2j1éke
e2j1in ej1kr ej1ol ej1op ej1os ejta2 ej2tak ej2tal ej2t1an ej2tau ej2tál
ej2tár ej2tát ej2tos e2j1ú2 ek1a2dó e2k1aj e2k1alj ek1áll ek1ár. e2k1e2lőt
ek1é2le ek1épí e2k1éss e2k1é2te ekie2 ek1ist e2kod ek1orv ekö2l ek1ölé
ek1ölő ek1őr ek2rit ek2tal ek1út e2k1üt ek2vit el1ad. el1a2da el1a2dá el1add
el1adh el1adj el1adn el1a2dó el1adt el1adv el1agg el1a2gy ela2j el1ajá
el1akc e2l1a2l ela2m el1ame el1ann el1a2ny el1a2pa el1apr el1aszá e2l1a2u
el1a2v el1ács e2l1á2g el1áj el1áll el1álm el1á2m el1á2p el1ár. el1ára el1árb
el1árn el1á2ro 1eláru el1árv elá2s el1ás. el1ása el1ásn el1áso el1ást el1ásv
el1áta el1átk el1átr el1ázi el1ázo el1ázv eld2 el1dr el1ebl ele2gal e2leges
el1e2gyen el1egz el1e2lev e2l1ell e2l1elm el1ember e2leme. 1e2lemei 1e2lemek
elem1ell 1e2lemez ele2m1érté el1e2més 1e2lemi. 1e2lemű 1e2lemzé 1e2lemző
ele2res el1erj el1erk el1ern e2l1e2rő ele2s1a ele2sem el1esh ele2sik. eles3s
ele2tele el1é2ges el1é2get eléka2 el1é2kel el1él. elé2lek el1é2lé el1élh
el1élj el1éln el1élt el1élv el1érc elé2rem elé2ren e2l1é2rez 1e2l1éré
e2l1érh el1é2ri e2l1érj e2l1érk e2l1érn el1érs e2l1érü el1f2 el1iga e2l1ige
2elik el1ikr el1ill el1ina e2l1ing elin1n elio1 eli2os el1ivá 1e2lix el1izm
e2l1ín el1ír. el1íra el1í2rá e2l1í2v 2elj. el1k2 ell1alk 1ellátásü 1ellens
ellu2s 1elmél 2elné e2l1o2l el1oml el1orc el1ord elo2s el1oso el1oss el1ox
el1ó2ri el1öm el1ön el1ör el1ö2z 1e2lőad 2előbé 1előd. 2előin 2előit 2előkh
2előkk 2előkö 2előkr 2előne 1e2lőő 2előr 2előt. 2előter e2lőtet 1elővés
el1ph el1p2l el1pra el1st 2eltek 2eltes 2eltet. 2eltete 2elteté 2elteti
2eltetn el2töv el1u2r el1u2s el1u2t el1ús el1üd el1üs e2l1üt el2val el2v1at
el2v1ó2 e2ly1a2 e2ly1á2 e2lyel elyes1s e2ly1o e2lyö ely2tál e2m1ab em1a2da
em1a2dó em1adt ema2j em1aja em1ajk ema1k em1a2ny em1arc e2m1ass em1atl
e2m1av e2m1áb em1ár. e2m1árn e2m1átl 1emberr 1embó eme2c emec3h 1emeled
1e2melen 1emelk e2m1elm e2m1e2l1o em1e2res e2m1e2rő em1ers e2m1é2j emé2lete
1e2méss e2mid e2m1im e2m1ip e2mirá e2mír 2emo em1old e2m1orv em1ó2ra em1ös
em1ph em1s 1e2mu. em1ur em1üg em1ür em1űz e2n1a2j e2n1ak e2n1a2n en1ar en1as
e2n1a2u e2n1áb en1áll e2n1ár. e2n1ára en1á2ro en1ázt en2c3h enci2ah enc3sze
en2d1ell en2dza en1elm e2n1emé ene1p2 2enerá 1e2nerg ene2t1o e2n1eze
e2n1é2ra engés3s eng1g 2eni eni2g en1igé e2n1ip en1ira e2n1ism e2n1ív enké2t
en1k2r en2nül en1old en1ó2r en1öl e2n1ö2t ensas3 en2sel en1ste ent1a2ny
entes1s en2tev ent1ért en1uc en1ud enu2t e2n1ú 2enw e2ny1ag en3ya2n eny1as
e2nyau enyeres1 e2ny1ó2 eny1us en2zed e1o e2oc eo2da e2odi eo1g2rá e2oh
e2oka e2o1k2l e2oko eo1k2r eo2ly e2omé eona2 eo2n1ar e2oná eont2 eon1tr
eo1p2 eo2pe eorgi2á e2os. eo2so eosz2f e2ov e2oz e1ó e2óf e2óm e1ö eö2l eö2m
eö2v eö2z e1ő2 epa2d ep1ado ep1aka ep1a2la ep1asz e2p1ág ep1elo e2perf ep2ha
ep2ho ep2lan ep2las e2pos ep2rav ep2rio ep2tol er1abl er1a2da er1adm er1ajk
er1akc e2r1akk e2r1a2la er1alk er1ana er1ann er1a2pa er1apá era2r er1ara
er1ass erato1s e2r1a2u e2r1a2v e2ra2z er1azo er1á2gy e2ráld e2r1áll er1ár.
erá2rak er1árb e2r1árf er1árk er1árn er1á2ro er1árr er1árt er1á2ru er1áta
er2cél er2c3h 1erdő 1e2redm ere2gá 1erejü ere2kar erekes3s ere2ko e2r1ela
e2r1ell er1elo e2remu ere2tel ere2vel e2r1ex erés3sze e2r1év. eré2zé er1gl
er1ide e2r1iga e2r1ind e2r1inj e2r1ism er1i2sza eri1szk er1izo er1izz er1íj
e2r1íz er1kr 1ernyő ero1g2r ero1t2r er1o2xi e2r1ó2r e2r1ö2k e2r1ö2l er1ön
er1ös e2r1ö2t er1ö2z 1e2rőmű 1erősítő. 1erősítőr 1erőszakol. er1pl erro1k2
er2s1a2d er2san er2sar er2s1ol er2s1ö er1str er2sú er2t1any erta2r ert1ara
2erte ertés3s e2r1ur e2r1ü2g er2vos e2s1ac es1a2n esa2p e2s1as es1áb es1á2g
e2s1ál es2co e2s1ekkén e2s1elm e2s1emb 1esemé e2s1ina es1i2pa es1ita es1í2rá
es1k es2kat 1eskü e1s2lat es2már es1ol e2s1os e2sőc e2sőe es3szö esta2l
est1ala es2t1áll es2t1á2p es2tim es2tí es2top es2tőr es1trá e1s2tu es2tú
es1ú2s es1út es1ü2ve e1sy es3zac e2sz1ad esza2ké e2szárp e2szev e2szég
esz1é2les esz1iz e1sz2kó es3zse eszta2 esz2t1al esz2t1an esz2tár esz2t1ö
e2t1a2d eta1g2r e2t1aj et1akt et1a2la et1alk e2t1ant eta2nya eta2nyá et1a2rá
etas2 et1ass et1ág. et1á2ga et1ágn et1áll etá2ru e2t1á2t1a et1átl eteá2
ete2l1eg ete2ná e2t1ene ete2szá eté2ká eté2kel eté2ko e2t1érté e2t1érz
etés3s eté2t1é et1év. et1é2vet et1é2véb et1é2vét et1évh e2t1i2d eti2g
e2t1iga e2t1igé e2t1ill e2tim et1inf e2t1iri e2t1iro et1írá et1í2v et1okm
et1okt eto2na e2t1ös et1őr. et1ő2ri et1őrn et1ő2rö et1őrs e1t2raf et2sz1ék
et2telő e2t1ug e2t1u2n et1u2r e2t1u2t et1új e2t1út e2t1üd e2t1üld e2t1üt
et3ya e2ty1o 2etz e1t2ze e1u eu2ga eu2go e2ume eumi2 eu2miv e2uras e2urá
eu2r1á2z e1ú2 e1ü2 e1ű evéle2 evé2l1el ex1io e2x1í2 1expe 1expo e1ye ez1a2d
e2z1ak 1e2z1a2l e2z1az ez1ál ez1egy e2zer. eze2ra eze2re. eze2reg eze2r1o
eze2ta eze2t1el ezé2rem ez1idá ez1ill ez1inf ez1int e2z1i2p ez1ir ezo1k2
ez1org ez1ó2t ez1ön ezőkész1 1ezred 1ezrei. 1ezreir 1ezreit 1ezreiv 1ezrek
1ezrel 1ezres 1ezret e2z3sé ez3sor ez1ut ez1út 1ezüs 2é. é1a2 é1á2 éb2bá
é2b1ir é2c1a é1ch é2c3so éc3sz é2d1ab é2d1a2c éd1ad é2d1ak éd1a2la é2d1a2n
éd1a2p é2d1as éd1ág é2d1e2g éde2leme é2d1els é2d1eml é2derb é2derek é2deré
é2dern é2derr é2dert é2desí édes3s é2d1él éd1értő éd1érv édi2a1s édiasz2
é2d1i2g é2d1in é2d1ír édna2 é2d1or éd1ut éd3za éd3zá é1e2 éesz1 é1é 2éf
é2g1ab éga2d ég1a2g é2g1aj é2g1a2k ég1a2l ég1a2z ég1á2g ég1ál é2g1ára
é2g1elb ége2lem é2g1elg é2g1ell é2g1elm é2g1e2lő égelői2 é2g1els ége2n
é2g1eny ége2r1ü 1é2gesd 1é2geti 1é2getn é2g1é2g é2g1é2l é2g1ép é2g1éri
ég1érte ég1érté ég1érv é2g1id égi2g égig1a égigé2 ég1ill é2g1ing ég1int
ég1ír ég1o2l ég1ot ég1ó2r é2g1ö 1é2gő ég1u2t é2g1ú2 ég1ü2g ég1ür é2gy1eg
é2gyele é2gy1em é2gy1esz é2gyeze é2gy1ér é2gy1év é2gy1o é2gy1ó2 é2gy1ö2
égyszáz1 égyszín1 é2gyu égyü2 é2gy1ül é2heze é2hezi é2hezt éh1ín é1i éi2ro
é1í2 éj1a2d éj1e2g é2ji éj1it é2k1ab ék1acé é2k1a2d é2k1a2g é2k1a2j ék1a2la
ék1alj ék1alk ék1ana éka2nya é2k1a2r ék1á2f é2k1ál é2k1árb é2k1á2ru é2k1áta
é2k1elm é2k1elo é2k1e2mel ék1eng éke2nya ékes3s ék1ég éké2p ék1épí ék1épü
ék1ér. é2k1é2rem ék1érl é2k1érte é2k1érté é2k1érz é2k1i2d ék1ing ék1int
é2k1i2p éki2r ék1ira ék1ism ék1ír ék1íz ék1k2 é2k1or é2k1osz é2k1ös é2k1ő2
ék1t2 é2k1ud é2k1u2t ék1út é2k1ünn é2k1ür é2k1üz él1abl él1akn é2l1a2l
é2l1a2u él1áj él1á2l é2l1árk é2látf éld2 él1dr é2lebé él1e2gy éle2k1a
éle2ker éle2kö é2l1emb é2leme é2l1emp é2l1enn éle2sa éle2sz éle2ta éle2t1á2
éle2t1e2l élet1érd éle2t1é2ve. éle2t1ö2 é2l1é2d é2l1é2l él1é2ret él1é2vet
él1é2vi él1f2 é2l1i2d é2l1iga éligaz1 éli2gá é2l1i2m é2l1in él1i2pa él1irá
é2l1í2 él1k2 é2l1o2l él1op é2l1o2r él1ó2r élö2 él1ök él1öl él1ön élőe2
élőkész1 él1őz él1p2 éls2 él1st él2t1e2v él1t2r él1ug él1uj él1u2t él1út
é2l1üt ély1á2l ély1egy élye2n1 é2lyil é2ly1in é2lyüt é2male ém1ass ém1ato
é2m1á2l ém1áru ém1edé ém1e2le é2m1elh ém1elő é2mesem é2m1esz éme2ta éme2tel
émé2l ém1étk é2m1ip é2m1ir ém1ol é2m1ó2 é2m1ö2 ém1p2 ém1u2r én1ant éna1p2
én1ass é2n1a2to é2n1áll én1átv én1egy é2nekel. 1é2nekes 1é2nekl én1elj
én1elő én1elt é2nem én1emb é2n1é2g éné2m1 én1f2 én1int é2n1i2o é2n1ip én1ita
é2n1i2z énkupac1 énmo2n1 2énn én1öve én2s1as én1s2p én2su én2sú én1t2r
én1u2t 2ény é2ny1a2 é2ny1á2 é2ny1ell ény1elv é2ny1e2r é2nyev é2nyél é2nyérté
é2ny1érv é2nyérz é2nyis é2ny1it é2ny1í2r é2ny1o2 é2ny1ó2 é2ny1ö2 é2nyu
én2z1a2 én2z1á2 én2zéh én2z1i2 én2z1o én2zö én2zse énz3sz én2z1u2 é1o2 é1ó
é1ö é1ő ép1a2d ép1a2g ép1a2l ép1a2n épa2r épar2t ép1ág ép1ál ép1ám ép1áp
é2p1á2r ép1e2gé ép1egy é2pe2lem é2p1ell é2p1ep ép1ern épes3s é2p1eti é2p1é2l
é2p1ép épés1s épi2a é2p1i2p ép1ira ép1iro é2p1irt é2p1isk é2p1iz é2p1ír épo2
ép1ok ép1ol ép2pak ép2p1an ép2pek éppo2 ép2pod ép2p1ol ép1pr é1p2ró ép1t2
ép1uj ép1u2n ép1u2r ép1u2t é2p1ú 1épüle ér1abr é2r1a2d ér1ajá ér1aká éra1kl
ér1akn é2r1a2l éra2n ér1any éra2r ér1ara ér1arc é2r1as é2r1a2t é2r1a2u
ér1á2g ér1ál é2r1ár. érá2rak ér1áru ér1áz ér2ca ér2cá 1ércb érc3c ér2ced
1ér2c3h ér2c1o ér2c1ö ér2cz ér2dz ére2b é2r1eb. é2r1ebe é2r1e2dé ére2ga
ére2g1á éreg1g é2r1e2gy ér1elk é2r1ell ér1e2lő é2r1els ér1eny é2r1e2re
é2r1e2ső éres3s ére2ta ére2tel é2r1e2v é2rezh é2rezv ér1é2kek ér1ékel
é2r1é2l é2réneke éri2as é2r1i2d éri2g ér1iga ér1ige ér1ill é2r1isk é2r1ist
ér1i2sz é2r1itt é2r1iz é2r1ív ér1old ér1olv ér1ont é2r1or é2r1ö2 é2rőse
ér1p2 érs2 ér1sk ér1st 1értelm 1értőc 1értőj 1értőr 1értőv ér1tra ér1u2r
ér1u2t ér1u2z ér1úr ér1út ér1üld ér1üt érva2d érvén3n 1érvv 1érzeté
1érzékenye 1érzékenyí 1érzékenyül. 1érzékenyülé 1érzékenyülh 1érzékenyülte
1érzékenyülts 1érzékenyülv ésa2 és1ad és1al é2s1an és1ál é2s1á2t é2s1eg
és1elá és1e2le és1ell é2s1elm és1elo és1e2mel é2s1esz é2s1eti é2s1ev é2s1él
é2s1érté és1é2ve. é2s1id é2s1in é2s1i2r é2s1i2v és1í2v és1k és1ol és1os
é2s1ó é2s1öl és1ös és1p2 éss2 és1st és3sza és3szá és3szi és3szö és3szú
és1t2r é2s1ú é2s1üg é2s1ü2t é1sy ész1a2d ész1a2g ész1a2l ész1a2n ész1a2r
és3zav ész1á2l é2sz1egy ész1ell é2sz1esz é2szex é2szez é2sz1é2l ész1éte
é2szid é2szim é2sz1ir észi2s ész1k2 é2sz1o2k é2sz1ol é2sz1ot ész1ös ész2t1o
é2s3zu é2szú ész1üg é2sz1üz é2t1a2d ét1ajt ét1a2ka ét1akk ét1akn ét1a2la
ét1ann ét1apr ét1arg état2 ét1a2to étau2 é2t1ág ét1áll ét1árb éte2rá éte2reg
éte2rel é2t1esé é2t1esh é2t1e2si éte2szem ét1e2ve ét1ezr é2t1é2g été2k
é2t1él é2t1érd é2t1érte é2t1érté ét1f2 é2t1ic é2t1id éti2g ét1igé é2t1i2r
ét1ist é2t1i2v é2t1ív é2t1o2l é2t1o2p éto2r1i é2t1ó2r ét1ök ét1ö2l ét1öm
ét1ön ét1ös ét1ö2t ét1ö2z é2t1őr étrás1 é1t2ró ét1sz ét1ug ét1uj ét1üt
ét1ű2z étve2g é1u é1ú é1ü2 é1ű é2vad év1adá év1adó é2valá é2v1a2z é2velő
1é2vent év1esté 1é2v1ez 1é2vérő év1érté é2vu é2v1ú2 1évz éz1ab éz1a2d éz1ak
é2z1at é2z1ág é2z1ál éz1á2p éz1d2r éze2d éze2m éz1eme éze2n éze2t1el éz1ev
é2z1é2d é2z1érc éz1ill éz1ing ézi2ók é2z1i2r ézi2sel é2z1í2 éz1k2 éz1or
éz1os é2z1ö éz3ser éz3sé éz3sis é2z3so éz3sö ézus1s ézü2 éz1üg éz1üs éz1üt
ézve2g 1fa faá2 fae2 fa2gyé fa2j1ö fa2jü fala2n fal1any fa2l1ár. fa2l1e2g
fa2l1e2l fa2lí fao2 faó2 faru2 fa2r1us fas2 fa1sk fa1st2 fa1t2r fa2us fazé2
1fá fás3s fá2sü fá2t1é fázás1 1fe fe2gy1i fe2j1a fe2já fe2jo fe2l1a2 fe2l1á2
fe2l1e2g fe2l1e2mel fel1emé fe2l1eml fel1eng fe2l1ere fe2l1esk fe2l1esz
fe2l1e2tet fel1eve fe2l1é2g fel1éke fel1éks fe2l1él fe2l1ép fe2l1ér.
fe2l1ért fel1érz fe2l1id fe2lij fe2l1i2s feli2t fe2l1ita fe2l1iz fe2l1í2
fe2l1o2 fe2l1ö2 fel1p2 felsőrész1 fel1t2 fe2lu fe2lú fen2ná fen2tí 1fé fé2ká
féko2 fé2kú fé2l1a fé2l1á fé2leg féle2m fél1eme fé2l1esz fé2l1ez fé2lir
fé2l1is fé2l1o fé2ló fé2lö fé2lu fé2lú féma2 fé2mat fé2med fé2mel fé2mét
fé2ny1e2l fé2ny1i fé2nyí 2férá 1f2fy 1fi fi2ah fia2la fi2ap fi2asz fi2av
fie2 fi2l1el fina2 fi2n1ag fi2nid fint2 fi2n1u2 fió2ki fi1sc fist2 fi2t1é2
fi2tor 1fí f1k2 fki2 fkis1 f2ló f2lú fme2 1fo fog1g fo2g1or fo2g1os fo2n1au
fo2nát fond2 fon1dr fo2nel fo2n1in fo2nü fo2r1il foto1s 1fó 1fö föle2 fö2leg
föl1el fö2len fö2l1esk fö2l1et fö2lék fö2lí 1fő főe2 főé2 fői2 fő2r1ü2 fő1tr
főu2 főú2 fra1s f2ric 1f2rö fs2 f1st ft1aj ft1ak 1fu fus3sz fu2sz1ol 1fú 1fü
fü2lo fül1t2 fü2mi f1ü2té f1ü2tő 1fű 1fy 1ga ga2a g1a2bál 2g1abl 2gabr
2g1a2cé ga2de gae2 ga2gi ga2kad 2g1a2lap 2g1a2lám g1alb g2ali 2g1alk 3g2all
2g1als ga2lul gan2cse gan1d2 2gank 2g1a2nya ga2nyá ga2nyó gaó2 ga2pa. 2g1apj
ga2pol 2g1a2pó 2g1app ga2se ga2sé ga2s1i ga2su ga2szág g2at gata2r gat1ará
ga2t1eu ga2tim ga2ut ga2vat 2gazg 2gazít. 2gazíta 2gazítá 2gazítók 2gazítón
2gazs 1gá 2g1á2bé 2gábr 3g2áci gá2dá 2g1á2f gá2gy g1á2hí 3g2áli g1állh
g1állí g1álls g1állu 2g1állv 2g1á2mí gána2 gá2nad gá2nal gá2nap gá2nau
gá2n1e2 gá2nén gá2n1ér gá2nin gá2n1ó gá2nú gá2nye gá2ny1út. 2g1á2p 2g1á2rad
g1á2rak gá2ral g1á2ram gá2r1as gá2r1ál gá2rár gá2r1e gá2riz gá2rö gá2ru
gá2r1út gá2san gá2s1e2 gásé2 gási2 gá2sir gá2sze gá2szé gá2s3zö gá2ta2l
gá2té 2gátk 3g2átló 3g2áto gá2t1os 2gátr 3g2átu 2g1á2tü 2g1átv gá2z1ad
gá2z1al gá2z1a2t gá2z1e2 gázi2 gá2z1ip gá2z1ó2r gázs2 gáz3sé gá2zsu 2g1á2zu
g1b2 g1d2r 1ge gea2 geá2r 2gebé ge2cet 2g1e2dén 2g1e2dz 2g1eff ge2gé gegész1
ge2gye ge2he g2ek. 3g2el. 2g1e2l1a2 ge2leg ge2lej gel1eng ge2lev ge2lég
3g2elés 2g1elhá 3g2elhe 3g2eli. 3gelik gel1ism ge2l1í2 3gelj. 2g1eljá
3g2elne 3gelné 3g2elni g1elny 2g1e2l1o ge2lö 3g2elő. ge2lőbb 3gelőbé 3g2előh
3g2előj 3g2elők. 3g2előket 3gelőkh 3gelőkk 3gelőkö 3gelőkr ge2lől 3g2előm
3g2előn. 3gelőne ge2lőre 3gelőrő ge2lős 3gelőt. 2g1előte 3g2előv ge2lőz
2g1elp g1első 3g2elt. 3geltek 3geltes 3geltet. 3geltete 3gelteté 3gelteti
3geltetn 2g1eltér 2g1elto ge2lu ge2lül 2g1elvá 2g1elvez 2g1elvo 2g1elz
3g2em. 2g1ember 2g1e2mel ge2més 2g1eml ge2na genci2as g2end 3generá 2g1enge
ge2n1is 2g1ennék 2g1enni. 2g1ennie 2g1enyh 2g1enyv ge2of ge2o1g2 ge2oló
ge2om ge2o1s ge2ral ge2r1a2n ge2r1ág ge2r1á2r g1e2rej ge2r1ekké ge2riz
ge2r1os ge2rö 2g1e2rő ge2rű 3g2es. ge1sc ge2sem ge2sett 2g1e2sés 2g1e2sik
2geskü 2g1e2ső ge2sú 2gesze ge2szé ge2szik 2g1eszl 2g1eszm 2g1eszn 2g1e2szü
ge2szű g2et. ge2ter 2g1e2tete 2g1e2tetn 2g1e2tette. ge2t1or ge2ur 2g1e2ve
2g1e2vé 2g1e2vő 1gé gé2ber 2g1ébr gé2da gé2d1e2l gé2d1esz gé2dé gé2dí gé2d1o
gé2gü gé2hes gé2led gé2let gé2lén gé2li. 2g1éls gé2lya gé2lyá gé2lyeg géna2
gé2nat géná2 gé2n1ár gé2nát gé2n1el gé2nit gé2nyemb gé2nyeme gé2nyir gé2p1a
gé2pá 3g2épe gé2peg gé2pir gé2pí gé2po gé2pül gé2r1á 2gérd g1é2red gé2reg
2gérh g1é2rin 2gérj 2gérk 2gérl g1érle 2gérs 2gérte 2gérté g1érth g1érti
g1értj g1érts g1értt g1értv gé2ru gé2rú 2gérv 2g1érz gé2sá gé2sel gé2so
gé2sza 2gészs 2g1é2te 2g1év. g1f2 gfe2li g1g2ra g2gy g3gyár g3gyér g3gyi
g3gyó g3gyö g3győ g3gyu ggy1ült g3gyű 1g2hék 1ghy 1gi gi2abo gi2ac gi2ad
gi2af gi2ag gi2ah gi2ako gi2am gi2ap gi2ar gi2asze gi2aszi gi2aszo gi2ata
gi2av 3g2iá 3g2idá 2g1i2de 2g1i2dé gi2dő gie2r gi1fl 2g1igaz gig1ár gig1e2s
gig1él gi2gén gig3g gi2gi gi2g1o gi2gö gi2g1u 2g1i2gy 2gihl gii2 2g1ikr
2g1i2má 2g1i2mi 2g1imp 2g1ind 2g1inf 2g1inn 2gino 2gint. 2g1inte 2g1inté
2g1inti 2g1intő 2g1inv 3g2io gi2or 2g1i2rá 2giro gi2rod giro1s 2g1irt g1isc
2g1isk 2g1ism 2g1iss 2g1iste 3g2iti 2gittam 2gittasodh 2gittasodi 2gittasodn
2gittasu gius3s gi2vó 2gizg 2g1i2zo 2g1izz 1gí 2g1íg 2g1íj 2g1í2n gí2ra
gí2rá gí2ro gí2ró 2g1í2z g1k2 gki2s1a g2lio glóa2 gme2g1á 1g2nac 1g2náb
g2n1e2l gnes3s gn1ing 1gnore 1go go2e go1g2r goi2 goka2 gok1ad 2g1o2ká
2g1o2laj g1olc 2g1olda go2lin go2lór 2golvas 2g1o2lyo go2m1as 2g1oml g2on
3goná gonc3s go2nye go2pe g2oro gosz2kó go1t2h 1gó 3gó. gó2ce gó2ch 2górá
gó2rák gó2rát gós3s gó2tí g1óv. g1ó2vo g1ó2vó 1gö 2göb 2g1ö2le g1ö2li g1öls
3gömb g1öml 3g2öng g1önt 3görb gör2cso 3gördí 3gördü g1ö2re 3görg 3görn
g1ö2rü gö2s1é2 2g1össz 2g1ö2v 1gő gő2g1ő 2g1ő2ri 2g1őrj 2g1őrl 2g1ő2rö
2g1ő2rü 2g1őrz gő2s1ü2 g1őszí g1őszü gőü2 gő2z1e2k gő2zel gő2zsu g1p2
1g2rafá 1grafik g2ra1p 1g2ráf. grá2fe 1gráfia 1gráfiái 1gráfián 1gráfiát
1gráfu grá2l1e2 grá2rip grás1s g2ríz g2róf gs2 g1sk g1sp2 g1st2 gszáz1 gszt2
g1t2 gtá2v1i2 gtíz1 1gu 3g2ub 2gu2g 3gugg gu2il gu2in 2g1uj gu2na gu2no
3gurí 3g2uru gu2sab gu2s1í2 gu2sol gus3s gu2tak gu2tam gu2tat gu2to 2g1u2tó
1gú g1úg 2g1ú2j gú2nyi gú2te gú2té g1úth g1ú2ti g1útj g1útn g1ú2to g1útr
g1útt 1gü 2g1üd 2güg 2g1üld gü2lik 2g1ünn 2gür gü2re gü2rü 2g1üs 2g1ü2t
2g1ü2v 2güz gü2ze 1gű 2g1űr. 2g1ű2z gví2z1 g1w g2y 1gya gy1acé gy1a2dá
gy1adm 2gy1a2j gya2lap gyan1ab gy1ann gy1a2nya 2gy1apa 2gyapi 2gyapó 2gyapu
gya2r1ó2 2gyaty 2gyazo 1gyá 2gyábr 2gyág gy1áll gy1álm gy1á2lo gyá2ma
gyá2ria 2gyáru 2gyáta gy1átl 1gye 3gyec 2gyedün 2gyela 2gyelemz 2gyelőg
2gyeltá gy1emel gy1eml 2gyerd gye2rős gye2seg 2gy1ev 2gyezres 1gyé gy1éle
2gyép gy1érd gy1érte gy1érté gy1érz 1gyi 2gyidő gy1iga 2gy1i2gé 2gyind
2gy1ing gy1i2ra gy1irá 2gy1iro 2gyist gy1ita 2gyivó 1gyí 2gy1í2r 2gyív gy1k2
1gyo 2gy1old 2gyolvad 2gyolvas 2gyope 1gyó gy1órá gy1óri 1gyö 2gyönt 1győ
2győr 3győz gy1pr gys2 gy1st 1gyu 2gy1ud gy1ug 2gyuj 2gyura 2gyurá 1gyú
2gyút 1gyü 2gyüt 2gy1üz 1gyű 3gyűr gza2te 1ha ha2b1ol ha2d1ál ha2d1os ha2dur
ha2d1ú ha2is ha2je 2h1akl ha2kol ha1k2r hal1áp ha2leb ha2l1e2l ha2len
ha2l1es hal1evő ha2lét ha2l1iv ha2lí ha2lol ha2l1ő ham1b ha2m1osz han2ch
han2c3s han2g1e2 han2gut 2hani ha2nyá haó2 ha1p2r ha2rál harán2 harc1c
har2ce har2c3h har2cso harc3sz ha2r1ist hart2 hasi2 ha2sim ha2sol ha2s1ű2
ha2t1em ha2tev ha2t1é2v ha2t1old ha2t1ök 1há 2h1ács. 2h1á2csi 2háji 2hájn
2hájo 2hájt há2lyú há2m1i há2nyin há2ral háro2ma há2sá há2t1e2 há2t1ol
há2t1or hátu2s hát1usz há2zab há2z1a2dó há2z1e2 ház3sz 1hä 1he he2ad 3heg
he2gy1a he2gyo he2gyó 2he2id he2io 2hela 2henó he2od he2ol he2rát 2heu he2za
he2zá 2hezn 1hé hé2m1 hé2nan hé2n1is hé1p2 hé2rab hé2rar hé2r1eg hé2r1ep
hé2tal hé2tá héte2 hé2t1es hé2t1ez hé2t1é2 hé2tó hé2z1á hé2z1e2 hé2z1o2
hé2zs 1hi 2hia hi2af hi2am hi2as 2hila 2hio hi2re hi1sc hi2se hi2t1á
hi2t1elv hi2t1o hi2tú 1hí 3híd hí2de hí2dí hí2ga hí2mi hí2r1a2 hí2r1o hí2rö
h1k2 h2m1é h2mi hno1g 1ho 2hob ho2gy1i hola2 ho2lad ho2l1át hol2d1al ho2le
ho2l1iv ho2n1al ho2nav ho2n1ál ho2nis ho2nü ho2o 2hory 2hosb 2hosi 2hosz
ho1szk 2how 1hó hóa2 hó2c3sa hó2dz hó1kr 2hónr 1hö hökö1 1hő hőe2 hő2s1er
hős3s h1p2 h2ri h2rü hs2 h1sc hsz2l h1t2r 1hu hu2i hu2me 2husi huszon1 3hú
hú2re hú2r1é hú2se hús3szé hú2sz1e hú2szé hú2sz1ó2 hú2szö 1hü 1hű hyá1 2i.
i1a ia2cel ia2cér ia2c3h ia2dós iae2 i2afa ia2gi i2ahá 2iai ia2kad ia2kar
ia2kas ia2kol ia1k2ré ia1kri ia1kv i2ale i2alé ia2lu i2ame ia2mur ia2nya
iaó2 ia1p2 i2asé ia1s2p i2aszá ia2szor ia2szö ia2ty iau2 i1á 2iáb iá2g 2iák
iá2kab iá2kév iála2 iá2l1al iá2l1e2 iá2l1in iána2 iá2n1an iá2nyi iá2p iá2ro
iá2ru iá2só iás3s iá2sü ibai2 ibi2o 2ibn ibu2c3 1iccé i1chy idá2szo 1iddo
1i2dej ide1k2v 2idel i2d1é2l i2dény 2idés 1i2déz id2ge idi2a1s i2d1id
i2d1i2ta i2domá i2domo 2idó 1i2dő. i2dőv 2idp idro1 idrosz2 id3ze i1e i2ec
ie2dé 2ieg ie2gé ie2gy ie2le ie2léb ie2lőb ie2p ie2rej ie2rez ie2rő.
ie2rőszakoln ie2rőv ier1s ie2set ie2sé ie2si ie2ső ie2sz ietz1 ie2ur ie2v
i1é ié2dere ié2g ié2ke ié2kí ié2l ié2pí ié2pü ifi1b if2lo 2ifö if2re if2ri
if2ta ig1a2lu iga1p2 1igazg ig1áll ig1álm igá2nya ig1á2t1e2 ig1e2le ig1ell
i2ges ig1e2se ig1esi ige2t1o i2g1ev i2g1ég ig1élv 1i2gény ig1ér. ig1é2rő
ig1ért ig2gas 2igi ig1im ig1ir ig1os ig2raf igro1 ig1s ig1ug i2g1ü2 iha2d
iha2re 1ihl i1i 2ii. ii2d ii2g i2ii ii2m ii2p ii2s i1í2 1i2jed 1i2jes ika1pr
iké2l ik1éle iki2s1 ik2kár ik2kí ik2lor ikro1s i1k2ru il1ald i2l1alk il1a2ny
il1ell 1illú il2maj il2m1a2k il2m1am il2man il2má il2mel il2m1esz il2mi
ilo1g2 ilumi2 ilumin1 i2lü 1i2mád ime2g1á imi2tár 2imka 2iml 2imog 2ims
im1sz 2imz i2n1aj in1akt in2c1ez in2c1él in2c1ö in2d1ab in2d1ah in2d1am
in2d1at inde2m in2d1eme ind1err in2d1e2s in2d1ett in2d1e2z in2dét in2din
1indí in2d1ö2 ineke2 ine2kel i2n1el 2inen 2iner ine2ta 1infl 1infr 1ingad
1ingec in1g2rá in1ido in2kaj in2kál in1old ino2má in1ó2ra in1öl in1ös in1sh
in1s2k 1insp int1étk in2t1in in1tré in2t1ú2 inú2 in1út i1o ioe2 i2of io1g2rá
io2ik i2oka io2ká io1kh io2laj i2oló iono1sz2 ion1s2 io1ny iop2 io2pe io1ps
io2so io1sz2f io1t2 i2ov io2xidd i1ó i2ódi ió1f2 ió1g2 i2óha ió2kár ió2kir
iókos1 i2ólá i2ólo ió2rák i2óri i2ósá ió1spe i1ö2 i1ő iő2r 1i2par. ipa2rág
ipa2r1e i2pari ipa2ris i2paro ipe2rak i1p2h ip2lak i1plexe i1p2lé 2ipo ippa2
ip2p1ad ippo1l ip2ri ip2rop ip2sz1a ip2szá ip2szö iqu2 1i2rati 1iratoz.
1iratozi irá2g1ál irá2nyá2ra irá2nye 2ires i2rew irgonc1 2irob iros3s ir2s1a
ir2s1á2 1irtó ir2tü is1abl i2s1a2d i2s1a2g is1a2la is1alk isa2n is1ang
is1ant is1any isa2p is1apa is1apá i2s1ar. i2s1a2ra i2sasz is1atá is1atl
i2s1a2u is1áll is1áru is1á2t1a2 i2s1á2z i2s1ege i2s1e2gy i2s1elm is1elo
i2s1eml ise2n is1enc is1ene i2serd is1esé is1est i2s1ev i2s1ép i2s1éri
i2s1i2n isi2p is1ipa i2s1i2r is1isk is1ist is1í2v is1k 1iskolát i1s2lis
1ismer. 1ismere 1ismeré 1ismerh 1ismeri 1ismerj 1ismern 1ismerő 1ismert
1ismerve 1ismervény. 1ismervénye 1ismervényt i1s2min 2iso is1ob i2s1ol
is1ord is1öc i2s1ör is1ös i2s1ő isp2 1ispán. i1s2pe i1s2por is1pr is1sr
is1st isszo2ba is3szó is3szö is3szú 1isten. 1istene 1istenhit 1istenné
1istennő. 1istennők 1istv is1úr. is1üg is1üs 1i2szák i1sz2f isz1il isz2kóp
isz1ön is3zsa isz2tár. it1adó itai2 itakész1 1ital. 1italt it1ant ita2tat
it1á2ras ite2la 2iter 2ité i2t1é2l it2há i2t1i2g itigaz1 it1ipa itköz1
ito1g2r ito2ká 1ittam 1ittasodh 1ittasodi 1ittasodn 1ittasu it2teg it2tot
it1ug it1uta i1u iu2g iu2mad iu2m1a2t iu2mál iu2me iu2mio iu2mí iu2n iu2r
iu2ta iu2tán. iu2tó iu2z i1ú i2úg i2úte i1ü2 i1ű2 iva2tin 1i2vás. 1i2vot
ix1eg ix1in ix1p2 iz1áll izene2 ize2n1é izen1n ize2s1á2 1i2zél 1izmú 1i2zom.
2izs iz1út 1i2zül 1izzí 1izzó 2í. í1a í1á í2d1a2 íde2 íd1el í2d1é2 íd1ív
í2d1os í1e í1é íg1a2g í2g1e2 í2gé í2g1o2 í2g1ö ígyá2 ígyász1 í1i í1í í2j1ác
íjá2t í2j1áta íji2 íj1ig íj1in í2j1os í2j1ö2 í2jü ík1a2l ík1es í2k1i2 íkké2
í2k1ü2 ílás1s íl1e2g í2l1i2r ílta2 íl2tag íl2t1e2 ím1a2d ímfe2 ím1i2r ím1i2v
í2m1í2 í2m1o2 ímok1 í2n1a2r í2n1a2u í2n1az ín1árn í2n1e2le í2n1elm í2n1észl
íni1k2 í2n1il ín3n ín1ol í2n1or í2n1ö ín1s2 ín1u2t ín1üt í1o í1ó í1ö í1ő
í2r1a2d ír1alk í2r1ar í2r1á2g ír1áll ír1ár. ír1á2ru írás3s 1í2rász.
1í2rászat. 1írdáb í2r1eg í2r1er í2r1é2l írfe2 ír1g2 írin2 íri2o í2r1i2p
í2r1ir í2r1í ír1k2 ír1old ír1oll ír1or ír1ös írs2 ír1sp ír1t2r írus3s í2r1ú
í2r1üg í2sz1a í2sz1á í2szeb í2sz1e2g í2szeln í2szelő í2szelv í2sz1e2m
í2sz1o2 í2sz1ö í2sz1ő ísz3s ísz1tr íszü2 í2sz1ül í2sz1ün í1u í1ú í1ü í1ű
íva2l ív1ala í2varc ív1árf ív1eg íve2lem ív1elté íve2n í2v1e2re í2v1in ívi2z
ív1izo ív1ol í2v1ö ív1üg íz1a2g íz1a2k íz1a2l íz1ág íz1ál íz1á2r íz1á2t1
í2z1ef í2z1eg í2zei. íz1ell íz1eln íz1elv í2z1e2m íze2r íz1ere í2z1esz íze2z
í2z1eze í2z1ezrei í2z1é2p í2z1ér. í2z1ill ízi1sp í2z1i2szo í2zivás í2zí ízo2
íz1os íz1ó2r í2z1ö2 íz1p2 íz3sa íz3sá íz3su íz3sú íz3sz í2zü íz1ü2g 1í2zű
1ja ja2dag ja2k1ev ja2kiz ja2k1í ja2lap jan2s j1a2ny jao2 jas1as 1já 2j1á2bé
2j1á2ga 2j1ágg 2j1áll 2jánd 2jánl 2j1á2p 2j1á2rak j1á2rasz 2j1árr j1árus
já2sal já2se jás3s já2t1ér 2játne2 já2t1osz jdona2 jdonat1 j1d2r 1je 2jegé
je2gés jegész1 jegyá2 je2gy1el je2gy1in je2gyo je2l1a2 jelá2 je2l1át
je2l1ele je2ler je2l1e2si je2l1int je2l1í2 2j1ellá je2l1o je2lu je2mu je2n1á
je2no je2ró je2sa je2s1á je2sem je2s1es je2su je2süv je2tál je2t1el je2t1o2
je2tu jeu2 2j1ex 1jé jé2ga jé2g1á2 jége2 jé2g1eg jég3g jé2gi jé2g1o jéne2s
jé2n1i 2j1é2p jé2reg 2j1érz jfeles1 jfölös1 j1g2r 1ji j1il 2j1i2p 2j1ism
2j1i2ta 2j1i2v 2j1i2z 1jí jítókész1 j1í2z j2jí jjob2 j1k2l jk1őr jk2ré
jlás1s jl1át jnal1u 1jo jo2g1ak jo2gal jo2g1ál jo2g1e jog3g jo2gin jo2g1or
jo2laj joma2 jo2mag jo2m1an jo2m1e2 jo2n1i 2jo2p joro2 jo2se 1jó jóa2 jóá2
jóle2s jó2l1eső jó2l1i jó2lö 2jórár jó2s1e2 jós3s jó2tál 1jö j1önt 1jő j1őr
2j1ő2sö j1p2 jra1s j1s2p jsza2ké jt1akn jt1akt jt1alk jt1aut jt1áll j2t1á2ru
jt1á2t1a2 j2t1e2gy j2teleme jt1elt j2t1é2le j2t1ér j2t1id j2t1i2r j2t1i2z
jt1osz j1t2rá jtu2 1ju jugo1 ju2hak 1jú 1jü j1üg jü2l 2j1ü2r j1ü2té j1ü2tő
2j1ü2v 2j1ü2z 1jű j2z1a2k j2z1a2ny j2z1as jz1es j2z1is jz1k2 jz3sín jz3sor
jz3sz 1ka 2k1abl 2kacé ka2ch ka2dom 2kadók kae2 kag2 ka1gr ka2iá kakas3s
2k1alg kanális1 2kang 2k1a2nyag ka2óv 2kapád ka1pré 2k1a2ras 2karcú ka2rén
ka2rig kar1ing. ka2rö ka2sem kasé2 ka2séh ka2sor kasó2 ka2s1ór kasü2l ka2tab
ka2uto ka2vat kazá2 1ká ká2csin ká2c3sor 2k1á2g ká2l1e 2k1állam. 2k1államr
ká2nye k1á2rad ká2rak k1á2ram ká2r1e2 ká2r1ér ká2r1oko ká2s1e kási2 ká2sir
2k1á2só ká2sü kásza2 kász1al kát1ad ká2tal 2k1átt 2k1átv k1b 1ke kea2k keá2
kee2 kegész1 ke2gya ke2gyu ke2gy1ú 2k1e2ke. 2k1elekt 2k1e2lemz 2keley 2k1elf
ke2l1os 2k1e2lőa k1e2lőá 2k1e2lől 2k1eltér 2k1ember ke2més kenés3s ken2tér
kenü2 ke2n1ül keó2 ke1p2r ke2rab ke2r1ar ke2rál ke2r1ár 2kerdő 2k1e2rej
kerekes1 ke2rős ker1s ke2s1eg 2k1esem 2k1e2ső ke2szi ke2s3zöl ke2tok keu2
ke2vez kevés3s 1ké 2ké2g ké2ja ké2j1u kéka2 ké2k1á ké2k1e2r ké2kin ké2lya
ké2ly1ü2 ké2nyú ké2pa ké2p1á ké2p1ele ké2per ké2pid ké2pí ké2po 2k1érm
ké2r1o 2k1értéke ké2ru ké2rú 2kérzeté ké2sá ké2so ké2sz1a2 ké2szá ké2szét
ké2s3zö ké2tan ké2tál ké2t1ele ké2t1em ké2t1esz ké2tev ké2t1é2 ké2tis ké2tí
ké2t1os ké2tö ké2tu ké2t1ü2 2k1év. 2k1évb 2k1é2vei ké2ves 2k1é2vi 2k1évn
ké2za ké2zá ké2z1e2l ké2zem kf2 k1fl kh2 k2hai k2hája 1k2hed khe2i k2hil
1k2hos k2hó 1khr 1k2hü 1ki kia2d kia2g kia2j kia2l kia2n kia2p kia2s kiá2z
kib2 ki1br 2kide kideg1 kie2l kie2m kie2re kié2h kié2r 2k1ifj ki1fr 2kiga
kigaz1 2k1i2gé kii2 ki1k2rá ki1k2ri kin2csá 2k1ind 2k1inf 2k1ink kin2tét
2k1inv kio2k kio2m kió2v ki1pla ki1ple ki1p2r ki2rat ki2rod ki2sab ki2s1ajtó
ki2sal ki2san kis1asz ki2sat ki2s1emb ki2sen ki2s1es ki2s1é2r ki2sis 2kisk
2kism ki2s1ok kiso2r ki2s1oro kis1p kis3sz ki1t2r kiu2 kiú2 1kí 2k1í2rá
2k1í2ró 2k1í2ve 2kí2z kjelenés1 kj1els k2k1ál kk1áru k2k1e2g k2k1in kk1ír
kk1ó2ra k1k2ri klá2má klá2mos kle2tin 1k2lí k2lub klu2bé klus3s kmá2nyú
kme2g 1ko ko1g2r 2k1o2koz 2k1okta kol1ajt 2k1olda ko2lim ko2naj ko2n1al
ko2n1ál ko2nor ko2nyé koo2 2kope ko1pro ko2r1ad ko2r1átl ko2rel ko2r1es
ko2r1il ko2r1osz kor1s2 kos3s 2koszl 2k1ou 2k1o2x 1kó kó2d1é kó1fr kóku2
2kóp. 2kópb 2kóph 2kópj 2kópk 2kópo 2kópp 2kóps 2kópu kóra2 kó2r1an kó2res
1kö kö2b1öl kö2dz 2k1öng 2köv. kö2zí 1kő kőe2 kőé2 kők2 kő1kr kőu2 k1p2
kpá2ra k2rip 1kripc krosz2k 1k2rómá krös3s k1s2h k1s2k k1s2p k1s2t2 k1sz2f
kszt2 kt1alk kta1p2 kte2rá k2t1i2r kto1g2 kto1s ktosz2 k1tré ktro1g ktro1s2
1ku 2k1udv 2ku2g 2kuj kul2csi kun2d1é ku1p2r ku2se 2kutaz 2k1u2tá ku2zs 1kú
2k1úg 2k1új kú2tá kú2t1o 2kútr 1kü 2küdü 2k1ü2g kü2la kü2lo kü2tö kü2tő
kü2tü kü2z 2k1üzl 1kű 1k2vad k2van kvés3s 1ky ky1ü 1la la2ar laá2 lab2la.
lac3há 2l1a2dag l1adl 2l1adm la2dod la2dog la2dom la2dóe la2dói la2dój
la2dól la2dór la2dóv la1dy lae2 laga2l lag1ala la2gan la2g1ál lag1ár.
la2gá2ra la2gép la2g1ér lag3gy la2gor la2g1osz lai2g la2i2re la2j1ad la2j1ág
la2j1ár. la2j1el la2jol la2jü la2kad la2k1a2n la2kel la2k1es la2kérte 2lakí
la2k1osz 2l1alg la2mad la2m1al la2map la2n1á2r la2n1ér lanye2 lany1er
lany1es la2nyé la2ny1í2r la2nyó laó2 la2p1at lap1áll lapá2r lap1ár. lap1áro
lap1áta la2p1el la2p1es la2p1ér la2p1in la2p1ir la2p1ír 2l1a2pó la2pö 2lapv
la2rán 2l1arc laro1 la2s1as las3sze la2su la2sz1ar laszkész1 lasz1óra
la2t1a2la lat1ará lata2t lat1ato la2t1árb la2t1e2g lat1ele la2t1ell la2tem
la2tep la2t1ék la2ting la2t1osz la2t1óri la2t1ö2v 2latti la2vu 2lazm la2zon
1lá lá2b1iz 2lábrá lá2but lá2d1al lá2dan lá2dor lá2d1ö lá2gal lág1g lá2g1ó2
lág1s lá2hor 2láí 2láldá 2láldo lá2l1e2 2l1állo lá2lö lá2man lá2m1eg lá2m1el
lá2mí lá2molv lám1osz lá2mu lána2 lá2nal lá2n1an lá2nas láná2 lá2nár lánc1c
lá2nyol lá2pol lár1akt lá2ral lá2rat 2láre lá2rö 2l1á2ru l1á2rú lá2sal
lá2s1e lá2s1ó2 lá2sut lá2sze lát1al 2láté 2lávi lá2z1e lbá2szá l1b2l lc1e2v
lc1e2z lc1é2v l2c3h lc1ív l2c1ó2 lcs1á2g l2cs1ál l2csáta l2csél lcs1ing
lcsó2r lc3sz ld1abr ld1a2ny lda1p lda2tál ld1áll ld1egy l2demel l2d1ep
ldes1s lde2tér l2d1igé l2d1i2p l2d1ín ldo2gas l2d1old ld1osz l2d1ó2r l2d1öl
ldt2 ld1tr l2d1ut l2d1út ld3zá 1le le2ab lea2k lea2v leá2p leá2zi 2l1edd.
l1eddi lee2s le1fr le3gali le2g1áll le3gáz le3ge. le3gek. le3gelőt lege2r
le3get legé2d le3gén leg3g le2gig le3görö legő2 le2gu le3gya le3gyá l1e2gye.
l1e2gyedne l1egyedü le3gyé le3gyi 2l1egyl le3gyö lei2g lei2s 2leji le2kad
leka2t le2kál le2k1á2r le2k1e2l le2k1e2m lek1e2rő le2k1id le2kő le1kri
2lektr le1kvi 2l1e2lemz 2lelev le2liz 2l1elz le2m1a2d 2leman. 2lemanj
2lemank lemec1 2l1e2mel. 2l1e2melé 2l1e2melh 2l1e2meli 2l1e2melj 2lemelk
le2mell 2l1e2meln 2l1e2melő le2melt 2l1e2melü 2l1e2melv le2m1éle 2lemés
lem1irá le2m1itt l1emleg le2mol le2m1osz le2m1ó2r le2mö 2lemu le2nál le2náz
le2n1e2g le2n1el le2ner lenés3s le2nir le2n1o2 len1tra 2lentrü l1e2nyész.
l1e2nyésze l1e2nyészé l1e2nyészi l1e2nyészne l1e2nyésző le2o1g2 leo2k le2oló
leó2v le1pla le1p2lo le1p2ré le1pri 2l1erd le2reg le2rő. le2rőb le2rők
le2rőn le2rőr 2lerőszakol. le2rőv ler1s2 le2s1es l1e2sése l1e2sésé l1e2sésü
2leskü le1smá lesp2 le1spr lest2 le1sto le1str le2szik 2l1eszm 2leszű
let1e2lem le2t1ell letes1s le2t1í2 le2t1ó2 let2tas 2lety leu2tá l1e2vez.
l1e2vezt 2lexer l1exp le2zer 2l1ezr 1lé 2l1ébr lé2c3h lé2gem l1é2gés 2légí
lé2g1o 2légt lé2gu lé2gyi lé2kal lé2k1any lé2k1eg lé2kesí lék1est lé2kép
lé2k1ir lé2kú l1éld lé2led lé2les lé2lén 2l1é2pí 2l1é2pül l1érd 2l1é2red
lé2r1el lé2rés lé2rik lé2rin lé2rit lé2r1osz 2l1é2rő. l1é2rők 2l1érté
2l1értü lé2rün l1érve 2lérz lé2sel lés3s 2l1észh léta1 lé2tál lé2te2l
lét1ele lét1elé lét1elü lé2tig lé2tö 2l1év. 2l1évb 2l1é2ven 2l1é2ves 2lévez
2l1é2véb 2lévi 2l1évs l1é2vü lfi2t1 lga1p2 l1g2ra lgy1as l2gyelőv l2gyis
l2gy1ol l2gy1ú2s 1li lia2d li2af li2ap li2at li2deg 2l1i2di li2dom 2l1i2dő
li2eu l1i2gaz li2ge. 2l1i2gé li1g2ra 2likk li1k2l 2lill li2nal 2lindí
2l1indu 2l1inf lin2ko li2nö 2l1inté li2o1g2 li2pő li1pro 2l1i2ram 2l1i2rat
li2rán l1irt 2l1isko 2lism l1ismé li1spo l1iste li2s1ü2 li2szö lis3zs
li2t1el li2t1old li2vad li2vás 2l1izg 2l1izmo 2l1i2zo 1lí l1íg 2l1í2ny
línyenc1 lí2ran 2lírá l1írh l1írj l1írn l1í2ró l1írt l1írv lí2tél lítés3s
2l1ív. l1í2vá 2l1ívs lízis1s lji2 l2j1ir lka2tel lke2ma lkis1a lkó2c3 l1k2v
lla2gal lla2m1el lla2nyá lla2ny1e lla2tal lla2tat lla2tel lla2tet lla2tor
l2l1azo l2l1áll l2l1á2p llá2rak llás3s l2l1edz l2l1elm lle2ma lle2m1á l2lemo
llen1n llé2ká llé2k1ol ll1í2rá ll1k2 l2l1ös ll1p2 lls2 ll1st l2l1üt l2ly
l3lyu lm1ajá lm1a2ny l2m1arc lmá2nyé lm1elm lm1e2lő lmfe2 l2m1o2p lm1s2
lmsz2 lná2ros lne2o lnia2 1lo 2lobá lo1dr loé2d lo2g1a2d lo2gál lo1g2rá
lo2g1ú lo2k1e2 lo2kos lo2laj lo2map lo2m1ál lom1á2ro lo2mec lomi2 lo2mid
lo2min lom1itt lo2mí lo2m1o2r lo2m1ó2 lona2 lo2nag lo2naj lon2c1i lo2ne
lo2pe l1org l1o2roz 2l1ors 2l1orv l1orz lo1sz2k 2l1oszl lo1t2h lo2u2 lo2xá
lo2xi lo1y 1ló lói2d 2l1ónn ló1p2 2lórab 2lórm ló1t2h ló1t2r ló2ze 1lö
lö2kel lök1o lö2m lö2n1á lö2n1é2 lön3n lö2nó lö2pé 2lö2r lö2sü l1ötl 1lő
2lőbbi 2lőbbr 2lőd. 2lőhív lői2d lőí2 2lőkel 2lőnn 2lőny. 2lőnyé 2lőnyh
2lőnyö 2lőnyr 2lőnyt 2lőnyü 2l1őrl 2l1ő2rö 2l1ő2rü lő2s1égg lős3s lő1szt
2lőté2t1 2lőtol lőu2 2lővés 2lővét l2p1a2láv lpen1 lp2h l1phe lp2lá lpo2ka
lpo2n lpon1á lp2rak l1p2re l1p2ri l1p2ro l1p2ró lreá2 lsá2r l1s2k l1s2l
l1s2m lsőü2 l1s2p l1s2r lst2 ls2ta ls2ti lsus3s lsza2ké l1sz2f l1sz2l lta2d
lt1ada lt1a2gy lt1ajt lta2l1e2 lta2l1é l2tarc l2t1a2u lt1ág. lt1á2ga lt1ápo
l2t1árf lt1áta ltá2v1i2 ltés1s lt1iva lt1í2v ltornác1 lt1ö2vi lt1őr. l1t2ri
l2t1u2t 1lu lu2de 2ludj 2ludn 2ludt 2ludv l1u2go l1ugr lu2le 2luli 2lulr
lu2m1é 2lumí lu2rak 2l1u2ru lu2se lu2sza lu1t2h 1lú lú2d1a lú2gá 2l1ú2j
2l1ú2sz lú2té 2l1úth lú2ti 2l1útj lú2to 1lü 2lüd 2l1ü2g l1üldö lü2l1em
lü2l1é 2l1ülhe 2l1üljö 2l1ülne 2l1ülni 2l1ü2lő 2l1ült. 2l1ültem 2l1ültet.
2l1ültett l1ülté lü2lü 2l1ü2r lü2té lü2ti lü2tö lü2tő lü2tü 2l1ü2v 2l1ü2z
1lű l1ű2z lv1alk lvás3s l2v1e2le l2v1előt l2v1els l2v1ember l2v1ép l2v1érzé
l2v1észl l2v1isk lvkész1 l2v1ok l2v1os l2v1öl l2v1ő l2v1ú l2y 1lya lya2gya
ly1akc ly1a2la ly1alk lya2m1é lyan3n ly1a2rá ly1arc 2ly1ass ly1aut 2lya2z
ly1azo 1lyá ly1áll ly1árn ly1á2ro 2ly1áru 1lye 2ly1e2c lye2gi lye2l ly1elm
2ly1elv lyenü2 ly1e2r 1lyé 2ly1ég 2ly1ép 2ly1érd 2ly1érte 2ly1érté 2ly1érv
2ly1érz 1lyi 2ly1id 2ly1i2gé ly1ill 2ly1im ly1int 2ly1irá 2ly1iro 2lyism
2ly1i2sz 1lyí 1lyo 2lyodú 2lyokoz 2ly1or 1lyó 1lyö ly1ös 2ly1ő2 lyt1áll 1lyu
2ly1ud 2ly1uj 2ly1ur 1lyú ly1úr. 2ly1út 1lyü ly1ü2v 2ly1üz 1lyű lzus1s 1ma
2m1abl 2m1a2cé ma2dé madi2a mad1osz ma2dóm ma2es ma2g1al magas1s ma2g1en
ma2gor ma2gö ma2gyú ma2io 2maja 2m1ajt 2m1akc 2makó ma2kón ma2kós makro1
2m1a2lak ma2lap 2m1alk mal1t ma2lu 2malv ma2nat maó2 ma2pas 2m1apj ma1p2r
ma2s1i2k mast2 ma1str ma2szö mat1á2ru ma2t1e2l ma2tig mat1int ma2t1ir 2matő
ma2va ma2z1e 1má 2m1ábr 2m1ág. 2m1á2ga 2m1á2gú 2m1á2gy má2j1o2 mála2 má2l1al
má2lál má2l1e má2l1ér m1állí má2lu má2m1as má2mu má2ne má2nin má2nyal
má2nyaz má2nye mányi2 má2ny1ir má2nyí má2ny1út m1á2p 2m1á2rad m1á2rak má2ru
2m1á2rú má2se má2sir más3zs 2m1á2t1a2 2m1átm 2m1átv mbak2 mb1á2gy m2b1e2p
mbe2r1a mbe2rep mbe2r1és m2b1ing mb2lo mb1ús m1d2 mda1p2 1me meá2r 2m1e2bé
me2cet me2dén me2g1ala me2gan me2gác me2gát me2g1á2z me2g1ec me2g1el me2g1em
me2g1e2r meg1esk meg1esn me2g1ette me2g1etté me2g1ettü me2g1éd me2g1é2l
me2gép megi2 me2gih me2gil me2g1ir me2giv me2giz me2g1í2 me2g1o2 me2g1ó2
me2g1ö2 megő2 meg1s me2g1u2 me2g1ü2l 2m1egys me2k1ar me2k1á2 me2kél me2k1és
me2k1id me2k1ir me2kis me2kí mek1k2 me2kor me2k1os me2k1ot me2köl 2meled
2melet 2melk mel2l1é2rü mel2l1u 2m1elmé 2m1elnö me2lőa m1e2lőá melőe2 2m1elr
m1elter 2m1emel men2tan me2nya me2op 2m1e2p 2m1e2rej 2mernyő me2rőm me2rőv
2m1e2rű me2s1er me2s1o2 me2su me2s1ú 2m1eszm met1ell me2t1or me2t1ó2r me2tú
me2vő me2zár me2zin me2zor me2zö me2zs 1mé 2m1ébr mé2g még1a még1i mé2k1á2
méke2l mék1ell mék1elő mé2kin 2m1élm mé2ly1a mé2lyá mé2lyig mé2ny1el mé2nyir
2m1é2p mér2v1a 2m1érz mé2sá mé2s1er mé2szé mé2szo mé2tap mé2t1em mé2té
2m1év. mé2ven mé2ves 2m1é2vi 2m1é2vü mé2zil mé2z1o mé2zs mf2rá mg2 m1gr
mgubanc1 1mi mi2ac mia2d mi2aj mia2l mi2ap mia2r mi2deá mi2deg m1i2dé 2midő
mie2lőt 2m1i2ga migaz1 2m1i2ge 2m1i2gé 2m1i2má m1imp min2ce min2ch min2da
min2der mi2ol mi2ono mi2onr mió2t 2m1i2pa 2m1i2ro 2mirt mis1eme mita2
mit1ár. mi2tu mi2zom 1mí m1í2rá mí2ró m2j1ol m1k2 mla2u mlás1s mlé2k1a2
mlé2kes mma2gá mme2g 1mo mo1g2rá mo2ir moki2 mo2kó 2m1oktá mo2k1ú mo2laj
2molvas mo2lyi mo2ne. mo2né. mo2nü mo2or mo1p2l mo2ren mo2rég mo2rid 2morie
mország1 mo2sar mo2se most2 mosz2fé mo2szim 2moszl mosz1th 2m1o2x 1mó
2m1ó2rá mó1th 1mö 2m1öld 2m1önt 2m1öv 1mő mőe2 mő2rá mp2he mp2hi mpor2t1á2r
m1p2resszi m1p2ró m1q mst2 m1str msza2ké msz2c m1sz2f mszt2 mte2o mtes2s
m1t2r 1mu 2mule mu2se mus3s mu2t1a2g mu2tár 1mú mú2g múgy1 2m1új mú2t m1úth
2m1úto 1mü 2müg mü2gy mü2ka 2m1ünn 2mü2r 2m1üs 2m1ü2v mü2z 1mű műi2 műk2
mű1kr mű1st 2műz mű2zé mű2zi mű2zö mű2ző mvas3s mze2t1á mze2tel mze2tő 1na
n1abbó 2n1abl 2n1a2cé 2n1a2dó na1dró n1adt nae2g na2ge 2n1a2gi nagy1ap
nagy1as na2gyer na2gyor na2gyó na2gy1ú na2ig nai2s 2n1a2kad. 2n1a2kadá
2n1a2kadh 2n1a2kadnak 2n1a2kado 2n1akadó 2n1a2kadt naki2á na2kőr nakü2
2n1a2lap na2lid 2nalí n1alj. 2n1ann na2nya nao2 naó2 na2pák nap1áll na2p1el
na2pen na2p1er na2p1ó2r napu2 na2pud na2put na2tab na2tel na2t1ér na2t1osz
2n1a2ty na2uk na2us na2uto na2zé na2zo 1ná ná2ch ná2cse ná2csü ná2d1e2
2n1á2g ná2lab 2n1álar nále2 n1állá 2n1állo ná2mí ná2nad nán2c1 n1á2ram
ná2r1e2 2nárui ná2rú 2n1árv ná2sü ná2szat ná2sze ná2s3zö ná2szú ná2szü
ná2tal n1átf ná2tí 2n1átl 2n1átm nba2l1 n1b2l n2c1a2g n2c1ann nc1asz
n2c1á2ro n2cáru nc3csö n2c1ép n2c3ha n2c3há nc3het n2c3hé nc3hoz nc3hu
nci2alis nc1ing nc1i2r nc1ork n2c1ó2 n2csab n2csáru ncs1árv ncs1elc n2csérte
n2cs1íz n2csosz ncs1őr n2csur nd1arr nda2tal nda2tel n2d1a2z n2d1eb n2d1edd
nd1egy n2deh nd1ekö nde2ná nde2n1ev n2d1é2l n2d1ész. nd1éti nd1ink ndme2
n2dőrá n2d1őrn n2d1ő2ro nd1őrö n1d2ram n1dy nd3zav n2d3zó 1ne nea2k ne2áp
2n1egy. n1egyf n1e2gyi n1egyk 2n1egyn n1egys ne2k1eg ne2k1ó2 ne1kri n1elk
2n1e2lőa ne2lők ne2lőt 2n1elv ne2má 2n1ember ne2m1eg nemes1s ne2més nem1id
nem1iss n1eng n1enn ne2of ne2oli ne2olo ne2oló ne2or ne2os ne2ot neó2r n1erd
2n1e2red 2n1e2rő ne2sá ne2sem ne2setr ne2so 2n1e2ső nesü2 ne2süv nes3zs
2n1e2szű ne2t1eg ne2tír ne2tok ne2tol ne2t1öl ne2vés. 2n1e2vő ne2zer 2nezüs
1né 2n1ég. 2nége né2get 2négő né2gyá né2lű né2p1a2 né2pá né2peg né2p1e2l
2n1é2pítk né2p1o né2pu 2n1érc 2n1érd né2ren 2n1é2ré 2n1érh 2n1érm né2rő.
2n1érte 2n1érté 2n1érv 2n1érz né2szá né2szeg 2n1étk néva2 néve2l név1elő
né2ves né2vü 2névz nfe2li nfüs2 nfüst1 nfüstö2 ng1a2dá ng1a2dó n2g1a2la
n2g1alj n2g1ág n2g1árn ngás1s n2g1e2g ng1elm nge2rá ng1érté n2g1id ngi2g
ng1iga ng1igé n2g1ing n2g1int ng1ír ngo1szk n2g1ön ng1ö2z ng1ug ng1üz n2g1ű
n2gy1em n2gyél n2gyis n2gyí nhá2zig nhe2i nhé2t1 1ni ni2ad ni2ah ni2am
2n1i2de ni2dom 2n1i2dő nie2l ni1f2 2n1i2ga 2n1i2gá 2nigé 2n1i2ly 2n1imp
2n1ind 2n1inj 2n1inté ni2pa ni1p2r ni2rat 2n1i2rá 2n1irg 2n1i2ro 2n1isk
ni2s1ü2 ni2sza ni2tal 1ní 2n1íg ní2ra 2n1í2rá 2n1í2ró 2n1í2tél nka1b nk1adós
nk1ajá n2k1alk nk1a2ut nk1áll nkás3s n2kátu nkci2ósz n2k1ell nké2pel nki2g
nk1iga n2k1ing nk1inté nki2s1a nk1k2 nk1old n2k1ö2lé nk2rit nkron1n nk1uz
nk1üz nlac1 nla2kos nlo2n1 nme2g1 nmono2x nn1áll nn1ége n2n1or nn1ug n2n1ú
nnü2 nn1ülő n2ny n3nyalá nny1a2n nny1ár nny1á2z n3nyil n3nyol nny1öz n3nyu
n3nyú 1no no1ch n1off 2nogn no1g2rá 2nogt no2kar no2kas noke2 no2ker no2k1ir
no2kö no1kro 2n1okta no2k1ú 2n1o2la 2n1o2ly no2mér no2mol nom1p non2c3
no2n1in no2o 2n1o2pe no1p2h no1p2la n1ord 2nore n1org no1szkó 2n1oszl nosz3s
1nó nó2dár nó2rák n1ó2rán nó2rí nósz2 nó2s3zen 1nö nö2l 2n1ö2r n1össz nö2t1e
nö2ti 1nő nőe2 nő2ir 2n1őr. 2n1ő2ré 2n1ő2ri 2n1őrk 2n1őrn 2n1ő2rö 2n1őrs
2n1őrt 2n1őrz n1p2 n1r n2s1akk n2s1a2l ns1e2le ns1elt ns1ív ns3s n1s2tab
nste2 ns2tei n1stein. n1steine ns1ur ns1úr n1sy nsza2ké nszeng1 nsz2fé
n2t1akk ntan2t1 ntap2 n2t1a2rá n2t1ark nt1aut nt1azo n2táld n2t1áll n2t1árf
n2t1áru nt1átl nt1e2dé n2t1egy n2t1ela nt1elr nte2m n2t1emb n2t1eme nte2r1a
nte2sz nt1esze nt1eva nté2ké n2t1é2le n2t1érté nt1érz ntés1s nt1étke n1t2hu
nti1k2l nti1p2 nt1irá nt1izo nt1írá nt1írt n2t1íz nto1g2 nt1o2ly nt1ó2ri
n2t1ö2v n2t1ug ntus3s n2t1ü2s n2t1üt ntya2 n2ty1al 1nu 2nud 2nug nu2go
nu2s1ol 2n1u2tó 1nú 2n1úg 2n1úté 1nü 2n1ü2g 2n1üld n1üss 2n1ü2te nü2ti nü2tö
n1ü2tő 2n1ü2z 1nű nvers1 n2y 1nya 2nyabl 2nyadó 2nyaga 2nyagá 2nyagb 2nyagh
2nyagn nya2gos 2nyagr 2nyagy nya2k1ék nya2kér nya2lap 2nyalm 2ny1ant nya2nyá
ny1apa 2ny1arc ny1aut 1nyá 2nyág ny1á2lo 2nyámh 2nyámn 2nyáp 2ny1á2rat
2nyárp 2nyáru 2nyásó 2nyáta 1nye 2ny1e2gy 2nyelc ny1elny ny1elo ny1előh
2ny1előn nyel2vá 2nyemel 2ny1ene nyereg1 2nyeső 1nyé 2nyéhes ny1éle 2ny1ép
2nyészőe 2nyév. 2nyévb 2nyévek 2nyévet 2nyévn 2nyévr 2nyévv 1nyi 2nyid
2ny1iga 2ny1ill 2nyimá 2nyind 2nyinf 2ny1int 2ny1i2p 2nyirt 2nyisk 2nyism
2ny1ist 2ny1iz 1nyí 2nyív ny1k2 nylo2n1 1nyo 2nyokoz 3nyom ny1op ny1ors
2ny1ott 1nyó 1nyö ny1öl 1nyő ny1p2 nys2 3nys. 1nyst. 1nyu ny1ud 2ny1uj
2nyuno 2nyus 2ny1u2t 1nyú 2nyújs ny1úr. ny1ú2sz 2nyútj 2nyútt 1nyü 2ny1üg
2nyüld ny1ür 2ny1üt 1nyű 2nyűr ny1űr. 2nyűz ny1ű2zé ny2van n2z1ak n2z1a2ny
n2z1a2p nz1á2ru nz1e2dé n2z1eg n2z1elf n2z1ell n2z1e2lő n2z1ember nz1e2més
nze2s nz1eszk nz1éhe nz1ére n2z1érm n2z1érő n2z1i2p nz1k2 n2z1ön nz1ös nz1p2
nz3ság nz3sár nz3seg n2z3só nz1ü2g nz1üz nz3z 2o. 2o1a oa2cé oa2n oan1e o1á
2oba obai2 obás3s ob2bol ob2bö 2obi obi2lan obi2o 2obo obo2ra o2b1ü2 2oca
2oce oc1e2m 2océ oc2k1é2 2ocö o2csí o2csú oc3sz o2c3z oda1p 1o2dáz. 1o2dázh
1o2dázn 1o2dázó 1o2dázt 1o2dázzu 2ode 2odé od1éve o2d1í2 1o2dú 2o1e o2ei
oe2l oe2m o2er o2es oe1t2 oe2v 2o1é oé2dere o2ég oé2l 2ofa 2ofe 2ofé o1fl
2ofo 2ofó o1f2ri og1a2la o2g1alt o2g1ap og1assz o2g1asz oga2t1e2 2ogá og1átk
og1átr 2oge o2g1e2d og1e2g o2g1e2l o2g1em oge2o og1ere oge2s o2g1ez o2g1él
og1érv o2g1id o2g1i2g og1int o2g1i2p og1ir o1g2lic og2lób o1g2nai 2ogó o2g1ö
2o1g2raf 2ográ o2g1u2t o2g1ü2 2ogy ohó2cs o1i o2ib o2ih o2inte oi2o 2oiz
oi2zo o1í 2ojá 2oje ok1ajá o2k1a2la ok1alj ok1a2ra ok1arc ok1ass o2k1a2to
okás1s 2oke oke2d ok1edé ok1e2g o2k1e2m ok1ere ok1erő o2k1es ok1e2v ok1ez
2oké ok1é2ne o2k1ér. o2k1érc o2k1érte o2k1éve ok2hi o2k1i2d ok1i2rá ok1ist
ok1izm o2kí ok1ív ok1íz ok2lim ok2lor okon1n o2k1osz ok1ó2r o2k1öb o2k1ö2l
okö2r ok2ris ok2rom 2oku o2k1u2s o2k1ü2 ola1d ol1ada 1olajf o2lajt olaszó2
ola2szór 2olat ola2tol ol1áll olás1s ol2c1e ol2cé ol2csin ol2c3sor ol2dap
ol2dál ol2d1is ol2d1ud ol2d1ü 2ole o2l1e2l o2l1é2r ol2f1ö ol2fü o2l1il
ol1ind 2olit olki2 ol2l1ő ol1ó2rá ol1p2 ol2taj ol2tág olte2 ol2t1el ol2t1em
ol2t1ér ol2tí ol2tőr ol2t1ü o2l1ug 1olvad 1olvas oly1agy o2lyaj o2lyál
o2ly1ü2 o2m1a2dó o2m1adt om1agy o2m1a2j o2m1akn om1akó o2m1a2l o2m1ana
o2m1ann om1app o2m1ar om1atom o2m1árb o2m1áru omás3s 2omb om2bág om2ber 2ome
om1ece o2m1edé o2meg om1egy ome2l o2m1ele o2m1ell o2m1elm o2m1elt o2m1e2m
om1ene ome2o1 omeosz2 o2m1e2rő o2m1e2v o2m1e2z 2oméd om1érc o2m1érte om1éve
om1ide o2m1il om1ind o2m1ip o2m1i2r o2m1ism om1izo om1ír om1í2v omog2
o2m1ola o2m1old o2m1op 2omor o2m1ö2 o2m1ő2 om1p2lo om1p2re oms2 om1st o2m1ud
o2m1uj o2m1ü2 2omű om1űz on1agá on1ajt ona2le on1a2va o2n1átr on1átv on2cas
on2c1é2 onc3sz on2d1ó2r o2n1e2g o2nek one2l on1ele on1elő one2r on1ern
o2n1ég on1éhe o2n1é2l on1éne o2n1é2p on1év. on1éve ong1asz oni2g on1igé
o2n1ip on1ism on1izz onkás1 on3nye on3nyo o2n1old on1opt ono2szi o2n1ö2
o2n1ő on2t1aj ont1á2ron on2tát on2ted on2t1ér on2t1ös on1t2ri on1ud on1ug
o2n1ut onú2 o2núj on1ús 2onü on1ü2l on1üt on1ü2v ony1any o2ny1ál ony1em
o2nyer ony1es ony1ég o2ny1ir o2ny1it o2ny1ó2 o2nyö o2ny1ő2 2onysiosá on2zed
on2z1es onzé2 on2zér on2zsá 2o1o o2ob o2of o2og o2ol oo2r o2ot oo2x o2ó o1ö2
o1ő 2opa op1e2g 2oph2 op2his 2opl op2le 2opo 2opr op2rod 2ops opsz2t 2opü1
or1abl or1ala or1alj or1alm ora2n o2r1ana o2r1ant or1any ora1p or1arc or1atl
o2r1á2g or1á2rak or1áru or2cső or2dö or2dú o2r1e2d or1elm or1eln or1elo
or1e2lő or1els o2r1e2m o2r1eng or1eny o2r1e2r ore2s or1eső or1é2gé o2r1é2l
or1é2ne o2r1ép 2orgia orgi2ai 2orgiá or1i2de 1o2rie o2riga o2r1ing or1i2pa
o2r1is. or1isp or1iss or1iste ori2sz o2r1ír o2ríz or1kr or2ne. o2r1or or1ó2r
o2r1ö or1pl or1pr or2r1e2v orse2 or2sel or2ses or2set orsé2 or2s1ég or2s1ér
or2sét or2sis ors3s 1ország. or2t1a2d or2táru or2teg ort1elm or2t1em ort1esz
ort1ért ort1ing or2t1osz or2t1ös o2r1ud o2r1ug or1u2r o2r1u2t o2r1ü o1ry
orz1z o2s1ac o2s1a2la o2s1alk osa2t os1aty o2s1áf os1áll ose2 os1ed os1eg
os1el os1emb os1en os1er os1esz o2s1ép o2s1id os1ina os1int o2sir o2s1í2r
os1k oski2 osme2 os1p2 ossz1es os3szék os3szö os3sző os2t1any os1the 1ostro
1ostya o2s1uj o2s1ut os1úr. os1út o2s1ü2 osz1alk o2sz1e2l o1sz2fe 2o1szkl
2oszkóp. 2oszkópk 2oszkópo 2oszkópp osz1ors osz1ó2r o2s3zö o2s3ző o2szut
o2szű ota1g o2t1a2u o2t1ág ot1á2rak 2ote o2t1e2g 2otéka o2t1érté o1t2he
ot2hi 2otí oto1g2 oto1p otosz2f otot2 otókész1 otó2pa o2t1ó2sá 2otro ot2tar
ot2tég o2t1ü2 o1u o2uc o2un o2ur o2us o2ut ouv2 o1ú o1ü o1ű ovas3s owat2
o2wi 1oxido o1ya o1ye oy1s2 oza2g1 oza2tí ozatkész1 oza2t1ol ozás3s oz1é2p
o2z1il ozi1sz2 oz3sz 2ó. ó1a óa2c 2óa2d óa2g óa2j 2óak 2óa2l 2óa2m óa2n
2óa2r 2óa2u óa2v 2óa2z 2ó1á óá2c óá2gak óá2l óá2p óá2r óá2t1 2óbar óba1s2
2óbec 2óbef 2óbem 2óbes 2óbé 2óbil 2óbio 2óbir 2óbit óbo2rá ó2c1an óc3c
óc3ho 2óci 2ócí óc3ság ó2csár óc3sip óc3sz 2ódar ó2d1ál ódá2r ód1ára ódás1s
2óde ó2d1id ód1isk 2ódí ó2d1ír ód1öl ód2res ód2rót ó1dy ód3zs 2ó1e2 óel1a2
ó1é óé2g óé2l óé2ne óé2p óé2ré óé2ri óé2rő 2óérté 2óérz óé2te óé2ve 2ófa.
2ófae 2ófá 2ófi 2ófo 2ófr óf2ri ófus1s 2ófü 2óg ógi2as ógo2r1as ógy1a2n
ógy1el ógy1es ógy1ér ógy1in ó2gyü 2óhá 2óhé 2óhi 2óhí 2óhor 2óhő 2óhu ó1i
2ói2de 2ói2dő ói2g 2óigé ói2má 2óing 2óint 2ói2p ói2rat 2ói2rá ói2ro 2ói2s
ói2ta ói2z 2ó1í2 2ójáté 2óje 2ókap 2ókate ók1áll ók1áru ó2k1eg 2ókel 2óker
2ókez 2ókény 2óképe 2óképl 2óképn 2óképt 2óképz 2ókéré 2ókérő 2ókés ó1k2hi
2ókie 2ókine ók1int 2ókiny ók1iro 2ókomm 2ókoo 2ókö ó1kraj ó1k2ré ók2ri
ó1k2ro ó2kum 2ókut 2ókül 2óküs ó2k1üz ól1aj ó2l1a2r ólás3s 1ólb 2óle
ó2l1e2se ó2l1e2si ó2l1ér óli2óé ólo2m1e ólo2mé ól1öl ólu2mi 2ómag 2óme
ómen1k2 2ómére 2óméré 2ómérn 2ómét 2ómi 2ómor 2ómoz 2ómó 2ómű 2ónag óna2ké
óna2kü ó2n1al ó2n1áll ón1e2d 2óné ón1o2x 2ónö ón1ön ón1öt ónus3s ó2n1ut
ó2n1ü2 2ónyil 2ó1o2 ó1ó2 2ó1ö2 ó1ő 2ópa óp1a2rá 2ópá ó1p2l ó1p2ré ó1p2ro
ó1p2ró ó2raa 1ó2rab 1ó2ral 2órád 1ó2ráé órá2g ór1ágy 1ó2ráh 1ó2rár 1ó2ráv
2óre ó2r1e2g ó2r1e2l óre2s ór1ese ó2r1e2ti ó2r1é2l ó2r1i2o ó2r1ism ór1ír
2óro ó2r1o2ki ó2r1o2ko 2órö ó2s1as ó2s1ál 2óse ó2s1e2g ós1emb ó2s1id ó2s1i2n
ós1isk ós1k óskész1 2ósor ó2s1os ósó2 ós1ór ós1p2 2óspe ós2pek ós2por óst2
ó1s2tí 2ó1str 2ósu ó2s1ur ó2s1úr ósü2l 2ósű 2ósza ós3zac 2ószá 2ószel 2ószem
2ószen 2ószé 2ószi 2ószí ósz2láv. 2ószol 2ószó ószóza2 2ószö ós3zs ósz1th
ószt2r ó1sz2v ót1a2d 2ótag 2ótar ót1áll 2ótár 2ótáv ótá2v1i2 ótá2v1í2 2ótec
ót1e2g ót1ej 2ótel 2óten 2óter 2ótén ót1il ót1ing 2ótí ót1í2v ótme2g1 ót1orr
ó2t1ors 2ótov 2ótö ót2ré óttűz1 2ótu ót1u2t 2ótü ó1u2 ó1ú2 2ó1ü2 ó1ű ó2vat
2óvál 2óve 2óvé 1óvh 2óvi óv1in 1óvj 1óvn 1ó2vod 1óvt 2óza óza1s óza2tak
óza2t1al óza2t1e óz1e2m ó2z1é2 ózi2a ó2z1i2n ó2z1u2r ó2z1ú2 óz3zs 2ö. ö1a
ö1á ö2b1á2 öb2b1a2 öb2bá öb2b1ez öb2b1is öb2b1o öb2bó ö2b1eg ö2b1e2l ö2b1e2n
ö2b1ért öbme2 ö2csé ö2d1a2 ö2d1é2v ödfé2 ö2d1is ö2d1í ö2d1o ö2d1ó2 öd3zs
öd3zu ö1e ö1é ö2g1a2 ö2g1elf ö2g1esz ö2g1ev ögés3s ög3g ö2g1id ö2g1im ö2g1in
ö2g1o ö2g1u2 ö1i ö1í ö2k1a2 ö2k1á ök1elh ö2k1e2m ö2k1érté ökés3s ök1ész öki2
ö2k1id ö2k1if ö2k1im ök1ív ök1íz ökma2 ö2ko ökös3s 1ökrö öksza2 ökü2l
ö2k1ülé ö2l1a2 ö2l1á öl2csá öl2cs1í öl2cso öl2dab öl2d1a2l öl2dan öl2d1as
öl2d1á2 öl2d1éh öld1ing öl2d1is öl2dos öl2dz öl1e2b öl1egy ö2lel ö2le2m
öl1eme öl1emé öl1eml öl1eng ö2l1e2r ö2l1esz ö2l1e2v ö2l1é2g ölé2k öl1éke
ö2l1é2l ölé2nyeg ö2l1é2p ö2l1ér. ö2l1é2rek ö2l1érh ö2l1érj ö2l1érn ö2lérő
ö2l1ért öl1f2 öl2gya öl2gyá ö2l1id ö2l1ij ö2l1il ö2l1i2r ö2l1is ö2l1iz
öl1í2r ö2l1o2 ö2l1öl ölös3s ö2l1ö2v öl1p2 ö2l1u2 ö2lú ö2l1ült ö2l1üt 1ölyv
ö2m1a2 ö2m1á2 2ömb öm2b1a2 öm2b1á ömbé2 öm2bú ö2m1eb öme2g1a ömeg1g ö2m1el
ö2m1él ö2m1it ö2m1o2 öm1p2 ömpin2 ö2m1u ö2n1a2 ön1ám ön1á2p ön1át 2önc ön2ci
ön1e2g ö2n1e2l ö2n1em ön1e2r ön1es ön1e2v ö2n1é2l ön1é2p ön1ész ö2név ön1év.
ön2gyá öni2 ön1ig ön1im ön1in ön1ir ön1is ö2n1ír önké2r1e ön2n1a2 ön2n1á
ön1or ön1os ön1ó2r ön1ő2r öntös3s ö2n1u2 ö2n1ú ön1üt ö2ny1a2 ö2ny1o öny2v1a2
öny2vár önyves1 öny2v1ég ö1o ö1ó ö1ö ö1ő öp1ép öpi2 öp1ir ö2r1a2 ö2r1á2 2örb
2örc 2ördí 2ördü 1ö2reg öre2ga öre2gu ö2r1el ö2r1er ö2rég ö2r1é2l ör1é2ri
örés1s örfö2l ör2f1ölé 2örg öri2g ö2r1iga ö2r1in ör1i2p ö2r1ir ö2r1is ör1i2v
ö2r1ír ö2r1ív 2örn ör2ny1a2 ö2r1o 2örög örös3s örösz2 ör2t1a2l ör2t1í2
ö2r1u2 ö2r1ú2 1örv. ör2zs1a ör2zs1á ö2s1a2 ö2s1á2 ö2s1el ö2s1em öses3s
ö2s1ez ösi2 ö2s1in ö2s1iz ös1k ö2s1o2 ö2s1ó2 ös1őr össz1ál ös3szí ö2s1üg
ös1ü2v öszt1an ösz2t1el öt1a öt1á2 öt1e2m öte2z öt1eze öt1ér öt1é2v 1ötf
2öth 1ötk 1ötm öt1ó2 ö2tön ö2tös öt2t1á2 öt1u ötü2 öt1ül ötve2g ötvé2nyé ö1u
ö1ú ö1ü ö1ű 1öv. öveg1g övis1s 1övn 1övv ö2z1a2 ö2z1á2 ö2z1eb ö2z1egés
ö2zellá ö2z1em ö2z1er ö2z1e2s öz1e2v öz1élel öz1é2let. özé2pem öz1é2pí
ö2z1érd öz1étk ö2z1i2g ö2z1int ö2z1i2r öz1ír özme2 ö2z1o2 özok1 ö2z1ó2 öz1p2
ö2z3s ö2z1u2 ö2z1ú2 öz1ü2g öz1ü2z ö1zy 2ő. ő1a őa2c őa2dan őa2dati őa2dá
őa2dok őa2dot őa2dó őadókész1 őa2g őa2j őa2k őa2l őa2n őa2p őa2r őa2t őa2u
őa2z ő1á2 őát1 őba2l1 őb2bis őb2bu ő1b2l ő1b2r ő2d1el ődés3s ő2d1in őd2ró
2ő1e őe2c őe2d őe2g őe2le őel1o őe2lőtte őe2n őe2p őe2re őe2rőb őe2rői
őe2rők őe2rőne őe2rőnk. őe2rőr őe2rős őe2rőt őe2rőv őe2s ő1é őé2g őé2l őé2p
őé2ri őé2te ő1f2r őga2z1 őge2o ő1i ői2dea ői2deá ői2deg ői2dé ői2dő ői2g
ői2ma ői2má ői2na ői2p ői2ra ői2ro ő2isz ői2ta ői2va ői2z ő1í őí2r őí2v
őjob2 ő1k2ré ő1o2 ő1ó2 ő1ö2 ő1ő2 ő1p2 őpo2ra őr1al őr1at 1ő2r1a2u 1ő2r1ál
őr1á2p őr1ár őr1át őr1e2ge őr1e2gé őrei2g őr1elv őrendő2 ő2r1é2l őr1iga
1ő2ril őr1ing ő2r1i2p ő2r1ir ő2r1o2l őron2g 1ő2r1or őr1osz 1őrör őrös1s
ő2r1öv 1ő2r1őr őr1p2 1őrse őrt2 1őrti 1őr1tr őr1u2t ő2rül ő2s1ad ős1a2g
ő2s1ak ő2s1a2l ős1a2n ős1ará ősa2v ő2s1ál ős1egy ős1e2l ő2s1em ő2s1ep ős1erd
őse2s ős1ese ő2sib ő2s1id ős1iz ős2kál ős1okt ős1p2 ős2pe őssz2 ős3szl
ős3szü őst2 ő1str ő2s1ür ősz1e2lő őszt2 őt1áll őt2ra őt2ri őt2t1o ő1u őu2r
ő1ú őú2r őú2s ő1ü őü2g őü2r őü2t őü2v őü2z ő1ű őz1a2g őz1elo ő2z1er őz3sug
ő2z3sü őz3sz őzü2 őz1üz 1pa paá2 pa2ce pa2ch pa2dag pa2de pae2 pa1g2n pa2kad
pa2lap 2p1alj pa2m1as pa2mur pa2nal p1a2nya pa2p1il par1ágá 2parb pa2r1el
par1isk pa2r1ok 2parr par2t1a2l par2tol pa2t1eg pat1ért pa2ul. pa2x 2p1axi
1pá pá2csü pá2ga pá2gy p1állí p1álló pá2mí 2pámn pá2mu pána2 pá2n1am pá2n1e
pá2ny1át pá2po 2páram pá2r1ato pá2r1e2 pár2tak pár2tál pár2tár pár2t1e
pár2tér pá2rus pá2sir pá2t1a 2p1átm pá2t1uk pba2l1 pci2ókér pd2 p1dr 1pe
pea2 2peci pe2dén pegész1 pe2is p1elemz 2p1elk 2p1elm 2pelo pe2l1os 2p1e2lőá
2p1eltér 2p1ember 2p1e2mel pe2mó 2pene pen3ny pe2no pe2nya pe2po per1akt
pe2r1él pe2rid per1int pe2r1os pe2rox pe2ró pe2rö 2p1e2rő pe2rú pe2sú
pe2sz1á pe2szu pe2sz1ü2 2p1e2v 1pé 2p1é2g 2p1é2je péki2 pé2kü 2p1é2lé 2p1élm
2p1élr 2p1é2neke 2p1é2pí pé2pü 2p1érd 2p1érv 2p1érz pé2s1e2l pé2sz1á2 pé2szü
2p1év. 2p1évv pfe2li p1f2ri p1f2ro p2hem p2hiá 1phila p2hiu 1p2hok 1p2hó 1pi
pi2ad 2picl pi2den pi2deo 2p1i2dő pi2eg pier2re. 2p1i2ga pigaz1 2pige
2p1i2gé 2p1i2ly 2p1i2má pin2g1a ping1ár pi2óh pi2ós pi2rat pi2rod 2p1ism
pi1t2h 2pizz 1pí 2p1íg pí2r1a2 pí2rá pí2rol 2p1í2ró p1í2tél pki2s1 plas2
1play. ple2is p2lö plüs2 plüss1 pno1g2 1po po2csiz po1gra po1g2rá 2p1okm
po2laj pol2c3s 2p1old 2p1oml pon2gas pon2t1e 2ponz 2po2pe po2p1é2 po1pl
pop1s por1adó po2res por1s por2t1el por2tes por2tér por2tin por2tü po2se
po2s1é po1szf 2p1oszl po2t1el 1pó póka2 pó2k1ag pó2rák pó2rán pó2rát pós3s
pó2t1a2 pó2tá pó2t1e2 pó2t1é2 pó2ti pó2tor pó2tu pó2t1ü2 1pö 1pő pő1kr
2p1őr. pp1akk pp1ekk ppko2 ppo2d pp1oda pp2ró p2pú p2rec prek2 pre1kl pre1p2
1presszionis 1p2resszí 1prédál. 1prédálá 1p2riv 1p2roce 1p2rog p2roj pro1k2h
pru2s1á prus3s p1sc p1s2k p2s1ork p1s2p p1st2 1pszic psz1ön pszt2 p2t1i2o
pto1g2 pt1olv 1pu pu2s1a2n pu2se 1pú p1úg pú2t 1pü 2p1ü2g pügyész1 2p1üld
2püle 2p1ünn püt2 pü1th 2p1ü2z 1pű 1py 1qa 1qá 1qe 1qé 1qi 1qí 1qo 1qó 1qö
1qő 1qu qu2i 1qú 1qü 1qű q2v 1ra ra2b1ár rabi2g rab1iga rab1igá r1a2dag
2radm rae2g ra2et raé2dere 2rafik ra1f2r rai2g 2rajc 2r1a2kar ra2kác ra1klé
ra1k2ro ra2lak ra2l1eg 2ralk ral2lá 2r1als ra2mad ra2mir ra2mí ra2nal ran2ga
ran2ge ran2szá ra2nyal ra2nyál ranye2 rany1er ra2nyé ra2nyó 2ranza 2ranziti
rao2 ra2pák 2rapp ra1p2r ra2rán rast2 ra1sta rasztá2r raszt1áro 2r1a2tád
2ratc ra1thó ra2tir 2ratké 2ratki 2ratokn ra2tomo 2ratoz. 2ratozi 2ratst
ra2tür ra2ub ra2us 2raví raza2 raz1any ra2zel 1rá 2r1ábr rá2ca 2ráfia
2ráfiái 2ráfián 2ráfiát 2ráfj 2ráfp 2ráfu 2rá2fü rá2ge rá2gén rág3g rá2gi.
rá2gil 2rágun 2rágy rá2l1a2l 2r1állan 2r1állo rá2lyú rá2nér ránt1ak ránt1a2l
ránt1á2r rán2t1e 2rányí rá2nyol 2rányú rá1p2 rá2rad 2rárak r1á2ram rá2r1ál
rá2rár rárboc1 rá2r1é rá2rin rá2rok rá2rol 2ráru rá2rul rá2rus 2r1á2rú
rá2sir rá2s1ol 2rásó rás1ó2r rát1ad rát1á2r 2rátlé 2r1átm rá1tro 2r1átv
rá2zsár rba2ná r1b2la r1b2r rc1ab r2c1al rc1a2m rc1ará r2c1ál r2c1áp rc1edé
r2c1ele r2c1elő rc1esz rcé2l r2c1él. r2c1éle r2c1élé r2c1éll rc1élt rcés3
rc3hel r2c1i2d rc1ing r2c1izo rckész1 rc1kr rc1or r2c1ó2 rc1ön rc1ös rc3sőt
rc3za rd1ág rde2kér rd1él. rd1é2le rd1élr r2d1id r2d1i2nak rd1íz r2d1or
rd1ös r1d2rá rdu2s1 rd1út r1dy rd3zo 1re re2ájá re2ák reá2l1u re2áv 2r1ebh
re2cá re2ch redenc1 re2dir re2dí re2dor ree2s re2et re2g1a2l re2gap r1egóv
2r1egy. re2gyez1 2r1e2gyé 2r1egys re2inh rei2rá 2rejü rek1arc re2k1asz
re2k1emb re2kid re2k1ot re2lad 2relemi. 2r1e2lemz 2relmél 2r1elnö 2relo
re2l1os re2lői 2r1elr 2r1elto 2r1elv. 2r1elvá 2r1elvek 2r1elvet 2r1elvév
2r1elvk 2r1elvn r1elz re2maj 2r1ember re2mel re2m1ő 2r1enti re2of re2o1g2
re2ok re2op re2ot re2ók re2ól re2ós re2ót re2óv re2pad re2pak re2pal re2pas
re1pla rep1os re1p2res 2r1erd re2res 2r1e2rő 2r1ers re2sa re2sá 2reskü
2resszionis 2r1eszm re2t1any ret1ell ret1elo reu2t rev1e2le re2vés 2r1e2vő
re2x1 2r1ezr 1ré 2rédál. 2rédálá 2r1é2de 2réf ré2gá ré2g1ó2 2r1é2hes ré2kal
ré2kel rék1elő r1é2kes ré2les 2r1é2let 2r1élm 2rély ré2mo ré2mu ré2p1ed
2r1é2pí ré2pü 2r1ér. r1érd 2r1é2ret 2réri 2r1érté 2r1érz ré2sza ré2szá
ré2sz1er ré2szo ré2sz1ö ré2tal ré2tí 2r1étk ré2tö 2réven 2r1é2ves ré2z1a
ré2z1e2 ré2zin ré2z1o ré2zsi réz3sz rfi1s r1f2l r2f1öln r1f2r rgás1s rgés3s
rg2ha rg2la r1g2r r2gy1asz r2gyál rgy1e2s r2gyis 1ri ri2ac ri2af ri2ag ri2ah
ri2aj ri2ala ri2amé ri2ap 2ribü ri2deá ri2dei ri2deo 2ridg 2r1i2dő ri2ed
rie2m riet2te r1i2gaz 2r1i2gé ri1klo ri1k2ló ri2lal ri1ly 2r1i2nas rinc3c
rin2c3s 2r1inf 2ringb 2r1integ rin1tho riog2 rio1k2 ri2om ri2ósz 2r1i2ra
ri2rá r1i2ri 2r1i2ro ri2rón 2r1irt 2riskolát ri2sor 2risten. 2ristene
2ristenné 2ristennő. 2ristennők 2risza 2r1i2tal ri2vás 2r1i2vó ri2x1el ri2xi
2r1izmo r1i2zom 2rizz 1rí 2ríj rí2ja rí2rá rí2ró rí2ve rí2vé rí2vü r2j1er
r2j1u2r rk1ang rkas3s rká2n1e2 rkes2 r1k2hé r1k2hi rki2g rk1iga r2k1ill
rki2s1 r1k2la rk2rá rk2re rk2ré r1k2ri r2k1út rle2g1a rle2g1á rm1akó rma2tal
rma2tár r2m1áll rmá2nyi rme2g1á rmé2kel r2m1ors rmo1sz2t rm1s2 rna1t2 rndi2
rne2mis rne2tel rnet1o r2n1ip rno1g2 r2n1ó2d rnó1s rn1s2 r2nyaj r2nyalak
rny1old r2nyús 1ro roá2 2robl 2rodú 2rofe ro2g1ad ro2gár ro1gráf ro2k1as
rok1ás roke2 ro2ked ro2ker ro2kiz ro1kli ro1kri 2r1o2laj 2r1olda 2r1oltó
ro2m1ál ro2m1á2ro 2rombi ro2mel ro2m1es rom1ért ro2mév rom1ist ro2m1iz ro2mí
ro2m1or ro2m1ó2r ro2nop ro2nú ro2nü ro2nyu ron2zá ro1p2r 2r1org 2r1o2ro
ro2ru 2r1orv ro2sar ro2se ro2sin ros3szá rosz2fé roszlán1n ro2szó 1ró róa2
ró2dis ró2dö róme2 rómeg1 ró2mez 2róms róp2 ró1pr ró2ráj ró2rák ró2rán
ró2rát ró2ri ró2t1ak ró2tin róz1z 1rö 2r1ö2b rö2k1é2l rö2k1érv 2rökí 2rökl
rök1o rö2le 2röme. 2römé 2römne rö2pi 2r1ö2r rö2ső rö2sü rö2ve 1rő 2rődb
2rődí 2rődr rőe2 rő1g2 rői2rá 2rőlk 2rőlt 2rőnke 2rőnkk 2r1őrn rő2s1á2 rő2si
2rősítő. 2rősítőr 2rőszakol. 2rőszakolá 2rőszakolj 2rőszakolt 2rőtl r1p2h
rp2la rp2le rpo2i rp2ré r1p2rí r1p2ro r1p2ró rqu2 r2r1árb rrás1s r2r1ir
rri2ta r2r1oll rró1d2 rr1p2 rr1sp r2rü r2s1ac rsa2il2 r2s1a2la rs1alu
rs1a2ny rs1ara rs1áru rs1ele rs1esz rse2t rs1ete r2s1ép rs1éte rs1étt
r2s1i2d rs1iga r2s1in r2s1i2o r2s1i2r rs1ist r2s1í2r r1smi r2s1olv r2s1ors
rsp2 rs1st rs3sza rs3szá rs3szí rs3szö rs2tab rste2i r1s2to rst2r r1stru
r2s1ur r2s1u2t r2s1ú2s rs1üld rsza2ké rszáraz1 r1sz2f r1sz2k r1szn r1sz2tá
r1szt2r rta1g2 r2t1aj r2t1als rta2me rt1app r2t1a2u r2t1a2z rt1ág. r2t1á2ga
rt1ágr rtá2ló. rtá2ly1át rtá2rul r2t1átl rteá2 rt1e2gy rt1eml r2t1ent rtes1s
r2t1é2le r2t1érd rt1érte r2t1érz rt2hág r1t2hen r2t1i2d rti2g rt1iga r2t1ill
r2t1inf r2t1iro r2t1isk rtme2g1 r2t1oli rt1oml rton1n r2t1org r2t1orv rt2ran
rt2rom rt1t2 r2t1u2t r2t1új r2tüd rt1ünn rt1ür r2t1üt rty2v 1ru rua2 r1ubb
r1udv rue2 rug3g rugós1 2r1ugr 2ruktú ru2ru ru2se ru2sin 2rusí ru2su ru2szu
ru2tak ru2tat ru2tó. ru2tu 2ruü 1rú rú2d1a2 rú2du rú2ri 2r1útj 2r1ú2to
2r1útr 1rü rü2lér rü2l1öl rü2l1ön 2r1ünn 2r1ü2r 2rü2t 2r1ü2v 2r1ü2z 1rű
r1űrt rvas3s r2v1e2lői2 rve2ra r2v1érte r2v1ip r2v1or rv1osz ry1é2 r2z1a2la
rza2t1al r2z1ára rze2tel rzs1a2n rz3ság r2zső rz3sz 1sa 2s1abr sa2cé sa2dói
sa2dój sa2dók sa2dór 3sajd 2s1ajta 2sajtón sa2kan 2sakar s1akc sa2k1el
sa2kis sa2k1í 2salf 2salji 2salm 2salu sa2m1il sa2mí sao2 2sapa 2sapá 2sapr
sa1pré sa2ran 2s1a2ri 3sas. sa2s1or sas3sze 2s1asszo sa2su 2s1a2tom 2saty
sa2uc sa2un savar1a sa2v1ál 1sá sábe2 sá2b1er sá2fo sá2gal sá2g1ál 3ságb
3ságd 3sá2ge 3ságé 3ságf 3ságg 3ságh 3sági sá2gí 3ságk 3ságm 3ságo sá2g1osz
3ságp 3ságr 3ságs 3ságu 3ságü 2s1á2gy 2s1áld 2sáll 3sámf sán1t2 sá2r1a2g
2s1á2rai. s1á2rak sá2r1al 2s1á2ram sá2r1ál s1árbo sá2r1e 2s1árfor 2sáru
s1á2ruk sá2rul s1á2rus 2s1á2rú sá2sás sáskész1 s1á2só 2sáta s1átad s1átf
2sáth 2s1á2ti s1á2tí 2sátl 2s1átm 2s1á2tü 2sátv sá2ve sá2v1i2 sb2 s1bl s1br
scar2 s2ch 1sche 1sché sda2d1 s1d2r s1dy 1se sea2 se2bat se2b1o se2bó
se2cs1a 2s1e2dén 2s1e2dz se2ger 2s1e2gér segész1 se2gyed 2s1egz se2her
se2il2 se1kra 2selá se2lál 2s1elnö 2selo se2l1os 2s1e2lőa 2s1e2lől 2s1elto
2s1e2lu 2semel 2semé 2senc sen2d1ő2r se2nyer se2nyir se2nyú se2ö 2s1e2pi
2s1e2po se2rej 2s1e2rő se2sá s1esem se2sés ses2ti s1eszkö se2szű se2tik
2seto se2tok se2t1ol 2s1e2vés se2vo 2s1e2vő 1sé sé2g1a sé2g1á2 sé2g1eg
sége2l sé2g1ele ség1eli sé2gés ség1ész ség3g sé2gigé sé2g1í2 sé2g1o2 ség1s
2s1éh. 2s1éhs s1élm 2sély sé2lyeg sé2mas sé2mu s1é2pí sé2pü 2s1érc sé2rin
s1értel 2s1érz 2s1étk 2sétt 2sétv 2séve. sé2vei s1f2 s1g2 sha2k s2hi. s2hin
1si si2bá si2dé s1i2dő sie2l si2em 2s1ifj 2siga s1i2gaz si2ge. 2s1i2gé si1gl
sig2ni s1i2nas 2sinf si2ójá 2sipa si2rod 3sisak 2sisk 2s1isp sis3s si2s1ü2
si2tal si2tá siú2 s1izmo 2sizo si2zol si2zom 1sí sí2ka sí2ke sí2nü sí2r1e2
sító1b 2sív s1í2zes s1ízü sk2 skás3s 1s2kic ski2s1a s1kl sközé2 skö2z1él
s1kv 1s2lag sla2te 1s2lav sle2t1á2 sme2g1á smerés1s 1so 2sob so2d1e sodé2
so2dév so2d1os 2sodú 3sof so2kaj so2kar so2ke so2kir 2s1okke 2s1o2kos so2kó
2sokta s1o2laj s1oldó s1oltó so2n1al so2n1e so2n1é son2t1ár son2t1e so2nye
sor1áll so2r1e2 2s1orrú 3sort 2sosz s1oszt 2s1otth 2s1o2x 1só só2kál
2s1ó2lo2 2sórá só2rán sós3s 1sö 2s1öbl 2söc s1öko s1önte 2s1öntv sö2r1i2
2sös s1ösv 1ső sőe2 sőé2 s1őrm s1őrn ső2sok 1sparh 1s2páj spiros1 spis3s
sp2l spor2te spor2tö 1s2pór 3spra s1pro s1p2s srau2 sren2d1ő sré2z1 1s2róf
s1s2k ss2rá ss3s sst2 s1s2ta ss2tí s2sz sszat2 ssz1á2ru s3szele s3szél
s3szép ssz1ér. ssz1ing s3szom s3szóko s3szól ssz1ös s3szöv ssz3s sszt2
s3sztr s3szű 1stafí s2t1alj s2t1alk 1stansan s2t1a2rá 1stájg s2t1eb 1steinig
1steinj 1steinr 1steint ste2ná ste2ra ste2u sté2ká s2t1é2li s2t1érc st1érm
s2t1érz s2t1i2r 1s2tíl st1íz sto1g2 sto2ris stő2r st1őr. st1őrc st1őre
st1őrö st1őrs s1trag st2rap st2róf 1struktú st1új s2t1ür 1su 2s1ugr 2s1ujj
s1u2ra s1u2rá 2surn su2tal sutas1s 2sutá su2tó 2suz 1sú sú2cse s1újd 2sújí
s1újs 2súr. sú2r1as s1úrb sú2t1á sú2té sú2ti sú2tü 1sü 2süd 2s1ü2gy 2süld
sü2nő sü2re sü2rü 2süs sü2sz 3sütő 2s1ü2z 1sű sűs1 1s2vin s2z 1sza 2szac
sza2dá sz1a2dó 3szaká 3szakm 3szaks 2sz1alj 2szalk sz1ass sza1tro 1szá 3szám
szá2m1e szá2m1ér s3záp 3szárn 3szárú 3száza 3százn 3százö szd2 sz1dr 1sze
2szedz 3szekr 2szelemz 2sz1elm 3szemc 3szemü 3szemű. 3szend 3szepl sze2rej
3szerk 3szers 3szerv 3szerz 2szesem sze1tro sz1ezr 1szé 3széké szé2nö sz1érc
2sz1érd 2sz1éré sz1értá 2szérté sz1érv széte2 1szférá 1szi 2szic 2sz1iga
sz1igé 3szigo 3szimb 2szira 2szisk 2szism sz1isza 1szí 3szívű 1szkóp.
1szkópk 1szkópo 1szkópp 1szo 3szob 3szof szo2l1ál 3szorg 2szors 2szorv 1szó
1szö 3szöge 3szögk 2szövv 1sző 3szől szs2 sz3sap sz3sas s3zsák sz3sár sz3sát
sz3seg sz3sel s3zsem sz3ser sz3sé sz3sik sz3sir sz3so sz3só s3zsö sz3sp
sz3st sz3sú sz3sü sz3sz sz2t1ap sz2t1á2rak sz2táro 1sztárr sz2táto 1sztorit
1sztráj 1szu 2szu2b 2szutó 2szuz 1szú 2szúth 2szúto 2szúts 1szü 2szüg 3szür
2szüt 1szű 2szűe 2szűs sz1z sz2zo sz3zs 1ta 2t1abr 2t1a2cé t1a2dan ta2datá
ta2dati ta2dato 2t1a2dá t1adh t1adj t1adl t1adn ta2dod ta2dog ta2dok ta2dom
ta2dot 2tadó t1a2dó. t1a2dója t1adv tae2 2tafí ta1fr ta2g1av ta2g1ál tag1g
tai2g ta2iga ta2i2re ta2kad 2t1akc 2t1aktí ta2l1adn ta2lakb 2talakí t1a2laku
2t1alany ta2lapú ta2l1as ta2l1áll talás1s ta2l1em ta2l1e2s talé2k1e2 ta2l1ur
talú2 ta2lúr ta2l1út ta2mid ta2nan ta2n1év 2t1anny tan1osz ta2n1ó2r 2tansan
ta2nyag tao2 ta2pa. ta2pán ta1ph 2tapp ta1pré 2taran 2tarán ta2s1á2r ta2sel
ta2sem ta1s2p 2tassz tas3szá ta1sta tat1ato 2tatiká 2t1a2ty ta2utó 2tazo
ta2zon 1tá 2t1ábr 2tádi 2tág. tá2ga. tá2gat 2t1á2gá 2t1ágb tá2ge 2t1ágg
2tágn 2t1á2go 2tágr t1á2gun tá2gú 2t1á2gy táje2 tá2j1eg 2tájg tá2lét 2t1áll.
2t1állj 2t1álln 2t1állo 2t1állt tá2lyéb tá2lyér tá2mí tá2m1os tán1alm tánc1c
tá2ne 2tánv tá2nyú 2tánz tá2pa tá2pe tá2pin tá2rab tá2raj tá2r1ál tá2r1e2
tá2r1ér tá2rid tá2rin tá2rí tá2r1osz 2táru. tá2ruh tár1ur 2t1á2rus tá2rút
tá2s1ár. tá2s1e tá2sin tá2só tás1ó2r tás3s tást2 tás1tr tá2sü tá2sza
2t1át1ad 2t1átm 2t1áts 2t1átt 2t1á2tü táva2 tá2v1ad tá2vér tá2zsa tb2 t1bl
tbor2dó t1br tca1f2 t1d2 1te te2ad te2ai te2aka te2as te2av 2t1e2dz te2g1á
tegész1 2t1egys tejá2 te2j1ell te2jo te2j1u2 te2k1ó2 te2kú te2kür tela2
te2l1ad 2t1e2lál te2lár te2lát 2t1elha 2t1elhá 2t1elhel 2telix 2te2l1os
2t1eltá 2t1elv. 2t1elvi 2t1elvű te2m1ál t1embl 2t1e2mel te2més te2m1os
te2n1a2 2t1endr te2nel te2ner ten3n te2n1u t1enyv te2oló te2rad te2ran
te2r1ar te2r1ár 2terdő ter1egy 2t1e2rej te2r1est te2ror te2rö 2t1e2rő te2rut
te2s1a 2tesemé 2t1esél t1e2sés tesi2 te2sin t1esni 2t1e2ső tes2tal te2süv
tesz1ál 2t1eszm tes3zs 2t1e2szű teto1s te2t1ot te2t1öl te1t2ro teu2tá
2t1e2zer 2tezr 1té 2t1ébr 2tég. té2hes t1éhs té2kal té2k1a2n té2k1as té2k1au
té2k1eg té2k1ell té2kép té2k1ir té2k1í2 té2kö té2lá té2l1e2r té2l1os 2téls
té2lu 2télü té2lű té2lyem té2nin té2ny1el 2t1é2pí té2ran térá2 té2r1ár
2térdek té2r1e2l té2rem tér1emb té2r1ész té2r1in té2ris 2térm 2t1érték té2sa
té2s1á té2sel té2s1o té2szes tés3zsí té2tar té2tál 2tév. 2t1évb 2t1é2vei
té2ven 2tévet 2tévéb 2tévét 2tévh 2t1é2vi 2t1évr 2t1é2vü té2zs t1f2l tg2
tgár2 t1gn t1gr tha2d1é t2hai t2hak tha2l1ás tha2me 1t2hau 1theid 1theu
1t2hod 1t2hos. 1thosb 1thosi t2hov 1thy 1ti tia2t 2t1i2dé 2t1i2do 2t1i2dő
tien2 tie2r 2t1ifj ti1f2r 2t1i2ge 2t1ign ti1g2ra ti1grá ti1kh ti1k2le
ti1k2ló ti1k2ri ti2lan ti2l1i2p t1illat 2t1imr 2t1ind ti2ne. ti2n1es tin2gi
tin2g1o ti2nö ti2nú ti2par 2t1i2rán ti2rén ti2rig ti2rod ti1sl 2t1ism tis3s
ti1str 2tistv ti2sü 2t1i2vó tize2n1 ti2zom 2t1i2zü 1tí 2t1íg 2t1í2j t1í2rás
2t1í2ró tí2vel tív1ele tí2v1ér tí2vű tí2za tí2zó t1k2 tká2nya tla2c3 tla2g1e
tlag1g tlas1s tme2g1é tna2k tno1g2 1to 2t1obj todé2 to2dév to2k1ad to2k1ö
2t1o2laj to2lim 2tolvas to2lyag tome2 to2mel to2men 2toml t1omlá to2mó 2tomú
to2nalm to2nel to2nü to2nye to1p2h to2rab to2ral to2r1as to2r1e2l 2torie
to2r1isk toros1s to2ró tor1s2 tosi2 to2sin to2sze tosz2kó 2t1oszl to1tra
2t1ou to1y 1tó 2tódok 2t1ó2év. 2t1ó2rai tó2rás tó2s1e2 tó1spe tó1spo tós3s
tó1t2r 2tóvod 1tö 2tödn 2tödü tö2kör t1öltöz tö2n1í2 tö2nő 2t1öntu tö2s1i
t1ötr 2töv. 2tövn 2tövö 1tő tő1dz2n tőe2 tői2rá 2tőrb 2t1őrl 2t1ő2rü 2t1őrz
tő1sté tőu2 tőü2 t1p2 tpen1 1t2rafá 1tranziti tra1ps tras2 t2rádá trás1s
1tréf 1t2ril troa2 tron3n trosz2 tro1szk tró2zs 1t2rup 1t2rü t1ry tsa2va
1t2sé. t1s2k t1s2p2 t1s2t2 1t2sub 1tsy tsza2ké tsza2te tszá2ze t1sz2f t1sz2t
tt1aszt t2t1a2u t2t1áll tt1egy t2t1elm tt1eml tté2g tt1ége tté2l tté2res
t2t1id t2t1isk t2t1ors tt1ott tt1óra ttő2sa tt2rén ttűz3s t2ty 1tu tu2go
tu2mál tu2min tu2se tu2sze tu2sz1é2 tu2szi 2t1u2tak tu2tas 2t1utc tu2u 1tú
2t1úg 2t1újd 2t1újs tú2lan tú2lat tú2l1á2 tú2l1é2 tú2ló tú2lö tú2r1e tú2r1é
tú2sze tú2to 1tü 2t1ü2g tü2len tü2l1e2s tü2lér tü2lo tü2lü tü2te tü2té tü2tö
tü2tő 2t1ü2v 2t1üzl 1tű tű2za tű2zőr tű2zs tva1k2 tva2ra tváro2s1u tve2neg
tve2ra twa2 t2y 1tya tyau2 1tyá tyás3s 1tye 1tyé 1tyi 1tyí 1tyo 1tyó 1työ
1tyő 1tyu 1tyú 1tyü 1tyű ty2vá tz3sc 2u. u1a u2ad ua2e ua1yi u1á uá2ru uba2l
u2b1ala ub1éle ub1ord uca2t1á 2uch uc3ság u2cs1ál uda2tal uda2tál uda2te
udás3s 2ude udi2o 2udiz 2udoc 2udod 2udoé 2udok 2udot u1dy u1e ue2g uel1o
ue2s ues3ze ue2v u1é ué2p 2uf ug1alj ug1ág ugá2rá 2uge ug1el u2gé. 2ugg 2ugh
u2g1ir u2g1iv ug1ír ugosz2l ug1ut u2g1ü ugya2 uh1ako u2h1ál uhás1s u1i u1í
uk2kór uk1üt ula2tin u2l1áll ulás3s ul2cs1e2 ul2cs1ó ul1ex u2l1ér ulit2
uli1tz u2l1í2 2ulk ul2l1os 2ulo ul2t1ü2 u2l1ü2 2uly um1a2da u2m1a2l um1áll
um1e2d ume2g1 u2m1e2l um1ev u2m1érté umik2 umi1kr um1ing um1i2on um1ivá
um1ív u2m1ol u2m1osz u2m1ő2 umplis1 um1pr u2nal u2n1ar 2une un1g2l 1unj u1o
u1ó uó2l u1ö u1ő2 2upa upe2r1e2 upe2rin upla1 u1p2ró u2rad 2uralgia 2uralgik
u2ralo 1u2ram ura2m1i 2uran ure2u 2uréká ur2f1e 2urí 2urob uro2ka 2urol
2uróp u1ry us1abl u2s1a2d u2s1a2l u2s1ar u2s1as u2s1a2u u2s1a2z us1áru us1eb
us1e2g us1e2l us1ez u2s1ék us1ép u2s1id us1int u2s1i2r u2s1is us1izo us1k
us1old us1ó2r u2s1ö2 us1p2 2uss. 2ussé 2ussh 2ussn 2ussr 2usst us1sy us3szem
us3szí u2s1u2t u2s1ü2 us3zav u2sz1ág u2sz1el u2sz1ö usz3s u2t1a2ny 1u2taz
után1n 1utánz u2t1e2l1 u1t2he u1t2hi ut2hu u2t1i2p utó1sz 2utü1 u1u uu2m1á
uu2miz u1ú u1ü2 u1ű 2ux. u1y 2uzal uzeo2 2uzó uzz2 2ú. ú1a úa2l úa2n ú1á2
2úbar ú1b2r ú2cs1as ú2csárb úcse2 úcs1em úcs1en úcs1er úcs1es ú2cs1é2 ú2csid
ú2csí úda2n ú2d1ez úd1ug ú1e2 ú1é úg1ál úgás1s ú2gya2 úgy1ag úgy1ah úgy1is
úhús3 ú1i úi2d 2úis ú1í új1ang új1es új1ez új1é2 1újf 1ú2jí új1k2 1újr új1ud
ú2k1al ú2k1a2n ú2k1e2s 2úkoro 2úkó úk1ól ú2k1ü2 2úl ú2l1ad. ú2l1ada ú2l1a2dá
ú2l1a2dó ú2l1aj úla2n úl1ál úl1d2 ú2l1e2d ú2l1eg ú2l1e2m ú2l1erő ú2l1e2s
ú2l1ex úl1é2d úl1ég úl1é2r ú2l1in ú2l1is ú2l1iz ú2l1í2 ú2l1ol úl1ó2r úl1öl
úl1öm úl2tag ú2lyar ú2ly1e2 ú2lyér 2úm únyi2 úny1ir ú2ny1ö2 ú1o ú1ó ú1ö ú1ő2
úpos3s ú2r1an ú2r1att ú2r1a2u ú2r1áll ú2r1á2ri úr1e2g úr1e2l úr1es 1ú2ré.
úr1ék úr1é2l ú2rig ú2rin úr1int ú2r1ot ú2r1ö úrs2 úr1sm 2úru úsa2 ú2s1ad
ús1e2l1 úsele2 ús1e2v ús1ex úsé2 ú2s1ét ú2s1il ú2s1i2p ú2s1old ú2s1ő ús3sze
ús3szí 2úsze úsz1ej úsz1es úsz1ev úsz1ez úsz1év úsz1öl út1a2d út1á2g út1á2s
út1át 1útd út1ef út1e2g út1ép út1ér útie2 úti1p út1irá út1old 1úto2n1 út1ő
2ú1u2 ú1ú ú1ü2 ú1ű úz3sz 2ü. ü1a ü1á ü2dí üd1íz 1ü2dü üd2v1i2 üd2vo ü1e2 ü1é
ü2g1el ügy1in ügy1o ü2gy1ő2 ü2h1a ü1i ü1í ük1ac üka2n üka2p ü1k2hé ü1k2hi
ük2ker ük1u2 ü2l1a2d ü2l1ag ül1a2l ü2l1á2 ül1eng 1ü2lep ül1esh ül1e2ső
üle2t1a üle2tá üle2teg üle2t1e2l ü2l1ég ü2l1ép ül1ér. ül1érz ü2l1í2 ül1ol
ül1or ül1ölt ü2l1ö2v ülőe2 ülői2r ül2tad ültá2 ül2táp ül2t1ár ül2tát 1ültes
ü2l1u2 ü2l1ú2 ü2l1ü2l ül1üt üm1il ün1id ün1kri ü2n1ó2 ü1o ü1ó ü1ö ü1ő ü1p2h
1ü2reg üreg1g 1ü2rí ür2ja ürke1 ü2röm üss1eg üs2t1a üs2t1á üs2t1e2 üsté2
üs2t1ére üs2t1il üs2t1í2 üs2t1o üs2t1ó2 ü2teg ütés3s üt2t1á üt2tem üt2t1é2
ü1u ü1ú ü1ü ü1ű üveg1g üze2ma üze2mo 1ü2zene 2ű. ű1a2 ű1á2 ű2cs1á űcs1ip
ű2d1e ű1e űe2g űe2p űe2v ű1é űé2l űé2p űfa2je ű1i űi2p ű1í2 űka2g ű1k2ro
űlőú2 ű2n1el ű2n1e2s űn3n ű1o2 ű1ó ű1ö ű1ő űra2 űr1an űr1at űr1ál űr1es
űr1i2p ű2r1ol űr1öl űr1p2 ű2r1u2 űr1üg ű1s2ká űs3s űs2to ű2s1u ű1t2r ű1u2
ű1ú ű1ü2 ű1ű űve2r űves1s űza2 űz1ak űz1an ű2z1á ű2z1el ű2z1e2r ű2z1e2s
űz1ev űz1érm ű2z1im ű2z1is ű2z1o2 ű2z1ör űz1ö2v űz1őr. űz1őrs űz1őrz űz1p2
űz3su 1va vaa2 va2d1ál va2d1ár va2d1e2 vadi2 va2did va2d1ol v1a2dón va2dö
va2d1ő va2d3z va2ge vaja2 va2j1ad va2j1e va2jö va2k1e2s va2n1eg van1es
va2név 2vang van3n va2nol va2n1ó2 va2nö vao2 va2r1al va2rany va2rar va2r1ir
2varú va2s1aj va2s1e2 va2sék va2sid va2s1in va2sö vas3sze vas3szé vast2
vas1tr va2s1ü vasz3s va2t1á2r va2t1é vat1int vatt2 1vá váb2bal váb2be vá2csi
vá2csü vá2dal vádi2 vá2dir v1állap v1állás 2vállo 2vállv vá2ma vá2m1á vá2m1e
vá2mi vá2m1u2 vá2n1a2r vá2n1ár vá2ne vá2nis vá2nú vá2ny1as vá2ny1e vány1ér
vá2ral vá2re 2várf vá2ris vá2r1oml vá2rö várs2 vár1sp vá2ruh vá2rus vá2r1ú2
vá2szi v1átm vá2t1ors 2v1á2tü vá2zal vá2z1e2 vá2ziz váz3sz vd2 v1dr 1ve
ve2g1a2 ve2g1á ve2g1ele veg1ért ve2g1i ve2gí ve2g1ö ve2gya ve2gy1e2lem
ve2gyemb ve2gyér ve2gyip ve2il 2v1elég 2v1elhá 2v1elm ve2l1o 2v1elvá
2v1e2mel 2v1eml vene2g ve2n1egy ve2n1emb ve2n1esz ve2n1e2v ve2n1év ven3n
ve2nó ve2nö ve1p2 vera2 ve2r1á ve2r1eng ve2r1esz ve2r1ip ve2r1ol ve2ror
ve2ró 2verősítő. 2verősítőr vers3s ver2s3ze ve2sa ve2ser ve2sú ve2süv
2v1eszm ve2s3zö ve2t1a2 ve2tár 1vé vé2der vé2do vé2dz vé2ga vé2gá vé2g1eg
vé2g1ele vé2g1er vé2geté végig1 vé2gin vé2g1í2 vé2gó vé2la vé2l1á2 vé2l1e2r
vé2l1o vé2lu vé2nas vé2ny1e2l vé1p2 vé2pí vé2r1á2 vé2rel vé2ron 2vérté vé2rú
vé2rül vé2sz1á2 vé2szeg vé2sz1o vé2tí 2v1év. v1f2 vgé2 vhez1 1vi 2vick
vi2cs1a vi2csö 2v1i2dő vi2dz 2v1inh vin2t1es vi2pa 2v1i2rat. 2v1i2rata
2v1i2ratá 2v1i2rath 2virati 2v1iratk 2v1i2ratok 2v1i2ratot 2viratoz.
2viratozi 2v1i2rats 2v1i2ratv 2v1ism vissz1e2 2v1izg 1ví 2v1í2rás. 2vírász.
2vírászat. 2v1írda 2vírdáb 2v1í2ró ví2z1a2 ví2zá víze2 ví2z1el ví2zer
ví2z1es ví2z1o ví2zó ví2zs 2vízű v1k2 1vo vo2lál vo2l1e2 vo2r1a vo2se vos3s
1vó vóé2 v1ó2rá vó1s2p vó2s3zen vóví2 1vö vöt2 1vő vőu2 v1p2 vso2rol vsza2ké
v1t2 1vu 2v1u2t 1vú vú2sz 1vü 2vüg vü2gy vü2les v1ünn 2v1ü2t 2v1üz 1vű 1vy
1wa wa2e wa2i wa2le wa1sh wat2t1á2 1wá 1we we2b1o we2i we2ö wesze1 1wé w2h
whi2 1wi wi2c wi2e wi2r 1wí 1wo2 1wó 1wö 1wő 1wu 1wú 1wü 1wű 1xa 2x1a2l x1ar
1xá x1b2 1xe 2xe2g 1xé 1xi 2xidj 2xido 2xidt 2xio xi2ol xi2on 1xí x1ív 1xo
xo2d xo2g 1xó 1xö 1xő xpor2t1á2r 1xu xusá2 xu2s1é xus3s 1xú 2x1ü2 1xű x2x
yaá2 y1abl ya2cél y1a2dag y1a2dón yae2 ya2gar ya2g1ál ya2gá2r yag1ár. ya2ged
ya2g1el ya2g1en yag3g ya2gig ya2gor yag1osz y1agy. ya2gyak y1agyk y1a2gyú
y1ajt ya2kala yaké2r yak1ére ya2kiz y1a2lak. ya2l1ék. y1alm ya2m1al ya2m1ár.
ya2m1árb ya2m1árh ya2m1árn ya2mel ya2m1is yan1ar yan1at ya2n1e ya2n1i
yan1o2d ya2nő yan1ők yan1őt ya2nyáé ya2nyái ya2nyák ya2nyám ya2nyán ya2nyát
y1anyj y1a2nyó ya2pa. ya2pas y1a2páb y1a2pák y1a2pám y1a2pán y1a2pár y1a2pás
y1a2pát y1a2páv y1a2pi yapo2tá y1a2pó y1app ya1p2r y1a2pu y1a2rány y1arcn
y1arco y1arcr 1yard. 1yardn ya2r1el y1argu ya2s1ol y1atád yat2te y1a2ty
ya2utó y1a2zon y1ábr yá2du y1á2g y1álar yálas3 y1áld yáma2 yá2man yá2map
yá2m1as yá2mí yá2m1os yá2mu y1á2po yá2rad y1á2ram y1árbo yá2r1e yá2r1i2p
yá2rö y1árp y1árud y1á2ruh yá2rul y1á2rus yár1ut yá2sar yási2 yá2sir y1á2só
yá2sze yás3zen yá2szé yá2szor yá2szü y1á2t1a y1á2t1e2 y1áth y1átk y1átn
yátne2 y1áts y1á2tü y1b2 ybee2 ybor2d y1d2r ye2d1a ye2dá ye2d1ó2 ye2d1u
ye2g1a2 ye2gá ye2g1el yegész1 yeg1g y1egy. ye2gyen ye2gyes y1egyl y1e2l1a2
y1elc y1e2lemz yeletes1 y1elha ye2l1o y1e2lőg y1elr y1eltá ye2man ye2mas
ye2mat y1ember ye2mel ye2m1er ye2m1es ye2na ye2n1á2 ye2ner yenes1s ye2n1é2l
ye2ni ye2n1o y1e2po yereg1g y1e2rej y1e2rőr ye2rőv yers1s yesa2 ye2s1al
ye2s1á yes1egy yesi2 ye2s1í2r ye2sú y1eszm ye2s3zö yes3zs yesz2t1ék ye2szű
ye2t1ér y1e2ve y1e2vé y1e2vő y1e2zer yezernyolc1 y1é2ber yé2g y1égb y1ége
y1éhes yé2let y1élm yé2pí y1é2pü yé2rak yér1ára y1érde yé2r1el yé2ri yé2rol
y1érték y1érze y1érzé yé2sza yé2szá y1étk y1é2tű y1év. y1évb y1é2vek yé2ven
y1é2ves y1é2vet y1é2vi y1évn y1évr y1évv y1f2l y1f2r yha2d y2h1adó y1icc
y1i2de y1i2do y1i2dő y1ifj y1i2gaz yi1k2ri y1i2má y1imp yi2nas y1ind y1inf
y1inge y1i2par yi2ram y1i2rat. y1i2rán y1i2rás y1i2rod y1irt y1ish y1isk
y1ism y1isn yi2tal y1i2vó yí2le yí2róé yí2rói yí2rón yí2rór yí2rót y1í2v
yki2s1a y1k2l yközé2 ykö2z1él yk1uj yme2g ymeg1á yo2gi yoki2 y1o2k1ir
y1o2kos y1o2koz y1oktá yo2laj y1oldá yo2m1e yo2m1ol yo2n1e yoné2 yo2néh
yo2nis yo2niz yo2n1í yon3n yon1s yo2nü y1o2pe y1orm y1orom y1orrú yo2se
yos3s yo2sú y1oszt yó2gya yó2gy1e2 yó2gyé yó2gyi y1ó2lo yó2rai yó2ráj yó2rák
yó2rán yó2rás yó2rát yó2riá yós3s yó2s1ü y1öb yö2kés y1önt yö2r1i y1ö2töd
y1ötv yö2zön yőe2 y1őrl y1őrr y1őrs y1őrz ypen3 yp2re yp2ré yp2ro yp2ró
y1p2s yren2d1ő ysé2gel y1s2k y1s2p yst2 y1stí y1str y1sz2l y1sz2r y1sz2t
y2t1ac y1t2h y1t2r yu2g1á yu2ge y1ugr yu2gy y1ujj yu2l1e yu2n y1uno y1u2ra
y1u2rá yu2sé y1u2szo y1utc y1újs yú2kó yúl1l yú1p2 y1ú2ri yú2szá y1útj
yú2ton y1útró y1útt y1üd y1ü2gy y1üld yü2t y1üte y1üté yü2ze yü2zé y1üzl
y1ű2ző yvai2 yva2j yv1ajá yv1ank yv1a2ra yv1állo yv1állv yv1á2rak y2v1áru
yve2g yvegy1 yv1érté y2v1in y2v1í2r y2vu y2v1ú yv1üg yze2t1a2 yze2tele
yze2t1o 1za 2z1a2cé z1a2dag za2d1ár zade2 za2del za2dí z1a2dog za2dóh za2dói
za2dót zae2 za2g1e zai2g za2jan za2j1árt z1a2kara z1a2karó za2k1av za2kem
za2k1i2r za2k1ír za2k1osz zalé2ké 2zanya zao2 z1a2pó. z1a2pók z1a2ráb
za2rány 2zarc za2sem za1s2l zat1akt za2t1a2n zata2r zat1ará za2tem za2t1in
za2t1ív 2zaty z1a2tya za2tyá zau2t z1az. z1azh za2zo 1zá z1ábr záé2 zá2g1al
zá2gat zá2g1ál zá2ge zág1g zá2g1ú 2z1á2gy zá2kin zále2 zá2l1eg zá2l1em
zá2lér 2záll zá2los zál1t2r zá2m1a2d zá2map zá2mar zá2mis zá2nú z1á2rada
z1á2ram zá2r1is zá2r1osz zá2r1ó2ra zá2rö zár1s2 zá2sin záskész1 2z1á2só
záte2r z1átv zázé2 zá2zév zá2z1ol záz3sz z1b2 zbee2 zdas2 z2d1ass zd2ró
zd1ur 1ze 2zea2 zeá2z 2z1eb. ze2bei ze2bek z1e2dz zee2 zegész1 ze2gol 2zegy
ze2gyes z1egyn z1egys zei2g zei2s ze2k1e2g ze2kép ze2lál z1elej ze2l1eml
z1e2lemz z1elhá 2zellen z1elnö ze2l1os ze2lőg z1elvo ze2mad ze2már zeme2iké
ze2m1ell 2zemés zem1id ze2m1iz zenci2a zene2tel zen2tan zen2t1est zen2tí
zeo2m zeö2 ze2per ze1ph ze1p2r ze2rad ze2raj ze2ran ze2r1ar ze2r1as zer1ára
ze2redő zere2g zer1egg ze2r1egy zer1ejt 2zerejü ze2r1él ze2r1ill ze2r1ip
ze2ris 2zerj ze2ror 2z1e2rő zer1s ze2r1u2 ze2set ze2sit ze2ső 2z1esté zes1ut
ze2süv zesz1ár. ze2széle ze2szip 2zeszk zes3zs ze2t1any ze2tál ze2tát
ze2t1ell ze2telm ze2térte ze2tí zetme2 zetmeg1 ze2tok ze2t1ó2r ze2ty z1e2ur
2zev ze2vez z1e2vol ze2vő ze2zer 2z1ezre. 2zezred 2zezrei. 2zezreir 2zezreit
2zezreiv 2zezrek 2zezrel 2zezres 2zezret 1zé zé2dal zé2d1elem zé2d1és zé2dz
2zég z1é2ge zé2hes 2z1éhs z1éjs zé2kin zé2lak zé2l1á2 2zé2lel zél1elt
zé2lene zé2l1e2r 2zélet. 2z1é2letb 2z1é2lete 2z1é2leté 2z1é2leti 2z1é2letr
zé2lir zé2lo zé2lu zéndio2 zéne2 zé2neg zé2n1el zé2n1is zé2nu zé2p1a2 zé2pá
zép1emb zé2p1ér zé2pir zé2pis 2zépí zé2p1o zé2pu 2zépül zé2r1á2 zé2reg
zér1emb zé2rés zé2rig zé2r1o zé2rőt 2z1érté 2z1érth 2z1érz zé2sa zé2sel
zé2sí zés3s zé2tap zé2t1á2 zé2te2l zét1ele zét1ese zét1esn zét1est zé2ti
2zétk zé2t1o zé2tö zé2tu zé2tú zé2t1ü zé2tű z1év. zé2vek 2z1é2ven 2z1é2ves
2z1é2vi 2z1évn zf2 zfe2l1em zfe2li z2féri z1fr z1g2 zgás3s zgá2sz zgés3s
zgó1s2 zhad1 zharminc1 1zi zi2ac 2zicc zi1ch z1i2dé z1i2dő zie2l zie2m
zigaz1 2zigazg zi2gáz 2z1i2gén zi1g2r zikus1s z1imma zi2n1á2r z1indu zi2nol
zi2nór zin2t1el zin2ter zin2to zi2ol zi2óc zi2óp z1i2par zi1p2l zi1p2r
z1i2rat 2z1i2rá zi2rod zi2sal zi2seg zise2l zis1ele z1isko 2z1ism zi2sor
zis3szi 2z1iste zi2tut ziú2 2z1i2vó 2zizm zi2zom 2zizz 1zí zí2n1ál zí2nár
zí2n1o zí2nu z1í2ró zítés3s zí2val zí2viz 2z1í2z z1kh zkiá2 zkie2 z2k1ing
zk2le z1k2lin z1k2lu zkon2tár z2kopi z2kópiá z2kópr z1k2ra z1k2ru z1k2val
zle2ta zle2tá zle2t1emb zle2ter zle2t1o z2log zme2g1á z2nob 1zo zoki2 zok1ir
zo2koz 2z1o2laj 2z1oll zo2mag 2zomh 2zomv 2zomz zo2naj zo2n1áll zo2ner zoné2
zo2név zon2tel zon2tol zo2nyan zo2nye zo1phi zo2ran zo2rar zo2r1as 2zorg
z1orke z1orr. z1orrú z1orvo 2z1o2x 1zó zóá2 zó1dre zó1p2 z1ó2rai z1ó2ras
zó2ta. zó2t1ért zó1t2h zó1t2r zózat1át 1zö zög1ért zö2g1öl 3zölde zöl2din
z1ölel zö2les 2zöne 2zönö 2z1ötv 2zöv 2z1ö2z 1ző ző1dr zőe2r zőren2 zőrend1
ző2ro 2zőrs 2zőrz zőu2 zpen3 z1p2l z1p2ro z1p2ró zre2d3z zren2d1ő z2s 1zsa
2zsaj zs1a2la 2zs1amu z3sapk 1zsá 2zság z3ság. z3sága z3ságá z3ságt zsá2rá
1zse z3sej 1zsé 2z3ség 2zs1ép zs1érv 1zsi 2zsir zs1iro 2zs1ita 1zsí z3sík
2z3síp 1zso 2zsor zs1orv 1zsó 1zsö 1zső zsp2 zs3s zst2 z3s2tí 1zsu 1zsú
z3súl 1zsü zs1üg zs1ü2té 1zsű zsz2 z3sza z3szá z3szeg z3szek z3szem zs3zen
z3szere z3szé z3szí z3szo z3szó z3sző zszt2 z3szu z3szü z3szű zt1a2dó zt1apá
z2t1arc zt1assz z2táld zt1á2ram z2tátj z2tátu ztá2v1i2 z2t1elo z2t1emb z2tep
z2tered z2tesem z2t1é2g z2t1é2le z2t1érté zt2hen ztia2 z2t1id zti2g z2t1igé
z2t1i2p z2t1irá z2t1iré z2t1í2r zto1g2 zto2n1 zto2ris z2torit zto1szk ztó1dr
zt2ráj z1t2ré z2túj z2t1út ztül1l z2t1üt 1zu zu2b1a 3zubb zu2b1i zu1c2
zu2g1ár zu2gí z1ugr zu2gu 2z1uj zule2 zu2l1esi z1urn zu2só 2zut z1u2ta
z1u2tá z1u2tó 3zuz 1zú 2zúj z1ú2jo z1újs 2zút z1úth z1útj z1ú2to z1úts 3zúz
1zü 2z1üd 2züg zü2ni zürkés1 2züs 2züt z1ü2té z1ü2tő z1ü2v 2züz zü2zem z1üzl
1zű zű2csi zű2k1a zű2za zű2zér z1ű2ző zz1áll z2zs2 z3zsí zz3st
  PATTERNS
end
Text::Hyphen::Language::HUN = Text::Hyphen::Language::HU
