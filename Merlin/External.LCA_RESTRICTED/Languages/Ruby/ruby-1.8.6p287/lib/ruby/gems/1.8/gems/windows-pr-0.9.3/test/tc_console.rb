######################################################################
# tc_console.rb
#
# Test case for the Windows::Console module.
######################################################################
require 'windows/console'
require 'test/unit'

class ConsoleFoo
   include Windows::Console
end

class TC_Windows_Console < Test::Unit::TestCase
   def setup
      @foo = ConsoleFoo.new
      @ver = `ver`.chomp
   end

   def test_numeric_constants
      assert_equal(0, ConsoleFoo::CTRL_C_EVENT)
      assert_equal(1, ConsoleFoo::CTRL_BREAK_EVENT)
      assert_equal(5, ConsoleFoo::CTRL_LOGOFF_EVENT)
      assert_equal(6, ConsoleFoo::CTRL_SHUTDOWN_EVENT)

      assert_equal(0x0001, ConsoleFoo::ENABLE_PROCESSED_INPUT)
      assert_equal(0x0002, ConsoleFoo::ENABLE_LINE_INPUT)
      assert_equal(0x0002, ConsoleFoo::ENABLE_WRAP_AT_EOL_OUTPUT)
      assert_equal(0x0004, ConsoleFoo::ENABLE_ECHO_INPUT)
      assert_equal(0x0008, ConsoleFoo::ENABLE_WINDOW_INPUT)
      assert_equal(0x0010, ConsoleFoo::ENABLE_MOUSE_INPUT)
      assert_equal(0x0020, ConsoleFoo::ENABLE_INSERT_MODE)
      assert_equal(0x0040, ConsoleFoo::ENABLE_QUICK_EDIT_MODE)

      assert_equal(-10, ConsoleFoo::STD_INPUT_HANDLE)
      assert_equal(-11, ConsoleFoo::STD_OUTPUT_HANDLE)
      assert_equal(-12, ConsoleFoo::STD_ERROR_HANDLE)
   end

   def test_method_constants
      assert_not_nil(ConsoleFoo::AddConsoleAlias)
      assert_not_nil(ConsoleFoo::AllocConsole)
      assert_not_nil(ConsoleFoo::CreateConsoleScreenBuffer)
      assert_not_nil(ConsoleFoo::FillConsoleOutputAttribute)
      assert_not_nil(ConsoleFoo::FillConsoleOutputCharacter)
      assert_not_nil(ConsoleFoo::FlushConsoleInputBuffer)
      assert_not_nil(ConsoleFoo::FreeConsole)
      assert_not_nil(ConsoleFoo::GenerateConsoleCtrlEvent)
      assert_not_nil(ConsoleFoo::GetConsoleAlias)
      assert_not_nil(ConsoleFoo::GetConsoleAliases)
      assert_not_nil(ConsoleFoo::GetConsoleAliasesLength)
      assert_not_nil(ConsoleFoo::GetConsoleAliasExes)
      assert_not_nil(ConsoleFoo::GetConsoleAliasExesLength)
      assert_not_nil(ConsoleFoo::GetConsoleCP)
      assert_not_nil(ConsoleFoo::GetConsoleCursorInfo)
      assert_not_nil(ConsoleFoo::GetConsoleMode)
      assert_not_nil(ConsoleFoo::GetConsoleOutputCP)
      assert_not_nil(ConsoleFoo::GetConsoleScreenBufferInfo)
      assert_not_nil(ConsoleFoo::GetConsoleTitle)
      assert_not_nil(ConsoleFoo::GetConsoleWindow)
      assert_not_nil(ConsoleFoo::GetLargestConsoleWindowSize)
      assert_not_nil(ConsoleFoo::GetNumberOfConsoleInputEvents)
      assert_not_nil(ConsoleFoo::GetNumberOfConsoleMouseButtons)
      assert_not_nil(ConsoleFoo::GetStdHandle)
      assert_not_nil(ConsoleFoo::PeekConsoleInput)
      assert_not_nil(ConsoleFoo::ReadConsole)
      assert_not_nil(ConsoleFoo::ReadConsoleInput)
      assert_not_nil(ConsoleFoo::ReadConsoleOutput)
      assert_not_nil(ConsoleFoo::ReadConsoleOutputAttribute)
      assert_not_nil(ConsoleFoo::ReadConsoleOutputCharacter)
      assert_not_nil(ConsoleFoo::ScrollConsoleScreenBuffer)
      assert_not_nil(ConsoleFoo::SetConsoleActiveScreenBuffer)
      assert_not_nil(ConsoleFoo::SetConsoleCP)
      assert_not_nil(ConsoleFoo::SetConsoleCtrlHandler)
      assert_not_nil(ConsoleFoo::SetConsoleCursorInfo)
      assert_not_nil(ConsoleFoo::SetConsoleCursorPosition)
      assert_not_nil(ConsoleFoo::SetConsoleMode)
      assert_not_nil(ConsoleFoo::SetConsoleOutputCP)
      assert_not_nil(ConsoleFoo::SetConsoleScreenBufferSize)
      assert_not_nil(ConsoleFoo::SetConsoleTextAttribute)
      assert_not_nil(ConsoleFoo::SetConsoleTitle)
      assert_not_nil(ConsoleFoo::SetConsoleWindowInfo)
      assert_not_nil(ConsoleFoo::SetStdHandle)
      assert_not_nil(ConsoleFoo::WriteConsole)
      assert_not_nil(ConsoleFoo::WriteConsoleInput)
      assert_not_nil(ConsoleFoo::WriteConsoleOutput)
      assert_not_nil(ConsoleFoo::WriteConsoleOutputAttribute)
      assert_not_nil(ConsoleFoo::WriteConsoleOutputCharacter)
   end

   def test_method_constants_xp_or_later
      if @ver =~ /XP/
         assert_not_nil(ConsoleFoo::AttachConsole)
         assert_not_nil(ConsoleFoo::GetConsoleDisplayMode)
         assert_not_nil(ConsoleFoo::GetConsoleFontSize)
         assert_not_nil(ConsoleFoo::GetConsoleProcessList)
         assert_not_nil(ConsoleFoo::GetConsoleSelectionInfo)
         assert_not_nil(ConsoleFoo::GetCurrentConsoleFont)
         assert_not_nil(ConsoleFoo::SetConsoleDisplayMode)
      end
   end

   def test_explicit_ansi
      assert_not_nil(ConsoleFoo::GetConsoleAliasA)
   end

   def test_explicit_unicode
      assert_not_nil(ConsoleFoo::GetConsoleAliasW)
   end

   def teardown
      @foo = nil
      @ver = nil
   end
end
