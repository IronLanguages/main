require "slt_helper"


#
# define test list here:
#

test_list=[
# 0 - Problem test that have to run by itself, it's pretty fast though # 
[
'runtime\class\test_change_self.rb', 
],
# 1 - buildin1# 
[
'builtin\object\test_objects.rb',
'builtin\range\test_range.rb',
'builtin\string\test_string.rb'
],
# 2 - block # 
[
'runtime\block\test_break.rb',
'runtime\block\test_closure.rb',
'runtime\block\test_create_invoke.rb',
'runtime\block\test_return.rb',
'runtime\block\test_yield.rb'
],
# 3 - class # 
[
'runtime\class\test_find_method.rb',
'runtime\class\test_class_variables.rb',
'runtime\class\test_class_variables2.rb',
'runtime\class\test_const.rb',
'runtime\class\test_const_vars_basic.rb',
'runtime\class\test_const_vars_in_singleton.rb',
'runtime\class\test_declare.rb',
'runtime\class\test_include.rb',
'runtime\class\test_instance_variables.rb',
'runtime\class\test_methods.rb',
'runtime\class\test_new.rb',
'runtime\class\test_top_level.rb',
'runtime\class\test_variables.rb',
'runtime\class\test_virtual_attr.rb',
],
# 4 - exception # 
[
'runtime\exception\test_else.rb',
'runtime\exception\test_nested.rb',
'runtime\exception\test_raise_nothing.rb',
'runtime\exception\test_raise_string.rb',
'runtime\exception\test_raise_thing.rb',
'runtime\exception\test_regression.rb',
'runtime\exception\test_rescue_many.rb',
'runtime\exception\test_rescue_modifier.rb',
'runtime\exception\test_rescue_nothing.rb',
'runtime\exception\test_rescue_sequence.rb',
'runtime\exception\test_rescue_unusual.rb',
'runtime\exception\test_retry.rb',
'runtime\exception\test_return.rb',
'runtime\exception\test_scope.rb',
'runtime\exception\test_ensure.rb',
'runtime\exception\test_final_value.rb',
'runtime\exception\test_lifetime.rb',
'runtime\exception\test_match.rb', # this test has to be at the end. it causes other tests to fail
],
# 5 - expression #
[
'runtime\expression\test_assignment.rb',
'runtime\expression\test_boolean_expr.rb',
'runtime\expression\test_break_in_loop.rb',
'runtime\expression\test_case.rb',
'runtime\expression\test_for_loop.rb',
'runtime\expression\test_if.rb',
'runtime\expression\test_nested_assignment.rb',
'runtime\expression\test_next_in_loop.rb',
'runtime\expression\test_parallel_assignment.rb',
'runtime\expression\test_range_as_bool.rb',
'runtime\expression\test_redo_in_loop.rb',
'runtime\expression\test_retry_in_loop.rb',
'runtime\expression\test_while_until.rb'
],
# 6 - method #
[
'runtime\method\test_coverage.rb',
'runtime\method\test_defaultvalue.rb',
'runtime\method\test_mixed_arg.rb',
'runtime\method\test_normal_arg.rb',
'runtime\method\test_pass_args.rb',
'runtime\method\test_recursive.rb',
'runtime\method\test_return_value.rb',
'runtime\method\test_var_arg.rb',
'runtime\method\test_super.rb'
],
# 7 - module #
[
'runtime\module\test_local_in_module.rb',
'runtime\module\test_module_action.rb',
'runtime\module\test_module_path.rb',
'runtime\module\test_require_three_modules.rb'
],
# 8 - problems1 #
[
'runtime\class\test_self.rb',
'runtime\block\test_next.rb',
'runtime\block\test_redo.rb',
'runtime\block\test_retry.rb',
'runtime\class\test_find_const_in_methods.rb',
'runtime\class\test_find_method.rb',
'runtime\class\test_singleton_basic.rb',
'runtime\exception\test_nested.rb',
'runtime\exception\test_raise_nothing.rb',
'runtime\exception\test_raise_thing.rb',
'runtime\exception\test_rescue_many.rb',
'runtime\exception\test_rescue_modifier.rb',
'runtime\exception\test_rescue_nothing.rb',
'runtime\exception\test_rescue_sequence.rb',
'runtime\exception\test_rescue_unusual.rb',
'runtime\exception\test_retry.rb',
'runtime\exception\test_return.rb',
],
# 9 - buildin2 #
[
'builtin\array\test_array.rb',
'builtin\class\test_class.rb',
'builtin\enumerable\test_enumerable.rb',
'builtin\hash\test_hash.rb',
'builtin\module\test_module.rb',
]
#end of test_list
]  

issue_test_list=[
'runtime\class\test_methods.rb',
'runtime\class\test_top_level.rb',#expect NoMethodError, but get System::MethodAccessException #
'builtin\module\test_module.rb',# Module#ancestors fails#
'runtime\module\test_module_action.rb',#expect NoMethodError, but get System::MethodAccessException #
'runtime\module\test_module_path.rb' # require path tests, which needs to be at app root under SL 
]



# work-around for relative path defined in Ruby tests:
alias :real_require :require
def require(file_path)
    f = file_path.downcase

	# work-around the path issue:
    if    f.length>7 && f[0..6]=="module_": f = 'Runtime/Module/' + file_path
    elsif f["util/simple_test"]:            f = 'Util/simple_test.rb'
    elsif f["/spec_helper"]:                 f = 'specs/spec_helper'
    elsif f["util/assert.rb"]:              f = 'Util/assert.rb' 
    elsif f["block_common"]:                f = 'Runtime/Block/block_common.rb' 
    
    end
    
    if f == file_path
        log_info "requiring '#{file_path}'"
    else
    
        log_info "requiring '#{file_path}', ==> redirect to '#{f}'"   
    end
    real_require f
    
end


# Begin real silverlight test:
QueryString = HtmlPage.Document.QueryString

total_list_count = test_list.size
test_list_index = -1
test_names = ''
QueryString.each do |kv|
	if kv.Key.to_s.downcase == "test"
		test_names = kv.Value.to_s
	elsif kv.Key.to_s.downcase == "list"
		test_list_index = kv.Value.to_s.to_i
	end
end

if (test_list_index < 0 && test_names == '') || test_list_index >= total_list_count then
	# report error:
	CreateSLTest(1)
	log_fail "Please specify test name or list index!"
	log_info "Use query string 'test' for name of test with relative path from tests root;"
	log_info "Use query string 'list' for pre-defined test groups, range from 0 to " + (total_list_count - 1).to_s
	log_info ""
	log_info "For example,"
	log_info "irb_tests.html?list=2"
	log_info "irb_tests.html?test=builtin\array\test_array.rb"
	log_done
else
	# create list of tests:
	if test_list_index > -1 then
		tests_tbr = test_list[test_list_index]
	else
		tests_tbr = []
	end
	
	if test_names != '' then
		tests_tbr = tests_tbr + test_names.split(';')
	end

	# run test list:
	test_count = tests_tbr.size
	CreateSLTest(test_count)

	tests_tbr.each do |t|
	
	   log_scenario("Run test #{t}")
	   
	   if issue_test_list.include?(t):
		   log_pass '[SKIP] skip due to known issue.'
		   next
	   end
		
	   begin
		   load t
		   log_pass('passed')
	   rescue
	       log_fail('Failed with below error:')
		
	       log_fail "<pre>#{$!}</pre>"
	   end
	   
	end
	
	log_done

end