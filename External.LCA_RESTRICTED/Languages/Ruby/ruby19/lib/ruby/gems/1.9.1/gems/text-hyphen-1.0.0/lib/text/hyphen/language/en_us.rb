# Hyphenation patterns for Text::Hyphen in Ruby: US English
#   Converted from the TeX hyphenation/bahyph.tex file, by Donald E. Knuth
#   with additions by Gerard D.C. Kuiken.
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
# The Plain TeX hyphenation tables [NOT TO BE CHANGED IN ANY WAY!]
#   % Patterns for standard Hyphenation Pattern Memory of 8000.
#   % Can be used with all standard TeX versions.
#   % Hyphenation trie becomes 6661 with 229 ops.
#   % These patterns are based on the Hyphenation Exception Log
#   % published in TUGboat, Volume 10 (1989), No. 3, pp. 337-341,
#   % and a large number of bad hyphened words not yet published.
#   % If added to Liang's before the closing bracket } of \patterns,
#   % the patterns run errorfree as far as known at this moment.
#   % Patterns do not find all admissible hyphens of the words in
#   % the Exception Log. The file ushyphen.max do.
#   % Can be used freely for non-commercial purposes.
#   % Copyright 1990 G.D.C. Kuiken. e-mail: wbahkui@hdetud1.tudelft.nl
#   % Postal address: P.O. Box 65791, NL 2506 EB Den Haag, Holland.
#   % Patterns of March 1, 1990.
#++
require 'text/hyphen/language'

Text::Hyphen::Language::EN_US = Text::Hyphen::Language.new do |lang|
  lang.patterns <<-PATTERNS
