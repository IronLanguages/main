# Hyphenation patterns for Text::Hyphen in Ruby: Spanish
#   Converted from the TeX hyphenation/eshyph.tex file, based on the work by
#   Javier Bezos in 1993, 1997, and 2001 - 2003.
#
# The original copyright holds and is reproduced in the source to this file.
# The Ruby version of these patterns are copyright 2004 Austin Ziegler and
# are available under an MIT license. See LICENCE for more information.
#--
# DIVISI'ON DE PALABRAS
# ~~~~~~~~~~~~~~~~~~~~~
# eshyph.tex 4.0a
#
# Why 4.0? Well, I know at least other three files with the same name, so
# this one is the fourth. The others should vanish as soon as posible.
#
# Javier Bezos. 1993, 1997, 2001-2003
# Some parts, by Francesc Carmona
# Licence: LPPL
#
# Hyphenation files
# ~~~~~~~~~~~~~~~~~
# Unfortunately, no hyphentation file for Spanish will produce the right
# result. Some of them are definitely wrong, while others at least were
# acceptable:
#
# - obsolete/sphyph: by Julio Sanchez. Includes shyphen.sh (a sh script),
#   and sphyph.tex, a set of patterns generated with the former.
#
# - obsolete/eshyph.tex: by Francesc Carmona. Requires cathyph.tex. However,
#   note the file includes the following copyright notice:
# =====
# % General permission for use and non-profit redistribution is granted.
# % For special commercial use, contact the above address.
# =====
#
# CervanTeX, the Spanish TeX User Group, has tackled the task of writing
# a more complete set of patterns, but unfortunately the work is a lot
# slower we expected. The file division.pdf is a draft of an article (in
# Spanish) explaining the rules to be applied and how they are being
# translated into TeX in a forthcoming unified set of patterns.
#
# A first version is available, but there is still a lot of work to be done.
# However, even if incomplete it should provide in its current form a better
# option to these above. However, if you experience problems, you might want
# to return to one of these pattern files.
#
# I would like to thanks Francesc Carmona for his permission to steal parts
# of his work without restrictions.
#
# 15/06/2003
#
# _____________________________________________________________
# Javier Bezos                | http://www.cervantex.org
# Presidente de CervanTeX     | presidente@cervantex.org
# .............................................................
# TeX y tipografia            | http://perso.wanadoo.es/jbezos/
#++
require 'text/hyphen/language'

Text::Hyphen::Language::ES = Text::Hyphen::Language.new do |lang|
  lang.patterns <<-PATTERNS
