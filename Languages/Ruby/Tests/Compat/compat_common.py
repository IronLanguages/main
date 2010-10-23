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

def replace_A(line):
    count = 0
    while True: 
        pos = line.find("A")
        if pos == -1: break
        line = line[:pos] + chr(ord('a') + count) + line[pos+1:]
        count += 1
    return line, count
    
def replace_B(line):
    count = 0
    while True:
        pos = line.find("B")
        if pos == -1: break
        line = line[:pos] + str(1 + count) + line[pos+1:]
        count += 1
    return line

def concat_char(i):
    return ",".join(["abcdefghijklmnopqrstuvwxyz"[x] for x in range(i)])


SPACE_LIMIT = 10
def _build_space_hash():
    h = {}
    for i in range(SPACE_LIMIT):  h[i] = "  " * i
    return h

_space_hash = _build_space_hash()
def space(d): 
    if d < SPACE_LIMIT: return _space_hash[d]
    return "  " * d

class FileCreator:
    def __init__(self, prefix, size, header=""):
        from datetime import datetime
        now = datetime.now()
        import nt
        if not nt.environ.has_key("DLR_BIN"):
            self.directory = prefix
        else:
            self.directory = "%s_%02d%02d_%02d%02d_%d" % (prefix, now.month, now.day, now.hour, now.minute, now.microsecond)
        self.size = size
        self.header = header
        self.count = 0
        self.func_count = 0
        self.file_list = []
        self.func_lines = []
        self.current_file = None
        
        try:
            nt.mkdir(self.directory)
        except: pass

    def save_block(self, *lines):
        self.save(False, *lines)

    def save_any(self, *lines):
        self.save(True, *lines)
        
    def save(self, do_not_counting, *lines):
        if self.count % self.size == 0:
            self.close()
            
            filename = "%s\\%s.rb" % (self.directory, self.count)
            self.file_list.append(filename)
            
            self.current_file = open(filename, 'w')
            self.current_file.writelines(self.header + "\n")
 
        if not do_not_counting:
            self.count += 1
            
        for x in lines: 
            self.current_file.writelines(x + "\n")

    def save_to_function(self, line):
        self.func_lines.append(line)
        
        if self.count % (self.size * self.func_size) == 0:
            self.close()
            
            filename = "%s\\%s.rb" % (self.directory, self.count)
            self.file_list.append(filename)
            
            self.current_file = open(filename, 'w')
            self.current_file.writelines(self.header + "\n")

        if len(self.func_lines) >=  self.func_size:
            self.current_file.writelines("def f_%s\n" % self.func_count)
            for x in self.func_lines:
                self.current_file.writelines(x + "\n")
            self.current_file.writelines("end\n")
            self.current_file.writelines("f_%s\n" % self.func_count)
            self.func_count += 1
            self.func_lines = []

        self.count += 1
                        
    def close(self):
        if self.current_file and not self.current_file.closed:
            self.current_file.close()
    
    def print_file_list(self):
        print "#Total: %s files" % len(self.file_list)
        for x in self.file_list:
            print x
