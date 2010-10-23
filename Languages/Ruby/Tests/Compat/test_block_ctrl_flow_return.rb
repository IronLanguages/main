# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************


# helper
def myeval(line); puts 1; eval(line); puts 2; end
def call1(x); puts 3; call2(x); puts 4; end
def call2(x); puts 5; call3(x); puts 6; end 
def call3(x); puts 7; puts x.call; puts 8; end

# producer

def get_block(&p);    p;                end 
def get_lambda(&p);   lambda(&p);       end 
def get_proc(&p);     Proc.new(&p);     end 

def get_local_block;        get_block { puts 9; return; puts 10 };    end 
def get_local_lambda;       lambda { puts 11; return; puts 12 };       end 
def get_local_proc;         Proc.new { puts 13; return; puts 14 };     end 

# consumer 

# taking arguments
def iterator_via_yield;                     puts 15; x = yield; puts x; puts 16;     end 
def iterator_via_call(&p);                  puts 17; puts(p.call); puts 18;   end 

def method_call_iterator_via_yield(&p);     puts 19; iterator_via_yield(&p); puts 20;     end
def method_call_iterator_via_call(&p);      puts 21; iterator_via_call(&p); puts 22;      end 

def method_use_lambda_and_yield;            puts 23; x = lambda { puts 24; yield; puts 25 }; puts x.call; puts 26; end 
def method_use_proc_and_yield;              puts 27; x = Proc.new { puts 28; yield; puts 29 }; puts x.call; puts 30; end 
def method_use_lambda_and_call(&p);         puts 31; x = lambda { puts 32; p.call; puts 33 }; puts x.call; puts 34; end 
def method_use_proc_and_call(&p);           puts 35; x = Proc.new { puts 36; p.call; puts 37 }; puts x.call; puts 38; end 

def method_use_lambda_and_yield_2;          puts 39; x = lambda { puts 40; yield; puts 41 }; call1(x); puts 42; end 

def method_yield_in_loop;                   puts 43; for i in [1, 2]; puts 44; yield; puts 45; end; puts 46; end 
def method_call_in_loop(&p);                puts 47; for i in [3, 4]; puts 48; p.call; puts 49; end; puts 50; end 

# created in-place
def test
$g = 0; begin; puts 51; iterator_via_yield { puts 52; return; puts 53}; puts 54; rescue; puts 55; puts $!.class; end
$g = 0; def m_1; puts 56; $g = 0; begin; puts 57; iterator_via_yield { puts 58; return; puts 59}; puts 60; rescue; puts 61; puts $!.class; end; puts 62; end; m_1 
$g = 0; begin; puts 63; iterator_via_call { puts 64; return; puts 65}; puts 66; rescue; puts 67; puts $!.class; end
$g = 0; def m_2; puts 68; $g = 0; begin; puts 69; iterator_via_call { puts 70; return; puts 71}; puts 72; rescue; puts 73; puts $!.class; end; puts 74; end; m_2 
$g = 0; begin; puts 75; method_call_iterator_via_yield { puts 76; return; puts 77}; puts 78; rescue; puts 79; puts $!.class; end
$g = 0; def m_3; puts 80; $g = 0; begin; puts 81; method_call_iterator_via_yield { puts 82; return; puts 83}; puts 84; rescue; puts 85; puts $!.class; end; puts 86; end; m_3 
$g = 0; begin; puts 87; method_call_iterator_via_call { puts 88; return; puts 89}; puts 90; rescue; puts 91; puts $!.class; end
$g = 0; def m_4; puts 92; $g = 0; begin; puts 93; method_call_iterator_via_call { puts 94; return; puts 95}; puts 96; rescue; puts 97; puts $!.class; end; puts 98; end; m_4 
$g = 0; begin; puts 99; method_use_lambda_and_yield { puts 100; return; puts 101}; puts 102; rescue; puts 103; puts $!.class; end
$g = 0; def m_5; puts 104; $g = 0; begin; puts 105; method_use_lambda_and_yield { puts 106; return; puts 107}; puts 108; rescue; puts 109; puts $!.class; end; puts 110; end; m_5 
$g = 0; begin; puts 111; method_use_proc_and_yield { puts 112; return; puts 113}; puts 114; rescue; puts 115; puts $!.class; end
$g = 0; def m_6; puts 116; $g = 0; begin; puts 117; method_use_proc_and_yield { puts 118; return; puts 119}; puts 120; rescue; puts 121; puts $!.class; end; puts 122; end; m_6 
$g = 0; begin; puts 123; method_use_lambda_and_call { puts 124; return; puts 125}; puts 126; rescue; puts 127; puts $!.class; end
$g = 0; def m_7; puts 128; $g = 0; begin; puts 129; method_use_lambda_and_call { puts 130; return; puts 131}; puts 132; rescue; puts 133; puts $!.class; end; puts 134; end; m_7 
$g = 0; begin; puts 135; method_use_proc_and_call { puts 136; return; puts 137}; puts 138; rescue; puts 139; puts $!.class; end
$g = 0; def m_8; puts 140; $g = 0; begin; puts 141; method_use_proc_and_call { puts 142; return; puts 143}; puts 144; rescue; puts 145; puts $!.class; end; puts 146; end; m_8 
$g = 0; begin; puts 147; method_use_lambda_and_yield_2 { puts 148; return; puts 149}; puts 150; rescue; puts 151; puts $!.class; end
$g = 0; def m_9; puts 152; $g = 0; begin; puts 153; method_use_lambda_and_yield_2 { puts 154; return; puts 155}; puts 156; rescue; puts 157; puts $!.class; end; puts 158; end; m_9 
$g = 0; begin; puts 159; method_yield_in_loop { puts 160; return; puts 161}; puts 162; rescue; puts 163; puts $!.class; end
$g = 0; def m_10; puts 164; $g = 0; begin; puts 165; method_yield_in_loop { puts 166; return; puts 167}; puts 168; rescue; puts 169; puts $!.class; end; puts 170; end; m_10 
$g = 0; begin; puts 171; method_call_in_loop { puts 172; return; puts 173}; puts 174; rescue; puts 175; puts $!.class; end
$g = 0; def m_11; puts 176; $g = 0; begin; puts 177; method_call_in_loop { puts 178; return; puts 179}; puts 180; rescue; puts 181; puts $!.class; end; puts 182; end; m_11 
end
test


