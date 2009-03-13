#!/usr/local/bin/ruby -w

module Autotest::Menu
  WINDOZE = /win32/ =~ RUBY_PLATFORM unless defined? WINDOZE

  if WINDOZE then
    require "Win32API"
    def self.getchar
      Win32API.new("crtdll", "_getch", [], "L").Call
    end
  else
    STTY_SAVE_STATE=`stty -g`
    def self.getchar
      system 'stty raw echo'
      STDIN.getc
    ensure
      system "stty '#{STTY_SAVE_STATE}'"
    end
  end

  def self.menu(choices)
    result = nil
    choices.sort.each do |c, desc|
      puts "#{c.chr}: #{desc}"
    end
    until choices[result]
      print "menu> "
      result = getchar
      print " invalid input" unless choices[result]
      puts
    end
    result
  end

  Autotest.add_hook(:interrupt) do |at|
    $stderr.puts "menu"
    case menu ?q => "quit", ?c => "continue", ?r => "restart"
    when ?c
      true
    when ?r
      at.reset
      true
    when ?q
      at.wants_to_quit = true
      true
    else
      false
    end
    # puts "you chose #{c.chr}"
  end
end
