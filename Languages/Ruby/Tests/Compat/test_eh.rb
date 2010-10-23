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

def less_than times
    $retry_count += 1
    $retry_count <= times
end 

class CodeHolder
    attr_accessor :depth
    def initialize 
        @text = ""
        @depth = 0
    end 
    def add(text)
        @text += text
    end 
    def clear
        @text = ""
    end 
    def print
        puts @text
    end 
end 

SPACE = " " * 4

class Begin_Ensure_generator
    def initialize(ch, beginBody, ensureBody)
        @code = ch
        @beginBody = beginBody
        @ensureBody = ensureBody
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("begin\n") 
        @beginBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("ensure\n")
        @ensureBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("end\n") 
    end 
end

class Begin_Rescue_generator
    def initialize(ch, beginBody, rescueBody)
        @code = ch
        @beginBody = beginBody
        @rescueBody = rescueBody
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("begin\n") 
        @beginBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("rescue\n")
        @rescueBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("end\n") 
    end 
end 

class Begin_Rescue_Else_generator
    def initialize(ch, beginBody, rescueBody, elseBody)
        @code = ch
        @beginBody = beginBody
        @rescueBody = rescueBody
        @elseBody = elseBody
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("begin\n") 
        @beginBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("rescue\n")
        @rescueBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("else\n")
        @elseBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("end\n") 
     end 
end 

class Begin_Rescue_Ensure_generator
    def initialize(ch, beginBody, rescueBody, ensureBody)
        @code = ch
        @beginBody = beginBody
        @rescueBody = rescueBody
        @ensureBody = ensureBody
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("begin\n") 
        @beginBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("rescue\n")
        @rescueBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("ensure\n")
        @ensureBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("end\n") 
    end 
end 

class Begin_Rescue_Else_Ensure_generator
    def initialize(ch, beginBody, rescueBody, elseBody, ensureBody)
        @code = ch
        @beginBody = beginBody
        @rescueBody = rescueBody
        @elseBody = elseBody
        @ensureBody = ensureBody
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("begin\n") 
        @beginBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("rescue\n")
        @rescueBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("else\n")
        @elseBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("ensure\n")
        @ensureBody.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("end\n") 
    end 
end 

class Pass_generator
    def initialize ch
        @code = ch
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("puts 1\n")
    end 
end 

class While_Loop_generator
    def initialize(ch, condition, body)
        @code = ch
        @condition = condition
        @body = body
    end 
    def generate(indent = 1)
        @code.add(SPACE*indent); @code.add("while \n") 
        @code.add(condition)
        @body.generate(indent + 1)
        @code.add(SPACE*indent); @code.add("end\n") 
    end 
end

def begin_ensure_maker(ch, body)
    Begin_Ensure_generator.new(ch, body, body)
end 

def begin_rescue_maker(ch, body)
    Begin_Rescue_generator.new(ch, body, body)
end 

def pass_maker(ch, body)
    Pass_generator.new(ch)
end 

methods = [:begin_ensure_maker, :begin_rescue_maker, :pass_maker].collect! { |x| self.method(x) }


ch = CodeHolder.new

def do_generate tc
    for tc2 in methods
        if ch.depth > 3
            yield tc.call(pass_maker(nil))
        else 
            
        end
    end 
end 

for tc in methods
    for y in do_generate(tc)
        $ch.clear
        x.generate
        $ch.print
    end
end
