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

require "pp"
require "compat_common"


def one_level_loop 
    bool_seq = Bool_Sequence.new(64)
    cb = Combination.new([1, 3, 5,], ['next', 'break', 'retry', 'redo'])

    ['while', 'until'].each do |keyword|
        bool_seq.each do |bool_sequence|
            cb.generate do |number, flow|
                t = "bools ="
                PP.pp(bool_sequence + [keyword == 'while' ? false : true] * 2, t)
                
                t += "i = 0
$g = \"\"
#{keyword} bools[i] do
    i += 1
    $g += \"a1\"
    if i == #{number}
        $g += \"#{flow[0,1]}1\"
        #{flow}
        $g += \"#{flow[0,1]}2\"
    end    
    $g += \"a2\"
end
puts $g
"
                yield t
            end 
            
            cb.generate2 do |number1, flow1, number2, flow2|
                t = "bools ="
                PP.pp(bool_sequence + [keyword == 'while' ? false : true] * 3, t)
                
                t += "i = 0
$g = \"\"
#{keyword} bools[i] do
    i += 1
    $g += \"a1\"

    if i == #{number1}
        $g += \"#{flow1[0,1]}1\"
        #{flow1}
        $g += \"#{flow1[0,1]}2\"
    end
    
    $g += \"a2\"
    
    if i == #{number2}
        $g += \"#{flow2[0,1]}1\"
        #{flow2}
        $g += \"#{flow2[0,1]}2\"
    end   
     
    $g += \"a3\"
end
puts $g
"
                yield t
            end 
        end 
    end
end


def three_level_loop
    bs2 = Bool_Sequence.new(8)
    bs3 = Bool_Sequence.new(8)
    cb1 = Combination.new([1, 2, 3,], ['next', 'break', 'retry', 'redo'])
    cb2 = Combination.new([1, 2, 3,], ['next', 'break', 'retry', 'redo'])
    
    [[true, true,], ].each do |bool_sequence1|
        bs2.each do |bool_sequence2|
            bs3.each do |bool_sequence3|
                cb1.generate do |number1, flow1|
                    cb2.generate do |number2, flow2|
                        t = "bools1 = "
                        PP.pp(bool_sequence1 + [false] * 2, t)
                        t += "bools2 = "
                        PP.pp(bool_sequence2 + [true] * 2, t)
                        t += "bools3 = "
                        PP.pp(bool_sequence3 + [false] * 2, t)
                        
                        t += "
$g = \"\"                        
i = 0
while bools1[i] do 
    i += 1
    $g += \"a1\"
    
    j = 0
    until bools2[j] do 
        j += 1
        $g += \"b1\"
        
        if j == #{number1}
            $g += \"#{flow1[0,1]}1\"
            #{flow1}
            $g += \"#{flow1[0,1]}2\"
        end 
        
        k = 0
        while bools3[k] 
            k += 1
            $g += \"c1\"
            if k == #{number2}
                $g += \"#{flow2[0,1]}1\"
                #{flow2}
                $g += \"#{flow2[0,1]}2\"
            end 
            $g += \"c2\"
        end 
        $g += \"b2\"
        
    end        
    $g += \"a2\"
end 
puts $g
    " 
    
                        yield t
                    end                
                end                
            end 
        end 
    end 
end 


total = 0

p = lambda do |t|
    total += 1    
    results = GenFile.create_temporaily(t).run()
    if results[0] == 0
        GenFile.create_sequentially(t + "\nraise if $g != \"#{results[1]}\"\n")
    else
        tf = GenFile.create_sequentially(t + "\n#expected to fail\n")
        GenFile.add_negative_test(tf)
    end 
end

GenFile.prepare("ctlf_one_")
one_level_loop &p
GenFile.finish

GenFile.prepare("ctlf_three_")
three_level_loop &p
GenFile.finish

printf("total generated: %s", total)
