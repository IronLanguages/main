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

# Try to generate the test which represents the specified module/class layout
# We verify the ancestors, and try to call class/module methods and access the constants.

from compat_common import *
count = 0
class Entity(object):
    def __init__(self, name):
        self.name = name
        self.parent = None
        self.mixins = []
        self.identifiers = {}
        self.dependency = []

    def include(self, *mixins):
        self.mixins = mixins
    
    def reset_mixin(self):
        self.mixins = []

    def get_body(self):
        body = ""
        if self.parent:
            body += "%s %s < %s\n" % (self.keyword, self.name, self.parent.name)
        else:
            body += "%s %s\n" % (self.keyword, self.name)
        if self.mixins:
            body += "  include %s\n" % ", ".join([x.name for x in self.mixins])
        for (k,v) in self.identifiers.iteritems():
            body += "  def m_%s; %s; end\n" % (k, v)
        for (k,v) in self.identifiers.iteritems():
            body += "  C_%s = %s\n" % (k, v)
        body += "end\n"
        return body
    
    def __str__(self):
        return self.name

    def generate_ids(self, available_entities):
        global count
        
        self.identifiers["%s_only" % self.name] = count 
        count += 1

        for x in available_entities:
            if x == self: continue

            if x.name > self.name:
                t = (x.name, self.name)
            else:
                t = (self.name, x.name)

            self.identifiers["%s_%s" % t] = count
            count += 1

class Module(Entity):
    def __init__(self, name):
        super(Module, self).__init__(name)
        self.keyword = "module"

class Class(Entity):
    def __init__(self, name):
        super(Class, self).__init__(name)
        self.keyword = "class"
    def derive_from(self, parent):
        self.parent = parent

# globals
class_count = 7
module_count = 10

# create modules 
m_kernel = Module("Kernel")
for x in range(1, module_count): exec "m%s = Module('M%s')" % (x, x)

# create classes with fixed parent-child relationships
c_object = Class("Object")
for x in range(1, class_count): exec "c%s = Class('C%s')" % (x, x)

#             c1
#            /  \
#           c2   c6
#          / 
#        c3
#       /  \
#     c4    c5

c2.derive_from(c1)
c3.derive_from(c2)
c4.derive_from(c3)
c5.derive_from(c3)
c6.derive_from(c1)

user_defined_classes = [eval("c%s" % x) for x in range(1, class_count)]
user_defined_modules = [eval("m%s" % x) for x in range(1, module_count)]
all_entities = [ m_kernel, c_object ] 
all_entities.extend(user_defined_classes)
all_entities.extend(user_defined_modules)
all_classes = [ x for x in all_entities if isinstance(x, Class) ]
all_modules = [ x for x in all_entities if isinstance(x, Module) ]

def get_header(identifiers):
    header = '''def check(l1, l2)
    cnt = 0
    puts "************************#{l2.inspect}***********************"
    l1.each do |c|
        x = c.new
        y = l2[cnt]
        cnt += 1
        puts "===================#{y}======================="
        puts "-------------ancestors: #{c.ancestors.inspect}--------"
'''

    for x in identifiers:  
        header += "        puts 'm_%s'; puts x.m_%s rescue puts 'error'\n" % (x, x)
    for x in identifiers:  
        header += "        puts 'C_%s'; puts c::C_%s rescue puts 'error'\n" % (x, x)
        
    header += '    end\nend\n'
    return header

fc = FileCreator("test_ancestor_graph", 1)

def merge_dict(*dicts):
    d = {}
    for x in dicts: d.update(x)
    return d
    
module_layout1 = { 
    m2 : [m1], 
    m4 : [m1, m3], m5 : [m3, m1], 
    m6 : [m2, m1], m7 : [m1, m2], 
    m8 : [m6, m1, m4],
}


#             m1    m3
#             /|\   /
#           m2 | m4/m5
#            \ | \|
#            m6/m7|
#                \|
#                 m8

#             c1
#            /  \
#           c2   c6
#          / 
#        c3
#       /  \
#     c4    c5

for (sequence, global_include) in [ 
    # hand-picked scenarios
    (
        merge_dict(module_layout1, { c1 : [m2] }), 
        { c3 : [m3]}  
    ), 
    (
        merge_dict(module_layout1, { m_kernel : [m9], c3 : [m5] }), 
        { c3 : [m_kernel, m4]}
    ),
    (
        merge_dict(module_layout1, { c2 : [m4], c6 : [m6] }), 
        { m4 : [m2], c4 : [m4]}
    ),
    (   
        merge_dict(module_layout1, { c1 : [m5], c3 : [m1], c_object : [m3] }), 
        { }
    ),
    (   
        merge_dict(module_layout1, { c4 : [m6, m3], c2 : [m1]}), 
        { m5 : [m3]}
    ),
] :
    current_entities = all_entities[:]
    
    # reset the class mixins
    for e in current_entities:
        e.reset_mixin()

    # set new mixins
    for (k, v) in sequence.iteritems():
        k.include(*v)
    
    # get nessary modules
    included_modules = set()
    for x in all_classes: 
        for y in x.mixins: included_modules.add(y)
        
    for y in m_kernel.mixins: included_modules.add(y)
    
    for (k, v) in global_include.iteritems():
        if isinstance(k, Module):
            included_modules.add(k)
        for y in v:
            included_modules.add(y)
    
    # omg!!
    while True:
        copy = included_modules.copy()
        l1 = len(copy)
        
        for x in included_modules:
            for y in x.mixins:
                copy.add(y)

        l2 = len(copy)
        if l1 == l2: break
            
        included_modules = copy.copy()

    current_entities = list(included_modules)
    current_entities.sort(lambda x, y: (x.name < y.name) and -1 or 1)
    
    # tweak Kernel location: Kernel will be first if currently present
    if m_kernel.mixins:
        current_entities.remove(m_kernel)
        current_entities.append(m_kernel)
    else: 
        current_entities.insert(0, m_kernel)

    current_entities.extend(all_classes)
    
    scenario = ",".join([x.name for x in current_entities])

    current_identifiers = set()
    for e in current_entities:
        e.generate_ids(current_entities)
        for y in e.identifiers.keys():
            current_identifiers.add(y)
            
    lines = "## %s\n\n" % scenario
    lines += get_header(current_identifiers)
    
    cx = []
    for e in current_entities:
        lines += e.get_body()

        if e.name.startswith("C"):
            cx.append(e.name)
            lines += "check [%s], %s\n" % (','.join(cx), cx)
        
        if e in global_include.keys():
            lines += "include " + ",".join([x.name for x in global_include[e]]) + "\n"
        
            if e.name.startswith("C"):
                lines += "check [%s], %s\n" % (','.join(cx), cx)

    fc.save_block(lines)

fc.close()
fc.print_file_list()