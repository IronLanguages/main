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

# Try to generate the test which represents the user-defined class, Class, Object, Module and 
# their Singleton classes (and their Singleton classes)... We also re-open such classes with 
# new members
#
# We verify by calling the methods through the class name and its' instance. We also call methods
# which access instance/class variables (this part is ugly, not sure how I can/should simplify it)

from compat_common import *

# globals 
entities = []
count = 0

class Entity:
    def __init__(self, name, body, depends=None):
        self.name = name
        self.body = body
        self.set_dependency(depends)
        self.updated = False
            
    def __str__(self):
        return self.name
            
    def set_dependency(self, d):
        self.depend_on = d
    
    def get_id(self):
        if "##" in self.body:
            start = self.body.find("##")
            end = self.body.find("\n", start)
            return int(self.body[start + 2 : end])
        else:
            return None
                    
    def create_singleton_class(self):
        global entities

        s_name = "S_" + self.name 
        if s_name.startswith("S_" * 4): return
        
        # exit if already created
        for t in entities:
            if t.name == s_name: return 
            
        new_entity = Entity(s_name, "%s = class << %s\n%s\n    self\nend\n" % (s_name, self.name, get_body()))
        
        # they are predefined classes, not necessary to be defined again
        if self.name not in ["Object", "Module", "Class"]: 
            new_entity.set_dependency(self)
            
        entities.append(new_entity)

    def update_body(self, ids):
        if self.name in 'xyz' or self.updated: 
            return 
            
        eol1 = self.body.find('\n')
        
        methods = "    def read_icheck();\n"
        for y in ids:    
            methods += '        puts "iv%s"; puts @iv%s rescue puts "inner_error"\n' % (y, y)
            methods += '        puts "sv%s"; puts @@sv%s rescue puts "inner_error"\n' % (y, y)
        methods += "    end\n    def self.read_scheck();\n"
        for y in ids:    
            methods += '        puts "iv%s"; puts @iv%s rescue puts "inner_error"\n' % (y, y)
            methods += '        puts "sv%s"; puts @@sv%s rescue puts "inner_error"\n' % (y, y)
        methods += "    end\n"
            
        self.body = self.body[:eol1+1] + methods + self.body[eol1+1:]
        self.updated = True
#
# create all entities 
#
def get_body():
    global count    
    count += 1    
    return '''
    ##%s
    CONST%s = -%s 
    @iv%s = %s   # class instance variable
    @@sv%s = %s0
    
    def initialize; @iv%s = -%s0; end  # instance variable; intentionally they have the same name
    def im%s; %s00; end
    def self.sm%s; -%s00; end
''' % ( (count,) * 13 )    

def get_definition(name, is_module=False): 
    return '%s %s\n%s\nend' % (is_module and "module" or "class", name, get_body())

module_m = Entity("M", get_definition("M", is_module=True))
class_b  = Entity("B", get_definition("B"))
class_c  = Entity("C", get_definition("C < B"), class_b)
class_d  = Entity("D", get_definition("D < B\n    include M"), class_b)

c_x = Entity("x", "x = C.new\n", class_c)
c_y = Entity("y", "y = C.new\n", class_c)
c_z = Entity("z", "z = D.new\n", class_d)

class_object = Entity("Object", get_definition("Object")) 
class_module = Entity("Module", get_definition("Module")) 
class_class = Entity("Class", get_definition("Class")) 

entities = [class_b, class_c, module_m, class_d, c_y, c_z, class_object, class_module, class_class ]
for e in entities: e.create_singleton_class()
entities.append(c_x)

# add explicitly re-opened classes
entities.extend([Entity(e.name + "_open", get_definition(e.name), e) 
                 for e in entities 
                 if e.name.startswith("S_")])

#    Module,  S_Module,      B, 
#                |           |
#            S_S_Module      C
#                           / \ 
#                          y   S_C
#                         /      \
#                       S_y     S_S_C

entities_dict = dict([(e.name, e) for e in entities])
def names_to_entities(s):
    return [entities_dict[x] for x in s.split()]

