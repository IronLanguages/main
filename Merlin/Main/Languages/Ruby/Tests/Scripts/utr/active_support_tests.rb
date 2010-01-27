class UnitTestSetup
  def initialize
    @name = "ActiveSupport"
    super
  end
  
  def gather_files
    gather_rails_files
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'activesupport', "= 2.3.5"
    require 'active_support/version'
  end

  def sanity
    # Do some sanity checks
    sanity_size(65)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/test/dependencies_test.rb")
    sanity_version('2.3.5', ActiveSupport::VERSION::STRING)
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
    # Bug 3466 - IronRuby causes an exception while printing the failure information of some tests with
    # non-ASCII names. So we disable them programatically
    TestJSONDecoding.class_eval do
      tests_with_bad_names = TestJSONDecoding.instance_methods(false).select { |m| m.to_s =~ /matzue/x }
      tests_with_bad_names.each { |e| undef_method(e.to_s) }
    end

    # ArgumentError: wrong number of arguments (1 for 0)
    # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activesupport/test/message_encryptor_test.rb:5:in `setup'
    disable MessageEncryptorTest, :setup
  end
  
  def disable_tests
    disable BufferedLoggerTest, 
      # <false> is not true.
      :test_should_create_the_log_directory_if_it_doesnt_exist

    disable ClassExtTest, 
      # <[#<Class:0x0005776>]> expected but was
      # <[]>.
      :test_subclasses_of_doesnt_find_anonymous_classes

    disable HashExtTest, 
      # <{0=>1, 1=>2}> expected but was
      # <{0=>1, :UnitTestRunner=>2}>.
      :test_symbolize_keys_preserves_fixnum_keys

    disable MessageEncryptorTest, 
      # NoMethodError: undefined method `encrypt' for nil:NilClass
      :test_encrypting_twice_yields_differing_cipher_text,
      # NoMethodError: undefined method `encrypt' for nil:NilClass
      :test_messing_with_either_value_causes_failure,
      # NoMethodError: undefined method `encrypt_and_sign' for nil:NilClass
      :test_signed_round_tripping,
      # NoMethodError: undefined method `encrypt' for nil:NilClass
      :test_simple_round_tripping

    disable MessageVerifierTest, 
      # <{:some=>"data", :now=>Fri Jan 22 10:43:59 -0800 2010}> expected but was
      # <{:some=>"data", :now=>Fri Jan 22 10:43:43 -0800 2010}>.
      # 
      # diff:
      # - {:some=>"data", :now=>Fri Jan 22 10:43:59 -0800 2010}
      # ?                                        ^^
      # + {:some=>"data", :now=>Fri Jan 22 10:43:43 -0800 2010}
      # ?                                        ^^
      :test_simple_round_tripping

    disable MultibyteCharsExtrasTest, 
      # ArgumentError: invalid utf-8 character
      :test_tidy_bytes_should_tidy_bytes

    disable MultibyteCharsUTF8BehaviourTest, 
      # IndexError: Index was outside the bounds of the array.
      :test_center_should_count_charactes_instead_of_bytes,
      # ActiveSupport::Multibyte::EncodingError: malformed UTF-8 character
      :test_index_should_return_character_offset,
      # <IndexError> exception expected but was
      # Class: <TypeError>
      # Message: <"can't convert Regexp into Fixnum">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.5/lib/active_support/multibyte/chars.rb:235:in `[]='
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/ActiveSupport/test/m
      :test_indexed_insert_should_raise_on_index_overflow,
      # IndexError: Index was outside the bounds of the array.
      :test_indexed_insert_should_take_character_offsets,
      # RangeError: Non-negative number required.
      # Parameter name: length
      :test_ljust_should_count_characters_instead_of_bytes,
      # ActiveSupport::Multibyte::EncodingError: malformed UTF-8 character
      :test_rindex_should_return_character_offset,
      # RangeError: Count must be positive and count must refer to a location within the string/array/collection.
      # Parameter name: count
      :test_should_know_if_one_includes_the_other,
      # <false> is not true.
      :test_split_should_return_an_array_of_chars_instances

    disable MultibyteUtilsTest, 
      # ArgumentError: invalid shift_jis character
      "test_clean_cleans_invalid_characters_from_Shift-JIS_encoded_strings",
      # ArgumentError: invalid utf-8 character
      "test_clean_cleans_invalid_characters_from_UTF-8_encoded_strings",
      # Exception raised:
      # Class: <ArgumentError>
      # Message: <"invalid utf-8 character">
      # ---Backtrace---
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.5/lib/active_support/multibyte/utils.rb:30:in `verify'
      # d:/vs_langs01_s/Merlin/Ext
      "test_verify!_doesn't_raise_an_exception_when_the_encoding_is_valid",
      # <ActiveSupport::Multibyte::EncodingError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"invalid utf-8 character">
      # ---Backtrace---
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\RubyRegexOps.cs:222:in `match'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.5/lib/active_support/multibyte
      "test_verify!_raises_an_exception_when_it_finds_an_invalid_character",
      # ArgumentError: invalid shift_jis character
      "test_verify_verifies_Shift-JIS_strings_are_properly_encoded",
      # ArgumentError: invalid utf-8 character
      "test_verify_verifies_UTF-8_strings_are_properly_encoded"

    disable OrderedHashTest, 
      # <false> is not true.
      :test_inspect

    disable TestJSONDecoding,
      # <StandardError> exception expected but was
      # Class: <IronRuby::StandardLibrary::Yaml::ParserException>
      # Message: <"while scanning a flow node: expected the node content, but found: #<ValueToken>">
      :test_failed_json_decoding
      
    disable TestJSONEncoding, 
      # <"\"\\u20ac2.99\""> expected but was
      # <"\"€2.99\"">.
      :test_utf8_string_encoded_properly_when_kcode_is_utf8

    disable TimeWithZoneTest, 
      # ArgumentError: invalid date
      :test_change,
      # NameError: uninitialized constant YAML::Emitter
      :test_ruby_to_yaml,
      # NameError: uninitialized constant YAML::Emitter
      :test_to_yaml

    disable TimeZoneTest, 
      # <Fri Dec 31 19:00:00 UTC 1999> expected but was
      # <Fri Jan 22 19:00:00 UTC 2010>.
      :test_parse_with_incomplete_date

  end
end
