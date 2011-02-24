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

from compat_common import *

'''
Exception
|- ScriptError
   |- LoadError
   |- SyntaxError
|- StandardError
   |- IOError
   |- TyperError
   |- ZeroDivisionError
   |- RuntimeError
   |- MyStdError   
|- MyException   
'''
   
from _random import Random
rnd = Random()

def randomly_choose(l):
    if l:
        return l[int(rnd.random() * len(l))]
    else:
        return None
    
class Ex:
    def __init__(self, name):
        self.name = name 
        self.children = []
        self.parent = None
        
    def add_child(self, ex):
        self.children.append(ex)
        ex.parent = self
    
    def __str__(self):
        return self.name
    
    @property    
    def any_sibling(self):
        if self.parent:
            return randomly_choose(self.parent.children)
        else:
            return None
            
    @property
    def any_child(self):
        return randomly_choose(self.children)

all_exceptions = "Exception ScriptError LoadError SyntaxError StandardError IOError TyperError ZeroDivisionError RuntimeError MyException MyStdError".split()   
for ex in all_exceptions: exec("%s = Ex('%s')" % (ex, ex))

# build the exception tree 
Exception.add_child(ScriptError)
Exception.add_child(StandardError)
Exception.add_child(MyException)

ScriptError.add_child(LoadError)
ScriptError.add_child(SyntaxError)

StandardError.add_child(IOError)
StandardError.add_child(TyperError)
StandardError.add_child(ZeroDivisionError)
StandardError.add_child(RuntimeError)
StandardError.add_child(MyStdError)

assign_to_bang = "($!=%s.new; %s)"

def assign_to_bang_generator(ex):
    yield  assign_to_bang % (ex, ex)
    if ex.parent:
        yield assign_to_bang % (ex.parent, ex)
    if ex.any_sibling:
        yield assign_to_bang % (ex.any_sibling, ex)
    if ex.any_child:
        yield assign_to_bang % (ex.any_child, ex)
    #yield "($!=nil; %s)" % ex

def catch_generator(ex):
    yield "(return B; IOError)"
    yield "(raise MyStdError; IOError)"
    
    yield ex.name
    
    for x in assign_to_bang_generator(ex):
        yield x
        
    if ex.parent:
        yield ex.parent
        for x in assign_to_bang_generator(ex.parent):
            yield x
    
    y = ex.any_sibling            
    if y:
        yield y
        for x in assign_to_bang_generator(y):
            yield x
            
    y = ex.any_child
    if y:
        yield y
        for x in assign_to_bang_generator(y):
            yield x

concat_string = "%s, %s"

def twice_catch_generator(ex):
    for x in catch_generator(ex):
        if ex.parent:
            yield concat_string % (x, ex.parent)
            for z in catch_generator(ex.parent):
                yield concat_string % (x, z)
                
        y = ex.any_sibling            
        if y:
            yield concat_string % (x, y)
            for z in catch_generator(y):
                yield concat_string % (x, z)
                
        y = ex.any_child
        if y:
            yield concat_string % (x, y)
            for z in catch_generator(y):
                yield concat_string % (x, z)


fc = FileCreator("test_rescue_clause", 300, '''
class MyException < Exception; end 
class MyStdError < StandardError; end
''')

count = 0

def write(raise_x, catch_y):
    global fc, count
    s = "begin\n"
    s += "    raise %s.new\n" % raise_x
    s += "rescue %s\n" % catch_y
    s += "    puts B, $!.class\n"
    
    line1 = "def f_%s\n" % count + s + "end\nputs B, $!.class\nend\n"
    line2 = "$g = 0; begin; print(f_%s); rescue Exception; puts B, $!.class; end; puts \" : f_%s\"\n" % (count, count)
    count += 1
    fc.save_block(replace_B(line1+line2))
    
    #line1 = "def f_%s\n" % count + s + "ensure;\n    puts B, $class\nend\nputs B, $!.class\nend\n"
    #line2 = "$g = 0; begin; print(f_%s); rescue Exception; puts B, $!.class; end; puts \" : f_%s\"\n" % (count, count)
    #count += 1
    #fc.save_block(replace_B(line1+line2))

interesting_exceptions = [eval(x) for x in "Exception ScriptError SyntaxError StandardError ZeroDivisionError MyException MyStdError".split()]

for raise_x in interesting_exceptions:
    for catch_y in catch_generator(raise_x):
        write(raise_x, catch_y)
    for catch_y in twice_catch_generator(raise_x):
        write(raise_x, catch_y)
        
fc.close()
fc.print_file_list()    
    