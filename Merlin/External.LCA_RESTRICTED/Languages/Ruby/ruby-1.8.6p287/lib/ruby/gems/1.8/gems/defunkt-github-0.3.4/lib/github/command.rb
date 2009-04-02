require 'fileutils'

if RUBY_PLATFORM =~ /mswin|mingw/
  begin
    require 'win32/open3'
  rescue LoadError
    warn "You must 'gem install win32-open3' to use the github command on Windows"
    exit 1
  end
else
  require 'open3'
end

module GitHub
  class Command
    include FileUtils

    def initialize(block)
      (class << self;self end).send :define_method, :command, &block
    end

    def call(*args)
      arity = method(:command).arity
      args << nil while args.size < arity
      send :command, *args
    end

    def helper
      @helper ||= Helper.new
    end

    def options
      GitHub.options
    end

    def pgit(*command)
      puts git(*command)
    end

    def git(command)
      run :sh, command
    end

    def git_exec(command)
      run :exec, command
    end

    def run(method, command)
      if command.is_a? Array
        command = [ 'git', command ].flatten
        GitHub.learn command.join(' ')
      else
        command = 'git ' + command
        GitHub.learn command
      end

      send method, *command
    end

    def sh(*command)
      Shell.new(*command).run
    end

    def die(message)
      puts "=> #{message}"
      exit!
    end

    def github_user
      git("config --get github.user")
    end

    def github_token
      git("config --get github.token")
    end

    def shell_user
      ENV['USER']
    end

    def current_user?(user)
      user == github_user || user == shell_user
    end

    class Shell < String
      attr_reader :error
      attr_reader :out

      def initialize(*command)
        @command = command
      end

      def run
        GitHub.debug "sh: #{command}"
        _, out, err = Open3.popen3(*@command)

        out = out.read.strip
        err = err.read.strip

        replace @error = err if err.any?
        replace @out = out if out.any?

        self
      end

      def command
        @command.join(' ')
      end

      def error?
        !!@error
      end

      def out?
        !!@out
      end
    end
  end

  class GitCommand < Command
    def initialize(name)
      @name = name
    end

    def command(*args)
      git_exec [ @name, args ]
    end
  end
end
