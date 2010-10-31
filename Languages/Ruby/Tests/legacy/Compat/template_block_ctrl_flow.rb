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
def myeval(line); puts B; eval(line); puts B; end
def call1(x); puts B; call2(x); puts B; end
def call2(x); puts B; call3(x); puts B; end 
def call3(x); puts B; puts x.call; puts B; end

# producer

def get_block(&p);    p;                end 
def get_lambda(&p);   lambda(&p);       end 
def get_proc(&p);     Proc.new(&p);     end 

def get_local_block;        get_block { puts B; ctrl_flow; puts B };    end 
def get_local_lambda;       lambda { puts B; ctrl_flow; puts B };       end 
def get_local_proc;         Proc.new { puts B; ctrl_flow; puts B };     end 

# consumer 

# taking arguments
def iterator_via_yield;                     puts B; x = yield; puts x; puts B;     end 
def iterator_via_call(&p);                  puts B; puts(p.call); puts B;   end 

def method_call_iterator_via_yield(&p);     puts B; iterator_via_yield(&p); puts B;     end
def method_call_iterator_via_call(&p);      puts B; iterator_via_call(&p); puts B;      end 

def method_use_lambda_and_yield;            puts B; x = lambda { puts B; yield; puts B }; puts x.call; puts B; end 
def method_use_proc_and_yield;              puts B; x = Proc.new { puts B; yield; puts B }; puts x.call; puts B; end 
def method_use_lambda_and_call(&p);         puts B; x = lambda { puts B; p.call; puts B }; puts x.call; puts B; end 
def method_use_proc_and_call(&p);           puts B; x = Proc.new { puts B; p.call; puts B }; puts x.call; puts B; end 

def method_use_lambda_and_yield_2;          puts B; x = lambda { puts B; yield; puts B }; call1(x); puts B; end 

def method_yield_in_loop;                   puts B; for i in [1, 2]; puts B; yield; puts B; end; puts B; end 
def method_call_in_loop(&p);                puts B; for i in [3, 4]; puts B; p.call; puts B; end; puts B; end 

