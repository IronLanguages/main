# Hyphenation patterns for Text::Hyphen in Ruby: Norway
#   Converted from the TeX hyphenation/nohyph.tex file, by L. Thoresen and
#   D.F. Langmyhr (1994).
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
# Norwegian hyphenation patterns generated Fri Nov 18 17:02:41 MET 1994.
# These patterns are made by `patgen2' and based on    16268 words
# collected by L. Thoresen and D.F. Langmyhr (dag@ifi.uio.no),
# at the Department for Informatics,
# University of Oslo.
#++
if defined?(Text::Hyphen::Language::NO)
  raise LoadError, "Text::Hyphen::Language::NO has already been defined."
end

require 'text/hyphen/language'

Text::Hyphen::Language::NO = Text::Hyphen::Language.new do |lang|
  lang.patterns <<-PATTERNS
.a4 .an6e .ann6 .an3s .ar6n .ask6 .att6 .av7s .b4 .bund6 .d2 .de6r5 .di6ri
.e2 .e6d .eft2 .ei6e .ell6 .el6se. .ent6e .erk5l .es6k .ett6 .eu3 .ev6e .f6e
.g2 .gel6 .g6i .hånd5 .i4 .i7a .ik6 .il6le .ing6e .inn6e .inn5s .is7k .iv6
.k2 .l2 .m2 .mann6s .m6e .mekl7 .mis5 .må6li .må6te. .n2 .nest6e .o2 .o6b
.o6l .ot6 .p2 .pl6 .r2 .r6e .ri6k .s4 .sk6 .s6l6 .st4 .t4 .tas7 .teo7
.test7e .t6i .til3 .tog7 .to6m .tr6 .u3a .ue2 .u2k .undr6e .u5o .u6t .ut7re
.v2 .v6e .y2 .y6d .y6t .æ6r .ær6e .ø4 .øk6t .å2 .åpn5i .år6e aa6s6 5aas. a2b
abl6 a2d ad3r a1e ae6l a2f agd6 ag3l agn6 ag3r a2h a5he ais6 a7jo 6ake ak2er
a5ki a3la ald6 a6lek a7let all6er al6lin al6sa als6e al6to al6van a1ly a6me.
a6mek a6na 5analy a7nat andr6et an6et an7kl ano7 ans6t 6ant a3nu a3pa a1pi
ap5pl ap3r aps5 a1ra arakt6 1arbe a1ri ar5ig a6rig. a6rio arr6 ar6sl ar6st
as6kel as6sp ast7in a1ta a3tor at3r ats7e at6sj a1tu at7v aust6 a1ve a6vt
1b2 3ba ba6ko ba7na b3b b6bl b6bs 3be behold6 be3k6 ber6s be5rø be1s2 be3sk
beting6 be3tr 3bi bi5d bind7 bi3st 2b3le 6blema bl2i bo6kr bort6e br2 brukk6
2bs bs6e bun6e bu6si byd6 c2 car6 carm6 5cel c6en cl6i 1d 3da data3 datt6
da5v 2db 2dd dder6 3de 6dea 6dei dek3l de6le. d1en d6eni der6es d1et 6dete
2df 2dg 2dh 3di d1ig dig6e 6dim di6mo d1ing dio7 2d1k dk6l 2d1l 2d1m 2d1n
6domr do5pi 6dov 2d3p 2dr d6rev 3drif d1rin 2ds ds6e d3ska d6so d6su6 2d1t
d6tj d6tø 3du 2d1v 3dy 3dø 6døs 6døy då6r dårli6 ea6m e5as eass6 eat6e e5av
2eb ebe6t ebi6 e6bn 2e2d e6dar ed2er edi6e ed6ig. e7e6l eep6 ee6r e6et 2ef
ef6e e6fl e6fn eg2e egl6 eg3le e1gr 2eh 2ei eig6 eit6 2ek e1ka e6kk eklag6
ekl6e ek5sa ek3v ek6va 6e1la 6ele e6lem 2eli el6la 6e5lo 1els 2els. 7elser
el6sk 6elsm el6so e5lu e5ly e3lø 2em e6mar e6mene e6mit emt6 e3mø 1en. 2ena
2enb 6enc 2enda 5ende. 2endi 6endø 1ene 3ene. 6eneg 6enen 2ener ene6v 2enf
2eng 6enh en1in 6enit 2enk 6enl 6enm 6enn 6eno 6enp 2enr 6ensa 2ense 6ensk
ens6t 6ent 6env e5ny 6enz 6enæ 2eo eo6r 2e1p ep6l 1er. 6e1ra er5c 6erd 2ere
er5es erett6 6erf 2e1ri er7j 6erk 7erklæ 6erl 6erm 2ern 6erp 2ers 2ert 6erti
6eru e3rud 6erø 6erå 2es 6esa 6es2e e3ska esk6en e3spi e3spr es2st 6essu
e2ste es6teme es2ti es5tin e6stu e5sy 1et. 6eta 2ete 6et6h 2eti 2etj 6etn
6e3to 6etr e5trek ets3 ets7e et6st 2ett 6etu e3ty 2e5tø e7un 2e1v ev6el
ev6en. ev5end ey6t 1f2 3fa fa6e farm6 fa5v 3fe fe6b feld6e f5en. f5et. 2f3f
f6fa f6fb f6fm f6fs 3fi fi6f fiks7 fi5lo fing6 f6is flad6 flat6e fl6e 3fo2
for7n for5tr for3u fos6e fot6e fr6e freds5 fr6i 2fs2 f7st 2f3t1 f6tb fte6r
f6tf f6th fti6 f6tige f6tk ft2s f6tt f6tv f6tw 5ful 1ga 6g6al g6at 1g1e g6ea
ge6b g2el g3els g7else g6eni gens6 6gepl g5ere. ger6es g6est gforsk6 g1g
gg5es gg3i gg7re g6gt 2gh g6ha 1gi 6gig. g7ing g1isk 1gj gj6e g3k g2l 2g1n
g6nd g6nh g6nv 7go. 3gori g2r2 g6ru 2g2s1 g6sa6 g6sd gs6e g6sef g6sev g6sk
gs3p gs2ta gs6to g1t 1gu g1v 3gyn gy6r gø6t 1gå 1h2 hamm6 ha6r hard5 ha6s
7hav h6en he6ni henn6 he6r her6e hest6 2hi 7hild his3 his6e 3hol 6hop hos7
ho1v ht6e hv6e6 hverk6 6hyr hå6k ia6le i7ansk iass6 iat6e i6bb i2bl i3ca
ico6 i2d i3do idob6 id7ra ids3 i1ell iels6 i3elt ie6ra i5ern i1et i2f i6ga.
i7ge. i6ged ig6ens ig5ere 6iget i6gje ig7l 6igm 2ign 6igo i1ka ik5an ik6end
ik6es ik6kes ik3r iks6e i5la il7de ilede6 i6leg ils6e im6s i1mu 6in. 6ina
6ind 2ine in6fl ing5r ings3 6ini 2inj 2ink 6inn 6ins in6sk ins3p int2 2inæ
ip5l ipn6 ira6n i5ri i6ris ir6s 2is. i3sa 6isd 2ise is5end is7es. 6isj i6sk
1isk. 5iske. is3ku is3l is3n 6iss 2ist is3to ist3r it2 i3to it6se i1tu i1v
i6vas iv5end i6vf i6vk iv6st i6vv j2 ja6n jans6 jd6s jek6to jell6er j5en.
je6ni je6se je6t jevn5 jons3 jord5e jøs6 jø3v 6jøven ka6ras 1ke keds5 ke6fø
k1en k6enes k5ere. k5es. k1et k6ha k6hs ki5lo k3ing 6kip kipp6 k1k kk6el
kk6ere k6kl k6km k6ko k6kp kk7s k6ku k6kv 6kla. k3lam 2k1n k6ny 1ko 6kog
ko2mi kort5 k2r 7kraf k6rate krist6 6kry 2ks ks6k ks3pe ks3po 2k1t k6tend
kto3 kto6ri ku6e ku6k 3kun 5kur k6vo 5la. 6lak 6lal 5land. 7las. latt6e
l6dene ld7re 1le le6mo l1en l6ens l5ere. ler6er l3ern l1et le6ti 2lf lg5re
l6gu 1li li6er 7lig lig5e li6mi l1in li6na l5ing 5lio 2lip l5ism list5 livs7
l1j lja7 ljø7 l1k lk2e lk6l l6kv l1l l6lb l6lc l6ld l6lg l6lh llig6 l6lp
l6lr lls6 l6lv 2l1m l6mf l6ms ln2 7log log7e lom2s 6lop 6lor l6ov lo1v3e l1p
l3r 2l1s lsa6 ls1e lses3 l7skapet. ls2t6 l6sti l6sv lsøk6 lsø6r l6så 2l1t
lt6h 3lud lupp6 lu6r lutt7 l5va lvan6 lvann6 l1ve l6ves l3vi l5vo læ6res
lø6p lør6e 5løs 6løy 1ma mak3r mar6e 7mark 2mb 1me 6mee m3end met2 met5r m1g
1mi min6e mi3nu m5iske mis6la mi6su m3k m1l m1m mmu3 1mo mod6e mok5 6mom
6mov m3pa m3pi mp7la mp3r m1r mr6i m1s2 msid6 m5sk m6sl m6sp m1t muss6 m3v
3my mø6r 3mål 1na na5b nag6 nak6e 6nap 2nb nbygg6 nd6ens nd6er n6dj 1ne n6ea
ne6di n6eg nekk6 nek6l 5n1en 6nerg n5ert ness6 n1et n6ets 6nez 2nf ng6el
ng3ig ng3l n7gø 1ni 6ni. ni6d ni6el 6nif ni6mo 3ni6n n3isk 6niø n1j nj3en
n7ka nk6es n3kj n3ku n1l 2nm nme6 n6nad n6neb n2neh nn5in nning6 nns6e
nn5s6t nn3t4 n5ny 1no no6e 2nop n3opp n6or no6to 6nov n3p n1r 2ns nsa6 ns5ak
n6sb ns2en nsi6d nsj6 n3ska ns3n ns3po nst6e nst3r ns5vi n3sy 2n1t n6tei
nt3r n6tw n6tz 2nu n1v nv6e 6nye 1næ 2o oa2 o2b ob3j ob5l o6br odd6 odus7er
o6dw o6då oelv6 o2f o1g o6ga o6gb og6e o6gg o6gl o6gt o6gu o2h o1ka o3kr6
oks3 o3ku ol2i ol6je. o1lo ols6 olt6 o3lu omass6 om2e 3områ on1e on3k ons5ta
onæ6 o2p o3pa o6per op6pa 3oppg or3an 3ordn o5reg o7rek o6rel o3re3s o3ret
3orga o1ri or6min orn6 oro6 o6sja os3n o3so oss6 ota6k o5tas ot2e oti6k o3to
oty6 o3va ov2e oved3 5ove2r3 o5vo ov6s p2a parer6 7parti p3d pd6r 1pe p2el
per6es 6perk p1et pet6e pe6ter pett6 p1g 5pie pin6 p3ing plei6 p6l6i p7lis
p1n 1po 3pol6 po6r 3pos 6pov 2pp1 p3pa p3pi pp3r pp3t p2r2 pr6e 7prøv 2ps
2p1t pt6e pt6r 3pul på1 6raa 6ranl rans5e 6rar 6ratie 2rav rbu6 r6dat rd2e
rdens5 rd3et rd6in r6dw 1re 3re. re6a red6i re6e reff5 re6gr rekk6es r5eld
relev6 r1en 5ren. ren6e 6renn re5o 3r1er r6ere res3p res7tas r3et. 7reti
r6ets rfatt6 rfi6 rfisk6 r1g2 rg6e r6gis r6gl r6gm r6gv r1in r5isk ri5ta
riv6er 6riø r1k r6kb r6kd rk6es r6kh r6kre r1l2 rli6n rls6 r6lz r1m r6mg
r6mk r6ms r6mt r6mø r1n r6nd rne3k rner6e r6nh r6no ro3b 6rog3 ro5pe r6ov
r1p2 r6ph r1r rrest6 r6ris r6rs r6ry r1s rs2e r3ska r3sku rs6li r2sp rs5pe
rs2t6 r6su r1t r6tav r6td rter6es rti6s r7tvi r6tå ru6h 6r6um rur2 r6us
rus6e r1v rv6er r6vh 6røv rø3ve råk7 5sa. 6sab s3adv 6sai 7salen 3sa2m sa3me
samm6 sammen7 3sat 2sb sbe6t 2s3d sd6e sd6r 3se sek3r s2el selg6 sel6v
sent6e se6si se7sn s6ev 2sf s6fæ 2s1g sgiv6 2sh sha6 sho6 1si s3ig. s6ik
sik6e sikk6 s5ing. sinn6 sin6t s3isk s6iv 1sj sjeld6 6sju 2sk 6sk. 1ska
5skap 6skar sk6i 7skilt 3skj 6sko s6ko. 7s2kol skr6 5skrif 3sky skår6 s5land
2sle 3slå 2s1m s6mu s2n 6sne6 s5nes 6s6ok 1som somm6 3son so6n5i sop6 2s3ord
s2p2 6spa sp3lo sp6r 3s6pø 2s1r sregl7 2s1s ss6in s3sk s2sl ss2t s7ste ss7tu
2st. 7stad 7stan. 5start st2e 2ste. 3sted ste6e s2tei s6tek 7stemm s6tep
ste6ri st3et 2sth s2til 6stj 6s6tk 6stn st5ro st3s s6tu 6stv 3s2ty 3stø 1su
6su. sub3 sukk6 2sut sva6k 7svare 6svars 2sv6e sving6e sy6s sy6t sæ6r 6sør
sør6en så6k6 3ta. 6tad 1tal tal7es. 5tame tand7er. 3tant ta6r ta7ri 6tark
tart5 tas6 tats3 5tatt 3tau 2tb 1te 3te. 2ted teff6 t6eg te2gr t1en t3en.
te6na t3ene 6teng6 t6eni 6tenst t6ep te6rend te6si teste6 test7in t1et 6teu
2tf tfo6 2t1g 2t2h t3he t7hu 1ti ti6da t1ig ti6gen ti6li 6tinn tins6 t1is
t3isk t6ism 7tiva tj6e 3tjen 2t1k tk6l 4t1l2 t5le 2t3m 2t3n tn2e tok6e to6me
3ton 6top tot6 6tov 2t1p 2t2r2 t6ram t6rek tren6i tre6p trett6 t3rib t6ro.
6tru 4ts ts6e ts7pa t1s2t ts3v 2t1t t7te tt7ere. t3ti tt3in t3tr t6tu tt3v
t6tø t6tå 6tun 3tur tur5e 2tv t7vek 3tæ tøl6 tøs6 u2b ub5l u2d uder6 u1e
u2er uer6s u5ert u2f ug6en ug3l u3gy uin6 uk6ene uk6er. u3la uld6e ul6e
u6like ulp6 ul6v u3ly u3læ u6mi umm6 undi6 un6ene un6gi u2ni un6s6 unt6 u3ra
ural6 u3ri u6rin u6sa u6sei u6sel us5p ust3r u6su u2t u7tal ut6e u3ter u7ti
ut5ing ut7j u3to ut6r ut7ryk ut1s6 3ut1v ut6ve ut7ø u1v uver6 vakk6 va5ku
3valg5 va5lu vang6 vart6e var6ti va6te 2vd ve6d v3en. v6eni 7verdi ver6es
3ves vesk6 v1g vi6det vik6er visk6 vit6e v5je v3k 2v1n v6na v6ng v6nl 6vop
2vr v5rig v7ro 2v1s2 v6sky v6sv 2v1t vunn6 v1v våk6e wa6g6 wa6r wegn6 w6en
wer6e wern6 wi6e wol6 w6r6e x7is yang6 y6bd y1e y5es yg6gr y6hn y6hr yn6s3
yns6e y5ri yr6se y3rå y6si ysk6 yt5r yttal6 z6en z6er zu6e æg6e ær1 æ1ri
ærn6 æt6e ø2d ø6de. ø2f ø6ga økk6 ø6ko ølg6er øll6 ømm6en. ønn6i ør1 ø1ri
ør6nø ørr6 ør2s ørs5t ø6sj øs7l øs3n øvi6 åd1 åd6r å1e åp2e å2r1 ård5e 5årsa
år6sl å6se ås6s6
  PATTERNS
end
Text::Hyphen::Language::NOR = Text::Hyphen::Language::NO
