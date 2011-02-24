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

# at 
#         top level code
#       | method call
#       | eval call (TODO)
#       | block
#       | proc
#       | lambda 
#       | loop

# use the 
#         block 
#       | lambda 
#       | Proc 
# and check the return value

# which is 
#         locally created 
#       | locally returned from a method call 
#       | passed as argument 

# return inside body and ensure 

print '''
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
'''

methods_take_argument = '''
    iterator_via_yield iterator_via_call 
    method_call_iterator_via_yield method_call_iterator_via_call
    method_use_lambda_and_yield  method_use_proc_and_yield method_use_lambda_and_call method_use_proc_and_call
    method_use_lambda_and_yield_2
    method_yield_in_loop  method_call_in_loop'''.split()

s = "{ puts B; ctrl_flow; puts B}"
i = 0

print "# created in-place"

print "def test"
for x in methods_take_argument:
    t1 = "$g = 0; begin; puts B; %s %s; puts B; rescue; puts B; puts $!.class; end" % (x, s)
    i += 1
    t2 = "$g = 0; def m_%s; puts B; %s; puts B; end; m_%s " % (i, t1, i)
    print t1
    print t2
    
print "end"
print "test"
print

print "\n# created locally or from method"

for x in [ 'lambda' + s, 'Proc.new'+s, 'get_block' + s, 'get_lambda' +s, 'get_proc' +s, 'get_local_block', 'get_local_lambda', 'get_local_proc']:
    print "def test"
    for y in methods_take_argument:
        print "$g = 0; begin; p = %s; puts B; %s(&p); puts B; rescue; puts B; puts $!.class; end" % (x, y)
        i += 1
        print "$g = 0; def m_%s; p = %s; puts B; %s(&p); puts B; end; \nbegin; puts B; m_%s; puts B; rescue; puts B; puts $!.class; end" % (i, x, y, i)
    print "end"
    print "test"
    print

print "def test"
for x in ['lambda' + s, 'Proc.new'+s, 'get_block' + s, 'get_lambda' +s, 'get_proc' +s, 'get_local_block', 'get_local_lambda', 'get_local_proc']:
    print "$g = 0; begin; puts B; p = %s; puts(p.call); puts B; rescue; puts B; puts $!.class; end" % x
    i += 1
    print "$g = 0; def m_%s; puts B; p = %s; puts(p.call); puts B; end; \nbegin; puts B; m_%s; puts B; rescue; puts B; puts $!.class; end" % (i, x, i)
    print "$g = 0; begin; puts B; puts m_%s; puts B; rescue; puts B; puts $!.class; end" % i
    i += 1    
    print "$g = 0; def m_%s; puts B; puts m_%s; puts B; end; \nbegin; puts B; m_%s; puts B; rescue; puts B; puts $!.class; end" % (i, i-1, i)
    
print "end"
print "test"
print

for x in 'lambda Proc.new get_block get_lambda get_proc'.split():
    print "def test"
    for y in ['lambda' + s, 'Proc.new'+s, 'get_block' + s, 'get_lambda' +s, 'get_proc' +s, 'get_local_block', 'get_local_lambda', 'get_local_proc']:
        print "$g = 0; begin; puts B; x = %s { puts B; p = %s; puts p.call; puts B}; puts x.call; puts B; rescue; puts B; puts $!.class; end" % (x, y)
        i += 1
        print '$g = 0; def m_%s; puts B; x = %s { puts B; p = %s; puts p.call; puts B}; puts x.call; puts B; end; \nbegin; puts B; m_%s; puts B; rescue; puts B; puts $!.class; end' % (i, x, y, i)
    print "end"
    print "test"
    print

print "def test"
for x in ['lambda' + s, 'Proc.new'+s, 'get_block' + s, 'get_lambda' +s, 'get_proc' +s, 'get_local_block', 'get_local_lambda', 'get_local_proc']:
    print '$g = 0; begin; puts B; for i in [1, 2]; puts B; p = %s; puts p.call; puts B; end; puts B; rescue; puts B; puts $!.class; end' % (x)
    i += 1
    print '$g = 0; def m_%s; puts B; for i in [1, 2]; puts B; p = %s; puts p.call; puts B; end; puts B; end;\nbegin; puts B; m_%s; puts B; rescue; puts B; puts $!.class; end' % (i, x, i)

print "end"
print "test"