def generate_entity_sequence():
    #yield (entities, str(entities_dict.keys())) # too slow
    
    # hand-picked sequences
    for x in [
        # need update
        'B C S_C y S_B S_S_B S_S_C x S_Class S_Object Object Class S_S_Object S_S_Class',
        'S_Object B Object S_Class Class C x S_C S_S_C y S_Module Module S_y S_S_y',
        'B C x y S_y S_S_y S_S_S_y S_B S_S_B S_S_S_B S_C S_S_C S_S_S_C Object S_Object S_S_Object S_S_S_Object Class S_Class S_S_Class S_S_S_Class Module S_Module S_S_Module S_S_S_Module',
        'S_Object S_Class S_Module S_S_Object S_S_S_Object S_S_Class S_S_S_Class S_S_Module Class Object Module S_S_S_Module B C y S_y S_S_y S_S_S_y S_B S_S_B S_S_S_B S_C S_S_C S_S_S_C x',
    ]:
        yield (names_to_entities(x), x) 
        
    import random
    
    for x in range(5): 
        sample = random.sample(entities, 4)

        #print [z.name for z in sample]
        
        # remove those be depended by other in sample
        for y in sample:
            current = y
            while current.depend_on:
                if current.depend_on in sample:
                    sample.remove(current.depend_on)
                current = current.depend_on
        
        #print [z.name for z in sample]
        
        # made a seqence
        sample_clone = []
        for y in sample:
            current = y 
            sample_clone.insert(0, y)
            while current.depend_on:
                if current.depend_on in sample_clone:
                    sample_clone.remove(current.depend_on)
                sample_clone.insert(0, current.depend_on)
                current = current.depend_on
        
        if class_d in sample_clone:
            if module_m in sample_clone:
                sample_clone.remove(module_m)
            sample_clone.insert(0, module_m)
            
        #print [z.name for z in sample_clone]
        
        yield (sample_clone, str([z.name for z in sample_clone]))
        
# the function we use to check, added to each file

def get_header(ids):
    header = 'def check_each(x, l)\n'
    for y in ids:    
        header += '    puts "#{l}.CONST%s"; puts x::CONST%s rescue puts "error"\n' % (y, y)
        header += '    puts "#{l}.im%s"; puts x.im%s rescue puts "error"\n' % (y, y)
        header += '    puts "#{l}.sm%s"; puts x.sm%s rescue puts "error"\n' % (y, y)
        
    header += '    puts "read_icheck"; x.read_icheck rescue puts "error"\n' 
    header += '    puts "read_scheck"; x.read_scheck rescue puts "error"\n' 
    header += "end\n"

    header += '''
def check(vars, names)
    puts '=========================================='
    cnt = 0
    vars.each do |x|
        puts "--------------#{cnt}, #{names[cnt]}---------------"
        cnt += 1
'''
    header += "        check_each(x, 'Class')\n"
    header += "        begin; y = x.new \n"
    header += "        rescue; puts 'fail to new'\n"
    header += "        else; check_each(y, 'Instance')\n"
    header += "        end\n    end\nend\n"
    return header

fc = FileCreator("test_singleton_graph", 1)

# give me a names_to_entities
for (seq, scenario) in generate_entity_sequence():
    vars = []
    lines = "# %s \n" % scenario
    
    all_ids = []
    for e in seq:
        if hasattr(e, "get_id"):
            idx = e.get_id()
            if idx: 
                all_ids.append(idx)
    
    lines += get_header(all_ids)    
    
    for e in seq:
        if not e.name.endswith("_open"):
            vars.append(e.name)
    
        temp = ', '.join(vars)

        e.update_body(all_ids)
        lines += e.body
        
        lines += '\nputs "after adding %s"\n' % e.name
        lines += 'puts "now vars: %s"\n' % temp
        lines += 'check [%s], %s\n' % (temp, vars)
    
    fc.save_block(lines)
       
fc.close()
fc.print_file_list()    
