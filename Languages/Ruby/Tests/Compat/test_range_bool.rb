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

#
#  expr1..expr2 or expr1...expr2
#
#  different boolean sequences are applied to expr1 and expr2, to check the behavior
#

def get_bool_repr(x)
    l = []
    0.upto(4) do |i|
        l << (x % 2 == 1) ? true: false
        x = x / 2
    end 
    l
end 

$bool_sequences = []

32.times do |x|
    $bool_sequences << get_bool_repr(x)
end 

def get_bool(i, j) 
    $bool_sequences[i][j]
end 

puts "2-dot"
32.times do |i|
    32.times do |j|
        l = []
        5.times do |k| 
            if get_bool(i, k)..get_bool(j, k)
                l << k
            end
        end
        l.each {|x| printf("%d", x) }
        puts
    end 
end

puts "3-dot"
32.times do |i|
    32.times do |j|
        l = []
        5.times do |k| 
            if get_bool(i, k)...get_bool(j, k)
                l << k
            end
        end
        l.each {|x| printf("%d", x) }
        puts
    end 
end