.ach4 .ad4der .af1t .al3t .am5at .an3te .an5c .ang4 .ani5m .ant4 .anti5s
.ar4tie .ar4ty .ar5s .as1p .as1s .as3c .aster5 .atom5 .au1d .av4i .awn4
.ba4g .ba5na .bas4e .be3sm .be5ra .be5sto .ber4 .bri2 .but4ti .ca4t .cam4pe
.can5c .capa5b .car5ol .ce4la .ch4 .chill5i .ci2 .cit5r .co3e .co4r .con5gr
.cor5ner .de3o .de3ra .de3ri .de4moi .de5riva .des4c .dictio5 .do4t .dri5v4
.driv4 .du4c .dumb5 .earth5 .eas3i .eb4 .eer4 .eg2 .el3em .el5d .en3g .en3s
.enam3 .eq5ui5t .er4ri .es3 .eth1y6l1 .eu3 .eu4ler .ev2 .ever5si5b .eye5
.fes3 .for5mer .ga2 .ga4s1om1 .ga4som .ge2 .ge4ome .ge5og .ge5ot1 .gen3t4
.gi4b .gi5a .go4r .han5k .hand5i .he2 .he3mo1 .he3p6a .he3roe .hero5i .hes3
.het3 .hi3b .hi3er .hon3o .hon5ey .hov5 .id4l .idol3 .im3m .im5pin .in1
.in2k .in3ci .in3s .in5u2t .ine2 .ir5r .is4i .ju3r .kil2n3i .kil2ni
.ko6r1te1 .ko6rte .la4cy .la4m .lat5er .lath5 .le2 .le6ice .le6ices .leg5e
.len4 .lep5 .lev1 .li2n .li3o .li4g .li4t .lig5a .mag5a5 .mal5o .man5a
.mar5ti .me2 .me4ga1l .me4gal .me5ter .mer3c .met4ala .mi1s4ers .mim5i2c1
.mimi4cr .mis1 .mis4ers .mist5i .mo3ro .mon3e .mu5ta .muta5b .ne6o3f .ni4c
.noe1th .non1e2m .od2 .odd5 .of5te .or1d .or3c .or3t .or5ato .os3 .os4tl
.oth3 .out3 .pe5te .pe5tit .ped5al .pi2t .pi4e .pio5n .poly1s .post1am
.pre1am .pre3m .ra4c .ran4t .ratio5na .rav5en1o .re1e4c .re5mit .re5stat
.ree2 .ree4c .res2 .ri4g .rit5u .ro4q .ros5t .row5d .ru4d .sci3e .se2n
.se5rie .self5 .sell5 .sem4ic .sem6is4 .semi5 .semid6 .semip4 .semir4
.semiv4
.sh2
.si2 .sing4 .sph6in1 .spin1o .st4 .sta5bl .sy2 .ta4 .ta5pes1tr .te3legr .te4
.ten5an .th2 .ti2 .til4 .tim5o5 .tin5k .ting4 .to2q .to4p .to6pog .ton4a
.top5i .tou5s .trib5ut .un1a .un1e .un3at5t .un3ce .un3u .un5err5 .un5k
.un5o .under5 .up3 .ure3 .us5a .ve5ra .ven4de .vi2c3ar .vi4car .we2b1l
.wil5i .ye4 1bat 1bel 1bil 1c4l4 1ca 1cen 1ci 1co 1cu2r1ance 1cus 1cy 1d2a
1d4i3a 1den 1di1v 1dina 1dio 1do 1dr 1du 1e6p3i3neph1 1eff 1exp 1fa 1fi 1fo
1fy 1g2nac 1ga 1gen 1geo 1gi4a 1gle 1go 1gr 1gy 1head 1hous 1je 1k2no
1ke6ling 1kee 1ki5netic 1kovian 1l4ine 1le1noid 1lec3ta6b 1lent 1lum5bia.
1lunk3er 1lut 1ly 1ma 1men 1mo 1mu 1na 1nen 1nes 1nou 1o1gis 1ogy 1p2l2
1p4or 1pa 1phy 1pole. 1pos 1prema3c 1room 1s2pacin 1s2tamp 1sci2utt 1sio
1sis 1siv 1so 1stor1ab 1su 1sync 1syth3i2 1ta 1tee 1tent 1teo 1teri 1tia
1tim 1tio 1tiv 1tiz 1to 1tra 1tu 1ty 1va 1verely. 1wo2 1zo 2a2r 2adi 2ale
2ang 2b1b 2b3if 2b5s2 2bf 2bt 2c1it 2c1t 2c5ah 2ce. 2cen4e 2ch 2cim 2cin
2cog 2d1ed 2d1lead 2d1li2e 2d1s2 2d3a4b 2d3alone 2d3lo 2d5of 2dag 2de. 2dly
2e1b 2e2da 2erb 2ere. 2ero 2ess 2estr 2f3ic. 2f3s 2fed 2fin 2ft 2g1o4n3i1za
2g5y3n 2gam 2ge. 2ged 2gue 2h1n 2i1a 2i1no 2ici 2id 2ie4 2ig 2ilit 2in.
2in4th 2ine 2ini 2inn 2ins 2int. 2io 2ip 2is. 2is1c 2ite 2ith 2itio 2iv 2l1b
2l1n2 2l1s2 2l1w 2l3h 2ld 2lf 2lm 2lout 2lp 2lys4 2mab 2mah 2med 2mes 2mh
2n1a2b 2n1s2 2ne. 2ne1ski 2ned 2nes. 2nest 2ogyn 2ok 2ond 2oph 2p1s2 2p1t
2p2ed 2p3k2 2p3n 2que. 2r2ed 2rab 2re. 2s1ab 2s1in 2s1m 2s3g 2s5peo 2sh.
2spa 2sper 2ss 2st. 2t1b 2t1ed 2t1f 2t1in 2t1n2 2t3i4an. 2t3up. 2tab 2taw
2th. 2ths 2ti2b 2tig 2tl 2tof 2trim 2tyl 2ui2 2us 2v1a4b 2vil 2wac 2z1i 2ze
3agog 3alyz 3analy 3away 3bet 3bi3tio 3bie 3bit5ua 3bod 3boo 3butio 3c4ut
3cei 3cell 3cenc 3cent 3cep 3cessi 3chemi 3chit 3cho2 3cia 3cili 3cinat
3cultu 3cun 3dat 3demic 3di1methy 3dict 3did 3dine. 3dle. 3dled 3dles. 3do.
3do5word 3dos 3dox 3efit 3fich 3fluor 3fu 3g4in. 3g4o4g 3gali 3gir 3giz 3glo
3go. 3guard 3gun 3gus 3hear 3hol4e 3hood 3isf 3ka. 3l4eri 3land 3lenc 3lerg
3less 3ley 3lidi 3ligh 3lik 3lo. 3logic 3logu 3lyg1a1mi 3ment 3mesti
3mi3da5b 3milia 3milita 3mind 3mous 3mum 3n4ia 3naut 3neo 3netic 3nitio 3noe
3nomic 3noun 3nu3it 3nu4n 3ogniz 3oncil 3opera 3orrh 3pare 3pay 3pe4a 3pede
3pedi 3phiz 3phob 3phone 3pi1o 3piec 3plan 3press 3pseu2d 3pseu4d 3quer
3quet 3ra4m5e1triz 3rab1o1loi 3raphy 3rimo 3s2og1a1my 3s2pace 3s4cie 3s4on.
3sanc 3sect 3ship 3side. 3sitio 3slova1kia 3som 3spher 3store 3syl 3sync
3ta. 3tel. 3tenan 3tenc 3tend 3ter1gei 3teu 3tex 3thet 3tien 3tine. 3tini
3tise 3tle. 3tled 3tles. 3tro1le1um 3trop1o5les 3trop1o5lis 3tum 3ture 3tus
3ufa 3vat 3verse 3viv 3vok 3volv 3wise 3yar4 3ysis 4a2ci 4ab. 4abr 4adu
4ag4l 4ageu 4aldi 4allic 4alm 4alys 4ama 4and 4anto 4ao 4aphi 4as. 4ath
4ati. 4b1d 4b1m 4b1ora 4b3h 4b3n 4b5w 4be. 4be2d 4be5m 4bes4 4bp 4brit 4buta
4c3reta 4c3s2 4c5utiv 4cag4 4calo 4casy 4cativ 4ced. 4ceden 4ceni 4cesa 4ch.
4ch1in 4ch3ab 4ched 4chs. 4cier 4cii 4cipe 4cipic 4cista 4cisti 4clar 4clic
4corb 4cutr 4d1f 4d1n4 4d3alone 4d5la 4d5lu 4d5out 4daf 4dary 4dativ 4dato
4dee. 4dey 4dlead 4dless 4dli4e 4drai 4drow 4dry 4duct. 4ducts 4dup 4ed3d
4edi 4edo 4egal 4ella 4en3z 4enn 4eno 4enthes 4erand 4erati. 4erene 4erit
4ernit 4ertl 4eru 4es2to 4esh 4etn 4eu 4f1f 4f3ical 4f5b 4f5p 4fa4ma 4fag
4fato 4fd 4fe. 4feca 4fh 4ficate 4fics 4fily 4fm 4fn 4fug 4futa 4g1g2
4g1lish 4g3o3na 4gano 4gativ 4gaz 4gely 4geno 4geny 4geto 4go4niza 4grada
4graphy 4gray 4gress. 4grit 4gu4t 4h1l4 4h1m 4h1s2 4h5p 4hk 4hr4 4i1cr
4i2tic 4i5i4 4i5w 4ian4t 4ianc 4icam 4icar 4iceo 4ich 4if. 4ific. 4ift 4igi
4ik 4iln 4imet 4imit 4inav 4ind 4inga 4inge 4ingi 4ingo 4ingu 4ink 4inl 4iny
4io. 4ir 4is1s 4is4k 4ise 4isms 4istral 4ita. 4ita5m 4itia 4itis 4iton 4itt
4itz. 4iy 4izar 4jestie 4jesty 4k1s2 4kley 4kly 4l1c2 4l1g4 4l1r 4l4i4l
4l4iq 4lateli 4lativ 4lav 4len. 4leye 4lics 4lict. 4lj 4lof 4lov 4lt 4lup
4lya 4lyb 4m1b 4m1f 4m1l 4m1n 4m1p 4m1s2 4m3r 4m5c 4mald 4map 4matiza 4me.
4med. 4mene 4mith 4mk 4mocr 4mok 4mora. 4mt 4mup 4mw 4n1b4 4n1h4 4n1l 4n1n2
4n3o2d 4n5i4an. 4nac. 4nalt 4nare 4nene 4neski 4nesp 4nesw 4nian. 4nk2 4nog
4nop 4nosc 4nz 4o5ria 4oa 4operag 4oscopi 4oth 4p1b 4p1m 4p1p 4pe. 4pf 4pg
4ph. 4phs 4plig 4raril 4rh. 4rhal 4rici 4rs2 4s1er. 4s3f 4s4ed 4s5b 4s5d
4scei 4scopy 4se. 4seme 4senc 4sentd 4sentl 4servo 4shw 4signa 4sily 4ske
4sov 4spio 4spot 4st3w 4stry 4sv 4swo 4syc 4t1d 4t1g 4t1m 4t1p 4t1s2 4t1wa
4t3t2 4taci 4taf4 4talk 4tarc 4tare 4tatic 4tc 4te. 4teat 4tenes 4tes. 4tess
4tey 4thea 4thil 4thl 4thoo 4tian. 4tick 4timp 4todo 4tono 4tony 4tout
4trics 4trony 4tue 4tuf4 4tv 4two 4tya 4tz 4u1t2i 4uab 4uk 4ul3m 4uls 4ultu
4ura. 4ute. 4utel 4uten 4v3iden 4ve. 4ved 4ves. 4vi4na 4ving 4viti 4vity
4votee 4vv4 4wt 4y3h 4z1z2 4zb 4zm 5a5lyst 5a5si4t 5alyt 5anniz 5ba. 5blesp
5bor. 5bore 5bori 5bos4 5bust 5by. 5cel. 5chanic 5chine. 5chini 5chio
5cific. 5cino 5ciz 5clare 5colo 5crat. 5cratic 5cred 5criti 5culi 5da. 5dav4
5day 5dem. 5derm 5di. 5di3en 5dini 5disi 5doe 5dren 5drupli 5dyn 5efici 5egy
5elec 5emniz 5eniz 5erick 5erniz 5erwau 5eu2clid1 5eyc 5eye. 5far 5fect
5ferr 5ficia 5ficie 5fina 5fon 5g4ins 5gal. 5gesi 5gi. 5gicia 5gies. 5gio
5giv 5glas 5glo5bin 5goe 5goo 5gos. 5graph. 5graphic 5gui5t 5hand. 5haz
5i5r2iz 5i5tick 5icap 5icra 5ie5ga 5initio 5iron. 5izont 5ja 5judg 5k2ic
5ki. 5leg. 5legg 5lene. 5lesq 5less. 5licio 5ligate 5litica 5long 5lope.
5los. 5losophiz 5losophy 5lumi 5lumnia 5magn 5mania 5maph1ro1 5maphro 5mate
5media 5metric 5mi. 5moc1ra1t 5mocrat 5mocratiz 5mult 5neck 5nege 5nine.
5nis. 5noc3er1os 5nologis 5nop1oly. 5nop5o5li 5ocrit 5ommend 5pagan 5pathic
5phie 5phisti 5phoni 5phu 5pidi 5po4g 5pod. 5point 5poun 5preci 5pri4e 5pus
5pute 5reav 5ricid 5rigi 5riman 5rina. 5riph 5role. 5root 5rynge 5sa3par5il
5sa3tio 5sack 5sai 5saw 5scin4d 5se5um 5sei 5self 5selv 5sev 5sex 5shev
5sides 5sidi 5sine. 5sion 5siu 5siz 5smith 5solv 5sophic 5spai 5stand 5stat.
5stick 5stir 5stock 5stone 5stratu 5tab1o1lism 5taboliz 5tar2rh 5tect 5tels
5ter3d 5ternit 5think 5thodic 5tidi 5tigu 5tiq 5tistica 5tour 5tria 5tricia
5tro1leum 5tu3i 5turi 5u5tiz 5ulche 5va. 5vere. 5vian 5vide. 5vided 5vides
5vidi 5vilit 5vo. 5volt 5ynx 5zl 6rk. 6rks. a1j a1tr a1vor a2cabl a2d a2f
a2go a2mo a2n a2pl a2ta a2tom a2tu a2ty a2va a3cie a3cio a3dia a3dio a3dit
a3duc a3ha a3he a3ho a3ic. a3nar a3nati a3nen a3neu a3nies a3nip a3niu
a3pher a3pitu a3pu a3ree a3riet a3roo a3sib a3sic a4cabl a4car a4gab a4gy
a4i4n a4lar a4lenti a4ly. a4m5ato a4matis a4n1ic a4pe5able a4peable a4pilla
a4soc a4tog a4top a4tos a5bal a5ban a5bolic a5ceou a5chet a5diu a5guer a5ia
a5le5o a5log. a5mon a5nee a5nimi a5nine a5nur a5rade a5ramete a5ratio a5rau
a5ress a5roni a5sia. a5terna a5then a5tia a5van ab3ul ab5erd ab5it5ab ab5lat
ab5o5liz ab5rog abe2 abi5a ac1er ac1in ac3ul ac4um ac5ard ac5aro ac5rob
act5if ad3ica ad3ow ad4din ad4le ad4su ad5er. ad5ran ad5um adi4er ae4r
aeri4e af6fish aff4 ag1i ag1n ag3oni ag5ell ag5ul aga4n age4o ah4l ai2 ai5ly
ain5in ain5o ait5en ak1en al1i al3ad al3end al4ia. al5ab al5lev ali4e
am1en3ta5b am1in am3ag am3ic am5ab am5asc am5era am5if am5ily ama5ra ami4no
amor5i amp5en an1dl an1gl an2sa an2sp an2tr an3age an3arc an3dis an3i3f
an3io an3ish an3it an3ti1n2 an3ua an3ul an4dow an4ime an4kli an4sco an4sn
an4st an4sur an4tie an4tw an5est. an5ot anal6ys anar4i ande4s ang5ie ano4
ano5a2c anoa4c anoth5 ans3po ans3v ans5gr antal4 anti1d anti1re anti3de
anti3n2 anti3re ap3in ap3ita ap5at ap5ero ap5illar ap5ola apar4 apoc5 apor5i
apos3t aps5es aque5 ar1i ar2iz ar2mi ar2p ar2range ar2sh ar3act ar3al
ar3che5t ar3ent ar3ian ar3io ar3q ar4at ar4chan ar4dr ar4fi ar4fl ar4im
ar4range ar4sa ar5adis ar5ativ ar5av4 ar5dine ar5eas ar5ial ar5inat ar5o5d
ara3p aran4g araw4 arbal4 arre4 as1tr as3ant as3ten as4ab as4l as4sh as5ph
as5ymptot ashi4 ask3i asur5a at1ic at3abl at3alo at3ego at3en. at3era at3est
at3if at3itu at3ul at3ura at4ho at4sk at4tag at4th at5ac at5ap at5ech at5ev
at5i5b at5omiz at5rop at5te at5ua at5ue at6tes. ate5c ater5n ath3er1o1s
ath5em ath5om ation5ar au1th au3gu au3r au4b au4l2 au5li5f au5sib augh3
augh4tl aun5d aut5en av1i av3ag av3era av3ig av3iou av5ern av5ery av5oc
ave4no avi4er aw3i aw4ly aws4 ax4ic ax4id ay5al aye4 ays4 azi4er azz5i
b1i3tive b1j b1v b2be b2l2 b3ber b3lis b3tr b4le. b4lo b4to b5itz b5ota
b5uto ba1thy ba4ge ba4z ba6r1onie back2er. bad5ger bal1a ban3i ban4e ban5dag
barbi5 bari4a bas4si bbi4na bbi4t be1li be2vie be3da be3de be3di be3gi be3lo
be3sp be3tw be3w be4vie be5gu be5nig be5nu be5str be5tr be5yo beak4 beat3
bet5iz bi1orb bi2b bi2t bi3liz bi3ogr bi3orb bi3tr bi4d bi4er bi5d2if bi5en
bi5net bi5ou bid4if bil2lab bil4lab bin4d bina5r4 bio1rh bio3rh bio5m bk4
blan2d1 blan4d blath5 blen4 blin2d1 blin4d blon2d2 blon4d blun4t bne5g
bo2t1u1l bo4e bo4to bo4tul bod3i bol3ic bom4bi bon4a bon5at bor1no5 bor5d
both5 bound3 broth3 brus4q bsor4 bt4l bu3li bu3re bu4ga bu4n buf4fer bumi4
bunt4i bus5ie bus6i2er bus6i2es buss4e buss4in buss4ing but2ed. but4ed.
but4ted bys4 c1ing c1q c2te c2tro3me6c c3c c3ter c3ume c4ina c4one c4rin
c4ticu c4tw c4uf c4ui c5e4ta c5ing. c5laratio c5n c5tant ca1bl ca3lat ca4th
ca5den ca5per cab3in cach4 cad5e1m cad5em cal4la call5in can3iz can4e can4ic
can4ty can5d can5is cany4 car5om cas5tig cast5er cat1a1s2 catas4 cav5al
ccha5 cci4a ccompa5 ccon4 ccou3t ce5ram ces5si5b ces5t cet4 cew4 ch3er.
ch3ers ch4ti ch5a5nis ch5ene ch5iness che2 che5lo cheap3 chi2z chie5vo
chs3hu ci2a5b ci3ph ci4la ci5c cia5r cig3a3r ciga3r cin2q cin3em cin4q cion4
cit3iz ck1 ck3i cle4ar cle4m clim4 cly4 co3inc co3pa co4gr co4pl co5ag co5zi
co6ph1o3n coe2 coi4 col3or col5i com5er con3g con4a con5t cop3ic coro3n
cos4e cous2ti cous4ti cov1 cove4 cow5a coz5e cras5t cre3at cre4v cri2
cri3tie cri5f cri5tie cris4 cro4pl cro5e2co cro5e4co croc1o1d crop5o cros4e
cru4d ct5ang cta4b ctim3i ctu4r cu2ma cu3pi cu4mi cu4ranc cu4tie cu5ity
cu5py cu5ria cud5 cul4tis cur5a4b cuss4i cze4 d1b d1d4 d1h2 d1if d1in d1j
d1m d1p d1ri3pleg5 d1u1a d1uca d1v d1w d2d5ib d2dib d2es. d2gy d2iti d2th
d2y d3eq d3ge4t d3tab d3ule d4em d4erh d4ga d4ice d4is3t d4og d4or d4sw d4sy
d5c d5k2 da2m2 dach4 dan3g dard5 dark5 data1b data3b dav5e dd5a5b de1p de1sc
de1t de1v de2c5lina de2clin de2mos de2pu de2s5o de2tic de2to de3fin3iti
de3no de3nu de3pa de3str de4als. de4bon de4cil de4mons de4mos de4nar de4su
de4tic de5clar1 de5com de5fin5iti de5if de5lo de5mil deaf5 deb5it decan4
del5i5q deli4e dem5ic. demor5 denti5f depi4 der5s dern5iz des2 des3ic des3ti
dev3il dg1i di1re di2ren di2rer di3ge di4cam di4lato di4pl di4ren di4rer
di5niz dia5b dic1aid dic5aid dif5fra dio5g dir2 dirt5i dis1 do3nat do4la
do4v do5de do5lor do5word doli4 dom5iz doni4 doo3d dop4p drag5on dre4 drea5r
dren1a5l dri4b drif2t1a drif4ta dril4 dro4p drom3e5d ds4p du1op1o1l du2al.
du2c du4g du4n du4pe du5el duc5er dum4be dy4se dys5p e1a4b e1ce e1cr e1cu
e1f e1h4 e1ing e1j e1la e1les e1loa e1me e1or e1po e1q e1ria4 e1rio e1s2e
e1s4a e1si e1sp e1vi e1wa e2col e2cor e2lis e2mel e2pa e2r3i4an. e2s5im
e2sca e2sec e2sic e2sid e2sol e2son e2sur e2vas e3act e3ass e3br e3chas
e3dia e3fine e3imb e3inf e3lea e3libe e3lier e3lio e3liv3 e3loa e3my e3new
e3nio e3ny. e3ol e3pai e3pent e3pro e3real e3rien e3scr e3sha e3spac6i
e3ston e3teo e3tra e3tre e3up e3wh e3wit e4a3tu e4bel. e4bels e4ben e4bit
e4cad e4cib e4clam e4clus e4comm e4compe e4conc e4crem e4cul e4d1er e4dol
e4dri e4dul e4f3ere e4fic e4fuse. e4go. e4gos e4jud e4l1er e4l3ing e4l5ic.
e4la. e4lac e4law e4led e4mac e4mag e4met e4mis e4mul e4nant e4nos e4oi4
e4ot e4pli e4prec e4pred e4prob e4put e4q3ui3s e4rian. e4riva e4sage.
e4sages e4sert. e4serts e4serva e4vin e4wag e5and e5atif e5cite e5ex
e5git5 e5gur e5ic e5inst e5ity e5len e5lim e5loc e5lud e5man e5miss e5nea
e5nee e5nie e5nil e5niu e5of e5out e5ow e5pel e5roc e5skin e5stro e5tide
e5tir e5titio e5un e5vea e5veng e5verb e5voc e5vu e5wee e6pineph ea2t ea2v
ea4ge ea4l ea4n3ies ea4nies ea5ger ea5sp ead1 ead5ie eal3ou eal5er eam3er
ear2t ear3a ear4c ear4ic ear4il ear5es ear5k eart3e east3 eat5en eath3i
eav3en eav5i eav5o ec2i ec3im ec3ora ec3ula ec4tan ec4te ec5essa ec5ificat
ec5ifie ec5ify ecan5c ecca5 eci4t eco5ro ed1it ed1uling ed3ib ed3ica ed3im
ed5ulo ede4s edg1l edg3l edi5z edon2 ee2c ee2f ee2m ee2s4 ee4ly ee4na ee4p1
ee4ty eed3i eel3i eest4 ef5i5nite efil4 efor5es eg1ul eg4ic eg5ib eg5ing
eg5n eger4 eher4 ei2 ei3th ei5d ei5gl eig2 eir4d eit3e ej5udi ek4la eki4n
el2f el2i el2sh el3ega el3ica el3op. el4lab el4ta el5ativ el5ebra el5igib
el5ish el5og el5ug elan4d elaxa4 eli2t1is eli4tis ello4 em1in2 em3i3ni
em3ica em3iz em3pi em5ana em5b em5igra em5ine em5ish em5ula emi4e emo4g
emoni5o emu3n en1dix en3dic en3dix en3em en3etr en3ish en3it en3ov en3ua
en4sw en5amo en5ero en5esi en5est en5ics en5uf ench4er eno4g ent5age eo2g
eo3grap eo3re eo4to eo5rol eop3ar eos4 ep3reh ep4sh ep5anc ep5etitio ep5reca
ep5ti5b ep5uta ephe4 equi3l er1a er1h er1i er1ou er1s er3ar er3ch er3emo
er3ent er3est er3ine er3m4 er3no er3set er3tw er4bl er4che er4iu er4nis
er5el. er5ena er5ence er5ess er5ob era4b ere3in ere4q ere5co eret4 eri4er
eri4v ero4r ert3er eru4t es2c es3olu es3per es3tig es4i4n es4mi es4pre
es4si4b es4w es5can es5cu es5ecr es5enc es5iden es5igna es5ona es5pira
es5tim es5urr esh5en esi4u esis4te espac6i estan4 estruc5 et1ic et3ric
et3rog et3ua et5itiv et5ona et5rif et5ros et5ym et5z eta4b eten4d
eth1y6l1ene ethod3 ethy6lene eti4no etin4 eu3ro eu4clid1 eu5tr eus4 eute4
euti5l ev1er ev3ell ev3id ev5ast eva2p5 evel3o even4i evi4l evi4v ew3ing
ewil5 eys4 f1in3g f2f5is f2fy f2ly5 f2ty f3ican f3icen f4fes f4fie f4fly
f4l2 f4to f5fin. f5less f5rea fa3bl fa3ta fa3the fa4ce fab3r fain4 fall5e
fam5is far5th fault5 fe3li fe4b fe4mo feas4 feath3 feb1rua feb3ru fen2d
fend5e fer1 fermi1o fermi3o fev4 fi2ne fi3a fi3cer fi3cu fi5del fic4i fight5
fil5i fill5in fin2d5 fin4n fis4ti fit5ted. fla1g6el flag6el flin4 flo3re
flow2er. fo2r fo5rat fon4de fon4t for4i for5ay fore5t fort5a fos5 fra4t
fres5c fri2 fril4 frol5 fu3ri fu4min fu5el fu5ne fus4s fusi4 g1ic g1lead g1m
g1ni g1no g1utan g2ge g2n1or. g2nin g2noresp g3b g3ger g3imen g3isl g3lead
g3lig g3p g3w g4ery g4ico g4my g4na. g4nac g4nio g4non g4nor. g4nores g4rai
g4ro g5amo g5rapher g5ste g6endre. ga3lo ga3niz ga5met gaf4 gan5is gani5za
gar5n4 gass4 gath3 gd4 ge3o1d ge3om ge4nat ge4ty ge4v ge5lis ge5liz ge5niz
geez4 gel4in gen2cy. geo3d get2ic. geth5 gglu5 ggo4 gh3in gh4to gh5out
ght1we ght3we gi4u gia5r gien5 gil4 gin5ge gir4l gl2 gla4 glad5i gli4b glo3r
gn4a gnet1ism gnet4t gno5mo go3is go3ni go5riz gob5 gon2 gondo5 gor5ou gov1
gran2 graph5er. gre4n griev1 griev3 gruf4 gs2 gth3 gu4a gy5ra h1b h1es h1f
h1h h1w h2lo h2t1eou h3ab4l h3ern h3ery h3i5pel1a4 h4ed h4era h4il2 h4ina
h4sh h4tar h4teou h4ty h4wart h5a5niz h5agu h5ecat h5elo h5erou h5odiz h5ods
ha2p3ar5r ha3la ha3ran ha4m ha4parr ha5ras hach4 hae4m hae4t hair1s hala3m
han4ci han4cy han4g han4k han4te hang5er hang5o hap3l hap5t har2d har4le
har5ter hard3e harp5en has5s hatch1 hatch3 haun4 haz3a he2n he2s5p he3l4i
he4can he4t he5do5 hel4lis hel4ly hem4p hen5at hena4 heo5r hep5 her4ba
hera3p here5a het4ed heu4 hex2a hex2a3 hi2v hi3ro hi4co hi4p hi5an high5
himer4 hion4e hipela4 hir4l hir4p hir4r his3el his4s hite3sid hith5er hlan4
hlo3ri hmet4 hnau3z ho4g ho4ma ho5ny ho5ris ho5ru ho5sen ho6r1ic. hoge4
hol5ar home3 hon4a hoon4 hor5at hort3e hos1p hos4e house3 hov5el hree5
hro3po hro5niz ht1en ht5es hu4g hu4min hu4t hun4t hun5ke hus3t4 hy2s hy3pe
hy3ph hypo1tha i1bl i1br i1er. i1est i1la i1ol i1ra i1tesima i1ti i1u i2al
i2an i2b5ri i2c5oc i2cip i2di i2du i2go i2l5am i2mu i2so i2su i2t5o5m i2tim
i3cur i3dle i3enti i3esc i3et i3fie i3fl i3gib i3h i3j i3leg i3mon i3nee
i3qua i3tan i3tat i4ativ i4atu i4car. i4cara i4cay i4cly i4cry i4dai i4dom
i4dr i4g4l i4jk i4lade i4mag i4n3au i4nia i4no4c i4not i4os i4our i4rac
i4ref i4rel4 i4res i4tag i4tism i4tram i4v3er. i4v3ot i4vers. i5bo i5bun
i5cid i5die i5enn i5gre i5mini i5ness i5ni. i5nite. i5nitely. i5nus i5oti
i5sis i5teri i5tesima i5tud i5vore ia4tric ia5pe iam4 iam5ete ian3i iass4
ib3era ib3in ib3li ib5ert ib5ia ib5it. ib5ite ibe4 ic3ipa ic3ula ic4t3ua
ic4te ic4um ic5ina ic5uo icas5 iccu4 ictu2 id1it id3io id3ow id4ios id5anc
id5d id5ian id5iu id5uo ide3al ide4s idi4ar idi5ou ied4e ield3 ien4e ien5a4
if4fr if5ero ifac1et iff5en ig1ur ig3era ig3il ig3in ig3it ig3or ig5ot iga5b
ight3i ign4it ignit1er igu5i il1er il1i il2ib il2iz il3a4b il3ia il3io il3oq
il3v il4ist il4ty il5f il5ur ila5ra ilev4 ill5ab im1i im3age im3ped3a im3ula
im4ni im5ida im5peda ima5ry imenta5r imi5le in1is in1u in3cer in3io in3ity
in3se in5dling in5gen in5gling incel4 iner4ar infra1s2 infras4 ino4s insur5a
io2gr io4m io4to io5ph io5th ioge4 ion3at ion3i ion4ery ior3i ip3i ip3ul
ip4ic ip4re4 ipe4 iphras4 iq3ui3t iq3uid iq5uef ir1i ir4is ir4min ir5gi
ir5ul ira4b ird5e ire4de iri3tu iri5de iro4g irre6v3oc is1p is1te is1ti
is2pi is3ar is3ch is3er is3hon is3ib is4py is4sal is4ses is4ta. is5ag is5han
is5itiv is5us isas5 ish5op isi4d islan4 iso5mer issen4 ist4ly it3era it3ica
it3ig it3uat it3ul it4es it5ill it5ry ita4bi ith5i2l ithi4l itin5er5ar iv1it
iv3ell iv3en. iv3o3ro iv5il. iv5io ix4o izi4 ja4p jac4q janu3a japan1e2s
je1re1m jer5s jew3 jo4p k1b k1er k1i k1l k1m k1w k2ed k3ab k3en4d k3est. k3f
k3ou k3sha k4ill k4im k4in. k4sc k4sy k5ag k5iness k5ish k5nes k5t kais4
kal4 ke4g ke4ty ke5li ke6ling kes4 kh4 ki4p ki5netic kilo5 kin4de kin4g kis4
kk4 ko5r kosh4 kro5n ks4l l1it l1iz l1l l1te l1tr l2de l2it. l2l3ish l2le
l2lin4 l2se l3chai l3chil6d l3chil6d1 l3ci l3dr l3eva l3icy l3ida l3kal
l3le4n l3le4t l3lec l3leg l3lel l3o3niz l3opm l3pha l3pit l3tea l4abo l4ade
l4dri l4ero l4ges l4icu l4iff l4im4p l4ina l4law l4lish l4mod l4pl l4sc
l4sie l5fr l5ga l5i5tics l5lea l5lina l5low l5met l5mo3nell l5ogo l5phi l5pr
l5ties. l5umn. l5ven l5vet4 l5yse la3dy la4c3i5e la4cie la4v4a la5tan lab3ic
laci4 lag4n lai6n3ess lai6ness lam3o lan4dl lan4te lan5et lar3i lar4g
lar5ce1n las4e lbin4 lce4 ld4ere ld4eri ld5is ldi4 le2a le3g6en2dre le3ph
le4bi le4mat le4pr le5sco lea4s1a lea4sa lead6er. lecta6b left5 lem5atic
ler4e lera5b les2 lev4er. lev4era lev4ers lgar3 lgo3 li2am li4ag li4as
li4ato li4cor li4fl li4gra li4mo li5bi li5og liar5iz lid5er lif3er lim3i
lim4bl lin3ea lin3i link5er lis4p lith1o5g litho5g liv3er lka3 lka4t ll1fl
ll2i ll4o ll5out lloqui5 lm3ing lmon4 lo1bot1o1 lo2ges. lo4ci lo4ges.
lo4rato lo4ta lo5rie load4ed. load6er. lob5al lom3er lon4i lood5 lop3i
lor5ou lora4 los4t los5et loun5d lp5ing lpa5b lt5ag ltane5 lten4 ltera4
lth3i lth5i2ly lthi4ly ltis4 ltu2 ltur3a lu3br lu3ci lu3en lu3o lu4ma lu5a
lu5id luch4 lue1p luf4 luo3r lus3te luss4 ly3no ly5me ly5styr m1m m1ou3sin
m2an. m2en. m2is m2iz m2pi m2py m3ma1b m3mab m3pet m4b3ing m4etr m4ill
m4ingl m4inu m4nin m4p1in m4pous m4sh m5bil m5e5dy m5ersa m5i5lie m5inee
m5ingly m5istry m5ouf m5pir m5shack2 m5si ma1la1p ma2ca ma3lig ma3tis ma4cl
ma5chine ma5lin ma5rine. ma5riz ma5sce mag5in maid5 mal4li mal4ty man3iz
man3u1sc man5is mar1gin1 mar3gin mar3v mar4ly mas1t mas4e math3 mba4t5 mbi4v
me1te me2g me2m me3die me3gran3 me3try me4ta me4v me5gran me5on me5thi
me5trie med3i3cin medi2c medi4c medi5cin medio6c medio6c1 mel4t mel5on
mem1o3 men4a men4de men4i men4te men5ac mens4 mensu5 met3al mi1n2ut1er
mi1n2ut1est mi3a mi3dab mi6n3is. mid4a mid4g mig4 mil2l1ag mil4lag mil5li5li
milli5li min4a min4t min4ute min5gli miot4 mis4er. mis4ti mis5l mma5ry mn4a
mn4o mo2d1 mo2r mo2v mo3me mo3niz mo3ny. mo3sp mo4go mo4no1en mo5e2las
mo5lest mo5sey moe4las moi5se mois2 mol1e5c mole5c mon4ey1l mon4ism mon4ist
mon5et mon5ge moni3a mono1s6 mono3ch mono5ch monol4 monos6 moro6n5is mos2
moth3 moth4et2 mou5sin mp4tr mp5ies mp5is mpa5rab mpar5i mpara5 mphas4 mpi4a
mpo3ri mpos5ite mpov5 mshack4 mu2dro mu4dro mu4u mul2ti5u mul4ti5u mula5r4
multi3 mun2 n1cr n1cu n1de n1dieck n1dit n1er n1gu n1im n1in n1j n1kl
n1o1mist n1p4 n1q n1r n1t n1v2 n1w4 n2an n2at n2au n2ere n2gy n2it n2se n2sl
n3ar4chs. n3ch2es1t n3cha n3chis n3diz n3ear n3f n3gel n3geri n3gib n3itor
n3ket n3tine n3uin n3uo n3za n4abu n4as n4ces. n4dai n4er5i n4erar n4gab
n4gla n4gum n4ith n4s3es n4soc n4t3ing n4um n5act n5arm n5cheo n5chil n5d2if
n5dan n5duc n5eve n5gere n5git n5igr n5kero n5less n5m n5o5miz n5ocl n5oniz
n5spi n5tib n5umi na3tal na4ca na4li na5lia na5mit nag5er. nak4 nan4it
nanci4 nank4 nar3c nar3i nar4chs. nar4l nas4c nas5ti nato5miz nau3se nav4e
nc1in nc4it ncar5 nch4est ncour5a nd2we nd3thr nd5est. ndi4b ndu4r ne2b ne2c
ne2q ne3back ne4gat ne4la ne4mo ne4po ne4v ne4w ne5back ne5mi neb3u neg5ativ
nel5iz ner4r nera5b nfi6n3ites nfin5ites ng1ho ng1in ng1spr ng3ho ng3spr
ng5ha ng5sh nge4n4e nge5nes ngov4 nha4 nhab3 nhe4 ni2fi ni3an ni3ba ni3miz
ni3tr ni4ap ni4bl ni4d ni4er ni4o ni5di ni5ficat nik4 nin4g nis4ta nk3in
nk3rup nme4 nmet4 nne4 nni3al nni4v no1vemb no3ble no3my no4mo no4n no4rary
no5l4i no5mist no5ta no5vemb nob4l noge4 nois5i nom1a6l nom5e1no nom5eno
non1eq non1i4so non4ag non5eq non5i noni4so nor5ab nos4e nos5t nov3el3 nowl3
npi4 npre4c nru4 ns3m ns4c ns4moo ns4pe ns5ab ns5ceiv nsati4 nsid1 nsig4
nsta5bl nt2i nt4s nta4b nter3s nti2f nti4er nti4p ntre1p ntre3p ntrol5li
ntu3me nu1a nu1me nu3tr nu4d nu5en nuf4fe nym4 nyp4 o1bi o1ce o1ge o1h2 o1la
o1lo3n4om o1pr o1q o1ra o1rio o1ry o2bin o2do4 o2fi o2g5a5r o2ly o2me o2n
o2pa o2so o3br o3chas o3chet o3er o3ev o3gie o3ing o3ken o3les3ter o3lesc
o3let o3li4f o3lia o3lice o3mecha6 o3mia o3nan o3nen o3nio o3no2t1o3n
o3norma o3nou o3ord o3pit o3riu o3scop o3tice o3tif o3tis o3vis o4cil o4clam
o4cod o4el o4gato o4ger o4gl o4gro o4lan o4met o4mon o4posi o4r3ag o4s3pher
o4spher o4tan o4tes o4wo o5a5les o5bar o5cure o5eng o5g2ly o5gene o5geo
o5ism o5j o5lil o5lio o5lis. o5lite o5litio o5liv o5lus o5mid o5mini o5niu
o5norma o5phan o5pher o5pon o5ra. o5real o5ril o5rof o5rum o5scr o5stati
o5tes3tor o5test1er o5v4ol o6v3i4an. o6v3ian. oad3 oard3 oas4e oast5e oat5i
ob3a3b ob3ul ob5ing obe4l obli2g1 oc3rac oc3ula oc5ratiz och4 ocif3 ocre3
octor5a od3ic od5ded od5uct. od5ucts odel3li odi3o odit1ic odor3 oe4ta
oerst2 oerst4 of5ite ofit4t og3it og5ativ ogu5i ohab5 oi2 oi3der oi3ter
oi5let oi5son oic3es oiff4 oig4 oint5er oist5en ok5ie oke1st oke3st ol2d
ol2i ol2t ol2v ol3er ol3ing ol3ish ol3ub ol3ume ol3un ol4fi ol5id. ol5ogiz
ol5pl olass4 old1e oli3gop1o1 olli4e olo4r olon4om om1in om2be om3ena om3ic.
om3ica om3pi om4bl om5ah om5atiz om5erse om5etry oma5l omo4ge ompro5 on1a
on1c on1ic on1is on3key on3omy on3s on3t4i on4ac on4gu on4odi on5do on5est
on5um ono4ton onom1ic onspi4 onspir5a onsu4 onten4 ontif5 onva5 oo2 oo4k
ood5e ood5i oop3i oost5 op1er op1ism. op1u op3ing op3ism. ope5d opy5 or1in
or2mi or3ei or3ica or3ity or3oug or3thi or3thy or4gu or4se or4tho3ni4t or4ty
or5aliz or5ange or5est. or5pe or5tively ore5a ore5sh orew4 orn2e ors5en
orst4 orth1ri orth5ri os2c os2ta os3al os3ito os3ity os4ce os4i4e os4l os4pa
os4po os5itiv os5til os5tit osi4u ot3er. ot3ic. ot5ers ot5ica otele4g
oth3e1o1s oth3i4 oth5esi oto5s ou2 ou3ba3do ou3bl ou4l ou5et ou5v ouch5i
oun2d ounc5er ov4en ov4ert over3s over4ne oviti4 ow1i ow3der ow3el ow5est
own5i oxi6d1ic oxi6d2ic oy1a p2pe p2se p2te p2th p3agat p3ith p3pen p3per
p3pet p3rese p3roca p3w p4a4ri p4ad p4ai p4al p4ee p4enc p4era. p4erag p4eri
p4ern p4id p4in. p4ino p4ot p4ped p4sib p4tw p5ida p5pel p5trol p5trol3 pa1p
pa2te pa3ny pa4ca pa4ce pa4pu pa4tric pa5ter pa5thy pac4t pain4 pal6mat
pan3el pan4a pan4ty par4a1le par4ale par4is par5age par5di par5el para3me
para5bl para5me parag6ra4 param4 pav4 pd4 pe2c pe2t pe4la pe4nan pe5on pe5ru
pe5ten pe5tiz pear4l ped4ic pedia4 pee2v1 pee4d pee4v1 pek4 peli4e pen4th
per1v per3o per3ti per4mal pera5bl peri5st perme5 ph1ic ph2l ph3t ph4er
ph4es. ph5ing phar5i phe3no phi2l3an phi2l3ant phi5lat1e3l phi5latel pho4r
pi2c1a3d pi2n pi2tu pi3a pi3de pi3en pi3lo pi4cad pi4cie pi4cy pi4grap
pi5tha pian4 pind4 pion4 plas5t pli2c1ab pli3a pli4cab pli4n pli5er pli5nar
ploi4 plu4m plum4b po3et5 po3lyph1ono po4c po4ni po4p po4ry po4ta po5em
poin2 poin3ca poly1e poly5t pos1s ppa5ra ppo5site pr2 pray4e pre1neu pre3em
pre3r pre3v pre4la pre5co pre5neu pre5ten pref5ac prema5c pres2pli pres4pli
pri4s prin4t3 pris3o pro1t pro2cess pro2g1e pro3l pro4cess pro4ge proc3i3ty.
proc5ity. prof5it pros3e ps4h pseu3d6o3d2 pseu3d6o3f2 pseud6od2 pseud6of2
pt5a4b pti3m pto3mat4 ptomat4 ptu4r pu2n pu2t pu3tr pu4m pu5bes5c pub3
pubes5c pue4 puf4 pul3c pur4r put3er put4ted put4tin qu2 qu6a3si3 qua5v
quain2t1e quain4te quasir6 quasis6 qui3v4ar quin5tes5s quiv4ar r1abolic r1b
r1c r1er4 r1f r1gl r1krau r1l r1m r1nis4 r1p r1r4 r1sa r1sh r1si r1sp r1thou
r1ti r1treu r1veil r1w r2ai r2amen r2ami r2as r2bin r2ce r2ina r2is r2led
r2me r2oc r2se r3a3dig r3bin1ge r3binge r3cha r3get r3gic r3gu r3ial. r3ish
r3j r3ket r3lo4 r3men r3mit r3nel r3ney r3nit r3niv r3nu r3pau5li r3pet r3po
r3sec r3teb r3thou r3tig r3treu r3tri r3veil r3ven r3vey r3vic r3vo r4amen
r4ani r4bab r4bag r4ci4b r4dal r4en4ta r4eri r4es. r4fy r4ib r4ice r4ico
r4iq r4is. r4lig r4lis r4ming. r4mio r4my r4nar r4ner r4nou r4pea r4reo
r4si4b r4tag r4tier r4tily r4tist r4tiv r5abolic r5acl r5bine r5ebrat
r5ev5er. r5gis r5git r5ited. r5le5qu r5net r5nic r5pent r5sha r5sw r5usc
r5vest ra1or ra3bi ra3chu ra3mou ra3or ra4lo ra4me3triz ra5n2has ra5no
ra5vai ra5zie rach4e radi1o6g raf4t raf5fi ram3et ran4ge ran4has rane5o
rap3er rar5c rar5ef rare4 ration4 rau4t rav3el rb4o rb5ing. rbi2 rbi4f rc4it
rcen4 rch4er rcum3 rd2i rd3ing rdi4a rdi4er rdin4 re1al re1de re1li re1o
re1pu re2c3i1pr re2f1orma re2fe re3an re3dis re3fi re3str re3tri re4aw
re4cipr re4cre re4fac re4fy re4posi re4spi re4t1ribu re4ter re4ti4z re4tribu
re4val re4wh re5arr re5fer. re5it re5lu re5pin re5ru re5stal re5uti re5vers
re5vert re5vil rec4ogn rec5oll rec5ompe rec5t6ang red5it reg3is ren4te rero4
res2t ress5ib reu2 rev2 rev3el rev5olu rfu4 rg2 rg3er rg3ing rgi4n rgo4n rh4
ri1er ri1o ri2pl ri2tu ri3a ri3enc ri3ent ri3ta3b ri4ag ri4cie ri5et ria4b
rib3a ric5as rid5er rig5an ril3iz rim4pe rim5i rin4d rin4e rin4g rip5lic
riph5e ris4c ris4p rit3ic rit5er. rit5ers rit5ur riv1o1l riv3et riv3i riv3ol
riv5el rk1ho rk3ho rk4le rk4lin rl5ish rle4 rm3ing rm5ers rma5c rno4 ro1bot1
ro1fe ro1tron ro3bot3 ro3cr ro3mesh ro3pel ro4e ro4the ro4ty ro4va ro5e2las
ro5epide1 ro5fil ro5ker ro5n4is ro5ro rob3l roe4las rok2 rom4i rom4p rom5ete
ron4al ron4e ron4ta rop3ic ror3i ros4s ros5per rov5el rox5 rp3ing rp4h4
rp5er. rre4c rre4f rre4st rri4o rri4v rron4 rros4 rrys4 rs3es rs4c rs5er.
rsa5ti rse1rad1i rse4cr rse5v2 rson3 rt4sh rt5ib rtach4 rte5o rten4d rti4d
rtil3i rtil4l rtroph4 ru2n ru3a ru3e4l ru3en ru3in ru4gl rum3pl run4ty runk5
ruti5n rv4e rv5er. rvel4i rvi4v ry3t ry4c rz1sc rz3sc s1ap s1cu s1e4s s1l2
s1n4 s1r s1sa s1si s1tic s1tle s2h s2ina s2le s2phe s2s1a3chu1 s2s3i4an.
s2s5c s2t1ant5shi s2tag s2tal s2ty s3act s3ing s3ket s3lat s3ma s3qui3to
s3sel s3the s3tif s4ced s4ces s4chitz s4cho s4cli s4erl s4ogamy s4op s4pace
s4pacin s4ply s4pon s4sa3chu s4ses. s4sian. s4sie s4sl s4sn s4ta4p s4tamp
s4tants s4ted s4ti. s4tie s4top s4trad s4tray s4trid s4ul s4y s5edl s5ened
s5enin s5icc s5men s5ophiz s5ophy s5quito s5seng s5set s5sign5a3b s5tero
s5tia sa2 sa5lo sa5ta sa5vor sac3ri sal4m sal4t salar4 sales3c sales5c
sales5w san4de sat3u sau4 sca2t1ol sca4p sca4tol sca6p1er scan4t5 scav5 sch2
schro1ding1 sci4utt scle5 scof4 scour5a scrap4er. scy4th scy4th1 se1le
se1mi6t1ic se1mi6t5ic se2c3o se2g se3mes1t se4a se4d4e se4mol se5sh sea5w
seas4 seg3r sem1a1ph sen4d sen5at sen5g sep3a3 sep3temb sep5temb ser4o ses5t
sev3en sew4i sh1er sh1in sh3io sh5old shiv5 sho4 shoe1st shoe5st shon3 shor4
short5 si1b si2r si5diz si5resid sid2ed. side5st side5sw sil4e sion5a sir5a
sk2 sk5ine sk5ing sky1sc sky3sc slith5 small3 sman3 smel4 smol5d4 so2lute
so3lic so4ce so4lab so4lute so5vi soft3 sol3d2 son4g sona4 sor5c sor5d
sp5ing spa4n spe3cio spe5cio spen4d spher1o spher5o spho5 spi2c1il spi4cil
spil4 spokes5w spor4 sports3c sports3w sports5c sports5w squal4l ss2t ss3hat
ss4li ss5ily ss5w ssas3 ssi4er sspend4 ssur5a st2i st3ing st4r st5b st5scr
sta1ti sta3ti stam4i star3tli star5tli ste2w stern5i stew5a stom3a stor5ab
strat1a1g strat5ag strib5ut stu1pi4d1 stupi4d styl1is styl5is su1al su2g3
su2m su2n su2per1e6 su2r su4b3 su5is suit3 sum3i sw2 swimm6 sy5rin syn5o
sythi4 t1a1min t1cr t1li2er t1ro1pol3it t1wh t2ina t3ess. t3ess2es t4ch
t4ic1u t4ico t4sc t4sw t4tes t5la t5let. t5lo t5to t6ap6ath ta2l ta3gon.
ta3min ta3riz ta4tur ta5bles ta5do ta5la ta5log ta5mo ta5per ta5pl ta5sy
tai5lo tal3i tal4lis tal5en talk1a5 talka5 tan4de tanta3 tar4a tar4rh tas4e
taun4 tav4 tax4is tch1c tch3c tch3i1er tch5ed tch5et tch5ier te2ma2 te4p
te5di te5ger te5gi te5pe teach4er. tead4i tece4 teg4 tele1r6o tele2g tele4g
teli4 tem3at ten4tag ter2ic. ter3c ter3is ter5ies ter5v teri5za tess4es
teth5e th1o5gen1i th2e th3eas th5ic. th5ica th5ode tha4l1am tha4lam than4
the3is the5at tho1k2er tho3don tho5don tho5geni tho5riz thok4er thor5it
thy3sc thy4l1an thy4lan ti2n3o1m ti3sa ti3tl ti3za ti3zen ti4ab ti4ato
ti4nom ti4u ti5fy ti5oc ti5so tif2 till5in tim5ul tion5ee tis4m tis4p tiv4a
tlan4 tli4er tme4 to2gr to2ma to2ra to3b to3my to3nat to3rie to3war to5crat
to5ic tolo2gy tom4b ton4ali tor5iz tos2 tot3ic tr4ial. tra1vers tra3b tra5ch
tra5ven tra5vers trac4it trac4te traci4 trai3tor1 trai5tor tras4 trav5es5
travers3a3b tre4m tre5f treach1e treach5e trem5i tri4v tri5ces tro1p2is
tro3fit tro3sp tro3v tro5fit tro5mi tro5phe trof4ic. tron5i trop4is
tropo5les tropo5lis tropol5it tru5i trus4 tsch3ie tsch5ie tsh4 ttrib1ut1
ttribut5 ttu4 tu1a tu3ar tu4bi tu4nis tu5ry tud2 tur3is tur5o turn3ar
turn5ar tw4 twis4 ty2p5al ty4pal ty5ph type3 tz4e u1at u1b4i u1dic u1ing
u1l4o u1la u1len u1mi u1ni u1ou u1pe u1ra u1rit u1v2 u2ne u2nin u2r1al.
u2ral. u2su u3ber u3ble. u3ca u3cr u3cu u3fl u3lu u3pl u3rif u3rio u3ru
u3sic u3tat u3tine u3u u4b5ing u4bel u4bero u4cy u4don u4du u4ene u4m3ing
u4ors u4rag u4ras u4t1l u4tis u4tou u5dit u5do3ny u5j u5lati u5lia u5os
u5pia u5sad u5san u5sia u5ton ua3drati ua5na uac4 uad1ratu uan4i uar2d uar3i
uar3t uar5ant uav4 ub4e uc4it uci4b ucle3 ud3er ud3ied ud3ies ud4si ud5d
ud5est ud5is udev4 uea1m uen4te uens4 uer4il ug5in ugh3en ui4n uil5iz uir4m
uita4 uiv3 uiv4er. ul1ti ul2i ul3der ul3ing ul4e ul4gi ul4lar ul4li4b ul4lis
ul5ish ul5ul ul5v ula5b ulch4 uls5es ultra3 um2p um4bi um4bly um5ab umor5o
un3s4 un4er un4im un4sw un4ter. un4tes un5ish un5y un5z unat4 uni3v unt3ab
unu4 up3ing up3p uper5s upport5 upt5ib uptu4 ur1d ur1in ur2l ur3iz ur3the
ur4be ur4fer ur4fr ur4no ur4pe ur4pi ur4tie ur5tes urc4 ure5at uri4al.
uri4fic url5ing. uros4 urs5er urti4 us1p us1tr us2er. us3ci us4ap us4lin
us5sl us5tere usc2 use5a usur4 ut3ing ut5of uta4b uten4i uti5liz ution5a
uto5g uto5matic uts4 uu4m uxu3 uz4e v1ativ v1er1eig v1in v1oir5du1 v2inc
v3ativ v3el. v3eren v3i3liz v3if v3io4r v4e2s v4ely v4erd v4erel v4eres v4y
v5enue v5ereig v5ole va4ge va5lie va5mo va5niz va5pi va6guer vac3u vac5il
vag4 val1u val5o var5ied vaude3v ve4lo ve4te ve4ty veg3 vel3li ven3om ver3ie
ver3th ver5enc vermi4n ves1tite ves4te vet3er vi1ou vi1vip3a3r vi3so vi3su
vi4p vi5ali vi5gn vi5ro vik4 vin5d vio3l vis3it vit3r vo4la vo4ry vo4ta voi4
voice1p voice5p vom5i vor5ab vori4 w1b w1er w3c w3ev w3sh w4k w4no w5abl
w5al. w5p w5s4t wa1te wa5ger wa5ver wag5o wait5 wam4 war4t was4t waste3w6
waste3w6a2 wave1g4 waveg4 wea5rie weath3 wed4n wee5v week1n week3n weet3
wel4l west3 whi4 wi2 wide5sp wil2 will5in win4de win4g wir4 with3 wiz5 wl3in
wl4es wo4k1en wo4ken wo5ven wom1 wra4 wrap3aro wrap5aro wri4 writ6er. writa4
ws4l ws4pe wy4 x1a x1e x1h x1q x1t2 x1u x2ed x3c2 x3i x3o x3p x3ti x4ago
x4ap x4ime x4ob xac5e xam3 xas5 xe4cuto xe5ro xer4i xhi2 xhil5 xhu4 xi5a
xi5c xi5di xi5miz xpan4d xpe3d xpecto5 xquis3 xu3a xx4 y1b y1c y1d y1er y1i
y1o4 y1stro y1w y2ce y3ch y3la y3lo y3po y3ro y3s2e y3stro y3thin y4erf
y4o5g y4ons y4os y4ped y4poc y4so y5ac y5at y5che3d y5ched y5ee y5gi y5lu
y5pu yc5er ych4e ycom4 ycot4 ye4t yes4 yes5ter1y ylla5bl ym5e5try ym5etry
ymbol5 yme4 ympa3 yn3chr yn5d yn5g yn5ic yo5d yo5net yom4 yp2ta yp3i yper5
yr4r yr5ia yra5m ys1t ys3ica ys3io ys3ta ys4c yss4 ysur4 yt3ic z1er z2z3w
z3ian. z3o1phr z4il z4is z4zy z5a2b za1 zar2 ze3ro ze4n ze4p zet4 zo4m zo5ol
zte4
  PATTERNS

  lang.exceptions <<-EXCEPTIONS
% Do NOT make any alterations to this list! --- DEK
as-so-ciate as-so-ciates dec-li-na-tion oblig-a-tory phil-an-thropic present
presents project projects reci-procity re-cog-ni-zance ref-or-ma-tion
ret-ri-bu-tion ta-ble
  EXCEPTIONS
end
Text::Hyphen::Language::ENG_US = Text::Hyphen::Language::EN_US
Text::Hyphen::Language::EN     = Text::Hyphen::Language::EN_US
