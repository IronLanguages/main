require 'code_statistics'
require 'rbosa'

module Autotest::Shame
  @@chat_app = :adium

  def self.chat_app= o
    @@chat_app = o
  end

  # Until the rails team learns how to write modular code... I must steal :/
  STATS_DIRECTORIES = [
                       %w(Controllers        app/controllers),
                       %w(Helpers            app/helpers),
                       %w(Models             app/models),
                       %w(Libraries          lib/),
                       %w(APIs               app/apis),
                       %w(Components         components),
                       %w(Integration\ tests test/integration),
                       %w(Functional\ tests  test/functional),
                       %w(Unit\ tests        test/unit),
                      ].select { |name, dir| File.directory?(dir) }

  def self.shame
    stats = CodeStatistics.new(*STATS_DIRECTORIES)
    code  = stats.send :calculate_code
    tests = stats.send :calculate_tests
    msg = "Code To Test Ratio: 1:#{sprintf("%.2f", tests.to_f/code)}"
    $-w = ! $-w
    case @@chat_app
    when :adium then
      OSA.app('Adium').adium_controller.my_status_message = msg
    when :ichat then
      OSA.app('ichat').status_message = msg
    else
      raise "huh?"
    end
    $-w = ! $-w
    $stderr.puts "Status set to: #{msg.inspect}"
  end

  Autotest.add_hook(:all_good) do |autotest|
    shame
  end
end