# created locally or from method
def test
$g = 0; begin; p = lambda{ puts 183; return; puts 184}; puts 185; iterator_via_yield(&p); puts 186; rescue; puts 187; puts $!.class; end
$g = 0; def m_12; p = lambda{ puts 188; return; puts 189}; puts 190; iterator_via_yield(&p); puts 191; end; 
begin; puts 192; m_12; puts 193; rescue; puts 194; puts $!.class; end
$g = 0; begin; p = lambda{ puts 195; return; puts 196}; puts 197; iterator_via_call(&p); puts 198; rescue; puts 199; puts $!.class; end
$g = 0; def m_13; p = lambda{ puts 200; return; puts 201}; puts 202; iterator_via_call(&p); puts 203; end; 
begin; puts 204; m_13; puts 205; rescue; puts 206; puts $!.class; end
$g = 0; begin; p = lambda{ puts 207; return; puts 208}; puts 209; method_call_iterator_via_yield(&p); puts 210; rescue; puts 211; puts $!.class; end
$g = 0; def m_14; p = lambda{ puts 212; return; puts 213}; puts 214; method_call_iterator_via_yield(&p); puts 215; end; 
begin; puts 216; m_14; puts 217; rescue; puts 218; puts $!.class; end
$g = 0; begin; p = lambda{ puts 219; return; puts 220}; puts 221; method_call_iterator_via_call(&p); puts 222; rescue; puts 223; puts $!.class; end
$g = 0; def m_15; p = lambda{ puts 224; return; puts 225}; puts 226; method_call_iterator_via_call(&p); puts 227; end; 
begin; puts 228; m_15; puts 229; rescue; puts 230; puts $!.class; end
$g = 0; begin; p = lambda{ puts 231; return; puts 232}; puts 233; method_use_lambda_and_yield(&p); puts 234; rescue; puts 235; puts $!.class; end
$g = 0; def m_16; p = lambda{ puts 236; return; puts 237}; puts 238; method_use_lambda_and_yield(&p); puts 239; end; 
begin; puts 240; m_16; puts 241; rescue; puts 242; puts $!.class; end
$g = 0; begin; p = lambda{ puts 243; return; puts 244}; puts 245; method_use_proc_and_yield(&p); puts 246; rescue; puts 247; puts $!.class; end
$g = 0; def m_17; p = lambda{ puts 248; return; puts 249}; puts 250; method_use_proc_and_yield(&p); puts 251; end; 
begin; puts 252; m_17; puts 253; rescue; puts 254; puts $!.class; end
$g = 0; begin; p = lambda{ puts 255; return; puts 256}; puts 257; method_use_lambda_and_call(&p); puts 258; rescue; puts 259; puts $!.class; end
$g = 0; def m_18; p = lambda{ puts 260; return; puts 261}; puts 262; method_use_lambda_and_call(&p); puts 263; end; 
begin; puts 264; m_18; puts 265; rescue; puts 266; puts $!.class; end
$g = 0; begin; p = lambda{ puts 267; return; puts 268}; puts 269; method_use_proc_and_call(&p); puts 270; rescue; puts 271; puts $!.class; end
$g = 0; def m_19; p = lambda{ puts 272; return; puts 273}; puts 274; method_use_proc_and_call(&p); puts 275; end; 
begin; puts 276; m_19; puts 277; rescue; puts 278; puts $!.class; end
$g = 0; begin; p = lambda{ puts 279; return; puts 280}; puts 281; method_use_lambda_and_yield_2(&p); puts 282; rescue; puts 283; puts $!.class; end
$g = 0; def m_20; p = lambda{ puts 284; return; puts 285}; puts 286; method_use_lambda_and_yield_2(&p); puts 287; end; 
begin; puts 288; m_20; puts 289; rescue; puts 290; puts $!.class; end
$g = 0; begin; p = lambda{ puts 291; return; puts 292}; puts 293; method_yield_in_loop(&p); puts 294; rescue; puts 295; puts $!.class; end
$g = 0; def m_21; p = lambda{ puts 296; return; puts 297}; puts 298; method_yield_in_loop(&p); puts 299; end; 
begin; puts 300; m_21; puts 301; rescue; puts 302; puts $!.class; end
$g = 0; begin; p = lambda{ puts 303; return; puts 304}; puts 305; method_call_in_loop(&p); puts 306; rescue; puts 307; puts $!.class; end
$g = 0; def m_22; p = lambda{ puts 308; return; puts 309}; puts 310; method_call_in_loop(&p); puts 311; end; 
begin; puts 312; m_22; puts 313; rescue; puts 314; puts $!.class; end
end
test

