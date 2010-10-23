# Hyphenation patterns for Text::Hyphen in Ruby: Norsk (Norwegian)
#   Converted from the TeX hyphenation/nohyph.tex file, by Ivvar Aavatsmark and
#   Frank Jensen (1992 - 1995).
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
# nohyphen.tex, skrevet av Ivar Aavatsmark, juni 1992,
# med utgangspunkt i danhyph.tex, skrevet av Frank Jensen.
# Revidert mars 1993.  I.A.
#
# 22/09/94 :Tilf¢yet noe kode ellers uforandret. - Preben Randhol
#                                                email: randhol@alkymi.unit.no
#
# 26/09/94 :Fikset en liten feil som ble oppdaget av Jon Martin Solaas
#							-Preben Randhol
# 27/09/94 :Fikset ny feil, skulle virke helt fint naa  -Preben Randhol
#
# 8/10/95  :This file was only usable with T1 encoded fonts.
#           I removed commands like \let\AA=^^c5
#           and substtuted the commands by the corresponding character
#           in Cork encoding.
#
#           This means: non-T1 encoding does work now, but best
#           hyphenation can only be expected by using T1 encoded fonts.
#
#                                               Okt. 1995, Thomas Esser
#                                  email: te@informatik.uni-hannover.de
#
# Denne filen inneholder orddelingsmønstre for norsk.
#
# Orddelingen er basert på følgende regler:
# 1. Først deles etter ordets oppbygning. Sammensatte ord deles
#    i sammensetningen. En bindebokstav kommer foran bindestreken.
#    Eksempler: bok-stav, tids-punkt, lande-vei.
#    På samme måte skilles også egne ord og forstavelser ut:
#    over-opp-hetet, fore-skrive, mis-unne, for-ut-sette,
#    bak-over, der-etter, be-undre, an-er-kjenne, atmo-sfære,
#    inter-esse, pro-blem, di-ftong, dis-kret.
#    Etterstavelsene -aktig, -inne og -skap skilles også ut på
#    denne måten: del-aktig, lærer-inne, sel-skap.
#    Bøyningsendelser og andre etterstavelser behandles imidlertid
#    som en del av et usammensatt ord.
# 2. Usammensatte ord og bestanddeler av et sammensatt ord
#    deles slik at minst to bokstaver, herav minst en vokal,
#    havner på hver side av bindestreken.
# 3. Usammensatte ord og bestanddeler av sammensatte ord kan deles
#    slik at ny linje innledes med en konsonant: vi-ten, kys-ten,
#    tun-ge, venst-re, led-de-ne, kjø-ring, kob-ling, klat-ret,
#    vans-ke-lig, fris-ke, he-vel-se, end-re, lig-gen-de.
#    Unntak fra denne regelen beskrives under punktene 4-6.
# 4. Hvislelyder behandles som en konsonant: ma-skin, ni-sje.
# 5. Enkelte fremmedord med dobbeltkonsonant må deles mellom de
#    like konsonantene: ag-gres-siv, at-trak-tiv, sup-ple-ment.
# 6. Ved deling foran trykktung stavelse føres så mange konsonanter
#    som et ord kan begynne med, til ny linje. Eksempler: nøy-tral,
#    ma-tri-se, sy-stem, ma-krell, in-du-stri, re-si-prok, hy-drat.
#    Slik deles også enkelte fremmedord selv om konsonantgruppen bak
#    bindestreken blir uvant: sym-ptom, asym-pto-te.
#    Delingsregelen for trykktung stavelse beholdes gjerne ved
#    avledninger av ordet, selv om stavelsen mister trykket:
#    sy-ste-ma-tisk, in-du-stri-ell.
#    Av og til foretrekkes imidlertid forskjellig deling, særlig hvis
#    trykket veksler mellom stavelsene foran og bak konsonantgruppen:
#    sta-ti-stikk, sta-tis-tisk, pro-te-ste-re, pro-tes-ter.
#    Regelen om flest mulig konsonanter til ny linje foretrekkes også
#    for en del fremmedord uten etterfølgende trykktung stavelse.
#    Eksempler: hy-dro-gen, fe-bru-ar, pu-bli-kum, bi-blio-tek.
#    Dersom de ledende konsonanter tydelig hører til stavelsen foran
#    bindestreken, deles imidlertid etter regel 3. Eksempler:
#    tek-nik-ken, re-sig-ne-re.
#    Konsonantgrupper som er fremkommet ved sammentrekning i
#    forbindelse med bøyningsendelser, deles etter regel 3. Eksempler:
#    ak-sep-tab-le, simp-le, met-risk, re-gist-re (men re-gi-stre-re),
#    sent-re (men sen-tre-re, sen-tral), te-at-re (men tea-tralsk).
# 7. Usammensatte ord og bestanddeler av sammensatte ord kan deles
#    mellom to vokaler som ikke danner en enhet. Eksempler:
#    vei-en, fløy-el, slå-ing, over-sjø-isk, ni-vå-et, ego-ist,
#    re-gi-on, pe-ri-o-de, va-ri-a-bel, in-ge-ni-ør.
#    Vokalgrupper som danner diftonger eller som utgjør klangenheter,
#    må imidlertid ikke deles: løy-pe, teo-ri, si-tua-sjon, fio-lin,
#    re-gio-nal, va-ria-sjon.
#    Foran endelsen -lig unngåes også deling mellom vokaler:
#    bøye-lig, opp-nåe-lig.
#    Ved vokalgrupper kan bindestrek etter regel 3 måtte bortfalle.
#    Således tillates ikke bindestrek foran n i endelsen -ene og foran r
#    i norsk endelse -ere når en vokal står foran endelsen: bro-ene,
#    skje-ma-ene, ei-ere, fri-ere, høy-ere.
#    I fremmedord tillates imidlertid deling foran r i -ere: va-ri-e-re.
#++
if defined?(Text::Hyphen::Language::NO)
  raise LoadError, "Text::Hyphen::Language::NO has already been defined."
