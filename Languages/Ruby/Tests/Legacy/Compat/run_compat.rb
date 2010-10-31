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
# run each test in c-ruby and ironruby, check the output difference.
# if a baseline log file (whether it is correct or not) exists, 
# c-ruby run will be skipped
#
# usage: 
#   * ruby.exe run_compat.rb  
#     -- run all test_*.rb under this directory
#   * ruby.exe run_compat.rb test_a.rb test_b.rb
#     -- run test_a.rb and test_b.rb only
#
# to do:
#   * support "smart" compare

require  File.dirname(__FILE__) + "/../common"
begin
    require 'fileutils'
rescue LoadError => e
    p $:
    raise e
end

$failure = 0
def log_error(comment)
    printf " #{comment}"
    $failure += 1
end 

CRubyDriver = Test::CRubyDriver.new(false, false)

$applicable_drivers = [
    Test::IronRubyDriver.new(2, "ironm2", false, false)
]

def compare_succeed?(log1, log2, diff_log)
    lines1 = open(log1) { |f1| f1.readlines }
    lines2 = open(log2) { |f2| f2.readlines }
    
    return false unless lines1.length == lines2.length
    
    diff = []
    last_repro_line1 = last_repro_line2 = ""
    
    0.upto(lines1.length - 1) do |i|
        last_repro_line1 = lines1[i].strip if lines1[i].include? "repro:"
        last_repro_line2 = lines2[i].strip if lines2[i].include? "repro:"
            
        next if lines1[i] == lines2[i]
        
        if last_repro_line1 == last_repro_line2
            temp = last_repro_line1
        else 
            temp = last_repro_line1 + " | " + last_repro_line2
        end  
        
        diff << temp +" | " + lines1[i].strip + " | " + lines2[i].strip + "\n"
    end 
    
    if diff.length != 0
        File.open(diff_log, "w") do |f|
            diff.each do |l|
                f << l
            end 
        end 
    end 
    return diff.length
end 

def run_one_test(f)
    printf ">>> testing #{f} ... "
    use_baseline = false
    fbn = f[0..-4]
    clog = File.expand_path "#{fbn}.c.log"
    ilog = File.expand_path "#{fbn}.i.log"
    baseline = File.expand_path"#{fbn}.bsl"
    diff_log = File.expand_path "#{fbn}.diff.log"
    
    if File.exist?(baseline)
        FileUtils.cp(baseline,clog)
        printf " S"
    elsif CRubyDriver.run(File.expand_path(f, File.dirname(__FILE__)), clog) != 0
        log_error("failed while running CRuby\n")
        return  
    end

    $applicable_drivers.each do |d|    
        if d.run(File.expand_path(f, File.dirname(__FILE__)), ilog) != 0
            log_error("failed while running #{d}\n")
        end
        
        diff_count = compare_succeed?(clog, ilog, diff_log)
        if diff_count != 0
            log_error "failed while comparing (#{diff_count})\n"
            printf "          windiff #{clog} #{ilog} | diff log: #{diff_log}\n"
        else
            File.delete(clog, ilog)
            printf " pass\n"
        end 
    end 
end 

test_files = []

ARGV.each do |arg|
    if arg =~ /-snap/
        # filter -xxx switches out
    else
        if arg.index("@") == 0
            File.open(arg[1..-1]) { |f| f.readlines.each { |l| test_files << l.strip unless l.include?("#") } }
        else 
            test_files << arg
        end 
    end 	
end 

if test_files.empty?
    test_files = [
        "test_parallel_assign1.rb",
        "test_parallel_assign2.rb",
        "test_parallel_assign3.rb",
        "test_assignment.rb",
        "test_func_splat.rb",
        "test_assignment_regression.rb",
        "test_exception_regression.rb",
        "test_block_ctrl_flow_break.rb",
        "test_block_ctrl_flow_next.rb",
        "test_block_ctrl_flow_normal.rb",
        "test_block_ctrl_flow_raise.rb",
        "test_block_ctrl_flow_redo.rb",
        "test_block_ctrl_flow_retry.rb",
        "test_block_ctrl_flow_return.rb",     
    ]
end 

test_files.each { |f| run_one_test(f) }

printf "\nSummary [failure: #{$failure}]\n"
exit($failure)