def test
$g = 0; begin; p = Proc.new{ puts 315; return; puts 316}; puts 317; iterator_via_yield(&p); puts 318; rescue; puts 319; puts $!.class; end
$g = 0; def m_23; p = Proc.new{ puts 320; return; puts 321}; puts 322; iterator_via_yield(&p); puts 323; end; 
begin; puts 324; m_23; puts 325; rescue; puts 326; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 327; return; puts 328}; puts 329; iterator_via_call(&p); puts 330; rescue; puts 331; puts $!.class; end
$g = 0; def m_24; p = Proc.new{ puts 332; return; puts 333}; puts 334; iterator_via_call(&p); puts 335; end; 
begin; puts 336; m_24; puts 337; rescue; puts 338; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 339; return; puts 340}; puts 341; method_call_iterator_via_yield(&p); puts 342; rescue; puts 343; puts $!.class; end
$g = 0; def m_25; p = Proc.new{ puts 344; return; puts 345}; puts 346; method_call_iterator_via_yield(&p); puts 347; end; 
begin; puts 348; m_25; puts 349; rescue; puts 350; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 351; return; puts 352}; puts 353; method_call_iterator_via_call(&p); puts 354; rescue; puts 355; puts $!.class; end
$g = 0; def m_26; p = Proc.new{ puts 356; return; puts 357}; puts 358; method_call_iterator_via_call(&p); puts 359; end; 
begin; puts 360; m_26; puts 361; rescue; puts 362; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 363; return; puts 364}; puts 365; method_use_lambda_and_yield(&p); puts 366; rescue; puts 367; puts $!.class; end
$g = 0; def m_27; p = Proc.new{ puts 368; return; puts 369}; puts 370; method_use_lambda_and_yield(&p); puts 371; end; 
begin; puts 372; m_27; puts 373; rescue; puts 374; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 375; return; puts 376}; puts 377; method_use_proc_and_yield(&p); puts 378; rescue; puts 379; puts $!.class; end
$g = 0; def m_28; p = Proc.new{ puts 380; return; puts 381}; puts 382; method_use_proc_and_yield(&p); puts 383; end; 
begin; puts 384; m_28; puts 385; rescue; puts 386; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 387; return; puts 388}; puts 389; method_use_lambda_and_call(&p); puts 390; rescue; puts 391; puts $!.class; end
$g = 0; def m_29; p = Proc.new{ puts 392; return; puts 393}; puts 394; method_use_lambda_and_call(&p); puts 395; end; 
begin; puts 396; m_29; puts 397; rescue; puts 398; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 399; return; puts 400}; puts 401; method_use_proc_and_call(&p); puts 402; rescue; puts 403; puts $!.class; end
$g = 0; def m_30; p = Proc.new{ puts 404; return; puts 405}; puts 406; method_use_proc_and_call(&p); puts 407; end; 
begin; puts 408; m_30; puts 409; rescue; puts 410; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 411; return; puts 412}; puts 413; method_use_lambda_and_yield_2(&p); puts 414; rescue; puts 415; puts $!.class; end
$g = 0; def m_31; p = Proc.new{ puts 416; return; puts 417}; puts 418; method_use_lambda_and_yield_2(&p); puts 419; end; 
begin; puts 420; m_31; puts 421; rescue; puts 422; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 423; return; puts 424}; puts 425; method_yield_in_loop(&p); puts 426; rescue; puts 427; puts $!.class; end
$g = 0; def m_32; p = Proc.new{ puts 428; return; puts 429}; puts 430; method_yield_in_loop(&p); puts 431; end; 
begin; puts 432; m_32; puts 433; rescue; puts 434; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts 435; return; puts 436}; puts 437; method_call_in_loop(&p); puts 438; rescue; puts 439; puts $!.class; end
$g = 0; def m_33; p = Proc.new{ puts 440; return; puts 441}; puts 442; method_call_in_loop(&p); puts 443; end; 
begin; puts 444; m_33; puts 445; rescue; puts 446; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_block{ puts 447; return; puts 448}; puts 449; iterator_via_yield(&p); puts 450; rescue; puts 451; puts $!.class; end
$g = 0; def m_34; p = get_block{ puts 452; return; puts 453}; puts 454; iterator_via_yield(&p); puts 455; end; 
begin; puts 456; m_34; puts 457; rescue; puts 458; puts $!.class; end
$g = 0; begin; p = get_block{ puts 459; return; puts 460}; puts 461; iterator_via_call(&p); puts 462; rescue; puts 463; puts $!.class; end
$g = 0; def m_35; p = get_block{ puts 464; return; puts 465}; puts 466; iterator_via_call(&p); puts 467; end; 
begin; puts 468; m_35; puts 469; rescue; puts 470; puts $!.class; end
$g = 0; begin; p = get_block{ puts 471; return; puts 472}; puts 473; method_call_iterator_via_yield(&p); puts 474; rescue; puts 475; puts $!.class; end
$g = 0; def m_36; p = get_block{ puts 476; return; puts 477}; puts 478; method_call_iterator_via_yield(&p); puts 479; end; 
begin; puts 480; m_36; puts 481; rescue; puts 482; puts $!.class; end
$g = 0; begin; p = get_block{ puts 483; return; puts 484}; puts 485; method_call_iterator_via_call(&p); puts 486; rescue; puts 487; puts $!.class; end
$g = 0; def m_37; p = get_block{ puts 488; return; puts 489}; puts 490; method_call_iterator_via_call(&p); puts 491; end; 
begin; puts 492; m_37; puts 493; rescue; puts 494; puts $!.class; end
$g = 0; begin; p = get_block{ puts 495; return; puts 496}; puts 497; method_use_lambda_and_yield(&p); puts 498; rescue; puts 499; puts $!.class; end
$g = 0; def m_38; p = get_block{ puts 500; return; puts 501}; puts 502; method_use_lambda_and_yield(&p); puts 503; end; 
begin; puts 504; m_38; puts 505; rescue; puts 506; puts $!.class; end
$g = 0; begin; p = get_block{ puts 507; return; puts 508}; puts 509; method_use_proc_and_yield(&p); puts 510; rescue; puts 511; puts $!.class; end
$g = 0; def m_39; p = get_block{ puts 512; return; puts 513}; puts 514; method_use_proc_and_yield(&p); puts 515; end; 
begin; puts 516; m_39; puts 517; rescue; puts 518; puts $!.class; end
$g = 0; begin; p = get_block{ puts 519; return; puts 520}; puts 521; method_use_lambda_and_call(&p); puts 522; rescue; puts 523; puts $!.class; end
$g = 0; def m_40; p = get_block{ puts 524; return; puts 525}; puts 526; method_use_lambda_and_call(&p); puts 527; end; 
begin; puts 528; m_40; puts 529; rescue; puts 530; puts $!.class; end
$g = 0; begin; p = get_block{ puts 531; return; puts 532}; puts 533; method_use_proc_and_call(&p); puts 534; rescue; puts 535; puts $!.class; end
$g = 0; def m_41; p = get_block{ puts 536; return; puts 537}; puts 538; method_use_proc_and_call(&p); puts 539; end; 
begin; puts 540; m_41; puts 541; rescue; puts 542; puts $!.class; end
$g = 0; begin; p = get_block{ puts 543; return; puts 544}; puts 545; method_use_lambda_and_yield_2(&p); puts 546; rescue; puts 547; puts $!.class; end
$g = 0; def m_42; p = get_block{ puts 548; return; puts 549}; puts 550; method_use_lambda_and_yield_2(&p); puts 551; end; 
begin; puts 552; m_42; puts 553; rescue; puts 554; puts $!.class; end
$g = 0; begin; p = get_block{ puts 555; return; puts 556}; puts 557; method_yield_in_loop(&p); puts 558; rescue; puts 559; puts $!.class; end
$g = 0; def m_43; p = get_block{ puts 560; return; puts 561}; puts 562; method_yield_in_loop(&p); puts 563; end; 
begin; puts 564; m_43; puts 565; rescue; puts 566; puts $!.class; end
$g = 0; begin; p = get_block{ puts 567; return; puts 568}; puts 569; method_call_in_loop(&p); puts 570; rescue; puts 571; puts $!.class; end
$g = 0; def m_44; p = get_block{ puts 572; return; puts 573}; puts 574; method_call_in_loop(&p); puts 575; end; 
begin; puts 576; m_44; puts 577; rescue; puts 578; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_lambda{ puts 579; return; puts 580}; puts 581; iterator_via_yield(&p); puts 582; rescue; puts 583; puts $!.class; end
$g = 0; def m_45; p = get_lambda{ puts 584; return; puts 585}; puts 586; iterator_via_yield(&p); puts 587; end; 
begin; puts 588; m_45; puts 589; rescue; puts 590; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 591; return; puts 592}; puts 593; iterator_via_call(&p); puts 594; rescue; puts 595; puts $!.class; end
$g = 0; def m_46; p = get_lambda{ puts 596; return; puts 597}; puts 598; iterator_via_call(&p); puts 599; end; 
begin; puts 600; m_46; puts 601; rescue; puts 602; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 603; return; puts 604}; puts 605; method_call_iterator_via_yield(&p); puts 606; rescue; puts 607; puts $!.class; end
$g = 0; def m_47; p = get_lambda{ puts 608; return; puts 609}; puts 610; method_call_iterator_via_yield(&p); puts 611; end; 
begin; puts 612; m_47; puts 613; rescue; puts 614; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 615; return; puts 616}; puts 617; method_call_iterator_via_call(&p); puts 618; rescue; puts 619; puts $!.class; end
$g = 0; def m_48; p = get_lambda{ puts 620; return; puts 621}; puts 622; method_call_iterator_via_call(&p); puts 623; end; 
begin; puts 624; m_48; puts 625; rescue; puts 626; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 627; return; puts 628}; puts 629; method_use_lambda_and_yield(&p); puts 630; rescue; puts 631; puts $!.class; end
$g = 0; def m_49; p = get_lambda{ puts 632; return; puts 633}; puts 634; method_use_lambda_and_yield(&p); puts 635; end; 
begin; puts 636; m_49; puts 637; rescue; puts 638; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 639; return; puts 640}; puts 641; method_use_proc_and_yield(&p); puts 642; rescue; puts 643; puts $!.class; end
$g = 0; def m_50; p = get_lambda{ puts 644; return; puts 645}; puts 646; method_use_proc_and_yield(&p); puts 647; end; 
begin; puts 648; m_50; puts 649; rescue; puts 650; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 651; return; puts 652}; puts 653; method_use_lambda_and_call(&p); puts 654; rescue; puts 655; puts $!.class; end
$g = 0; def m_51; p = get_lambda{ puts 656; return; puts 657}; puts 658; method_use_lambda_and_call(&p); puts 659; end; 
begin; puts 660; m_51; puts 661; rescue; puts 662; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 663; return; puts 664}; puts 665; method_use_proc_and_call(&p); puts 666; rescue; puts 667; puts $!.class; end
$g = 0; def m_52; p = get_lambda{ puts 668; return; puts 669}; puts 670; method_use_proc_and_call(&p); puts 671; end; 
begin; puts 672; m_52; puts 673; rescue; puts 674; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 675; return; puts 676}; puts 677; method_use_lambda_and_yield_2(&p); puts 678; rescue; puts 679; puts $!.class; end
$g = 0; def m_53; p = get_lambda{ puts 680; return; puts 681}; puts 682; method_use_lambda_and_yield_2(&p); puts 683; end; 
begin; puts 684; m_53; puts 685; rescue; puts 686; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 687; return; puts 688}; puts 689; method_yield_in_loop(&p); puts 690; rescue; puts 691; puts $!.class; end
$g = 0; def m_54; p = get_lambda{ puts 692; return; puts 693}; puts 694; method_yield_in_loop(&p); puts 695; end; 
begin; puts 696; m_54; puts 697; rescue; puts 698; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts 699; return; puts 700}; puts 701; method_call_in_loop(&p); puts 702; rescue; puts 703; puts $!.class; end
$g = 0; def m_55; p = get_lambda{ puts 704; return; puts 705}; puts 706; method_call_in_loop(&p); puts 707; end; 
begin; puts 708; m_55; puts 709; rescue; puts 710; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_proc{ puts 711; return; puts 712}; puts 713; iterator_via_yield(&p); puts 714; rescue; puts 715; puts $!.class; end
$g = 0; def m_56; p = get_proc{ puts 716; return; puts 717}; puts 718; iterator_via_yield(&p); puts 719; end; 
begin; puts 720; m_56; puts 721; rescue; puts 722; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 723; return; puts 724}; puts 725; iterator_via_call(&p); puts 726; rescue; puts 727; puts $!.class; end
$g = 0; def m_57; p = get_proc{ puts 728; return; puts 729}; puts 730; iterator_via_call(&p); puts 731; end; 
begin; puts 732; m_57; puts 733; rescue; puts 734; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 735; return; puts 736}; puts 737; method_call_iterator_via_yield(&p); puts 738; rescue; puts 739; puts $!.class; end
$g = 0; def m_58; p = get_proc{ puts 740; return; puts 741}; puts 742; method_call_iterator_via_yield(&p); puts 743; end; 
begin; puts 744; m_58; puts 745; rescue; puts 746; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 747; return; puts 748}; puts 749; method_call_iterator_via_call(&p); puts 750; rescue; puts 751; puts $!.class; end
$g = 0; def m_59; p = get_proc{ puts 752; return; puts 753}; puts 754; method_call_iterator_via_call(&p); puts 755; end; 
begin; puts 756; m_59; puts 757; rescue; puts 758; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 759; return; puts 760}; puts 761; method_use_lambda_and_yield(&p); puts 762; rescue; puts 763; puts $!.class; end
$g = 0; def m_60; p = get_proc{ puts 764; return; puts 765}; puts 766; method_use_lambda_and_yield(&p); puts 767; end; 
begin; puts 768; m_60; puts 769; rescue; puts 770; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 771; return; puts 772}; puts 773; method_use_proc_and_yield(&p); puts 774; rescue; puts 775; puts $!.class; end
$g = 0; def m_61; p = get_proc{ puts 776; return; puts 777}; puts 778; method_use_proc_and_yield(&p); puts 779; end; 
begin; puts 780; m_61; puts 781; rescue; puts 782; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 783; return; puts 784}; puts 785; method_use_lambda_and_call(&p); puts 786; rescue; puts 787; puts $!.class; end
$g = 0; def m_62; p = get_proc{ puts 788; return; puts 789}; puts 790; method_use_lambda_and_call(&p); puts 791; end; 
begin; puts 792; m_62; puts 793; rescue; puts 794; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 795; return; puts 796}; puts 797; method_use_proc_and_call(&p); puts 798; rescue; puts 799; puts $!.class; end
$g = 0; def m_63; p = get_proc{ puts 800; return; puts 801}; puts 802; method_use_proc_and_call(&p); puts 803; end; 
begin; puts 804; m_63; puts 805; rescue; puts 806; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 807; return; puts 808}; puts 809; method_use_lambda_and_yield_2(&p); puts 810; rescue; puts 811; puts $!.class; end
$g = 0; def m_64; p = get_proc{ puts 812; return; puts 813}; puts 814; method_use_lambda_and_yield_2(&p); puts 815; end; 
begin; puts 816; m_64; puts 817; rescue; puts 818; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 819; return; puts 820}; puts 821; method_yield_in_loop(&p); puts 822; rescue; puts 823; puts $!.class; end
$g = 0; def m_65; p = get_proc{ puts 824; return; puts 825}; puts 826; method_yield_in_loop(&p); puts 827; end; 
begin; puts 828; m_65; puts 829; rescue; puts 830; puts $!.class; end
$g = 0; begin; p = get_proc{ puts 831; return; puts 832}; puts 833; method_call_in_loop(&p); puts 834; rescue; puts 835; puts $!.class; end
$g = 0; def m_66; p = get_proc{ puts 836; return; puts 837}; puts 838; method_call_in_loop(&p); puts 839; end; 
begin; puts 840; m_66; puts 841; rescue; puts 842; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_local_block; puts 843; iterator_via_yield(&p); puts 844; rescue; puts 845; puts $!.class; end
$g = 0; def m_67; p = get_local_block; puts 846; iterator_via_yield(&p); puts 847; end; 
begin; puts 848; m_67; puts 849; rescue; puts 850; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 851; iterator_via_call(&p); puts 852; rescue; puts 853; puts $!.class; end
$g = 0; def m_68; p = get_local_block; puts 854; iterator_via_call(&p); puts 855; end; 
begin; puts 856; m_68; puts 857; rescue; puts 858; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 859; method_call_iterator_via_yield(&p); puts 860; rescue; puts 861; puts $!.class; end
$g = 0; def m_69; p = get_local_block; puts 862; method_call_iterator_via_yield(&p); puts 863; end; 
begin; puts 864; m_69; puts 865; rescue; puts 866; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 867; method_call_iterator_via_call(&p); puts 868; rescue; puts 869; puts $!.class; end
$g = 0; def m_70; p = get_local_block; puts 870; method_call_iterator_via_call(&p); puts 871; end; 
begin; puts 872; m_70; puts 873; rescue; puts 874; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 875; method_use_lambda_and_yield(&p); puts 876; rescue; puts 877; puts $!.class; end
$g = 0; def m_71; p = get_local_block; puts 878; method_use_lambda_and_yield(&p); puts 879; end; 
begin; puts 880; m_71; puts 881; rescue; puts 882; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 883; method_use_proc_and_yield(&p); puts 884; rescue; puts 885; puts $!.class; end
$g = 0; def m_72; p = get_local_block; puts 886; method_use_proc_and_yield(&p); puts 887; end; 
begin; puts 888; m_72; puts 889; rescue; puts 890; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 891; method_use_lambda_and_call(&p); puts 892; rescue; puts 893; puts $!.class; end
$g = 0; def m_73; p = get_local_block; puts 894; method_use_lambda_and_call(&p); puts 895; end; 
begin; puts 896; m_73; puts 897; rescue; puts 898; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 899; method_use_proc_and_call(&p); puts 900; rescue; puts 901; puts $!.class; end
$g = 0; def m_74; p = get_local_block; puts 902; method_use_proc_and_call(&p); puts 903; end; 
begin; puts 904; m_74; puts 905; rescue; puts 906; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 907; method_use_lambda_and_yield_2(&p); puts 908; rescue; puts 909; puts $!.class; end
$g = 0; def m_75; p = get_local_block; puts 910; method_use_lambda_and_yield_2(&p); puts 911; end; 
begin; puts 912; m_75; puts 913; rescue; puts 914; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 915; method_yield_in_loop(&p); puts 916; rescue; puts 917; puts $!.class; end
$g = 0; def m_76; p = get_local_block; puts 918; method_yield_in_loop(&p); puts 919; end; 
begin; puts 920; m_76; puts 921; rescue; puts 922; puts $!.class; end
$g = 0; begin; p = get_local_block; puts 923; method_call_in_loop(&p); puts 924; rescue; puts 925; puts $!.class; end
$g = 0; def m_77; p = get_local_block; puts 926; method_call_in_loop(&p); puts 927; end; 
begin; puts 928; m_77; puts 929; rescue; puts 930; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_local_lambda; puts 931; iterator_via_yield(&p); puts 932; rescue; puts 933; puts $!.class; end
$g = 0; def m_78; p = get_local_lambda; puts 934; iterator_via_yield(&p); puts 935; end; 
begin; puts 936; m_78; puts 937; rescue; puts 938; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 939; iterator_via_call(&p); puts 940; rescue; puts 941; puts $!.class; end
$g = 0; def m_79; p = get_local_lambda; puts 942; iterator_via_call(&p); puts 943; end; 
begin; puts 944; m_79; puts 945; rescue; puts 946; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 947; method_call_iterator_via_yield(&p); puts 948; rescue; puts 949; puts $!.class; end
$g = 0; def m_80; p = get_local_lambda; puts 950; method_call_iterator_via_yield(&p); puts 951; end; 
begin; puts 952; m_80; puts 953; rescue; puts 954; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 955; method_call_iterator_via_call(&p); puts 956; rescue; puts 957; puts $!.class; end
$g = 0; def m_81; p = get_local_lambda; puts 958; method_call_iterator_via_call(&p); puts 959; end; 
begin; puts 960; m_81; puts 961; rescue; puts 962; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 963; method_use_lambda_and_yield(&p); puts 964; rescue; puts 965; puts $!.class; end
$g = 0; def m_82; p = get_local_lambda; puts 966; method_use_lambda_and_yield(&p); puts 967; end; 
begin; puts 968; m_82; puts 969; rescue; puts 970; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 971; method_use_proc_and_yield(&p); puts 972; rescue; puts 973; puts $!.class; end
$g = 0; def m_83; p = get_local_lambda; puts 974; method_use_proc_and_yield(&p); puts 975; end; 
begin; puts 976; m_83; puts 977; rescue; puts 978; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 979; method_use_lambda_and_call(&p); puts 980; rescue; puts 981; puts $!.class; end
$g = 0; def m_84; p = get_local_lambda; puts 982; method_use_lambda_and_call(&p); puts 983; end; 
begin; puts 984; m_84; puts 985; rescue; puts 986; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 987; method_use_proc_and_call(&p); puts 988; rescue; puts 989; puts $!.class; end
$g = 0; def m_85; p = get_local_lambda; puts 990; method_use_proc_and_call(&p); puts 991; end; 
begin; puts 992; m_85; puts 993; rescue; puts 994; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 995; method_use_lambda_and_yield_2(&p); puts 996; rescue; puts 997; puts $!.class; end
$g = 0; def m_86; p = get_local_lambda; puts 998; method_use_lambda_and_yield_2(&p); puts 999; end; 
begin; puts 1000; m_86; puts 1001; rescue; puts 1002; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 1003; method_yield_in_loop(&p); puts 1004; rescue; puts 1005; puts $!.class; end
$g = 0; def m_87; p = get_local_lambda; puts 1006; method_yield_in_loop(&p); puts 1007; end; 
begin; puts 1008; m_87; puts 1009; rescue; puts 1010; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts 1011; method_call_in_loop(&p); puts 1012; rescue; puts 1013; puts $!.class; end
$g = 0; def m_88; p = get_local_lambda; puts 1014; method_call_in_loop(&p); puts 1015; end; 
begin; puts 1016; m_88; puts 1017; rescue; puts 1018; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_local_proc; puts 1019; iterator_via_yield(&p); puts 1020; rescue; puts 1021; puts $!.class; end
$g = 0; def m_89; p = get_local_proc; puts 1022; iterator_via_yield(&p); puts 1023; end; 
begin; puts 1024; m_89; puts 1025; rescue; puts 1026; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1027; iterator_via_call(&p); puts 1028; rescue; puts 1029; puts $!.class; end
$g = 0; def m_90; p = get_local_proc; puts 1030; iterator_via_call(&p); puts 1031; end; 
begin; puts 1032; m_90; puts 1033; rescue; puts 1034; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1035; method_call_iterator_via_yield(&p); puts 1036; rescue; puts 1037; puts $!.class; end
$g = 0; def m_91; p = get_local_proc; puts 1038; method_call_iterator_via_yield(&p); puts 1039; end; 
begin; puts 1040; m_91; puts 1041; rescue; puts 1042; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1043; method_call_iterator_via_call(&p); puts 1044; rescue; puts 1045; puts $!.class; end
$g = 0; def m_92; p = get_local_proc; puts 1046; method_call_iterator_via_call(&p); puts 1047; end; 
begin; puts 1048; m_92; puts 1049; rescue; puts 1050; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1051; method_use_lambda_and_yield(&p); puts 1052; rescue; puts 1053; puts $!.class; end
$g = 0; def m_93; p = get_local_proc; puts 1054; method_use_lambda_and_yield(&p); puts 1055; end; 
begin; puts 1056; m_93; puts 1057; rescue; puts 1058; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1059; method_use_proc_and_yield(&p); puts 1060; rescue; puts 1061; puts $!.class; end
$g = 0; def m_94; p = get_local_proc; puts 1062; method_use_proc_and_yield(&p); puts 1063; end; 
begin; puts 1064; m_94; puts 1065; rescue; puts 1066; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1067; method_use_lambda_and_call(&p); puts 1068; rescue; puts 1069; puts $!.class; end
$g = 0; def m_95; p = get_local_proc; puts 1070; method_use_lambda_and_call(&p); puts 1071; end; 
begin; puts 1072; m_95; puts 1073; rescue; puts 1074; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1075; method_use_proc_and_call(&p); puts 1076; rescue; puts 1077; puts $!.class; end
$g = 0; def m_96; p = get_local_proc; puts 1078; method_use_proc_and_call(&p); puts 1079; end; 
begin; puts 1080; m_96; puts 1081; rescue; puts 1082; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1083; method_use_lambda_and_yield_2(&p); puts 1084; rescue; puts 1085; puts $!.class; end
$g = 0; def m_97; p = get_local_proc; puts 1086; method_use_lambda_and_yield_2(&p); puts 1087; end; 
begin; puts 1088; m_97; puts 1089; rescue; puts 1090; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1091; method_yield_in_loop(&p); puts 1092; rescue; puts 1093; puts $!.class; end
$g = 0; def m_98; p = get_local_proc; puts 1094; method_yield_in_loop(&p); puts 1095; end; 
begin; puts 1096; m_98; puts 1097; rescue; puts 1098; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts 1099; method_call_in_loop(&p); puts 1100; rescue; puts 1101; puts $!.class; end
$g = 0; def m_99; p = get_local_proc; puts 1102; method_call_in_loop(&p); puts 1103; end; 
begin; puts 1104; m_99; puts 1105; rescue; puts 1106; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1107; p = lambda{ puts 1108; return; puts 1109}; puts(p.call); puts 1110; rescue; puts 1111; puts $!.class; end
$g = 0; def m_100; puts 1112; p = lambda{ puts 1113; return; puts 1114}; puts(p.call); puts 1115; end; 
begin; puts 1116; m_100; puts 1117; rescue; puts 1118; puts $!.class; end
$g = 0; begin; puts 1119; puts m_100; puts 1120; rescue; puts 1121; puts $!.class; end
$g = 0; def m_101; puts 1122; puts m_100; puts 1123; end; 
begin; puts 1124; m_101; puts 1125; rescue; puts 1126; puts $!.class; end
$g = 0; begin; puts 1127; p = Proc.new{ puts 1128; return; puts 1129}; puts(p.call); puts 1130; rescue; puts 1131; puts $!.class; end
$g = 0; def m_102; puts 1132; p = Proc.new{ puts 1133; return; puts 1134}; puts(p.call); puts 1135; end; 
begin; puts 1136; m_102; puts 1137; rescue; puts 1138; puts $!.class; end
$g = 0; begin; puts 1139; puts m_102; puts 1140; rescue; puts 1141; puts $!.class; end
$g = 0; def m_103; puts 1142; puts m_102; puts 1143; end; 
begin; puts 1144; m_103; puts 1145; rescue; puts 1146; puts $!.class; end
$g = 0; begin; puts 1147; p = get_block{ puts 1148; return; puts 1149}; puts(p.call); puts 1150; rescue; puts 1151; puts $!.class; end
$g = 0; def m_104; puts 1152; p = get_block{ puts 1153; return; puts 1154}; puts(p.call); puts 1155; end; 
begin; puts 1156; m_104; puts 1157; rescue; puts 1158; puts $!.class; end
$g = 0; begin; puts 1159; puts m_104; puts 1160; rescue; puts 1161; puts $!.class; end
$g = 0; def m_105; puts 1162; puts m_104; puts 1163; end; 
begin; puts 1164; m_105; puts 1165; rescue; puts 1166; puts $!.class; end
$g = 0; begin; puts 1167; p = get_lambda{ puts 1168; return; puts 1169}; puts(p.call); puts 1170; rescue; puts 1171; puts $!.class; end
$g = 0; def m_106; puts 1172; p = get_lambda{ puts 1173; return; puts 1174}; puts(p.call); puts 1175; end; 
begin; puts 1176; m_106; puts 1177; rescue; puts 1178; puts $!.class; end
$g = 0; begin; puts 1179; puts m_106; puts 1180; rescue; puts 1181; puts $!.class; end
$g = 0; def m_107; puts 1182; puts m_106; puts 1183; end; 
begin; puts 1184; m_107; puts 1185; rescue; puts 1186; puts $!.class; end
$g = 0; begin; puts 1187; p = get_proc{ puts 1188; return; puts 1189}; puts(p.call); puts 1190; rescue; puts 1191; puts $!.class; end
$g = 0; def m_108; puts 1192; p = get_proc{ puts 1193; return; puts 1194}; puts(p.call); puts 1195; end; 
begin; puts 1196; m_108; puts 1197; rescue; puts 1198; puts $!.class; end
$g = 0; begin; puts 1199; puts m_108; puts 1200; rescue; puts 1201; puts $!.class; end
$g = 0; def m_109; puts 1202; puts m_108; puts 1203; end; 
begin; puts 1204; m_109; puts 1205; rescue; puts 1206; puts $!.class; end
$g = 0; begin; puts 1207; p = get_local_block; puts(p.call); puts 1208; rescue; puts 1209; puts $!.class; end
$g = 0; def m_110; puts 1210; p = get_local_block; puts(p.call); puts 1211; end; 
begin; puts 1212; m_110; puts 1213; rescue; puts 1214; puts $!.class; end
$g = 0; begin; puts 1215; puts m_110; puts 1216; rescue; puts 1217; puts $!.class; end
$g = 0; def m_111; puts 1218; puts m_110; puts 1219; end; 
begin; puts 1220; m_111; puts 1221; rescue; puts 1222; puts $!.class; end
$g = 0; begin; puts 1223; p = get_local_lambda; puts(p.call); puts 1224; rescue; puts 1225; puts $!.class; end
$g = 0; def m_112; puts 1226; p = get_local_lambda; puts(p.call); puts 1227; end; 
begin; puts 1228; m_112; puts 1229; rescue; puts 1230; puts $!.class; end
$g = 0; begin; puts 1231; puts m_112; puts 1232; rescue; puts 1233; puts $!.class; end
$g = 0; def m_113; puts 1234; puts m_112; puts 1235; end; 
begin; puts 1236; m_113; puts 1237; rescue; puts 1238; puts $!.class; end
$g = 0; begin; puts 1239; p = get_local_proc; puts(p.call); puts 1240; rescue; puts 1241; puts $!.class; end
$g = 0; def m_114; puts 1242; p = get_local_proc; puts(p.call); puts 1243; end; 
begin; puts 1244; m_114; puts 1245; rescue; puts 1246; puts $!.class; end
$g = 0; begin; puts 1247; puts m_114; puts 1248; rescue; puts 1249; puts $!.class; end
$g = 0; def m_115; puts 1250; puts m_114; puts 1251; end; 
begin; puts 1252; m_115; puts 1253; rescue; puts 1254; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1255; x = lambda { puts 1256; p = lambda{ puts 1257; return; puts 1258}; puts p.call; puts 1259}; puts x.call; puts 1260; rescue; puts 1261; puts $!.class; end
$g = 0; def m_116; puts 1262; x = lambda { puts 1263; p = lambda{ puts 1264; return; puts 1265}; puts p.call; puts 1266}; puts x.call; puts 1267; end; 
begin; puts 1268; m_116; puts 1269; rescue; puts 1270; puts $!.class; end
$g = 0; begin; puts 1271; x = lambda { puts 1272; p = Proc.new{ puts 1273; return; puts 1274}; puts p.call; puts 1275}; puts x.call; puts 1276; rescue; puts 1277; puts $!.class; end
$g = 0; def m_117; puts 1278; x = lambda { puts 1279; p = Proc.new{ puts 1280; return; puts 1281}; puts p.call; puts 1282}; puts x.call; puts 1283; end; 
begin; puts 1284; m_117; puts 1285; rescue; puts 1286; puts $!.class; end
$g = 0; begin; puts 1287; x = lambda { puts 1288; p = get_block{ puts 1289; return; puts 1290}; puts p.call; puts 1291}; puts x.call; puts 1292; rescue; puts 1293; puts $!.class; end
$g = 0; def m_118; puts 1294; x = lambda { puts 1295; p = get_block{ puts 1296; return; puts 1297}; puts p.call; puts 1298}; puts x.call; puts 1299; end; 
begin; puts 1300; m_118; puts 1301; rescue; puts 1302; puts $!.class; end
$g = 0; begin; puts 1303; x = lambda { puts 1304; p = get_lambda{ puts 1305; return; puts 1306}; puts p.call; puts 1307}; puts x.call; puts 1308; rescue; puts 1309; puts $!.class; end
$g = 0; def m_119; puts 1310; x = lambda { puts 1311; p = get_lambda{ puts 1312; return; puts 1313}; puts p.call; puts 1314}; puts x.call; puts 1315; end; 
begin; puts 1316; m_119; puts 1317; rescue; puts 1318; puts $!.class; end
$g = 0; begin; puts 1319; x = lambda { puts 1320; p = get_proc{ puts 1321; return; puts 1322}; puts p.call; puts 1323}; puts x.call; puts 1324; rescue; puts 1325; puts $!.class; end
$g = 0; def m_120; puts 1326; x = lambda { puts 1327; p = get_proc{ puts 1328; return; puts 1329}; puts p.call; puts 1330}; puts x.call; puts 1331; end; 
begin; puts 1332; m_120; puts 1333; rescue; puts 1334; puts $!.class; end
$g = 0; begin; puts 1335; x = lambda { puts 1336; p = get_local_block; puts p.call; puts 1337}; puts x.call; puts 1338; rescue; puts 1339; puts $!.class; end
$g = 0; def m_121; puts 1340; x = lambda { puts 1341; p = get_local_block; puts p.call; puts 1342}; puts x.call; puts 1343; end; 
begin; puts 1344; m_121; puts 1345; rescue; puts 1346; puts $!.class; end
$g = 0; begin; puts 1347; x = lambda { puts 1348; p = get_local_lambda; puts p.call; puts 1349}; puts x.call; puts 1350; rescue; puts 1351; puts $!.class; end
$g = 0; def m_122; puts 1352; x = lambda { puts 1353; p = get_local_lambda; puts p.call; puts 1354}; puts x.call; puts 1355; end; 
begin; puts 1356; m_122; puts 1357; rescue; puts 1358; puts $!.class; end
$g = 0; begin; puts 1359; x = lambda { puts 1360; p = get_local_proc; puts p.call; puts 1361}; puts x.call; puts 1362; rescue; puts 1363; puts $!.class; end
$g = 0; def m_123; puts 1364; x = lambda { puts 1365; p = get_local_proc; puts p.call; puts 1366}; puts x.call; puts 1367; end; 
begin; puts 1368; m_123; puts 1369; rescue; puts 1370; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1371; x = Proc.new { puts 1372; p = lambda{ puts 1373; return; puts 1374}; puts p.call; puts 1375}; puts x.call; puts 1376; rescue; puts 1377; puts $!.class; end
$g = 0; def m_124; puts 1378; x = Proc.new { puts 1379; p = lambda{ puts 1380; return; puts 1381}; puts p.call; puts 1382}; puts x.call; puts 1383; end; 
begin; puts 1384; m_124; puts 1385; rescue; puts 1386; puts $!.class; end
$g = 0; begin; puts 1387; x = Proc.new { puts 1388; p = Proc.new{ puts 1389; return; puts 1390}; puts p.call; puts 1391}; puts x.call; puts 1392; rescue; puts 1393; puts $!.class; end
$g = 0; def m_125; puts 1394; x = Proc.new { puts 1395; p = Proc.new{ puts 1396; return; puts 1397}; puts p.call; puts 1398}; puts x.call; puts 1399; end; 
begin; puts 1400; m_125; puts 1401; rescue; puts 1402; puts $!.class; end
$g = 0; begin; puts 1403; x = Proc.new { puts 1404; p = get_block{ puts 1405; return; puts 1406}; puts p.call; puts 1407}; puts x.call; puts 1408; rescue; puts 1409; puts $!.class; end
$g = 0; def m_126; puts 1410; x = Proc.new { puts 1411; p = get_block{ puts 1412; return; puts 1413}; puts p.call; puts 1414}; puts x.call; puts 1415; end; 
begin; puts 1416; m_126; puts 1417; rescue; puts 1418; puts $!.class; end
$g = 0; begin; puts 1419; x = Proc.new { puts 1420; p = get_lambda{ puts 1421; return; puts 1422}; puts p.call; puts 1423}; puts x.call; puts 1424; rescue; puts 1425; puts $!.class; end
$g = 0; def m_127; puts 1426; x = Proc.new { puts 1427; p = get_lambda{ puts 1428; return; puts 1429}; puts p.call; puts 1430}; puts x.call; puts 1431; end; 
begin; puts 1432; m_127; puts 1433; rescue; puts 1434; puts $!.class; end
$g = 0; begin; puts 1435; x = Proc.new { puts 1436; p = get_proc{ puts 1437; return; puts 1438}; puts p.call; puts 1439}; puts x.call; puts 1440; rescue; puts 1441; puts $!.class; end
$g = 0; def m_128; puts 1442; x = Proc.new { puts 1443; p = get_proc{ puts 1444; return; puts 1445}; puts p.call; puts 1446}; puts x.call; puts 1447; end; 
begin; puts 1448; m_128; puts 1449; rescue; puts 1450; puts $!.class; end
$g = 0; begin; puts 1451; x = Proc.new { puts 1452; p = get_local_block; puts p.call; puts 1453}; puts x.call; puts 1454; rescue; puts 1455; puts $!.class; end
$g = 0; def m_129; puts 1456; x = Proc.new { puts 1457; p = get_local_block; puts p.call; puts 1458}; puts x.call; puts 1459; end; 
begin; puts 1460; m_129; puts 1461; rescue; puts 1462; puts $!.class; end
$g = 0; begin; puts 1463; x = Proc.new { puts 1464; p = get_local_lambda; puts p.call; puts 1465}; puts x.call; puts 1466; rescue; puts 1467; puts $!.class; end
$g = 0; def m_130; puts 1468; x = Proc.new { puts 1469; p = get_local_lambda; puts p.call; puts 1470}; puts x.call; puts 1471; end; 
begin; puts 1472; m_130; puts 1473; rescue; puts 1474; puts $!.class; end
$g = 0; begin; puts 1475; x = Proc.new { puts 1476; p = get_local_proc; puts p.call; puts 1477}; puts x.call; puts 1478; rescue; puts 1479; puts $!.class; end
$g = 0; def m_131; puts 1480; x = Proc.new { puts 1481; p = get_local_proc; puts p.call; puts 1482}; puts x.call; puts 1483; end; 
begin; puts 1484; m_131; puts 1485; rescue; puts 1486; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1487; x = get_block { puts 1488; p = lambda{ puts 1489; return; puts 1490}; puts p.call; puts 1491}; puts x.call; puts 1492; rescue; puts 1493; puts $!.class; end
$g = 0; def m_132; puts 1494; x = get_block { puts 1495; p = lambda{ puts 1496; return; puts 1497}; puts p.call; puts 1498}; puts x.call; puts 1499; end; 
begin; puts 1500; m_132; puts 1501; rescue; puts 1502; puts $!.class; end
$g = 0; begin; puts 1503; x = get_block { puts 1504; p = Proc.new{ puts 1505; return; puts 1506}; puts p.call; puts 1507}; puts x.call; puts 1508; rescue; puts 1509; puts $!.class; end
$g = 0; def m_133; puts 1510; x = get_block { puts 1511; p = Proc.new{ puts 1512; return; puts 1513}; puts p.call; puts 1514}; puts x.call; puts 1515; end; 
begin; puts 1516; m_133; puts 1517; rescue; puts 1518; puts $!.class; end
$g = 0; begin; puts 1519; x = get_block { puts 1520; p = get_block{ puts 1521; return; puts 1522}; puts p.call; puts 1523}; puts x.call; puts 1524; rescue; puts 1525; puts $!.class; end
$g = 0; def m_134; puts 1526; x = get_block { puts 1527; p = get_block{ puts 1528; return; puts 1529}; puts p.call; puts 1530}; puts x.call; puts 1531; end; 
begin; puts 1532; m_134; puts 1533; rescue; puts 1534; puts $!.class; end
$g = 0; begin; puts 1535; x = get_block { puts 1536; p = get_lambda{ puts 1537; return; puts 1538}; puts p.call; puts 1539}; puts x.call; puts 1540; rescue; puts 1541; puts $!.class; end
$g = 0; def m_135; puts 1542; x = get_block { puts 1543; p = get_lambda{ puts 1544; return; puts 1545}; puts p.call; puts 1546}; puts x.call; puts 1547; end; 
begin; puts 1548; m_135; puts 1549; rescue; puts 1550; puts $!.class; end
$g = 0; begin; puts 1551; x = get_block { puts 1552; p = get_proc{ puts 1553; return; puts 1554}; puts p.call; puts 1555}; puts x.call; puts 1556; rescue; puts 1557; puts $!.class; end
$g = 0; def m_136; puts 1558; x = get_block { puts 1559; p = get_proc{ puts 1560; return; puts 1561}; puts p.call; puts 1562}; puts x.call; puts 1563; end; 
begin; puts 1564; m_136; puts 1565; rescue; puts 1566; puts $!.class; end
$g = 0; begin; puts 1567; x = get_block { puts 1568; p = get_local_block; puts p.call; puts 1569}; puts x.call; puts 1570; rescue; puts 1571; puts $!.class; end
$g = 0; def m_137; puts 1572; x = get_block { puts 1573; p = get_local_block; puts p.call; puts 1574}; puts x.call; puts 1575; end; 
begin; puts 1576; m_137; puts 1577; rescue; puts 1578; puts $!.class; end
$g = 0; begin; puts 1579; x = get_block { puts 1580; p = get_local_lambda; puts p.call; puts 1581}; puts x.call; puts 1582; rescue; puts 1583; puts $!.class; end
$g = 0; def m_138; puts 1584; x = get_block { puts 1585; p = get_local_lambda; puts p.call; puts 1586}; puts x.call; puts 1587; end; 
begin; puts 1588; m_138; puts 1589; rescue; puts 1590; puts $!.class; end
$g = 0; begin; puts 1591; x = get_block { puts 1592; p = get_local_proc; puts p.call; puts 1593}; puts x.call; puts 1594; rescue; puts 1595; puts $!.class; end
$g = 0; def m_139; puts 1596; x = get_block { puts 1597; p = get_local_proc; puts p.call; puts 1598}; puts x.call; puts 1599; end; 
begin; puts 1600; m_139; puts 1601; rescue; puts 1602; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1603; x = get_lambda { puts 1604; p = lambda{ puts 1605; return; puts 1606}; puts p.call; puts 1607}; puts x.call; puts 1608; rescue; puts 1609; puts $!.class; end
$g = 0; def m_140; puts 1610; x = get_lambda { puts 1611; p = lambda{ puts 1612; return; puts 1613}; puts p.call; puts 1614}; puts x.call; puts 1615; end; 
begin; puts 1616; m_140; puts 1617; rescue; puts 1618; puts $!.class; end
$g = 0; begin; puts 1619; x = get_lambda { puts 1620; p = Proc.new{ puts 1621; return; puts 1622}; puts p.call; puts 1623}; puts x.call; puts 1624; rescue; puts 1625; puts $!.class; end
$g = 0; def m_141; puts 1626; x = get_lambda { puts 1627; p = Proc.new{ puts 1628; return; puts 1629}; puts p.call; puts 1630}; puts x.call; puts 1631; end; 
begin; puts 1632; m_141; puts 1633; rescue; puts 1634; puts $!.class; end
$g = 0; begin; puts 1635; x = get_lambda { puts 1636; p = get_block{ puts 1637; return; puts 1638}; puts p.call; puts 1639}; puts x.call; puts 1640; rescue; puts 1641; puts $!.class; end
$g = 0; def m_142; puts 1642; x = get_lambda { puts 1643; p = get_block{ puts 1644; return; puts 1645}; puts p.call; puts 1646}; puts x.call; puts 1647; end; 
begin; puts 1648; m_142; puts 1649; rescue; puts 1650; puts $!.class; end
$g = 0; begin; puts 1651; x = get_lambda { puts 1652; p = get_lambda{ puts 1653; return; puts 1654}; puts p.call; puts 1655}; puts x.call; puts 1656; rescue; puts 1657; puts $!.class; end
$g = 0; def m_143; puts 1658; x = get_lambda { puts 1659; p = get_lambda{ puts 1660; return; puts 1661}; puts p.call; puts 1662}; puts x.call; puts 1663; end; 
begin; puts 1664; m_143; puts 1665; rescue; puts 1666; puts $!.class; end
$g = 0; begin; puts 1667; x = get_lambda { puts 1668; p = get_proc{ puts 1669; return; puts 1670}; puts p.call; puts 1671}; puts x.call; puts 1672; rescue; puts 1673; puts $!.class; end
$g = 0; def m_144; puts 1674; x = get_lambda { puts 1675; p = get_proc{ puts 1676; return; puts 1677}; puts p.call; puts 1678}; puts x.call; puts 1679; end; 
begin; puts 1680; m_144; puts 1681; rescue; puts 1682; puts $!.class; end
$g = 0; begin; puts 1683; x = get_lambda { puts 1684; p = get_local_block; puts p.call; puts 1685}; puts x.call; puts 1686; rescue; puts 1687; puts $!.class; end
$g = 0; def m_145; puts 1688; x = get_lambda { puts 1689; p = get_local_block; puts p.call; puts 1690}; puts x.call; puts 1691; end; 
begin; puts 1692; m_145; puts 1693; rescue; puts 1694; puts $!.class; end
$g = 0; begin; puts 1695; x = get_lambda { puts 1696; p = get_local_lambda; puts p.call; puts 1697}; puts x.call; puts 1698; rescue; puts 1699; puts $!.class; end
$g = 0; def m_146; puts 1700; x = get_lambda { puts 1701; p = get_local_lambda; puts p.call; puts 1702}; puts x.call; puts 1703; end; 
begin; puts 1704; m_146; puts 1705; rescue; puts 1706; puts $!.class; end
$g = 0; begin; puts 1707; x = get_lambda { puts 1708; p = get_local_proc; puts p.call; puts 1709}; puts x.call; puts 1710; rescue; puts 1711; puts $!.class; end
$g = 0; def m_147; puts 1712; x = get_lambda { puts 1713; p = get_local_proc; puts p.call; puts 1714}; puts x.call; puts 1715; end; 
begin; puts 1716; m_147; puts 1717; rescue; puts 1718; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1719; x = get_proc { puts 1720; p = lambda{ puts 1721; return; puts 1722}; puts p.call; puts 1723}; puts x.call; puts 1724; rescue; puts 1725; puts $!.class; end
$g = 0; def m_148; puts 1726; x = get_proc { puts 1727; p = lambda{ puts 1728; return; puts 1729}; puts p.call; puts 1730}; puts x.call; puts 1731; end; 
begin; puts 1732; m_148; puts 1733; rescue; puts 1734; puts $!.class; end
$g = 0; begin; puts 1735; x = get_proc { puts 1736; p = Proc.new{ puts 1737; return; puts 1738}; puts p.call; puts 1739}; puts x.call; puts 1740; rescue; puts 1741; puts $!.class; end
$g = 0; def m_149; puts 1742; x = get_proc { puts 1743; p = Proc.new{ puts 1744; return; puts 1745}; puts p.call; puts 1746}; puts x.call; puts 1747; end; 
begin; puts 1748; m_149; puts 1749; rescue; puts 1750; puts $!.class; end
$g = 0; begin; puts 1751; x = get_proc { puts 1752; p = get_block{ puts 1753; return; puts 1754}; puts p.call; puts 1755}; puts x.call; puts 1756; rescue; puts 1757; puts $!.class; end
$g = 0; def m_150; puts 1758; x = get_proc { puts 1759; p = get_block{ puts 1760; return; puts 1761}; puts p.call; puts 1762}; puts x.call; puts 1763; end; 
begin; puts 1764; m_150; puts 1765; rescue; puts 1766; puts $!.class; end
$g = 0; begin; puts 1767; x = get_proc { puts 1768; p = get_lambda{ puts 1769; return; puts 1770}; puts p.call; puts 1771}; puts x.call; puts 1772; rescue; puts 1773; puts $!.class; end
$g = 0; def m_151; puts 1774; x = get_proc { puts 1775; p = get_lambda{ puts 1776; return; puts 1777}; puts p.call; puts 1778}; puts x.call; puts 1779; end; 
begin; puts 1780; m_151; puts 1781; rescue; puts 1782; puts $!.class; end
$g = 0; begin; puts 1783; x = get_proc { puts 1784; p = get_proc{ puts 1785; return; puts 1786}; puts p.call; puts 1787}; puts x.call; puts 1788; rescue; puts 1789; puts $!.class; end
$g = 0; def m_152; puts 1790; x = get_proc { puts 1791; p = get_proc{ puts 1792; return; puts 1793}; puts p.call; puts 1794}; puts x.call; puts 1795; end; 
begin; puts 1796; m_152; puts 1797; rescue; puts 1798; puts $!.class; end
$g = 0; begin; puts 1799; x = get_proc { puts 1800; p = get_local_block; puts p.call; puts 1801}; puts x.call; puts 1802; rescue; puts 1803; puts $!.class; end
$g = 0; def m_153; puts 1804; x = get_proc { puts 1805; p = get_local_block; puts p.call; puts 1806}; puts x.call; puts 1807; end; 
begin; puts 1808; m_153; puts 1809; rescue; puts 1810; puts $!.class; end
$g = 0; begin; puts 1811; x = get_proc { puts 1812; p = get_local_lambda; puts p.call; puts 1813}; puts x.call; puts 1814; rescue; puts 1815; puts $!.class; end
$g = 0; def m_154; puts 1816; x = get_proc { puts 1817; p = get_local_lambda; puts p.call; puts 1818}; puts x.call; puts 1819; end; 
begin; puts 1820; m_154; puts 1821; rescue; puts 1822; puts $!.class; end
$g = 0; begin; puts 1823; x = get_proc { puts 1824; p = get_local_proc; puts p.call; puts 1825}; puts x.call; puts 1826; rescue; puts 1827; puts $!.class; end
$g = 0; def m_155; puts 1828; x = get_proc { puts 1829; p = get_local_proc; puts p.call; puts 1830}; puts x.call; puts 1831; end; 
begin; puts 1832; m_155; puts 1833; rescue; puts 1834; puts $!.class; end
end
test