end

require 'text/hyphen/language'

Text::Hyphen::Language::NO = Text::Hyphen::Language.new do |lang|
  lang.patterns <<-PATTERNS
.ae3 .ak3k2l .ak3k2r .an1d2ra .an3k .an1s .anti1k2l .anti1k6r .anti1k4v2
.anti1p .be5la .be1t .bi4tr .der3i .der4iv .de2s1to .diagno5 .di4s3 .ek2s1k
.ek4s5p .er1in .er1ob .for4st .hove4d3 .hoveds2 .il4t1r .nabo1 .om1 .ove4
.pro1g6r .pro5k2l .pro1sp .re4in .re4u .sinn4s1t .si4s3te .sk6 .stat4s5
.st4r .så3 .så5re .til3 .unn3s2l .ve4l1e4g .ve4l1e4t .yt5r .øv3r .øy4e a1a
a2b1av ab1le a2b1re 3abst ade5la 5adg ad2op ads2k ad1st a1e ae4der ae2ne.
a1fj a4f3le a4gef a4gi ag5in ag3lø ag5si a4gy a3h a1ikk a1ing a1isk a1ism
a1ist a3j a1k a3ka a4kad a2kb a2k1d a3ke a2kf a2kh a4kk a2kl a2kn a3kr
ak2rel ak3s2m a4kt 3akti a3la a4l1ad alder4s5 a1le a1li a4l3ins a2l1i2so
al3k al4kn 4alkv all2s5kap a1lo a2lomr a2l1oms al1sa al5si al1st a3lu a1ly
a1læ 1a2manu am4pa 3a4naly an3erk an5g2rep an5g2rip ang2s3e ang3s4et
ang3s4ik angs3in ang3som an1ion an1i2so an4k5r an5si 1ansku an1s2t an1sv
a3nu 3anv a5o ao2pe a5pe a3pi a5po ap3p4lau ap3p4li ap3p2re ap3p2ro a1ra
ar5av 1arb a1re a2rea 3arg a1ri 1arkiv a3ro ar3so ar1st a1ry a1rå a3sa a1si
a3so as4s5kr as4t3ro a1ta1 a2t1aks a1te a5t2e5g a1ti a1to ato5v a3tr a4t1re
at1s4pr a1tu a3tø a1tå a4u4v aue4r a5va a1ve av1b 3avg a1vi 1a2vis. 1a2vise
3a4v1l av3r av1s4 av1v a5væ a3z 1ba ba4k1 ba4k2e ba5k2er ba5k2u ba4ti 4b1b
b2b1h b2b1k b2b1l b2b1m b2b1t 4bd 1be be1dra be1driv be1dro be1k be3kj be3ro
be5ru be3s2 bes4p be4s3te1 be5s4te2m. be5s4te2mm be5s4te2mt be1tr 1bi b1j
blom4st3r 1blå1 4b1n 1bo bo4gr bo3ra bo5re bor2t5r 1br4 1bruk2s1 bry5som
brød3 4bs b1si b1s4k b3so b1st b5t 3bu bu4s5tr 4b1uts b5w 1by by3s 5bæ 1bø
1bå 1ce 1ci ci3e4s1 3da d5anta 4d1art da4s 5dat 4d3av d1b d2b1k 4d1d d2d1b
d2d1f d2dh d2d1p d2d1r d2d4s1 d2d1t d2dv d2d1år 3de de5d ded4ren 4de4lem
de2n1om de4n1un der5eri de4r1etter de4rig de4s3ill d1f 2d5g4 d3h 1di di2ale
di2alo di1e dig5sti di3l di4ll 2d1int di5s4i dis4k3l dis5t6r d3j d1k d1l d1m
4d1n 3do dom2s3l do1o 4dop 4d1ord 4d5ov d1p 3d2rif d1risk 3drøv ds5an ds5av
d2s1ev ds5in ds1k 2d4s3l 2d4sm d2s3n ds3or ds1p ds3s4k ds3s2l ds3s4t ds3s4v
d4s1tj dstå4 d4su ds5v d5s4y d3ta d5t2e d1to d3tr 1du dub5 d1v 3dy 3dø
dør1g2l 4d1øs e1ab e2abi e5ad ea1g e3ak e1al e2a1li e3a2l1inn e3an e4ane
e4ano e5ap e1a2rea e1ark e1a1t e2atra ea4t1re e1a2tt e3av e1b2l eblikk4s5
ebs3 e1ch e4d5ar edb2l e4d1ei ed3re ed3rin ed4str e1då e3e 4eelem ee2ne.
ee4r 3eff e3fj e3fr egg5a e4ggek e4gg1r eg2g4s5 e1gr e3gu e1h e4i eid2s1
eid4s3k eid3s2om eid4s3t2 eid4s3pr eie4r e5inn e5inv ei5el e5isk. e5iske.
e1j e2jn e2k e3ka e3ke 2e3kj e3k2l e4k3ler e4k3let e4k3ling 4e3ko e3kr ek5sa
3ek5sem eksi5s2t 3eksp ek4s3t eks4t1av ek5su e3ku e5kv e5ky e1la. e3lad
e1lag el3ak el1al el3ar e1las e3le e4lek 3e4lem e1li 4elig 5elim e4l3int
elle2 e3lo e4l1om el5sa e4lse el5s4en el1to el4t1ri el4t1ro e5lu e3ly e3læ
e3lø em4p5le em1s em2s1l e1mu en5ak e4nan en1d2rek en5g2rad en5g2ro 4enn
en1opp e2nord en4s3inte en3so en1s4v ents1k e3nu e4n1ut e4n1ø2v e1nå e2n1år
e1o e2o3an e2o3d e2ogr e2o3li e2o3o eo2pe e3opp e3ord e3ov epi3 e1pl e1po
e1pr e3ra e4rag e4rak e4r3av er1d2r e3re er5ege e4r1ess er4g3r e3ri e4rib
e4r1inn er1k er2kna er2kni er2kt er3o4pp ero5d er5ov er1s er2sk er3s2ki
er3s2kj er3som er5t4r e3rum er5un e5ry e3rø er5øn e3se e3si e4s1m e3so
e5sper e4s3ting e1ta eta1b2le e1te etek4s e1ti e3tj e1to e3tr e4t1ris
ette6r1a e3tu e1ty e5tæ e5tø e1u2ke eu1kl e3um e3un 3euro e1usi e3ut e1va
e3ve e4v3erf e1vi e1væ e3æ2 2e3ø2 e5å eå4r 1fa fa2g1r fags3 fall2s1 f4ar
fa1se fa4t1r f1b f1d 3fe fei2l5o fe5no fe2s1t fe5s4tere fe5s4tert f1f f2f1b
f2f1c f2f1k f4f3l f2f1m f2f1opp f2f1r f2f1s f1g f1h 1fi fis4k3l f1k 3f2l
5f4la 1fo fol2k5l fo2r1an for1d2ra for1d2riv for1edl fo2r1eld fo2r1els
for1e2vig fo4r1e6n for1g2l fo4ri fo2r1o for1som for5st for3u4 fo2r1ø4 f1p
fra5t2 fred1som frie4re f1s4 4ft ft1b f3ta f1te f1ti ft4s3 fts5k ft5s4pr
ft4sår f5tvi 1fu f1v 3fy 3fø fø5f4 føl1som fø4r5en få1k 1ga 2g3art gart4s5
g1b g1d 3ge ge1d4ren 4gelem 4g5enden ge2o ger3in ge3s g3f 4g1g g2g1b g2g1d
g2g1f g2g1h g2g1l g2g1m g2g1s g2g5t g2g1v g1h 1gi gi1b 2g1inte gi1on. gi1one
gi1ons gi1po gi5st4r 5gj gje2n 3gjø1 g3k g1l 1g2lad 1g2las g2lb 1g2lyf
1g2lys g2lø g1m gn1d2r g1nu 3go 4g3om g5ov 2g3p 1gr gra2m1a 5g4rav 5g4rens
5g4rep 5g4r2o5v gro6v5s 5g4rup gr4u g4s3a g5s4at gsde4len gsha4 g3sla g2s1n4
gs3or g4s3p gs3s2k gs3s2t g4sti g1su gs3v g4s5y4t g4s1ø g3så g3t g4t1i1f4
g3v 1gy g5yd 3gå 4ha. ha2s1t h4av hav2s5p hav2s5m heds3 he4r1etter 5het
het4s5 2h1h hi4e hi3s ho5ko ho1pe ho5ve hovs3 4h3t hun4 hun5nen hun5kj hund3
hu4s3tr hve4r3an hvo4 i1a i2a1b4 i3a2b5le i2a1d i2a1f i2a1g2 i2a1k i2a1li
i2a1m i2a1ne i2a1s i3a2s1m i3a2s1t i2a1t i3b i4b1le i3dr ids5k idi1om.
idi1ot. id4t1 i1e4gen i2ek i1el i1en 4i5en. i5e2ne. i2eo i2ep i3er i2es
i3es. i3et. i2eti if3f2r i4f3le if3r i5fto i3gu i4gut i3h i5i i5j i1ka i1ke
i1kj ik2k1ord ik1l ik5led i5ko ik3re ik5ri iks5t ik4tu i3ku ik3v i3lag i3le
il5ei il5el i3li i4l5id i4l3int il3k4 ilmeld5 ilmel4di i1lo i5m2u ind4eks
in1eks 3inf ing2s1 in4n1ar in4n1ad in2n1ord inn5s4e inn3s2l in2n3t in2n1u
in4n1y in1s in4sv 5integ intet5s6 in2t1re i3nu io3a i5pi i3pli i3plu i5pr
i3r i4rd i4rf i4r3k ir4k3n i3ri i4rl i4r1m i4r1s i4r5t i4r1v i1s i2sf i3si
i2sk is5kj is1ko is5ku is1kø i2sl i4sm i2s1n i4s5p i2sr i2s1t i3s2to i2s1un
i3s2und i5sua i2s1v i1ta i1te i1ti i3to i3tr i4t5re i1tu i3ty i5tæ i1u i2ume
ium2s1 i1va i1ve i1vi i1ø iø4k i2ø1si i1å4r jds1 j4e je2s1t j5k ju5s2t 3kap
k2arb ka2s1t 1kav k5b k1d2r kel5s ke1mu 2k1endr 2k1e2nerg ke1ple ke3sk
ke4s1kv ke3s2t 2k1f 4k1g k3h ki3e 1k2ing ki4n1u2 3kjø 4k1k k2k1aks
k2k1a4naly k2k1a2rea k2k3arb k2k1au k2k1av k2k1b k2k1d k2k1ei k2k3f k2k1g2
k2k1h k2k1int k2k1l k2k1m k2k1n k2k1opp k2k3p k2k1r k2k1s kk1s4a kk2s5av
kk2s1f kk3s4k kk3s4n kk3s2pe kk2s1s kk3s2l kk3s2t kk4s3tep kk2s1v k2k3t
k4k1ut k2k3v k2k1øk k2k1øl 6kl. k5lak kla4t1r k1le k4led k1lig 3klu kne2b3l
k4ny 3kod 1kon 1ko1o ko3ra 3kort ko3v k1pl k1pr 1kra kraft5s4 5kry kry5sta
4k1s ks3an k2s1f k1si ksi3st k2s3k k2s3l k2s1m k2sn k2s1p k2s1r ks3s2t
k2s3tant k2ste k2s1ti k2str k2stu k2s5v k1t 6kt. k4tar k2td k4terh kti4e
k2t1inn k2tm k2tn k2t1opp k2tov k2t5re kt3s kt4s1al kt4s1dr kt4s1f kt4s1g
kt4s1kil kt4s1kon kt4s1l kt4s1m kt4s1p kt4s1r kt4s1s kt4s1tr kt4s1ut kt4s1v
kt4s1økn k2t1ut k2tv ku2e ku3el ku3er ku3et 2kuly 3kur 1kus ku2s1t 3kut
1k4var k2ved k2vet 1k2vis 1k2vot k4vu 3kå 1lab lad3r la1d4ra 5lagd la4g1opp
la4g3r 1lam 4la4nal la2s1t 1lat l1b ldiagnos5 ld1r ld1so ld2s1t 1le. 1led
le4k1re 4lele le4mo 1len le2o 1ler 1les 1let leu2m l1f lfin4 lfind5 l1go
l1gæ l3h li4ga 1ling 2lingeni li1on li2onæ l3j 4lk l1ka l1ke l1kj l1k2l4
l1ko lk3s4k lk3s4p l3ky 4l1l l2l1au l2l1b l2l1d ll4e l2l5e2sti l2l1f l2l1g
l2l1h l2l1k2 l2l1l l2l1m l2l1oks ll2om l2l3områ l2l5opp l2l1os l3l2o1sk
l2l1r l2l1s ll2s1d ll2s1f lls4i lls4k ll3s2l ll4s3lo ll2s1m ll3s2med ll2sn
ll2s3ord ll2sr ll2s3s2t ll4s3try l2l1t l2l1v lm1opp l5mu lo4du l3op 2lo2per
l4opp 4l1ord 4l3org 3lo2v l2o5ve 4l3p l4ps l4pt l3r 4ls lses1 l1sig ls3in
l5sj ls1l ls3s2l ls3s2t 6lt. l1ta lt1bl l1te l4t5erf l3ti lt3o l3tr lt3s6
l3tu l1ty lu3l lut2t5r lv1an l3ve l3vi l2v1l l2v1s l3væ l4v1å4r lønn2s5kr
lønn2s3n lønn2s3tr lønn2s3øk løp4s1 5løs 1ma ma2g1r ma4k1ro 2ma4nal m1b
mbet2s1 mb2l m3d 1me 2melem ments1 mer2kv me4t1re m4e1ur m3f m1g m3h 1mi
mi3k mi4k1ro min2s3 mi4o mis3s2t mi4s3ta m3k m1l 4m1m mmen5 mm2end2r mm2ut
m1n 3mo mo4da 4mop m1opp mor1som mot1s2v 4m5ov m5pa m1pe m3pi m3p2l m5po
m3pr mp5s4kr m1på m1r mse5s ms5in m1sk m1s2ki m2s1kl m2s1kr m4s5p ms3s2t
ms3v ms1år m3ta m3te m3ti m3tr m3tv m3ty m5tå 4m1ut 1mul mu1li 2m1ull m1v
3my 3mæ 3mø mød1re 3må 3na 4nak 4n1art na2s1ki na4t1re n1b 4nd nd3d2 nd1k2l
nd1r nd5si nds5n nds1om nd5s4p nd6s5par nd3st 1ne ne4da ne4d1r ned5s4l ned5s
ned6sv ned5t ne5in nemen4 nement5e ne5sl ne4utr neæ3r n1f n4g1enh n2g3lys
ng3lø n4go n4g1r ng4s1la n4g4s3t ng5s4tige ng5s4toff ng4s1u ng4s1år n1gu
4n1h 1ni ni3e4s 5ning ning6s5 ningst6 2n1inj 2n1j nje3s2ty n1ka n1k4e nk1in
nk5led nk3k2r n1kj n1ko n3kr nk3s4k nkt4s nkt5s4k n3ku nk1v n3kjæ 4n1l n1m
4n1n n2n1ak n4na4na n4n1ant n2n1b n2n1d2r n2n1ei n2n1eur n2n1f n2n1g n2n1int
n2n1k6 n2n1m n2n1ove n2n1p n2n1r n2n1s nn2s1ar nn2s1av nn2s1d nn2s1ei
nn2s5e4ff nn2s1f nn2s5kl nn2s1l nn2s1m nns2n nn2s1ord nns4p nn2s5pl nn4s5pr
nn2s1r nn2s1s nn2s1ut nn2s1v nn3s2vin nn2s1ø2ko n2n1t n2n1ut n2n1år nnå4re
1no 2n3ord n5p n3r 4ns ns4e3f nse4ff ns4e1p n3si n5s4inf nsin1k n4s1inn
n2s1kom n2skt n1s4ku n2s1omr ns3po ns3s4k ns3s2t n1sta n1s2tem nst1v n1sty
6nt. n1ta nta3le n1te n1ti ntiali4 n3to n1tr nt4su n3tu n4t1ul n3ty n5tæ
2nuly 4n1v 3ny n3z 3næ 3nø o2a oa4nal o3ar o4as ob3li o2b1r odel2s1t o4din
od5ri o1e 2oelem oe2ne. oe4r o4f3le of5r o4gek o4gel og5re og5sk o5h o1i4d
4o3in o1isk o1ism o1ist o1j o1k o3ka o1ke o2k1h o4k1l o2km o4kn o2k1t o3ku
o3la o3le o1li o1lo ol4t1r o3lu o5ly o5læ o2marb om2ele om2ene 1omr om1si
om1sl om1s4ve o4n1av on5g2r on3k o1nu oo2pe o1p6e o3pi op4p1ad op4p1arb
op2p1etter op2p1of op2p1und op2p1ø2 1opsj 1o4ptim 4or. o1ra 3ordn ord3s
o3re. o3reg o3rek o3ren o3rer o3re3s o3ret o3ri 3o4rient or5im o4r5in or3k
or2n1ne o1ro or3sl or3so or3s2t o2r1t4r o1r4u5m o1rø or5å4r o1sa o3si o3so
os1tu o3t o4t1v o5un o1v 1o4ver over1al ove4r3an ove4r1ens over5s4 o4v1l
o2v4s ov5si o5å 3pa pa5gh p5anl p3d 3pen 1per pe1ra perb2l pe3s pe4sk pe4u
4p5h 1p2la plag1som pla4s3t 5p4lek ple2o ple4u 1p2lo p4løy p3m p3n pne4u
5pok 4po3re po2st1o po2st1å 3pot potet5s6 4p1p p2p1b p2p1d4 p2p1evn
ppel1s4in p2p1f p2p1g p2p1il p2p1j2 p2p1k p2p3l p2p1m p2p1n p2p1over p2p1r
p2p1s4 pp5s4kr p2p3t2 p2p1v pr4ak 1pre 1pros 6ps. p3s2k p4s3kau p4s3kr 4p3so
ps4p ps3s2t p3st ps3v 2p1t 6pt. p2t1r 1pu pu5b p5ule p5v 3py 5ped 1pæ på3
på4sk på5skj på5s4kr på5sku på5sky qu4 ra5is rak1au ral1v2a 4r1angi 4rarb
r4avl r1b2 r4d5ar rd3d2r r4deks rd1n rd1r rd4s3 rd5s4e rd5s4j rd5s4ki rd5skj
rd5s4p rd5s4t redd5s2om re1dra 2reff 1rel re5la 6relem 4r1endr 2r1e2nerg
1re2o 1re3org re1pe re3sp 5r2essu re5s4tere re5s4tert 4r1e2stim re5s4u
re5u2ni r1f r1gu r1h ri3abe ri1e rig4s1 rik4s1 ri5la 4rimo r4ing 2r5ingeni
ringse4 ringso4r 4rinp 4rint ri1od ris3s2t r1j r3ka r2kb r1ke r1ki r1k2l
r1kr rk3so rk4s3pr r3ku r5kjæ r1l rm1s6l r3mu r1n r2n2s1n r2n1f ro1b ro3p
ro4p4s ropp2s5 4ropti r3ord r3org r3ork r3p r4p1s rps4k 4r1r r2rakti r2r1b
r2r1d rrd2r rre3s r2r1f r2r1g r2r1k r2r1l r2r1m rro4n5 r2r1p r2r1s4 r2r1v
r1sa r1si rs1k r3s2ka r4s4k1n r3s2ko r3s4kr rsk3t2 r3s4ku r4s4k5v r3s2ky
r3s2kå rs4n r2s5nød r2som r3sp r4s1po r4s3r rs3s4k rs3s4t r5s2tu r5su r3s4v
r3s4ø 6rt. r1ta r5tal r1te r4teli r1ti r1tj r3to r4t5or rt5rat rt3re r5tri
r5tro rt3s rt4s1d rt4s1f rt4s1kon rt4s1lig rt4s1m rt4s1r r5ty r5tæ r5tø
run4da ru2s1t r1uts r3va r1ve r3vi r2v1l r1v4r r3væ ry4s 4røn 5rør rø1vi
3råd s3af 5s6aga 3s2ak sak2s1 sak3s2e sak3s2øk 1sam sa4ma sam5s6 s3ap s1ar
2sart sa2rea 1sat 4s1b 1s2ce 1s2ci 6s1d sdy4 1se s4ed 4s1e4gen sek4s 2seksp
se5lek sel1som 2s1e2nerg 2s1endr sen2t1re sen3t2rer se2ps 4s1erkl se5ro
se5ru se4som se4s3pr 4s1evn s1f 3s2fær 4s1g4 sim4p3le sin1d2rev 4s3h 4sinf
s6ingu 4s1i2ni si3s2te si4s3te. si5stens 5sit si2tera 5siu 3sj 4sj. sjek2t1a
5s4jon sjon4s5 s3ju s4juk 5s2jø sjøl1 6sk. 3s4kaf 3s2kala 3s4kap 4s5kapa
skap4s3 3s2katt s2k1d 2s1ke s2k1h 1ski s2kildr ski4n1a 2s1king s2kip skip4s5
1s4kj s4kjøv s3k2l 4s5koef 3s4kole s3kr 3s4krank 3s4krap s4kei 3s4krenk
3s4krev 5s6krib 3s4krid 3s4krif 5s6krip 3s4kriv 3s4kritt sk5s4 6skt. skue4r
s4kut 3s2ky s4kå skån1som sl4akt s1le slen2t1r 1s2lett s2leng s2lep s1li
1s2lip s2lit slit1som slo3 3s2lu s5ly s2lør 3slå s1m s2murn s2murt sm2ut
s4my s4møri sn2a 1snak sne4k1r s2nik 4snin 1s4nit snitt4s1t s4næ so3k 5sol
4s3omf 4s3omk 4s3omr som4tv 3son 4s1o2pp 4s1ord sp4 s4palt s4pan spar1som
3s2peil 3s4pen s2pera s2perr 3spes 3s4pi s1pl 3s2pr språk1l språk1v 1s2pur
s4py s5r4 4s1s s2s1all s2s1b s2s1d s3s4en s2s1f s2s1inj s2s1inn s2s1k ss2kr
s2s1l s4s3luf s2s1m s2s1op ss2pl s4s3pr s2s1r s2s1t ss3tab s4s1u4l s2s1v
s2s1å 6st. 1s2tab 5s4tam 3s2tan 4sta4na 3s2tasj 3s2tat 1stav 2st1b 4s4td
s1te 4ste. 3s2ted 4s3teda st2eg 3s2tein 4sten. s2tendelig s2tendig 4stene.
1s2teng 4stens. 4s3teo 4ster. s2terk 4stes. 4stet. s2teu 1sti 2stid 3s2tikk
3stj 6s2t1m 1sto s2t5om 3s2t2r 5s4trat 6s4t5re. 6s4t5rene. 6s4t5ret.
4s4t3ring 4s3tro 5s4truk 5s4trøm strøms5 4st1s 5s2tud st4uv 2s2t1v 1sty
5s6tyr 5s2tø 3s2tå 1su2b1 s1u2ke suk1r 3sul 5s2um 3sur sur4s5k 2s1u2t s2var
svar4s s3ve s4vev s4vu s4vø 3s4y syn1d2r 1sy1s 1sys2t 1sæ sæ2r1e4g sæ2r1e2i
sær1int 1sø så4r 5ta. ta1b2lett 1tag ta4l1ak tanns3 4tanv 1ta4s3t ta5s4t4r
t4avl 4tb tede4l te1dra teds5 1teg 4t1e4gen 5tekn te5lek te2n1om te2o teo1re
te5ret 5term te3ro tes2teks te5s4tere te5s4tert 2t1e2stim tet4s5 te4uto 4t5f
6t3g 4t1h tialis5t 3tid tid4s5 ti4en til1d2r ti4l3eg ti4li4s til1s2t
2t1i2tera ti5t6r tiø4 tj4uv 4t3k 4t1l tli4s5 4t1m 4t1n t4ok. 4t1o2per to1po
4topti to3ra to4r1ar to4r1as to4r1au to4r1av to1re to1ri tor4m 4t3p t4r4a
tran4s tr4eff 3trekk. 4tres t6rettel 4t1ring tr2o5ver 1try t4råd 4ts t3si
t4s1inn t1s2kr ts1kv ts4pa ts3pr ts3s2k ts5s2t t3st ts1v t3så 4t1t t2takti
tte4r tte5ra tte6r1alg tte5re tte5ri tte5ræ t2t1h t2t1inn t2t1m t4t1opp
t2t1s tt2s1adv tt2s1ald tt2s1an tt2s1ar tt2s1d tt2s1f tt4s1j tt2s1k tt3s4kr
tt2s1l tt2s1m tt2s5p tt4s1tj tt2s1v t2t1v t2t1å4 t5uts 5tur t3ve tvil1som
1t2vist t4vu t3væ tynn3s2l 1typ tys4k3l u1a u2ale u3alen u3aler u3alet u2ali
u2ane u2are u2asj 1u2av ud3d4hi ud1r u1e u3e2n ue4t5 uf5f4l ug3lø ugs3 u5gu
u3i u4ida u4ide u4idum u4ine u4ise u5i2sen u4isj u4ite u4iti u1ke uke4ri
u1ko uk4ta uk4tr u1la u1le 3u4ly u3læ u1lø u2m1enh u2mint ums3l2 um2sn
um4p3le um4p3li uni1o2n unions1 un2s1t un2t1r u5pe u1pi up3l up3p4l u3ra
u3re u4r3eg u1ri u3ro u4r1opp urs1l u3si u4sikker u5ska u5so u5spek u4s5pen
us5v u1ta ut1ad1 u1te ut1eksa u1ti ut1j 5utl u1to ut5r ut3s uts4pr 3ut1v
ut4vet ut2vil u2t3vist ut1ø2v ut1ånd u1u2m 3u3v u4vl u4vst u1ø va3d va4dm
va4ds va4k1re 2vakti 1var van4n1av 3varm var1som va4t1r 6v1bl 2v1d 1ved
veg1g2r ve4k4s vek5sel ve4l5opp ve4l1ord vel1s2t ve6l1ut 6v1endr ven2s1ta
ven4st3re ven2s1ty 3verd ve4reg 1verk v4erv ve3s ve4ske ve4st ve5s4tere
ve5s4teri ve5s4tert 1vet 6v1g4 v5h vi2ce vide2o vi4l3in vil4t1r vim4p3l
vin1d2ru 1vis vi2ss v1j 1v2ju v5k vl4 v3le v5li vls1 2v1m 2vn 1vo 4v5om
vor1ett v5p v1r v2s5f vs3s2k vs3s2t v3st v5su v3t 6vt. v4t1l 3vu v1v 3vå
vår3s2 1xa 1xe 1xi x1n 1xy y3a yde4rer y5dr y3e ye4der ye2ne. ye4r y4f3l y3i
y3ke y5ki yk3li y3kl4o y3ko yk4s5 y3kv y5li y5lo ym3p2t y5mu y5na yns5 yn1t4
y5o y1pe y3pi y3po y3re yr3ek y3ri yr1ull y1rå y5se y3si y2sl y3te y3ti y3tr
y1u y5ve y1vi y5væ y1å4 1ze ze2o zi5o 4z1z æ1re æ3ri ær4ma ær4mo ær3opp
ær1s2ki øde3 ø3e øe2ne. øe4r ø4f3l ø1i ø3ke 1ø2ko ø3le øl1v2a ønn4s1t øn4t3
ø1pe ø1pi ø1re øre5d ø3ri ørne3 ør5o ør2s1t ør1ø ø1si ø1te øt1r ø1va ø1ve
øver2st ø2v1r ø1væ ø4y øy4em øy5en øy5er øy5et øy1f øy3s2 øy4s3t øy1v å1d
å2d1f å2d1h å2d1k å2d1l å2d1m å2d5s4l å2d3s4n å2d3s4p å2d4s5r å2d1t å2d1v
å1e åe2ne. åe4r åg3lø å5h å1i å1ke å1kj å3l å4lb å4lf å4lg å4lr å4lt å4l1ø
å1pe å1ra å3re år2s1 år4s2j år4s5k år4s5p år2s5t år4s5v å3t
  PATTERNS
end
Text::Hyphen::Language::NOR = Text::Hyphen::Language::NO
