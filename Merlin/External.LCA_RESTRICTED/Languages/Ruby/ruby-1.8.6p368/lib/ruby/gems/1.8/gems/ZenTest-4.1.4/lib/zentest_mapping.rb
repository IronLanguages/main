##
# ZenTestMapping - mapping method names from impl to test.
#
# Method names are mapped bidirectionally in the following way:
#
#   method      test_method
#   method?     test_method_eh          (too much exposure to Canadians :)
#   method!     test_method_bang
#   method=     test_method_equals
#   []          test_index
#   *           test_times
#   ==          test_equals2
#   ===         test_equals3
#
# Further, any of the test methods should be able to have arbitrary
# extensions put on the name to distinguish edge cases:
#
#   method      test_method
#   method      test_method_simple
#   method      test_method_no_network
#
# To allow for unmapped test methods (ie, non-unit tests), name them:
#
#   test_integration_.*

module ZenTestMapping

  @@orig_method_map = {
    '!'   => 'bang',
    '%'   => 'percent',
    '&'   => 'and',
    '*'   => 'times',
    '**'  => 'times2',
    '+'   => 'plus',
    '-'   => 'minus',
    '/'   => 'div',
    '<'   => 'lt',
    '<='  => 'lte',
    '<=>' => 'spaceship',
    "<\<" => 'lt2',
    '=='  => 'equals2',
    '===' => 'equals3',
    '=~'  => 'equalstilde',
    '>'   => 'gt',
    '>='  => 'ge',
    '>>'  => 'gt2',
    '+@'  => 'unary_plus',
    '-@'  => 'unary_minus',
    '[]'  => 'index',
    '[]=' => 'index_equals',
    '^'   => 'carat',
    '|'   => 'or',
    '~'   => 'tilde',
  }

  @@method_map = @@orig_method_map.merge(@@orig_method_map.invert)

  @@mapped_re = @@orig_method_map.values.sort_by { |k| k.length }.map {|s|
    Regexp.escape(s)
  }.reverse.join("|")

  def munge name
    name = name.to_s.dup

    is_cls_method = name.sub!(/^self\./, '')

    name = @@method_map[name] if @@method_map.has_key? name
    name = name.sub(/=$/, '_equals')
    name = name.sub(/\?$/, '_eh')
    name = name.sub(/\!$/, '_bang')

    name = yield name if block_given?

    name = "class_" + name if is_cls_method

    name
  end

  # Generates a test method name from a normal method,
  # taking into account names composed of metacharacters
  # (used for arithmetic, etc
  def normal_to_test name
    "test_#{munge name}"
  end

  def unmunge name
    name = name.to_s.dup

    is_cls_method = name.sub!(/^class_/, '')

    name = name.sub(/_equals(_.*)?$/, '=') unless name =~ /index/
    name = name.sub(/_bang(_.*)?$/, '!')
    name = name.sub(/_eh(_.*)?$/, '?')
    name = name.sub(/^(#{@@mapped_re})(_.*)?$/) {$1}
    name = yield name if block_given?
    name = @@method_map[name] if @@method_map.has_key? name
    name = 'self.' + name if is_cls_method

    name
  end

  # Converts a method name beginning with test to its
  # corresponding normal method name, taking into account
  # symbolic names which may have been anglicised by
  # #normal_to_test().
  def test_to_normal(name, klassname=nil)
    unmunge(name.to_s.sub(/^test_/, '')) do |n|
      if defined? @inherited_methods then
        known_methods = (@inherited_methods[klassname] || {}).keys.sort.reverse
        known_methods_re = known_methods.map {|s| Regexp.escape(s) }.join("|")
        n = n.sub(/^(#{known_methods_re})(_.*)?$/) { $1 } unless
          known_methods_re.empty?
        n
      end
    end
  end
end