def test
$g = 0; begin; puts 1835; for i in [1, 2]; puts 1836; p = lambda{ puts 1837; return; puts 1838}; puts p.call; puts 1839; end; puts 1840; rescue; puts 1841; puts $!.class; end
$g = 0; def m_156; puts 1842; for i in [1, 2]; puts 1843; p = lambda{ puts 1844; return; puts 1845}; puts p.call; puts 1846; end; puts 1847; end;
begin; puts 1848; m_156; puts 1849; rescue; puts 1850; puts $!.class; end
$g = 0; begin; puts 1851; for i in [1, 2]; puts 1852; p = Proc.new{ puts 1853; return; puts 1854}; puts p.call; puts 1855; end; puts 1856; rescue; puts 1857; puts $!.class; end
$g = 0; def m_157; puts 1858; for i in [1, 2]; puts 1859; p = Proc.new{ puts 1860; return; puts 1861}; puts p.call; puts 1862; end; puts 1863; end;
begin; puts 1864; m_157; puts 1865; rescue; puts 1866; puts $!.class; end
$g = 0; begin; puts 1867; for i in [1, 2]; puts 1868; p = get_block{ puts 1869; return; puts 1870}; puts p.call; puts 1871; end; puts 1872; rescue; puts 1873; puts $!.class; end
$g = 0; def m_158; puts 1874; for i in [1, 2]; puts 1875; p = get_block{ puts 1876; return; puts 1877}; puts p.call; puts 1878; end; puts 1879; end;
begin; puts 1880; m_158; puts 1881; rescue; puts 1882; puts $!.class; end
$g = 0; begin; puts 1883; for i in [1, 2]; puts 1884; p = get_lambda{ puts 1885; return; puts 1886}; puts p.call; puts 1887; end; puts 1888; rescue; puts 1889; puts $!.class; end
$g = 0; def m_159; puts 1890; for i in [1, 2]; puts 1891; p = get_lambda{ puts 1892; return; puts 1893}; puts p.call; puts 1894; end; puts 1895; end;
begin; puts 1896; m_159; puts 1897; rescue; puts 1898; puts $!.class; end
$g = 0; begin; puts 1899; for i in [1, 2]; puts 1900; p = get_proc{ puts 1901; return; puts 1902}; puts p.call; puts 1903; end; puts 1904; rescue; puts 1905; puts $!.class; end
$g = 0; def m_160; puts 1906; for i in [1, 2]; puts 1907; p = get_proc{ puts 1908; return; puts 1909}; puts p.call; puts 1910; end; puts 1911; end;
begin; puts 1912; m_160; puts 1913; rescue; puts 1914; puts $!.class; end
$g = 0; begin; puts 1915; for i in [1, 2]; puts 1916; p = get_local_block; puts p.call; puts 1917; end; puts 1918; rescue; puts 1919; puts $!.class; end
$g = 0; def m_161; puts 1920; for i in [1, 2]; puts 1921; p = get_local_block; puts p.call; puts 1922; end; puts 1923; end;
begin; puts 1924; m_161; puts 1925; rescue; puts 1926; puts $!.class; end
$g = 0; begin; puts 1927; for i in [1, 2]; puts 1928; p = get_local_lambda; puts p.call; puts 1929; end; puts 1930; rescue; puts 1931; puts $!.class; end
$g = 0; def m_162; puts 1932; for i in [1, 2]; puts 1933; p = get_local_lambda; puts p.call; puts 1934; end; puts 1935; end;
begin; puts 1936; m_162; puts 1937; rescue; puts 1938; puts $!.class; end
$g = 0; begin; puts 1939; for i in [1, 2]; puts 1940; p = get_local_proc; puts p.call; puts 1941; end; puts 1942; rescue; puts 1943; puts $!.class; end
$g = 0; def m_163; puts 1944; for i in [1, 2]; puts 1945; p = get_local_proc; puts p.call; puts 1946; end; puts 1947; end;
begin; puts 1948; m_163; puts 1949; rescue; puts 1950; puts $!.class; end
end
test
