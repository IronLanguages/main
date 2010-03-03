class UnitTestSetup
  def initialize
    @name = "ActiveSupport"
    super
  end
  
  def gather_files
    gather_rails_files "3.0.pre"
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', '= 2.0.5'
    gem 'activesupport', '= 3.0.pre'
    require 'active_support/version'
  end

  def sanity
    # Do some sanity checks
    sanity_size(65)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/dependencies_test.rb")
    sanity_version('3.0.pre', ActiveSupport::VERSION::STRING)
  end

  def disable_mri_failures
    disable TimeZoneTest, 
      # <Sat Jan 01 05:00:00 UTC 2000> expected but was
      # <Sat Jan 01 08:00:00 UTC 2000>.
      # 
      # diff:
      # - Sat Jan 01 05:00:00 UTC 2000
      # ?             ^
      # + Sat Jan 01 08:00:00 UTC 2000
      # ?             ^
      :test_now,
      # <Sun Apr 02 03:00:00 UTC 2006> expected but was
      # <Sun Apr 02 05:00:00 UTC 2006>.
      # 
      # diff:
      # - Sun Apr 02 03:00:00 UTC 2006
      # ?             ^
      # + Sun Apr 02 05:00:00 UTC 2006
      # ?             ^
      :test_now_enforces_spring_dst_rules

    disable TimeWithZoneMethodsForTimeAndDateTimeTest, 
      # <Sat Jan 01 00:00:00 UTC 2000> expected but was
      # <Sat Jan 01 03:00:00 UTC 2000>.
      # 
      # diff:
      # - Sat Jan 01 00:00:00 UTC 2000
      # ?             ^
      # + Sat Jan 01 03:00:00 UTC 2000
      # ?             ^
      :test_current_returns_time_zone_now_when_zone_default_set,
      # <"Fri, 31 Dec 1999 15:00:00 AKST -09:00"> expected but was
      # <"Fri, 31 Dec 1999 18:00:00 AKST -09:00">.
      # 
      # diff:
      # - Fri, 31 Dec 1999 15:00:00 AKST -09:00
      # ?                   ^
      # + Fri, 31 Dec 1999 18:00:00 AKST -09:00
      # ?                   ^
      :test_in_time_zone_with_time_local_instance

    disable DependenciesTest, 
      # <false> is not true.
      :test_warnings_should_be_enabled_on_first_load

    disable TestJSONEncoding, 
      # <"\"2005-02-01T15:15:10-05:00\""> expected but was
      # <"\"2005-02-01T15:15:10-08:00\"">.
      # 
      # diff:
      # - "2005-02-01T15:15:10-05:00"
      # ?                       ^
      # + "2005-02-01T15:15:10-08:00"
      # ?                       ^
      :test_time_to_json_includes_local_offset

    disable DurationTest, 
      # <Sat Jan 01 00:00:05 UTC 2000> expected but was
      # <Sat Jan 01 03:00:05 UTC 2000>.
      # 
      # diff:
      # - Sat Jan 01 00:00:05 UTC 2000
      # ?             ^
      # + Sat Jan 01 03:00:05 UTC 2000
      # ?             ^
      :test_since_and_ago_anchored_to_time_zone_now_when_time_zone_default_set

    disable DateTimeExtCalculationsTest, 
      # <Fri, 31 Dec 1999 23:59:59 -0500> expected but was
      # <Fri, 31 Dec 1999 23:59:59 -0800>.
      # 
      # diff:
      # - Fri, 31 Dec 1999 23:59:59 -0500
      # ?                             ^
      # + Fri, 31 Dec 1999 23:59:59 -0800
      # ?                             ^
      :test_current_returns_date_today_when_zone_default_not_set,
      # <Fri, 31 Dec 1999 23:59:59 -0500> expected but was
      # <Sat, 01 Jan 2000 02:59:59 -0500>.
      :test_current_returns_time_zone_today_when_zone_default_set,
      # <Rational(-5, 24)> expected but was
      # <Rational(-1, 3)>.
      # 
      # diff:
      # - Rational(-5, 24)
      # ?           ^  ^^
      # + Rational(-1, 3)
      # ?           ^  ^
      :test_local_offset

    disable KernelTest, 
      # Errno::EBADF: Bad file descriptor
      :test_silence_stderr,
      # Errno::ENOENT: No such file or directory - /dev/null
      :test_silence_stderr_with_return_value

    disable DateExtCalculationsTest, 
      # <"1980-02-28T00:00:00-08:00"> expected to be =~
      # </^1980-02-28T00:00:00-05:?00$/>.
      :test_xmlschema

    disable TimeExtCalculationsTest, 
      # <"-05:00"> expected but was
      # <"-08:00">.
      # 
      # diff:
      # - -05:00
      # ?   ^
      # + -08:00
      # ?   ^
      :test_formatted_offset_with_local,
      # <true> expected but was
      # <false>.
      :test_future_with_time_current_as_time_local,
      # <false> expected but was
      # <true>.
      :test_future_with_time_current_as_time_with_zone,
      # <false> expected but was
      # <true>.
      :test_past_with_time_current_as_time_local,
      # <true> expected but was
      # <false>.
      :test_past_with_time_current_as_time_with_zone,
      # just after DST end.
      # <10801> expected but was
      # <7201.0>.
      :test_seconds_since_midnight_at_daylight_savings_time_end,
      # just after DST start.
      # <7201> expected but was
      # <10801.0>.
      :test_seconds_since_midnight_at_daylight_savings_time_start,
      # <Sun Apr 02 03:00:00 -0700 2006> expected but was
      # <Sun Apr 02 02:00:00 -0700 2006>.
      # 
      # diff:
      # - Sun Apr 02 03:00:00 -0700 2006
      # ?             ^
      # + Sun Apr 02 02:00:00 -0700 2006
      # ?             ^
      :test_time_created_with_local_constructor_cannot_represent_times_during_hour_skipped_by_dst,
      # <"Thu, 05 Feb 2009 14:30:05 -0600"> expected but was
      # <"Thu, 05 Feb 2009 14:30:05 -0800">.
      # 
      # diff:
      # - Thu, 05 Feb 2009 14:30:05 -0600
      # ?                             ^
      # + Thu, 05 Feb 2009 14:30:05 -0800
      # ?                             ^
      :test_to_s

    disable TimeWithZoneTest, 
      # <true> expected but was
      # <false>.
      :test_future_with_time_current_as_time_local,
      # <false> expected but was
      # <true>.
      :test_past_with_time_current_as_time_local

    disable AtomicWriteTest, 
      # <33206> expected but was
      # <33188>.
      :test_atomic_write_preserves_default_file_permissions,
      # <33261> expected but was
      # <33188>.
      :test_atomic_write_preserves_file_permissions

  end

  def disable_critical_failures
    # ArgumentError: wrong number of arguments (1 for 0)
    # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/activesupport/test/message_encryptor_test.rb:5:in `setup'
    disable MessageEncryptorTest, :setup
  end
  
  def disable_unstable_tests  
    disable BufferedLoggerTest,
      # <false> is not true.
      :test_should_create_the_log_directory_if_it_doesnt_exist

    # There seems to be a bug in 'active_support/core_ext/string/interpolation' which causes the finished() method in
    # test-unit-2.0.5\lib\test\unit\ui\console\testrunner.rb to throw an exception. finished() does '"%g%% passed" % pass_percentage'
    # whereas String::INTERPOLATION_PATTERN disallows %%. So we remove /%%/ from String::INTERPOLATION_PATTERN
    require 'active_support/core_ext/string/interpolation'
    String.const_set(:INTERPOLATION_PATTERN, Regexp.union(/%\{(\w+)\}/, /%<(\w+)>(.*?\d*\.?\d*[bBdiouxXeEfgGcps])/))  
    # The hack above causes this test to fail though
    disable TestGetTextString, :test_percent
  end
  
  def disable_tests

    disable ClassExtTest, 
      # <[#<Class:0x00056b6>]> expected but was
      # <[]>.
      :test_subclasses_of_doesnt_find_anonymous_classes,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/object_and_class_ext_test.rb:101:in `test_subclasses_of_with_multiple_roots'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_subclasses_of_with_multiple_roots

    disable DelegatingAttributesTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/class/delegating_attributes_test.rb:51:in `test_simple_accessor_declaration'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_simple_accessor_declaration,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/class/delegating_attributes_test.rb:59:in `test_simple_accessor_declaration_with_instance_reader_false'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_simple_accessor_declaration_with_instance_reader_false,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/class/delegating_attributes_test.rb:30:in `test_simple_reader_declaration'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_simple_reader_declaration

    disable DependenciesTest, 
      # NameError: constant Object::Hello not defined
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ModuleOps.cs:793:in `remove_const'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/dependencies_test.rb:529:in `test_new_constants_in_with_a_single_constant'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/dependencies_test.rb:529:in `test_new_constants_in_with_a_single_constant'
      :test_new_constants_in_with_a_single_constant,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/dependencies_test.rb:536:in `test_new_constants_in_with_nesting'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/dependencies_test.rb:533:in `test_new_constants_in_with_nesting'
      # dependencies.rb:506:in `new_constants_in'
      :test_new_constants_in_with_nesting

    disable DurationTest, 
      # <Mon Mar 30 00:00:00 -0700 2009> expected but was
      # <Mon Mar 30 01:00:00 -0700 2009>.
      # 
      # diff:
      # - Mon Mar 30 00:00:00 -0700 2009
      # ?             ^
      # + Mon Mar 30 01:00:00 -0700 2009
      # ?             ^
      :test_adding_hours_across_dst_boundary

    disable EnumerableTests, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/enumerable_test.rb:26:in `test_group_by'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_group_by,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/enumerable_test.rb:38:in `test_sums'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_sums

    disable HashExtTest, 
      # <{0=>1, 1=>2}> expected but was
      # <{0=>1, :disabled=>2}>.
      :test_symbolize_keys_preserves_fixnum_keys

    disable MessageEncryptorTest, 
      # NoMethodError: undefined method `encrypt' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/whiny_nil.rb:52:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/message_encryptor_test.rb:16:in `test_encrypting_twice_yields_differing_cipher_text'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_encrypting_twice_yields_differing_cipher_text,
      # NoMethodError: undefined method `encrypt' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/whiny_nil.rb:52:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/message_encryptor_test.rb:22:in `test_messing_with_either_value_causes_failure'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_messing_with_either_value_causes_failure,
      # NoMethodError: undefined method `encrypt_and_sign' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/whiny_nil.rb:52:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/message_encryptor_test.rb:30:in `test_signed_round_tripping'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_signed_round_tripping,
      # NoMethodError: undefined method `encrypt' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/whiny_nil.rb:52:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/message_encryptor_test.rb:11:in `test_simple_round_tripping'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_simple_round_tripping

    disable ModuleTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/module_test.rb:171:in `test_local_constants'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_local_constants

    disable MultibyteCharsExtrasTest, 
      # <"Абвг абвг"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a058 @wrapped_string="Абвг абвг">>.
      :test_capitalize_should_be_unicode_aware,
      # <"абвгд\000f"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a0f2 @wrapped_string="абвгд\000f">>.
      :test_downcase_should_be_unicode_aware,
      # <"こにちわ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a1ac @wrapped_string="こ">>.
      :test_limit_should_work_on_a_multibyte_string,
      # ArgumentError: invalid utf-8 character
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:652:in `tidy_bytes'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:649:in `tidy_bytes'
      :test_tidy_bytes_should_tidy_bytes,
      # <"АБВГД\000F"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a362 @wrapped_string="АБВГД\000F">>.
      :test_upcase_should_be_unicode_aware

    disable MultibyteCharsUTF8BehaviourTest, 
      # <"こにちわ "> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a564
      #  @wrapped_string="こ\343 \201\253ちわ">>.
      # 
      # diff:
      # - "こにちわ "
      # + #<ActiveSupport::Multibyte::Chars:0x1a564
      # +  @wrapped_string="こ\343 \201\253ちわ">
      :test_center_should_count_charactes_instead_of_bytes,
      # ActiveSupport::Multibyte::EncodingError: malformed UTF-8 character
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:203:in `index'
      # chars.rb:483:in `u_unpack'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/multibyte_chars_test.rb:240:in `test_index_should_return_character_offset'
      :test_index_should_return_character_offset,
      # <"こに わ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a650 @wrapped_string="こに わ">>.
      :test_indexed_insert_accepts_fixnums,
      # <IndexError> exception expected but was
      # Class: <TypeError>
      # Message: <"can't convert Regexp into Fixnum">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:237:in `[]='
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/mu
      :test_indexed_insert_should_raise_on_index_overflow,
      # <"こにaわ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a740 @wrapped_string="こにaわ">>.
      :test_indexed_insert_should_take_character_offsets,
      # <"こわにちわ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a7cc @wrapped_string="こわにちわ">>.
      :test_insert_should_be_destructive,
      # RangeError: Non-negative number required.
      # Parameter name: length
      # mscorlib:0:in `Copy'
      # mscorlib:0:in `Copy'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:1836:in `insert'
      :test_ljust_should_count_characters_instead_of_bytes,
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: ASCII-8BIT and utf-8
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:402:in `+'
      # output_safety.rb:16:in `+'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/multibyte_chars_test.rb:353:in `test_lstrip_strips_whitespace_from_the_left_of_the_string'
      :test_lstrip_strips_whitespace_from_the_left_of_the_string,
      # <"Òu"> expected but was
      # <"Òu">.
      :test_overridden_bang_methods_change_wrapped_string,
      # <"わちにこ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1a98e @wrapped_string="わちにこ">>.
      :test_reverse_reverses_characters,
      # ActiveSupport::Multibyte::EncodingError: malformed UTF-8 character
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:217:in `rindex'
      # chars.rb:483:in `u_unpack'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/multibyte_chars_test.rb:249:in `test_rindex_should_return_character_offset'
      :test_rindex_should_return_character_offset,
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: utf-8 and ASCII-8BIT
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:1836:in `insert'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:688:in `justify'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:270:in `rjust'
      :test_rjust_should_count_characters_instead_of_bytes,
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: utf-8 and ASCII-8BIT
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:402:in `+'
      # output_safety.rb:16:in `+'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/multibyte_chars_test.rb:359:in `test_rstrip_strips_whitespace_from_the_right_of_the_string'
      :test_rstrip_strips_whitespace_from_the_right_of_the_string,
      # RangeError: Count must be positive and count must refer to a location within the string/array/collection.
      # Parameter name: count
      # mscorlib:0:in `IndexOf'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:1821:in `include?'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/chars.rb:192:in `include?'
      :test_should_know_if_one_includes_the_other,
      # <"こわにちわ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1ab32 @wrapped_string="こわにちわ">>.
      :test_should_use_character_offsets_for_insert_offsets,
      # <"こわ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1abe0 @wrapped_string="こわ">>.
      :test_slice_bang_removes_the_slice_from_the_receiver,
      # <"にち"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1ac70 @wrapped_string="にち">>.
      :test_slice_bang_returns_sliced_out_substring,
      # <"こ"> expected but was
      # <#<ActiveSupport::Multibyte::Chars:0x1ad04 @wrapped_string="こ">>.
      :test_slice_should_take_character_offsets,
      # <false> is not true.
      :test_split_should_return_an_array_of_chars_instances,
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: ASCII-8BIT and utf-8
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:402:in `+'
      # output_safety.rb:16:in `+'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/multibyte_chars_test.rb:365:in `test_strip_strips_whitespace'
      :test_strip_strips_whitespace,
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: utf-8 and ASCII-8BIT
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\MutableStringOps.cs:402:in `+'
      # output_safety.rb:16:in `+'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/multibyte_chars_test.rb:371:in `test_stripping_whitespace_leaves_whitespace_within_the_string_intact'
      :test_stripping_whitespace_leaves_whitespace_within_the_string_intact

    disable MultibyteUtilsTest, 
      # ArgumentError: invalid shift_jis character
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:249:in `==='
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/utils.rb:53:in `clean'
      "test_clean_cleans_invalid_characters_from_Shift-JIS_encoded_strings",
      # ArgumentError: invalid utf-8 character
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:249:in `==='
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/utils.rb:53:in `clean'
      "test_clean_cleans_invalid_characters_from_UTF-8_encoded_strings",
      # Exception raised:
      # Class: <ArgumentError>
      # Message: <"invalid utf-8 character">
      # ---Backtrace---
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/utils.rb:30:in `verify'
      # d:\vs_langs01_s\Merlin\M
      "test_verify!_doesn't_raise_an_exception_when_the_encoding_is_valid",
      # <ActiveSupport::Multibyte::EncodingError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"invalid utf-8 character">
      # ---Backtrace---
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multiby
      "test_verify!_raises_an_exception_when_it_finds_an_invalid_character",
      # ArgumentError: invalid shift_jis character
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/utils.rb:30:in `verify'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:54:in `<TrueForItems>b__0'
      "test_verify_verifies_Shift-JIS_strings_are_properly_encoded",
      # ArgumentError: invalid utf-8 character
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/multibyte/utils.rb:30:in `verify'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:54:in `<TrueForItems>b__0'
      "test_verify_verifies_UTF-8_strings_are_properly_encoded"

    disable Notifications::InstrumentationTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:23:in `drain'
      :test_event_is_pushed_even_without_block,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:23:in `drain'
      :test_instrument_publishes_when_exception_is_raised,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:23:in `drain'
      :test_nested_events_can_be_instrumented

    disable Notifications::PubSubTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:30:in `test_events_are_published_to_a_listener'
      :test_events_are_published_to_a_listener,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:62:in `test_multiple_subscribers'
      :test_multiple_subscribers,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:41:in `test_subscriber_with_pattern'
      :test_subscriber_with_pattern,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications/fanout.rb:27:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/notifications.rb:68:in `wait'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/notifications_test.rb:53:in `test_subscriber_with_pattern_as_regexp'
      :test_subscriber_with_pattern_as_regexp

    disable ObjectInstanceVariableTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/core_ext/object/instance_variables.rb:64:in `copy_instance_variables_from'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/object_and_class_ext_test.rb:210:in `test_copy_instance_variables_automatically_excludes_protected_instance_variables'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      :test_copy_instance_variables_automatically_excludes_protected_instance_variables,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/core_ext/object/instance_variables.rb:64:in `copy_instance_variables_from'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/object_and_class_ext_test.rb:197:in `test_copy_instance_variables_from_with_explicit_excludes'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_copy_instance_variables_from_with_explicit_excludes,
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/core_ext/object/instance_variables.rb:64:in `copy_instance_variables_from'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/core_ext/object_and_class_ext_test.rb:187:in `test_copy_instance_variables_from_without_explicit_excludes'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_copy_instance_variables_from_without_explicit_excludes

    disable OrderedHashTest, 
      # <false> is not true.
      :test_inspect

    disable SetupAndTeardownTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/test_test.rb:105:in `test_inherited_setup_callbacks'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_inherited_setup_callbacks

    disable SubclassSetupAndTeardownTest, 
      # TypeError: can't convert Symbol into Proc
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/ActiveSupport/test_test.rb:136:in `test_inherited_setup_callbacks'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:780:in `__send__'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_inherited_setup_callbacks

    disable TestGetTextString, 
      # <KeyError> exception expected but was
      # Class: <System::NotSupportedException>
      # Message: <"Can't inherit from a sealed type.">
      # ---Backtrace---
      # d:\vs_langs01_s\ndp\fx\src\Core\Microsoft\Scripting\Actions\DynamicMetaObjectBinder.cs:107:in `Bind'
      # d:\vs_langs01_s\ndp\fx\src\Core\Microsoft\Scripting\Actions\CallSiteBinder.cs:121:in `BindCore'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRIC
      :test_sprintf_lack_argument

    disable TestJSONDecoding, 
      # Exception raised:
      # Class: <StandardError>
      # Message: <"Invalid JSON string">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-3.0.pre/lib/active_support/json/backends/yaml.rb:17:in `decode'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:812:in `__send__'
      # d:1:in `decode'
      # decoding_
      "test_json_decodes_{\"matzue\":_\"\346\235\276\346\261\237\",_\"asakusa\":_\"\346\265\205\350\215\211\"}_with_the_Yaml_backend"

    disable TimeExtCalculationsTest, 
      # st-24.hours=>dt.
      # <Sat Oct 29 05:03:00 -0700 2005> expected but was
      # <Sat Oct 29 04:03:00 -0700 2005>.
      # 
      # diff:
      # - Sat Oct 29 05:03:00 -0700 2005
      # ?             ^
      # + Sat Oct 29 04:03:00 -0700 2005
      # ?             ^
      :test_daylight_savings_time_crossings_backward_end,
      # dt-24.hours=>st.
      # <Sat Apr 02 03:18:00 -0700 2005> expected but was
      # <Sat Apr 02 04:18:00 -0700 2005>.
      # 
      # diff:
      # - Sat Apr 02 03:18:00 -0700 2005
      # ?             ^
      # + Sat Apr 02 04:18:00 -0700 2005
      # ?             ^
      :test_daylight_savings_time_crossings_backward_start,
      # dt+24.hours=>st.
      # <Sun Oct 30 23:45:00 -0700 2005> expected but was
      # <Mon Oct 31 00:45:00 -0700 2005>.
      # 
      # diff:
      # - Sun Oct 30 23:45:00 -0700 2005
      # ? ^^        ^^^
      # + Mon Oct 31 00:45:00 -0700 2005
      # ? ^^       ++ ^
      :test_daylight_savings_time_crossings_forward_end,
      # st+24.hours=>dt.
      # <Sun Apr 03 20:27:00 -0700 2005> expected but was
      # <Sun Apr 03 19:27:00 -0700 2005>.
      # 
      # diff:
      # - Sun Apr 03 20:27:00 -0700 2005
      # ?            ^^
      # + Sun Apr 03 19:27:00 -0700 2005
      # ?            ^^
      :test_daylight_savings_time_crossings_forward_start

    disable TimeWithZoneTest, 
      # ArgumentError: invalid date
      # date.rb:1479:in `civil'
      # mscorlib:0:in `_InvokeMethodFast'
      # mscorlib:0:in `InvokeMethodFast'
      :test_change

    disable TimeZoneTest, 
      # <Fri Dec 31 19:00:00 UTC 1999> expected but was
      # <Fri Feb 19 19:00:00 UTC 2010>.
      # 
      # diff:
      # - Fri Dec 31 19:00:00 UTC 1999
      # ?     ^ ^^^                ^^^
      # + Fri Feb 19 19:00:00 UTC 2010
      # ?     ^ ^^ +              ++ ^
      :test_parse_with_incomplete_date


  end
end