1ñ 1b 2bb 2bc 2bd 2bf 2bg 2b1h 2bj 2bk b2l 2bl. 2bm 2bn 2bp 2bq b2r 2br. 2bs
2bt 2bv 2bx 2by 2bz 1c 2cb 2cc 2cd 2cf 2cg c4h 2cj c2k c2l 2cl. 2cm 2cn 2cp
2cq c2r 2cr. 2cs 2ct 2cv 2cx 2cy 2cz 1d 2db 2dc 2dd 2df 2dg 2d1h 2dj 2dk 2dl
2dm 2dn 2dp 2dq d2r 2dr. 2ds 2dt 2dv 2dx 2dy 2dz 1f 2fb 2fc 2fd 2ff 2fg 2f1h
2fj 2fk f2l 2fl. 2fm 2fn 2fp 2fq f2r 2fr. 2fs 2ft 2fv 2fx 2fy 2fz 1g 2gb 2gc
2gd 2gf 2gg 2g2h 2gj 2gk g2l 2gl. 2gm 2gn 2gp 2gq g2r 2gr. 2gs 2gt 2gv 2gx
2gy 2gz 2hb 2hc 2hd 2hf 2hg 2h1h 2hj 2hk 2hl 2hm 2hn 2hp 2hq 2hr 2hs 2ht 2hv
2hx 2hy 2hz 1j 2jb 2jc 2jd 2jf 2jg 2j1h 2jj 2jk 2jl 2jm 2jn 2jp 2jq 2jr 2js
2jt 2jv 2jx 2jy 2jz 1k 2kb 2kc 2kd 2kf 2kg 2k2h 2kj 2kk k2l 2kl. 2km 2kn 2kp
2kq k2r 2kr. 2ks 2kt 2kv 2kx 2ky 2kz 1l 2lb 2lc 2ld 2lf 2lg 2l1h 2lj 2lk l4l
2ll. 2lm 2ln 2lp 2lq 2lr 2ls 2lt 2lv 2lx 2ly 2lz 1m 2mb 2mc 2md 2mf 2mg 2m1h
2mj 2mk 2ml 2mm 2mn 2mp 2mq 2mr 2ms 2mt 2mv 2mx 2my 2mz 1n 2nb 2nc 2nd 2nf
2ng 2n1h 2nj 2nk 2nl 2nm 2nn 2np 2nq 2nr 2ns 2nt 2nv 2nx 2ny 2nz 1p 2pb 2pc
2pd 2pf 2pg 2p1h 2pj 2pk p2l 2pl. 2pm 2pn 2pp 2pq p2r 2pr. 2ps 2pt 2pv 2px
2py 2pz 1q 2qb 2qc 2qd 2qf 2qg 2q1h 2qj 2qk 2ql 2qm 2qn 2qp 2qq 2qr 2qs 2qt
2qv 2qx 2qy 2qz 1r 2rb 2rc 2rd 2rf 2rg 2r1h 2rj 2rk 2rl 2rm 2rn 2rp 2rq r2r
2rr. 2rs 2rt 2rv 2rx 2ry 2rz 1s 2sb 2sc 2sd 2sf 2sg 2s1h 2sj 2sk 2sl 2sm 2sn
2sp 2sq 2sr 2ss 2st 2sv 2sx 2sy 2sz 1t 2tb 2tc 2td 2tf 2tg 2t1h 2tj 2tk 2tm
2tn 2tp 2tq t2r 2tr. 2ts 2tt 2tv t2x 2ty 2tz 1v 2vb 2vc 2vd 2vf 2vg 2v1h 2vj
2vk v2l 2vl. 2vm 2vn 2vp 2vq v2r 2vr. 2vs 2vt 2vv 2vx 2vy 2vz 4w 1x 2xb 2xc
2xd 2xf 2xg 2x1h 2xj 2xk 2xl 2xm 2xn 2xp 2xq 2xr 2xs 2xt 2xv 2xx 2xy 2xz 1y
2yb 2yc 2yd 2yf 2yg 2y1h 2yj 2yk 2yl 2ym 2yn 2yp 2yq 2yr 2ys 2yt 2yv 2yx 2yy
2yz 1z 2zb 2zc 2zd 2zf 2zg 2z1h 2zj 2zk 2zl 2zm 2zn 2zp 2zq 2zr 2zs 2zt 2zv
2zx 2zy 2zz 2t2l 2no. .no2 4caca4 4cago4 4caga4 4cagas. 4teta. 4tetas.
4puta4 4puto4 .hu4mea .hu4meo .he4mee 4meo. 4meable. 4meables. 4pedo4 4culo4
a4i3go a4es a4e3mos a4éis a4en a4ías a4ía a4ía3mos a4íais a4ían a4eré
a4e3rás a4e3rá a4e3remos a4e3réis a4e3rán a4i3ga a4ai3gan a4e3ría a4edme
a4edl a4eos 4er. a4erme a4érme a4erte a4érte a4erle a4erse a4érse a4erlo
a4erla a4ernos a4érnos a4eros 4ído. 4ída. 4ídos. 4ídas. 4as 4amos 4áis 4an
4ásteis 4aron 4aré 4ará 4ara 4áramos 4árais 4aran 4are 4áremos 4áreis 4aren
4adme 4adl 4aos 4ar. 4arme 4árme 4arte 4árte 4arle 4arse 4árse 4arlo 4arla
4arnos 4árnos 4aros 4ado. 4ada. 4ados. 4adas. acto1a2 acto1e2 acto1i2
acto1o2 acto1u2 acto1h acto1á2 acto1é2 acto1í2 acto1ó2 acto1ú2 afro1a2
afro1e2 afro1i2 afro1o2 afro1u2 afro1h afro1á2 afro1é2 afro1í2 afro1ó2
afro1ú2 .a2 .an2a2 .an2e2 .an2i2 .an2o4 .an2u2 .an2á2 .an2é2 .an2í2 .an2ó2
.an2ú2 .ana3lí .aná3li .ana3li .an3aero .an3e2pigr .an3h .an3i2so .anua3l
.anu3bl .anu3da .anu3l aero1a2 aero1e2 aero1i2 aero1o2 aero1u2 aero1h
aero1á2 aero1é2 aero1í2 aero1ó2 aero1ú2 anfi1a2 anfi1e2 anfi1i2 anfi1o2
anfi1u2 anfi1h anfi1á2 anfi1é2 anfi1í2 anfi1ó2 anfi1ú2 anglo1a2 anglo1e2
anglo1i2 anglo1o2 anglo1u2 anglo1h anglo1á2 anglo1é2 anglo1í2 anglo1ó2
anglo1ú2 ante1a2 ante1e2 ante1i2 ante1o2 ante1u2 ante1h ante1á2 ante1é2
ante1í2 ante1ó2 ante1ú2 .ante2o3je .anti1a2 .anti1e2 .anti1i2 .anti1o2
.anti1u2 .anti1h .anti1á2 .anti1é2 .anti1í2 .anti1ó2 .anti1ú2 ti2o3qu
ti2o3co archi1a2 archi1e2 archi1i2 archi1o2 archi1u2 archi1h archi1á2
archi1é2 archi1í2 archi1ó2 archi1ú2 auto1a2 auto1e2 auto1i2 auto1o2 auto1u2
auto1h auto1á2 auto1é2 auto1í2 auto1ó2 auto1ú2 biblio1a2 biblio1e2 biblio1i2
biblio1o2 biblio1u2 biblio1h biblio1á2 biblio1é2 biblio1í2 biblio1ó2
biblio1ú2 bio1a2 bio1e2 bio1i2 bio1o2 bio1u2 bio1h bio1á2 bio1é2 bio1í2
bio1ó2 bio1ú2 bi1u2ní cardio1a2 cardio1e2 cardio1i2 cardio1o2 cardio1u2
cardio1h cardio1á2 cardio1é2 cardio1í2 cardio1ó2 cardio1ú2 cefalo1a2
cefalo1e2 cefalo1i2 cefalo1o2 cefalo1u2 cefalo1h cefalo1á2 cefalo1é2
cefalo1í2 cefalo1ó2 cefalo1ú2 centi1a2 centi1e2 centi1i2 centi1o2 centi1u2
centi1h centi1á2 centi1é2 centi1í2 centi1ó2 centi1ú2 ciclo1a2 ciclo1e2
ciclo1i2 ciclo1o2 ciclo1u2 ciclo1h ciclo1á2 ciclo1é2 ciclo1í2 ciclo1ó2
ciclo1ú2 cito1a2 cito1e2 cito1i2 cito1o2 cito1u2 cito1h cito1á2 cito1é2
cito1í2 cito1ó2 cito1ú2 3c2neor cnico1a2 cnico1e2 cnico1i2 cnico1o2 cnico1u2
cnico1h cnico1á2 cnico1é2 cnico1í2 cnico1ó2 cnico1ú2 .co1a2 .co1e2 .co1i2
.co1o2 .co1u2 .co1h .co1á2 .co1é2 .co1í2 .co1ó2 .co1ú2 .co2 co4á3gul co4acci
co4acti co4adju co4a3dun co4adyu co4a3gul co4a3lic co4aptac co4art co4árt
co4e3fic co4erc co4e3tá co4imbr co4inci co4i3to co4o3per co4o3pér co4opt
co4ord con1imbr con1urb cripto1a2 cripto1e2 cripto1i2 cripto1o2 cripto1u2
cripto1h cripto1á2 cripto1é2 cripto1í2 cripto1ó2 cripto1ú2 crono1a2 crono1e2
crono1i2 crono1o2 crono1u2 crono1h crono1á2 crono1é2 crono1í2 crono1ó2
crono1ú2 contra1a2 contra1e2 contra1i2 contra1o2 contra1u2 contra1h
contra1á2 contra1é2 contra1í2 contra1ó2 contra1ú2 deca1a2 deca1e2 deca1i2
deca1o2 deca1u2 deca1h deca1á2 deca1é2 deca1í2 deca1ó2 deca1ú2 4e3dro.
4e3dros. 4é3drico. 4é3dricos. 4é3drica. 4é3dricas. deca2i3mient decimo1
.des1a2 .des1e2 .des1i2 .des1o2 .des1u2 .des1á2 .des1é2 .des1í2 .des1ó2
.des1ú2 des2a2 des2e2 3sa. 3sas. de2s3órde de2s3orde de2s3abast de2s3aboll
de2s3aboto de2s3abr desa3brid de2s3abroch de2s3aceit de2s3aceler desa3cert
desa3ciert de2s3acobar de2s3acomod de2s3acomp de2s3acons de2s3acopl
de2s3acorr de2s3acostum de2s3acot desa3craliz desa3credit de2s3activ
de2s3aderez de2s3adeud de2s3adorar de2s3adormec de2s3adorn de2s3advert
desa3fí de2s3aferr desa3fi de2s3afic de2s3afil de2s3afin de2s3afor desa3gü
desa3garr de2s3agraci desa3grad de2s3agravi de2s3agreg de2s3agrup de2s3agu
desa3guisado de2s3aherr de2s3ahij de2s3ajust de2s3alagar de2s3alent
de2s3alfom de2s3alfor de2s3aliñ desa3lin de2s3alien de2s3aline desa3liv
de2s3alm de2s3almid desa3loj de2s3alquil de2s3alter de2s3alumbr desa3marr
desa3mobl de2s3amold de2s3amort de2s3amuebl de2s3and de2s3angel de2s3anid
de2s3anim de2s3aním de2s3anud desa3pañ desa3pacib de2s3apadr de2s3apare
desa3parec desa3paric desa3peg desa3percib de2s3aplic de2s3apolill de2s3apoy
desa3prens de2s3apret de2s3apriet de2s3aprob de2s3apropi de2s3aprovech
de2s3arbol de2s3aren de2s3arm des4arme de2s3arraig de2s3arregl de2s3arrend
de2s3arrim desa3rroll de2s3arrop de2s3arrug de2s3articul de2s3asent
de2s3asist de2s3asn desa3soseg desa3sosieg de2s3atenc de2s3atend de2s3atiend
de2s3atent desa3tin de2s3atorn de2s3atranc de2s3autor de2s3avis desa3yun
desa3zón desa3zon de2s3embal de2s3embál de2s3embar de2s3embár de2s3embarg
de2s3embols de2s3emborr de2s3embosc de2s3embot de2s3embrag de2s3embrág
de2s3embrave de2s3embráve de2s3embroll de2s3embróll de2s3embruj de2s3embrúj
de3semej de2s3empañ de2s3empáñ de2s3empac de2s3empaquet de2s3empaquét
de2s3emparej de2s3emparéj de2s3emparent de2s3empat de2s3empé de2s3empedr
de2s3empeg de2s3empeor de2s3emperez de2s3empern de2s3emple de2s3empolv
de2s3empotr de2s3empoz de2s3enam de2s3encab de2s3encad de2s3encaj de2s3encáj
de2s3encall de2s3encáll de2s3encam de3sencant de2s3encap de2s3encar
de2s3encár de2s3ench de2s3encl de2s3enco de2s3encr de2s3encu de2s3end
de3senfad de3senfád de2s3enfi de2s3enfo de2s3enfó de3senfren de2s3enfund
de2s3enfur de3sengañ de3sengáñ de2s3enganch de2s3engar de2s3engas de2s3engom
de2s3engoz de2s3engra de2s3enhebr de2s3enj de2s3enlad de2s3enlaz de2s3enlo
de2s3enm de2s3enr de2s3ens de2s3enta de3sentend de3sentien de3sentién
de2s3enter de2s3entier de2s3entiér de2s3ento de2s3entr de2s3entu de2s3envain
de3senvolvim de3seo de2s3eq de3serci de3sert de2s3espa de3sesperac
de2s3esperanz de3sesper de2s3estabil de2s3estim de3sider de3sidia de3sidio
de3siert de3sign de3sigual de3silusi de2s3imagin de2s3iman de2s3impon
de2s3impresX de2s3incent de2s3inclin de2s3incorp de2s3incrust de3sinenc
de3sinfec de2s3inflam de2s3infl de2s3inform de2s3inhib de2s3insect
de2s3instal de3sintegr de3sinter de2s3intox de2s3inver de3sisten de2s3obedec
de2s3oblig de2s3obstr de3socup de2s3odor de3solac de3solad de3soll de3suell
de3sonce .dieci1o2 dodeca1a2 dodeca1e2 dodeca1i2 dodeca1o2 dodeca1u2
dodeca1h dodeca1á2 dodeca1é2 dodeca1í2 dodeca1ó2 dodeca1ú2 ecano1a2 ecano1e2
ecano1i2 ecano1o2 ecano1u2 ecano1h ecano1á2 ecano1é2 ecano1í2 ecano1ó2
ecano1ú2 eco1a2 eco1e2 eco1i2 eco1o2 eco1u2 eco1h eco1á2 eco1é2 eco1í2
eco1ó2 eco1ú2 ectro1a2 ectro1e2 ectro1i2 ectro1o2 ectro1u2 ectro1h ectro1á2
ectro1é2 ectro1í2 ectro1ó2 ectro1ú2 endo1a2 endo1e2 endo1i2 endo1o2 endo1u2
endo1h endo1á2 endo1é2 endo1í2 endo1ó2 endo1ú2 ento1a2 ento1e2 ento1i2
ento1o2 ento1u2 ento1h ento1á2 ento1é2 ento1í2 ento1ó2 ento1ú2 entre1a2
entre1e2 entre1i2 entre1o2 entre1u2 entre1h entre1á2 entre1é2 entre1í2
entre1ó2 entre1ú2 euco1a2 euco1e2 euco1i2 euco1o2 euco1u2 euco1h euco1á2
euco1é2 euco1í2 euco1ó2 euco1ú2 euro1a2 euro1e2 euro1i2 euro1o2 euro1u2
euro1h euro1á2 euro1é2 euro1í2 euro1ó2 euro1ú2 fono1a2 fono1e2 fono1i2
fono1o2 fono1u2 fono1h fono1á2 fono1é2 fono1í2 fono1ó2 fono1ú2 foto1a2
foto1e2 foto1i2 foto1o2 foto1u2 foto1h foto1á2 foto1é2 foto1í2 foto1ó2
foto1ú2 gastro1a2 gastro1e2 gastro1i2 gastro1o2 gastro1u2 gastro1h gastro1á2
gastro1é2 gastro1í2 gastro1ó2 gastro1ú2 geo1a2 geo1e2 geo1i2 geo1o2 geo1u2
geo1h geo1á2 geo1é2 geo1í2 geo1ó2 geo1ú2 gluco1a2 gluco1e2 gluco1i2 gluco1o2
gluco1u2 gluco1h gluco1á2 gluco1é2 gluco1í2 gluco1ó2 gluco1ú2 hecto1a2
hecto1e2 hecto1i2 hecto1o2 hecto1u2 hecto1h hecto1á2 hecto1é2 hecto1í2
hecto1ó2 hecto1ú2 helio1a2 helio1e2 helio1i2 helio1o2 helio1u2 helio1h
helio1á2 helio1é2 helio1í2 helio1ó2 helio1ú2 hemato1a2 hemato1e2 hemato1i2
hemato1o2 hemato1u2 hemato1h hemato1á2 hemato1é2 hemato1í2 hemato1ó2
hemato1ú2 hemo1a2 hemo1e2 hemo1i2 hemo1o2 hemo1u2 hemo1h hemo1á2 hemo1é2
hemo1í2 hemo1ó2 hemo1ú2 2al. 2ales. hexa1a2 hexa1e2 hexa1i2 hexa1o2 hexa1u2
hexa1h hexa1á2 hexa1é2 hexa1í2 hexa1ó2 hexa1ú2 hidro1a2 hidro1e2 hidro1i2
hidro1o2 hidro1u2 hidro1h hidro1á2 hidro1é2 hidro1í2 hidro1ó2 hidro1ú2
hipe2r1a2 hipe2r1e2 hipe2r1i2 hipe2r1o2 hipe2r1u2 hipe2r3r hipe2r1á2
hipe2r1é2 hipe2r1í2 hipe2r1ó2 hipe2r1ú2 per4emia histo1a2 histo1e2 histo1i2
histo1o2 histo1u2 histo1h histo1á2 histo1é2 histo1í2 histo1ó2 histo1ú2
homo1a2 homo1e2 homo1i2 homo1o2 homo1u2 homo1h homo1á2 homo1é2 homo1í2
homo1ó2 homo1ú2 icono1a2 icono1e2 icono1i2 icono1o2 icono1u2 icono1h
icono1á2 icono1é2 icono1í2 icono1ó2 icono1ú2 infra1a2 infra1e2 infra1i2
infra1o2 infra1u2 infra1h infra1á2 infra1é2 infra1í2 infra1ó2 infra1ú2
.inte2r1a2 .inte2r1e2 .inte2r1i2 .inte2r1o2 .inte2r1u2 .inte2r3r .inte2r1á2
.inte2r1é2 .inte2r1í2 .inte2r1ó2 .inte2r1ú2 .in3ter2e3sa .in3ter2e3se
.in3ter2e3so .in3ter2e3sá .in3ter2e3sé .in3ter2e3só .in3te2r3ino
.in3te2r3ina .in3te2r3inidad .in3te3r4rog .in3te3r4rupc .in3te3r4rupt
.in3te3r4rump intra1a2 intra1e2 intra1i2 intra1o2 intra1u2 intra1h intra1á2
intra1é2 intra1í2 intra1ó2 intra1ú2 iso1a2 iso1e2 iso1i2 iso1o2 iso1u2 iso1h
iso1á2 iso1é2 iso1í2 iso1ó2 iso1ú2 kilo1a2 kilo1e2 kilo1i2 kilo1o2 kilo1u2
kilo1h kilo1á2 kilo1é2 kilo1í2 kilo1ó2 kilo1ú2 macro1a2 macro1e2 macro1i2
macro1o2 macro1u2 macro1h macro1á2 macro1é2 macro1í2 macro1ó2 macro1ú2 mal2
ma4l3h .ma4l3edu bien2 bien3h maxi1a2 maxi1e2 maxi1i2 maxi1o2 maxi1u2 maxi1h
maxi1á2 maxi1é2 maxi1í2 maxi1ó2 maxi1ú2 megalo1a2 megalo1e2 megalo1i2
megalo1o2 megalo1u2 megalo1h megalo1á2 megalo1é2 megalo1í2 megalo1ó2
megalo1ú2 mega1a2 mega1e2 mega1i2 mega1o2 mega1u2 mega1h mega1á2 mega1é2
mega1í2 mega1ó2 mega1ú2 micro1a2 micro1e2 micro1i2 micro1o2 micro1u2 micro1h
micro1á2 micro1é2 micro1í2 micro1ó2 micro1ú2 mini1a2 mini1e2 mini1i2 mini1o2
mini1u2 mini1h mini1á2 mini1é2 mini1í2 mini1ó2 mini1ú2 2o. 2os. 2oso. 2osos.
multi1a2 multi1e2 multi1i2 multi1o2 multi1u2 multi1h multi1á2 multi1é2
multi1í2 multi1ó2 multi1ú2 miria1a2 miria1e2 miria1i2 miria1o2 miria1u2
miria1h miria1á2 miria1é2 miria1í2 miria1ó2 miria1ú2 mono1a2 mono1e2 mono1i2
mono1o2 mono1u2 mono1h mono1á2 mono1é2 mono1í2 mono1ó2 mono1ú2 2ico. 2icos.
namo1a2 namo1e2 namo1i2 namo1o2 namo1u2 namo1h namo1á2 namo1é2 namo1í2
namo1ó2 namo1ú2 necro1a2 necro1e2 necro1i2 necro1o2 necro1u2 necro1h
necro1á2 necro1é2 necro1í2 necro1ó2 necro1ú2 neo1a2 neo1e2 neo1i2 neo1o2
neo1u2 neo1h neo1á2 neo1é2 neo1í2 neo1ó2 neo1ú2 neto1a2 neto1e2 neto1i2
neto1o2 neto1u2 neto1h neto1á2 neto1é2 neto1í2 neto1ó2 neto1ú2 norte1a2
norte1e2 norte1i2 norte1o2 norte1u2 norte1h norte1á2 norte1é2 norte1í2
norte1ó2 norte1ú2 octo1a2 octo1e2 octo1i2 octo1o2 octo1u2 octo1h octo1á2
octo1é2 octo1í2 octo1ó2 octo1ú2 octa1a2 octa1e2 octa1i2 octa1o2 octa1u2
octa1h octa1á2 octa1é2 octa1í2 octa1ó2 octa1ú2 oligo1a2 oligo1e2 oligo1i2
oligo1o2 oligo1u2 oligo1h oligo1á2 oligo1é2 oligo1í2 oligo1ó2 oligo1ú2
omni1a2 omni1e2 omni1i2 omni1o2 omni1u2 omni1h omni1á2 omni1é2 omni1í2
omni1ó2 omni1ú2 i2o. i2os. paleo1a2 paleo1e2 paleo1i2 paleo1o2 paleo1u2
paleo1h paleo1á2 paleo1é2 paleo1í2 paleo1ó2 paleo1ú2 para1a2 para1e2 para1i2
para1o2 para1u2 para1h para1á2 para1é2 para1í2 para1ó2 para1ú2 penta1a2
penta1e2 penta1i2 penta1o2 penta1u2 penta1h penta1á2 penta1é2 penta1í2
penta1ó2 penta1ú2 piezo1a2 piezo1e2 piezo1i2 piezo1o2 piezo1u2 piezo1h
piezo1á2 piezo1é2 piezo1í2 piezo1ó2 piezo1ú2 pluri1a2 pluri1e2 pluri1i2
pluri1o2 pluri1u2 pluri1h pluri1á2 pluri1é2 pluri1í2 pluri1ó2 pluri1ú2
proto1a2 proto1e2 proto1i2 proto1o2 proto1u2 proto1h proto1á2 proto1é2
proto1í2 proto1ó2 proto1ú2 radio1a2 radio1e2 radio1i2 radio1o2 radio1u2
radio1h radio1á2 radio1é2 radio1í2 radio1ó2 radio1ú2 ranco1a2 ranco1e2
ranco1i2 ranco1o2 ranco1u2 ranco1h ranco1á2 ranco1é2 ranco1í2 ranco1ó2
ranco1ú2 rmano1a2 rmano1e2 rmano1i2 rmano1o2 rmano1u2 rmano1h rmano1á2
rmano1é2 rmano1í2 rmano1ó2 rmano1ú2 retro1a2 retro1e2 retro1i2 retro1o2
retro1u2 retro1h retro1á2 retro1é2 retro1í2 retro1ó2 retro1ú2 romo1a2
romo1e2 romo1i2 romo1o2 romo1u2 romo1h romo1á2 romo1é2 romo1í2 romo1ó2
romo1ú2 sobre1a2 sobre1e2 sobre1i2 sobre1o2 sobre1u2 sobre1h sobre1á2
sobre1é2 sobre1í2 sobre1ó2 sobre1ú2 semi1a2 semi1e2 semi1i2 semi1o2 semi1u2
semi1h semi1á2 semi1é2 semi1í2 semi1ó2 semi1ú2 i2a. i2as. 2ótic emi2o2
seudo1a2 seudo1e2 seudo1i2 seudo1o2 seudo1u2 seudo1h seudo1á2 seudo1é2
seudo1í2 seudo1ó2 seudo1ú2 o2os. socio1a2 socio1e2 socio1i2 socio1o2
socio1u2 socio1h socio1á2 socio1é2 socio1í2 socio1ó2 socio1ú2 a3rio. a3rios.
4ón. 4ones. 4i4er. 4o2ide. 4o2ides. 4i2dal. 4i2dales. 4i3deo. 4i3deos.
sub1a2 sub1e2 sub1i2 sub1o2 sub1u2 sub1á2 sub1é2 sub1í2 sub1ó2 sub1ú2 su2b
.sub2ast sub2i1ll sub2i1mien sub2intra sub2lev sub2lim sub3ray supe2r1a2
supe2r1e2 supe2r1i2 supe2r1o2 supe2r1u2 supe2r3r supe2r1á2 supe2r1é2
supe2r1í2 supe2r1ó2 supe2r1ú2 supra1a2 supra1e2 supra1i2 supra1o2 supra1u2
supra1h supra1á2 supra1é2 supra1í2 supra1ó2 supra1ú2 talmo1a2 talmo1e2
talmo1i2 talmo1o2 talmo1u2 talmo1h talmo1á2 talmo1é2 talmo1í2 talmo1ó2
talmo1ú2 termo1a2 termo1e2 termo1i2 termo1o2 termo1u2 termo1h termo1á2
termo1é2 termo1í2 termo1ó2 termo1ú2 tetra1a2 tetra1e2 tetra1i2 tetra1o2
tetra1u2 tetra1h tetra1á2 tetra1é2 tetra1í2 tetra1ó2 tetra1ú2 topo1a2
topo1e2 topo1i2 topo1o2 topo1u2 topo1h topo1á2 topo1é2 topo1í2 topo1ó2
topo1ú2 tropo1a2 tropo1e2 tropo1i2 tropo1o2 tropo1u2 tropo1h tropo1á2
tropo1é2 tropo1í2 tropo1ó2 tropo1ú2 ultra1a2 ultra1e2 ultra1i2 ultra1o2
ultra1u2 ultra1h ultra1á2 ultra1é2 ultra1í2 ultra1ó2 ultra1ú2 xeno1a2
xeno1e2 xeno1i2 xeno1o2 xeno1u2 xeno1h xeno1á2 xeno1é2 xeno1í2 xeno1ó2
xeno1ú2 inter4és inter4esar inter4in inter4ino inter4ior mili4ar mili4ario
mini4atur para4íso para4ulata poli4árq poli4éste poli4andr poli4antea
poli4arq poli4omiel post4ín post4óni post4a post4al post4e post4elero
post4emero post4erga post4eri post4eta post4ila post4ill post4ine post4izo
post4or post4ul post4ura pos4t3rom pos4t3ope pos4t3rev pro4emio pro4eza
super4able super4ación super4ar super4ior tele4olótico tele4ología tran4sacc
trans4ar trans4eúnte trans4iber trans4ición trans4ido trans4igen trans4igir
trans4istor trans4itab trans4it trans4itorio trans4ubsta ultra4ísmo wa3s4h
.bi1anual .bi1aur .bien1and .bien1apa .bien1ave .bien1est .bien1int .bi1ox
.bi1óx .bi1un .contra1a .contra1ind .en1aceit .en1aciy .en1aguach .en1aguaz
.en1anch .en1apa .en1arb .en1art .en2artr .en1ej .hepta1e .intra1o .intra1u
.mal1acon .mal1acos .mala1e .mal1andant .mal1andanz .mal1est .mal1int
.pan1ame .pan1esl .pan1eur .pan1isl .pan1ópt 1p2teríneo 3p2sic 3p2siq .re1a
.re2al .re3alc .re3aleg .re3alq .re3alz .re1e .re1im .re1inc .re1ing .re1ins
.re1int .re1ob .re1oc .re1oj .re1org .re1unt .retro1a .so1a .sud1afr
.sud1ame .sud1est sud1oes .sur1ame .sur1est .sur1oes .tele1imp .tele1obj
.tras1a .tras1o .tras2oñ .tran2s1alp .tran2s1and .tran2s1atl .tran2s1oce
.tran2s1ur .tri1óx
  PATTERNS
end
Text::Hyphen::Language::SPA = Text::Hyphen::Language::ES