# created in-place
def test
$g = 0; begin; puts B; iterator_via_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_1; puts B; $g = 0; begin; puts B; iterator_via_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_1 
$g = 0; begin; puts B; iterator_via_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_2; puts B; $g = 0; begin; puts B; iterator_via_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_2 
$g = 0; begin; puts B; method_call_iterator_via_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_3; puts B; $g = 0; begin; puts B; method_call_iterator_via_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_3 
$g = 0; begin; puts B; method_call_iterator_via_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_4; puts B; $g = 0; begin; puts B; method_call_iterator_via_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_4 
$g = 0; begin; puts B; method_use_lambda_and_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_5; puts B; $g = 0; begin; puts B; method_use_lambda_and_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_5 
$g = 0; begin; puts B; method_use_proc_and_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_6; puts B; $g = 0; begin; puts B; method_use_proc_and_yield { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_6 
$g = 0; begin; puts B; method_use_lambda_and_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_7; puts B; $g = 0; begin; puts B; method_use_lambda_and_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_7 
$g = 0; begin; puts B; method_use_proc_and_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_8; puts B; $g = 0; begin; puts B; method_use_proc_and_call { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_8 
$g = 0; begin; puts B; method_use_lambda_and_yield_2 { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_9; puts B; $g = 0; begin; puts B; method_use_lambda_and_yield_2 { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_9 
$g = 0; begin; puts B; method_yield_in_loop { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_10; puts B; $g = 0; begin; puts B; method_yield_in_loop { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_10 
$g = 0; begin; puts B; method_call_in_loop { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_11; puts B; $g = 0; begin; puts B; method_call_in_loop { puts B; ctrl_flow; puts B}; puts B; rescue; puts B; puts $!.class; end; puts B; end; m_11 
end
test


# created locally or from method
def test
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_12; p = lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_12; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_13; p = lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_13; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_14; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_14; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_15; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_15; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_16; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_16; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_17; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_17; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_18; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_18; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_19; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_19; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_20; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_20; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_21; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_21; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_22; p = lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_22; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_23; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_23; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_24; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_24; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_25; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_25; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_26; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_26; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_27; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_27; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_28; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_28; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_29; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_29; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_30; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_30; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_31; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_31; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_32; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_32; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_33; p = Proc.new{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_33; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_34; p = get_block{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_34; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_35; p = get_block{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_35; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_36; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_36; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_37; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_37; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_38; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_38; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_39; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_39; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_40; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_40; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_41; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_41; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_42; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_42; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_43; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_43; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_44; p = get_block{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_44; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_45; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_45; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_46; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_46; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_47; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_47; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_48; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_48; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_49; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_49; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_50; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_50; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_51; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_51; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_52; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_52; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_53; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_53; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_54; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_54; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_55; p = get_lambda{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_55; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_56; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_56; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_57; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_57; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_58; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_58; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_59; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_59; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_60; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_60; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_61; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_61; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_62; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_62; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_63; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_63; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_64; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_64; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_65; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_65; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_66; p = get_proc{ puts B; ctrl_flow; puts B}; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_66; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_local_block; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_67; p = get_local_block; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_67; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_68; p = get_local_block; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_68; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_69; p = get_local_block; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_69; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_70; p = get_local_block; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_70; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_71; p = get_local_block; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_71; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_72; p = get_local_block; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_72; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_73; p = get_local_block; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_73; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_74; p = get_local_block; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_74; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_75; p = get_local_block; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_75; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_76; p = get_local_block; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_76; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_block; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_77; p = get_local_block; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_77; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_local_lambda; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_78; p = get_local_lambda; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_78; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_79; p = get_local_lambda; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_79; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_80; p = get_local_lambda; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_80; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_81; p = get_local_lambda; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_81; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_82; p = get_local_lambda; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_82; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_83; p = get_local_lambda; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_83; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_84; p = get_local_lambda; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_84; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_85; p = get_local_lambda; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_85; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_86; p = get_local_lambda; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_86; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_87; p = get_local_lambda; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_87; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_lambda; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_88; p = get_local_lambda; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_88; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; p = get_local_proc; puts B; iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_89; p = get_local_proc; puts B; iterator_via_yield(&p); puts B; end; 
begin; puts B; m_89; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_90; p = get_local_proc; puts B; iterator_via_call(&p); puts B; end; 
begin; puts B; m_90; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_call_iterator_via_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_91; p = get_local_proc; puts B; method_call_iterator_via_yield(&p); puts B; end; 
begin; puts B; m_91; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_call_iterator_via_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_92; p = get_local_proc; puts B; method_call_iterator_via_call(&p); puts B; end; 
begin; puts B; m_92; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_use_lambda_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_93; p = get_local_proc; puts B; method_use_lambda_and_yield(&p); puts B; end; 
begin; puts B; m_93; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_use_proc_and_yield(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_94; p = get_local_proc; puts B; method_use_proc_and_yield(&p); puts B; end; 
begin; puts B; m_94; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_use_lambda_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_95; p = get_local_proc; puts B; method_use_lambda_and_call(&p); puts B; end; 
begin; puts B; m_95; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_use_proc_and_call(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_96; p = get_local_proc; puts B; method_use_proc_and_call(&p); puts B; end; 
begin; puts B; m_96; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_use_lambda_and_yield_2(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_97; p = get_local_proc; puts B; method_use_lambda_and_yield_2(&p); puts B; end; 
begin; puts B; m_97; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_yield_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_98; p = get_local_proc; puts B; method_yield_in_loop(&p); puts B; end; 
begin; puts B; m_98; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; p = get_local_proc; puts B; method_call_in_loop(&p); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_99; p = get_local_proc; puts B; method_call_in_loop(&p); puts B; end; 
begin; puts B; m_99; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_100; puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; end; 
begin; puts B; m_100; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_100; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_101; puts B; puts m_100; puts B; end; 
begin; puts B; m_101; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_102; puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; end; 
begin; puts B; m_102; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_102; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_103; puts B; puts m_102; puts B; end; 
begin; puts B; m_103; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_104; puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; end; 
begin; puts B; m_104; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_104; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_105; puts B; puts m_104; puts B; end; 
begin; puts B; m_105; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_106; puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; end; 
begin; puts B; m_106; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_106; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_107; puts B; puts m_106; puts B; end; 
begin; puts B; m_107; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_108; puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts(p.call); puts B; end; 
begin; puts B; m_108; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_108; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_109; puts B; puts m_108; puts B; end; 
begin; puts B; m_109; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = get_local_block; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_110; puts B; p = get_local_block; puts(p.call); puts B; end; 
begin; puts B; m_110; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_110; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_111; puts B; puts m_110; puts B; end; 
begin; puts B; m_111; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = get_local_lambda; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_112; puts B; p = get_local_lambda; puts(p.call); puts B; end; 
begin; puts B; m_112; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_112; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_113; puts B; puts m_112; puts B; end; 
begin; puts B; m_113; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; p = get_local_proc; puts(p.call); puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_114; puts B; p = get_local_proc; puts(p.call); puts B; end; 
begin; puts B; m_114; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; puts m_114; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_115; puts B; puts m_114; puts B; end; 
begin; puts B; m_115; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; x = lambda { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_116; puts B; x = lambda { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_116; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_117; puts B; x = lambda { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_117; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_118; puts B; x = lambda { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_118; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_119; puts B; x = lambda { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_119; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_120; puts B; x = lambda { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_120; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_121; puts B; x = lambda { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_121; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_122; puts B; x = lambda { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_122; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = lambda { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_123; puts B; x = lambda { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_123; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; x = Proc.new { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_124; puts B; x = Proc.new { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_124; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_125; puts B; x = Proc.new { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_125; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_126; puts B; x = Proc.new { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_126; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_127; puts B; x = Proc.new { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_127; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_128; puts B; x = Proc.new { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_128; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_129; puts B; x = Proc.new { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_129; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_130; puts B; x = Proc.new { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_130; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = Proc.new { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_131; puts B; x = Proc.new { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_131; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; x = get_block { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_132; puts B; x = get_block { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_132; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_133; puts B; x = get_block { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_133; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_134; puts B; x = get_block { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_134; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_135; puts B; x = get_block { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_135; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_136; puts B; x = get_block { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_136; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_137; puts B; x = get_block { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_137; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_138; puts B; x = get_block { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_138; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_block { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_139; puts B; x = get_block { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_139; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; x = get_lambda { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_140; puts B; x = get_lambda { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_140; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_141; puts B; x = get_lambda { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_141; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_142; puts B; x = get_lambda { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_142; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_143; puts B; x = get_lambda { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_143; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_144; puts B; x = get_lambda { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_144; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_145; puts B; x = get_lambda { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_145; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_146; puts B; x = get_lambda { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_146; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_lambda { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_147; puts B; x = get_lambda { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_147; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; x = get_proc { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_148; puts B; x = get_proc { puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_148; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_149; puts B; x = get_proc { puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_149; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_150; puts B; x = get_proc { puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_150; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_151; puts B; x = get_proc { puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_151; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_152; puts B; x = get_proc { puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_152; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_153; puts B; x = get_proc { puts B; p = get_local_block; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_153; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_154; puts B; x = get_proc { puts B; p = get_local_lambda; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_154; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; x = get_proc { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_155; puts B; x = get_proc { puts B; p = get_local_proc; puts p.call; puts B}; puts x.call; puts B; end; 
begin; puts B; m_155; puts B; rescue; puts B; puts $!.class; end
end
test

def test
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_156; puts B; for i in [1, 2]; puts B; p = lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; end;
begin; puts B; m_156; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_157; puts B; for i in [1, 2]; puts B; p = Proc.new{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; end;
begin; puts B; m_157; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_158; puts B; for i in [1, 2]; puts B; p = get_block{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; end;
begin; puts B; m_158; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_159; puts B; for i in [1, 2]; puts B; p = get_lambda{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; end;
begin; puts B; m_159; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_160; puts B; for i in [1, 2]; puts B; p = get_proc{ puts B; ctrl_flow; puts B}; puts p.call; puts B; end; puts B; end;
begin; puts B; m_160; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = get_local_block; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_161; puts B; for i in [1, 2]; puts B; p = get_local_block; puts p.call; puts B; end; puts B; end;
begin; puts B; m_161; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = get_local_lambda; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_162; puts B; for i in [1, 2]; puts B; p = get_local_lambda; puts p.call; puts B; end; puts B; end;
begin; puts B; m_162; puts B; rescue; puts B; puts $!.class; end
$g = 0; begin; puts B; for i in [1, 2]; puts B; p = get_local_proc; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end
$g = 0; def m_163; puts B; for i in [1, 2]; puts B; p = get_local_proc; puts p.call; puts B; end; puts B; end;
begin; puts B; m_163; puts B; rescue; puts B; puts $!.class; end
end
test
