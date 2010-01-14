class UnitTestSetup
  def initialize
    @name = "ActiveSupport"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'activesupport', "= 2.3.3"
    require 'active_support/version'
  end

  def gather_files
    @root_dir = File.expand_path '..\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-2.3.3\activesupport', ENV['MERLIN_ROOT']
    $LOAD_PATH << @root_dir + '/test'
    @all_test_files = Dir.glob("#{@root_dir}/test/**/*_test.rb").sort
  end

  def sanity
    # Do some sanity checks
    sanity_size(65)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/test/dependencies_test.rb")
    sanity_version('2.3.3', ActiveSupport::VERSION::STRING)
  end

  def disable_tests
    # <false> is not true.
    disable BufferedLoggerTest, :test_should_create_the_log_directory_if_it_doesnt_exist

    disable MessageEncryptorTest, :setup
    # NameError: uninitialized constant OpenSSL::Cipher
    disable MessageEncryptorTest, :test_encrypting_twice_yields_differing_cipher_text
     
    # <Fri, 31 Dec 1999 23:59:59 -0500> expected but was
    # <Sat, 01 Jan 2000 02:59:59 -0500>.
    disable DateTimeExtCalculationsTest, :test_current_returns_time_zone_today_when_zone_default_set
     
    # <33206> expected but was
    # <33024>.
    disable AtomicWriteTest, :test_atomic_write_preserves_default_file_permissions

    # <33261> expected but was
    # <33152>.
    disable AtomicWriteTest, :test_atomic_write_preserves_file_permissions


    # <[#<Class:0x0004f08>]> expected but was
    # <[]>.
    disable ClassExtTest, :test_subclasses_of_doesnt_find_anonymous_classes

    # <"1980-02-28T00:00:00-08:00"> expected to be =~
    # </^1980-02-28T00:00:00-05:?00$/>.
    disable DateExtCalculationsTest, :test_xmlschema

    # <Fri, 31 Dec 1999 23:59:59 -0500> expected but was
    # <Fri, 31 Dec 1999 23:59:59 -0800>.
    disable DateTimeExtCalculationsTest, :test_current_returns_date_today_when_zone_default_not_set


    # <"+00:00"> expected but was
    # <"000:00">.
    disable DateTimeExtCalculationsTest, :test_formatted_offset_with_utc

    # <Rational(-5, 24)> expected but was
    # <Rational(-1, 3)>.
    disable DateTimeExtCalculationsTest, :test_local_offset

    # <"Mon, 21 Feb 2005 14:30:00 +0000"> expected but was
    # <"Mon, 21 Feb 2005 14:30:00 00000">.
    disable DateTimeExtCalculationsTest, :test_readable_inspect

    # <"Mon, 21 Feb 2005 14:30:00 +0000"> expected but was
    # <"Mon, 21 Feb 2005 14:30:00 00000">.
    disable DateTimeExtCalculationsTest, :test_to_s


    # <false> is not true.
    disable DependenciesTest, :test_warnings_should_be_enabled_on_first_load

    # <Sat Jan 01 00:00:05 Z 2000> expected but was
    # <Sat Jan 01 03:00:05 Z 2000>.
    disable DurationTest, :test_since_and_ago_anchored_to_time_zone_now_when_time_zone_default_set

    # <{0=>1, 1=>2}> expected but was
    # <{0=>1, :f=>2}>.
    disable HashExtTest, :test_symbolize_keys_preserves_fixnum_keys

    # NameError: uninitialized constant Errno::ESPIPE
    disable KernelTest, :test_silence_stderr

    # Errno::EINVAL: Invalid argument - FileStream will not open Win32 devices such as disk partitions and tape drives. Avoid use of "\\.\" in the path.
    disable KernelTest, :test_silence_stderr_with_return_value



    # ArgumentError: wrong number of arguments (1 for 0)
    disable MessageEncryptorTest, :test_messing_with_either_value_causes_failure

    # ArgumentError: wrong number of arguments (1 for 0)
    disable MessageEncryptorTest, :test_signed_round_tripping

    # ArgumentError: wrong number of arguments (1 for 0)
    disable MessageEncryptorTest, :test_simple_round_tripping


    # <{:some=>"data", :now=>Fri Dec 04 14:52:04 -08:00 2009}> expected but was
    # <{:some=>"data", :now=>Fri Dec 04 14:36:04 -08:00 2009}>.
    disable MessageVerifierTest, :test_simple_round_tripping

    # <"-É-¦-¦-¦ -¦-¦-¦-¦"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x18eae
    #  @wrapped_string=
    #   "\320\220\320\261\320\262\320\263 \320\260\320\261\320\262\320\263">>.
    disable MultibyteCharsExtrasTest, :test_capitalize_should_be_unicode_aware

    # <"-¦-¦-¦-¦-¦\000f"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x18eec
    #  @wrapped_string="\320\260\320\261\320\262\320\263\320\264\000f">>.
    disable MultibyteCharsExtrasTest, :test_downcase_should_be_unicode_aware

    # ArgumentError: invalid utf-8 character
    disable MultibyteCharsExtrasTest, :test_tidy_bytes_should_tidy_bytes

    # <"-É-æ-Æ-ô-ö\000F"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x18fda
    #  @wrapped_string="\320\220\320\221\320\222\320\223\320\224\000F">>.
    disable MultibyteCharsExtrasTest, :test_upcase_should_be_unicode_aware


    # <"püôpü½püípéÅ "> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19130
    #  @wrapped_string="\343\201\223\343 \201\253\343\201\241\343\202\217">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_center_should_count_charactes_instead_of_bytes

    # ActiveSupport::Multibyte::EncodingError: malformed UTF-8 character
    disable MultibyteCharsUTF8BehaviourTest, :test_index_should_return_character_offset

    # <"püôpü½ péÅ"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x191b2
    #  @wrapped_string="\343\201\223\343\201\253 \343\202\217">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_indexed_insert_accepts_fixnums

    # <IndexError> exception expected but was
    # Class: <TypeError>
    # Message: <"can't convert Regexp into Fixnum">
    # ---Backtrace---
    # C:/Users/jdeville/projects/jredville/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.3/lib/active_support/multibyte/chars.rb:229:in `[]='
    # C:/Users/jdeville/projects/jredville/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/R
    disable MultibyteCharsUTF8BehaviourTest, :test_indexed_insert_should_raise_on_index_overflow

    # <"püôpü½apéÅ"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19234
    #  @wrapped_string="\343\201\223\343\201\253a\343\202\217">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_indexed_insert_should_take_character_offsets

    # <"püôpéÅpü½püípéÅ"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19264
    #  @wrapped_string=
    #   "\343\201\223\343\202\217\343\201\253\343\201\241\343\202\217">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_insert_should_be_destructive

    # RangeError: Non-negative number required.
    # Parameter name: length
    disable MultibyteCharsUTF8BehaviourTest, :test_ljust_should_count_characters_instead_of_bytes

    # <"péÅpüípü½püô"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19374
    #  @wrapped_string="\343\202\217\343\201\241\343\201\253\343\201\223">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_reverse_reverses_characters

    # RangeError: Count must be positive and count must refer to a location within the string/array/collection.
    # Parameter name: count
    disable MultibyteCharsUTF8BehaviourTest, :test_should_know_if_one_includes_the_other

    # <"püôpéÅpü½püípéÅ"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19450
    #  @wrapped_string=
    #   "\343\201\223\343\202\217\343\201\253\343\201\241\343\202\217">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_should_use_character_offsets_for_insert_offsets

    # <"püôpéÅ"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19498
    #  @wrapped_string="\343\201\223\343\202\217">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_slice_bang_removes_the_slice_from_the_receiver

    # <"pü½püí"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x194cc
    #  @wrapped_string="\343\201\253\343\201\241">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_slice_bang_returns_sliced_out_substring

    # <"püô"> expected but was
    # <#<ActiveSupport::Multibyte::Chars:0x19502 @wrapped_string="\343\201\223">>.
    disable MultibyteCharsUTF8BehaviourTest, :test_slice_should_take_character_offsets

    # <false> is not true.
    disable MultibyteCharsUTF8BehaviourTest, :test_split_should_return_an_array_of_chars_instances


    # <"+00:00"> expected but was
    # <"000:00">.
    disable NumericExtConversionsTest, :test_to_utc_offset_s_with_colon

    # <"+0000"> expected but was
    # <"00000">.
    disable NumericExtConversionsTest, :test_to_utc_offset_s_without_colon


    # <false> is not true.
    disable OrderedHashTest, :test_inspect

    # NoMethodError: You have a nil object when you didn't expect it!
    # The error occurred while evaluating nil.message
    disable RescueableTest, :test_rescue_from_with_block_with_args

    # <ActiveSupport::JSON::ParseError> exception expected but was
    # Class: <IronRuby::StandardLibrary::Yaml::ParserException>
    # Message: <"while scanning a flow node: expected the node content, but found: #<ValueToken>">
    # ---Backtrace---
    # c:\Users\jdeville\projects\jredville\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\Yaml\IronRuby.Libraries.Yaml\Engine\Parser.cs:251:in `Produce'
    # c:\Users\jdeville\proje
    disable TestJSONDecoding, :test_failed_json_decoding

    # <"\"2005/02/01 15:15:10 +0000\""> expected but was
    # <"\"2005/02/01 15:15:10 00000\"">.
    disable TestJSONEncoding, :test_time

    # <"\"\\u20ac2.99\""> expected but was
    # <"\"\342\202\2542.99\"">.
    disable TestJSONEncoding, :test_utf8_string_encoded_properly_when_kcode_is_utf8

    # <"-05:00"> expected but was
    # <"-08:00">.
    disable TimeExtCalculationsTest, :test_formatted_offset_with_local

    # <"+00:00"> expected but was
    # <"000:00">.
    disable TimeExtCalculationsTest, :test_formatted_offset_with_utc

    # <false> expected but was
    # <true>.
    disable TimeExtCalculationsTest, :test_future_with_time_current_as_time_local

    # <true> expected but was
    # <false>.
    disable TimeExtCalculationsTest, :test_future_with_time_current_as_time_with_zone

    # <Mon Feb 21 17:44:30  2039> expected but was
    # <Mon, 21 Feb 2039 17:44:30 -0800>.
    disable TimeExtCalculationsTest, :test_local_time

    # <true> expected but was
    # <false>.
    disable TimeExtCalculationsTest, :test_past_with_time_current_as_time_local

    # <false> expected but was ## <true>.
    disable TimeExtCalculationsTest, :test_past_with_time_current_as_time_with_zone

    # just after DST end.
    # <10801> expected but was
    # <7201.0>.
    disable TimeExtCalculationsTest, :test_seconds_since_midnight_at_daylight_savings_time_end

    # just after DST start.
    # <7201> expected but was
    # <10801.0>.
    disable TimeExtCalculationsTest, :test_seconds_since_midnight_at_daylight_savings_time_start

    # <Sun Apr 02 03:00:00  2006> expected but was
    # <Sun Apr 02 02:00:00  2006>.
    disable TimeExtCalculationsTest, :test_time_created_with_local_constructor_cannot_represent_times_during_hour_skipped_by_dst

    # <Mon Feb 21 17:44:30 Z 2039> expected but was
    # <Mon, 21 Feb 2039 17:44:30 00000>.
    disable TimeExtCalculationsTest, :test_time_with_datetime_fallback

    # <"Mon, 21 Feb 2005 17:44:30 +0000"> expected but was
    # <"Mon, 21 Feb 2005 17:44:30 00000">.
    disable TimeExtCalculationsTest, :test_to_s

    # <Mon Feb 21 17:44:30 Z 2039> expected but was
    # <Mon, 21 Feb 2039 17:44:30 00000>.
    disable TimeExtCalculationsTest, :test_utc_time


    # <Sat Jan 01 00:00:00 Z 2000> expected but was
    # <Fri Dec 31 16:00:00 -08:00 1999>.
    disable TimeExtMarshalingTest, :test_marshaling_with_frozen_utc_instance

    # <Sat Jan 01 00:00:00 Z 2000> expected but was
    # <Fri Dec 31 16:00:00 -08:00 1999>.
    disable TimeExtMarshalingTest, :test_marshaling_with_utc_instance


    # <Sat Jan 01 00:00:00 Z 2000> expected but was
    # <Sat Jan 01 03:00:00 Z 2000>.
    disable TimeWithZoneMethodsForTimeAndDateTimeTest, :test_current_returns_time_zone_now_when_zone_default_set

    # NoMethodError: You have a nil object when you didn't expect it!
    # The error occurred while evaluating nil.period_for_utc
    disable TimeWithZoneMethodsForTimeAndDateTimeTest, :test_in_time_zone

    # <"Sat, 01 Jan 2000 00:00:00 UTC +00:00"> expected but was
    # <"Sat, 01 Jan 2000 00:00:00 UTC 000:00">.
    disable TimeWithZoneMethodsForTimeAndDateTimeTest, :test_in_time_zone_with_argument

    # <"Fri, 31 Dec 1999 15:00:00 AKST -09:00"> expected but was
    # <"Fri, 31 Dec 1999 18:00:00 AKST -09:00">.
    disable TimeWithZoneMethodsForTimeAndDateTimeTest, :test_in_time_zone_with_time_local_instance

    # <"Sun, 02 Apr 2006 03:00:00 EDT -04:00"> expected but was
    # <"Sun, 02 Apr 2006 02:00:00 EDT -04:00">.
    disable TimeWithZoneTest, :test_advance_1_month_into_spring_dst_gap

    # ArgumentError: invalid date
    disable TimeWithZoneTest, :test_change

    # <nil> is not true.
    disable TimeWithZoneTest, :test_eql?

    # <false> expected but was
    # <true>.
    disable TimeWithZoneTest, :test_future_with_time_current_as_time_local

    # <true> expected but was
    # <false>.
    disable TimeWithZoneTest, :test_instance_created_with_local_time_enforces_fall_dst_rules

    # <Sun Apr 02 03:00:00 Z 2006> expected but was
    # <Sun Apr 02 02:00:00 Z 2006>.
    disable TimeWithZoneTest, :test_instance_created_with_local_time_enforces_spring_dst_rules

    # <true> expected but was
    # <false>.
    disable TimeWithZoneTest, :test_past_with_time_current_as_time_local

    # <Sun Oct 29 01:59:59 Z 2006> expected but was
    # <Sun Oct 29 00:59:59 Z 2006>.
    disable TimeWithZoneTest, :test_plus_and_minus_enforce_fall_dst_rules

    # <Sun Apr 02 01:59:59 Z 2006> expected but was
    # <Sun Apr 02 02:59:59 Z 2006>.
    disable TimeWithZoneTest, :test_plus_and_minus_enforce_spring_dst_rules

    # NameError: uninitialized constant YAML::Emitter
    disable TimeWithZoneTest, :test_ruby_to_yaml

    # NameError: uninitialized constant YAML::Emitter
    disable TimeWithZoneTest, :test_to_yaml

    # <"1999-12-31T19:00:00.123456-05:00"> expected but was
    # <"1999-12-31T19:00:00.123000-05:00">.
    disable TimeWithZoneTest, :test_xmlschema_with_fractional_seconds

    # <"+00:00"> expected but was
    # <"000:00">.
    disable TimeZoneTest, :test_formatted_offset_zero

    # <Sun Oct 29 05:00:00 Z 2006> expected but was
    # <Sun Oct 29 06:00:00 Z 2006>.
    disable TimeZoneTest, :test_local_enforces_fall_dst_rules

    # <Sun Apr 02 01:59:59 Z 2006> expected but was
    # <Sun Apr 02 02:59:59 Z 2006>.
    disable TimeZoneTest, :test_local_enforces_spring_dst_rules

    # <Sat Jan 01 05:00:00 Z 2000> expected but was
    # <Sat Jan 01 08:00:00 Z 2000>.
    disable TimeZoneTest, :test_now

    # <Sun Apr 02 03:00:00 Z 2006> expected but was
    # <Sun Apr 02 05:00:00 Z 2006>.
    disable TimeZoneTest, :test_now_enforces_spring_dst_rules
  end
end
