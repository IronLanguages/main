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

# rescue_clause = empty | rescue_default | rescue_E | rescue_which_raise_E
# else_clause = empty | "else"
# ensure_clause = empty | "ensure"
# body = not_raise | raise_E

class RescueInfo:
    def __init__(self, present, except_type, raise_in_param):
        self.present = present
        self.except_type = except_type
        self.raise_in_param = raise_in_param
    
    def get_code(self):
        if self.present:
            if self.raise_in_param: 
                s = "rescue (puts B, $!.class; raise IOError)"
            else: 
                s = "rescue "
                if self.except_type: 
                    s += self.except_type 
                    
            for x in possible_bodies:
                yield s + "; \n" + x
        else:
            yield "#empty_rescue"
                
class ElseInfo:
    def __init__(self, present):
        self.present = present
    def get_code(self):
        if self.present:
            for x in possible_bodies:
                yield "else; \n" + x
        else:
            yield "#empty_else"

class EnsureInfo:        
    def __init__(self, present):
        self.present = present
    def get_code(self):
        if self.present:
            for x in possible_bodies:
                yield "ensure; \n" + x
        else:
            yield "#empty_ensure"
        
class EH:
    def __init__(self, rescue_info, else_info, ensure_info):
        self.rescue_info = rescue_info
        self.else_info = else_info
        self.ensure_info = ensure_info
        
    def get_code(self):
        for x in possible_bodies:
            for ri in self.rescue_info.get_code():
                for eli in self.else_info.get_code():
                    for eni in self.ensure_info.get_code():
                        s = "begin\n"
                        s += x + "\n"
                        s += ri + "\n"
                        s += eli + "\n"
                        s += eni + "\n"
                        s += "end\n"
                        yield s
            
def rescueinfo_generator():
    for x in [True, False]:
        for y in [None, "ZeroDivisionError", "IOError"]:
            for z in [True, False]:
                yield RescueInfo(x, y, z)

def elseinfo_generator():
    for x in [True, False]:
        yield ElseInfo(x)

def ensureinfo_generator():
    for x in [True, False]:
        yield EnsureInfo(x)

possible_bodies = [""] + map(lambda x : "     puts B, $!.class;" + x, [
        "1/0", 
        "raise TypeError", 
        "raise SyntaxError.new", 
        "return 3", 
        "break",
        "next", 
        "$g += 1; retry if $g < 4;",
        "$g += 1; redo if $g < 4;"
        
        "begin; puts B, $!.class; raise ArgumentError; rescue TypeError; puts B; end", 
        "begin; puts B, $!.class; ensure; puts B, $!.class; raise TypeError; puts B; end",
        "$! = nil",
        "$! = IOError.new", 
        "$! = TypeError.new", 
        ])

# another body set
# possible_bodies = 

from compat_common import *
       
fc = FileCreator("test_exception2", 200)
count = 0

for x in rescueinfo_generator():
    for y in elseinfo_generator():
        for z in ensureinfo_generator():
            for w in EH(x, y, z).get_code():
                line1 = "def f_%s\n" % count + w + "puts B, $!.class\nend\n"
                line1 += "$g = 0; begin; print(f_%s); rescue Exception; puts B, $!.class; end; puts \" : f_%s\"\n" % (count, count)
                count += 1
                
                fc.save_block(replace_B(line1))

fc.close()
fc.print_file_list()    
