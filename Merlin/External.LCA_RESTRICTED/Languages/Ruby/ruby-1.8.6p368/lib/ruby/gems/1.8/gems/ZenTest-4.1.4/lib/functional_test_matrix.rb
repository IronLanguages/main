
########################################################################
# The Idea:
#
# This is supposed to get us thinking about the various dimensions our
# testing should address. If there are states orthogonal to each other
# (eg. readable vs unreadable, logged in vs not logged in) each of
# those states should comprise a dimension in the matrix. By
# addressing it this way, we should be able to minimize the amount of
# setup/teardown code and get full coverage across our actions for all
# these edge cases and as a result have extremely clear tests.
#
########################################################################
# Example Test Matrix Specification:
#
# matrix :example, :edge1, :edge2, :edge3, ...
# action :action1, :OK,    :e_NF,  :mod,   ...
# action :action2, :OK,    :e_RO,  :na,    ...
# action ...
#
########################################################################
# Matrix:
#
# I envision the setups being a code that combines the different
# dimensions of edge case state.
#
# Something for a CMS might look like: `[df]_[ugo]_[rRwW]` where:
#
# + `[df]` for dir/file.
# + and the rest is in the style of symbolic args to chmod:
#   + u/g/o = user, group, or other
#   + lowercase `X` == `X`able, uppercase `X` == un`X`able, where `X`
#     is read/write.
#
########################################################################
# Action:
#
# :new/:err/:del are just examples, they should have semantic info
# attached to them.
#
# Use :na to specify an inapplicable edge case for that action.
#
# Use :OK to specify the standard positive state. It is equivalent to
# a result with the same name as the action. (eg
# matrix_test_index). This cleans up the matrix a lot and allows for
# narrower and more readable columns.
#
# Edge cases specific to an action that fall outside the matrix are
# regular tests.
#
########################################################################
# Matrix Methods (the legos):
#
# Everything else basically equates to lego pieces:
#
# + There is one "init" method per matrix: matrix_init_#{descr}(setup_args)
# + There is one "setup" method per action: matrix_setup_#{action}(setup, expect)
# + There is one "test" method per result: matrix_test_#{result}(setup)
#
# Thus, for the matrix "example" above, the top left-most test will
# end up calling:
#
#     matrix_init_example(:edge1)
#     matrix_setup_action1(:edge1, :new)
#     matrix_test_new(:edge1)
#
# Read the action method for exact details.
########################################################################

module FunctionalTestMatrix
  def matrix(name, *setups)
    @@matrix, @@setups = name, setups
  end

  def action(action, *results)
    testcases = @@setups.zip(results).reject { |a,b| b == :na }
    testcases = Hash[*testcases.flatten]
    matrix = @@matrix # bind to local scope for define_method closure

    testcases.each do |setup, expected|
      expected_action = expected == :OK ? action : expected
      define_method "test_#{matrix}_#{action}_#{setup}" do
        @action = action
        send "matrix_init_#{matrix}", *setup.to_s.split(/_/).map {|c| c.intern }
        send "matrix_setup_#{action}", setup, expected
        send "matrix_test_#{expected_action}", setup
      end
    end
  end

  module_function :matrix, :action
end
