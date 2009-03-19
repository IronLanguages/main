require "yaml"
require "irb"
require Pathname("irb/completion")

# TODO: error handling for:
#   missing adapter, host or database
module DataMapper

  class CLI

    class << self

      def usage
        <<-USAGE

dm - Data Mapper CLI

  Usage Examples\n#{'='*80}

* If one argument is given the CLI assumes it is a connection string:
  $ dm mysql://root@localhost/test_development

  The connection string has the format:
    adapter://user:password@host:port/database
  Where adapter is in: {mysql, pgsql, sqlite...} and the user/password is optional

  Note that if there are any non-optional arguments specified, the first is
  assumed to be a database connection string which will be used instead of any
  database specified by options.

* Load the database by specifying cli options
  $ dm -a mysql -u root -h localhost -d test_development

* Load the database using a yaml config file and specifying the environment to use
  $ dm --yaml config/database.yml -e development

* Load everything from a config file, this example is equivalent to the above
  $ dm --config config/development.yml

* Load the database and some model files from a directory, specifying the environment
  $ dm --yaml config/database.yml -e development --models app/models

* Load an assumed structure of a typical merb application
  $ dm --merb -e development

  This is similar to merb -i without the merb framework being loaded.

* Load the dm-validations and dm-timestamps plugins before connecting to the db
  $ dm -P validations,dm-timestamps mysql://root@localhost/test_development

  If dm- isn't at the start of the file, it will be prepended.


USAGE
      end

      attr_accessor :options, :config

      def parse_args(argv = ARGV)
        @config ||= {}

        # Build a parser for the command line arguments
        OptionParser.new do |opt|
          opt.define_head "DataMapper CLI"
          opt.banner = usage

          opt.on("-m", "--models MODELS", "The directory to load models from.") do |models|
            @config[:models] = Pathname(models)
          end

          opt.on("-c", "--config FILE", "Entire configuration structure, useful for testing scenarios.") do |config_file|
            @config = YAML::load_file Pathname(config_file)
          end

          opt.on("--merb", "--rails", "Loads application settings: config/database.yml, app/models.") do
            @config[:models] = Pathname("app/models")
            @config[:yaml]   = Pathname("config/database.yml")
          end

          opt.on("-y", "--yaml YAML", "The database connection configuration yaml file.") do |yaml_file|
            if (yaml = Pathname(yaml_file)).file?
              @config[:yaml] = yaml
            elsif (yaml = Pathname("#{Dir.getwd}/#{yaml_file}")).file?
              @config[:yaml] = yaml
            else
              raise "yaml file was specifed as #{yaml_file} but does not exist."
            end
          end

          opt.on("-l", "--log LOGFILE", "A string representing the logfile to use. Also accepts STDERR and STDOUT") do |log_file|
            @config[:log_file] = log_file
          end

          opt.on("-e", "--environment STRING", "Run merb in the correct mode(development, production, testing)") do |environment|
            @config[:environment] = environment
          end

          opt.on("-a", "--adapter ADAPTER", "Number of merb daemons to run.") do |adapter|
            @config[:adapter] = adapter
          end

          opt.on("-u", "--username USERNAME", "The user to connect to the database as.") do |username|
            @config[:username] = username
          end

          opt.on("-p", "--password PASSWORD", "The password to connect to the database with") do |password|
            @config[:password] = password
          end

          opt.on("-h", "--host HOSTNAME", "Host to connect to.") do |host|
            @config[:host] = host
          end

          opt.on("-s", "--socket SOCKET", "The socket to connect to.") do |socket|
            @config[:socket] = socket
          end

          opt.on("-o", "--port PORT", "The port to connect to.") do |port|
            @config[:port] = port
          end

          opt.on("-d", "--database DATABASENAME", "Name of the database to connect to.") do |database_name|
            @config[:database] = database_name
          end

          opt.on("-P", "--plugins PLUGIN,PLUGIN...", "A list of dm-plugins to require", Array) do |plugins|
            @config[:plugins] = plugins
          end

          opt.on("-?", "-H", "--help", "Show this help message") do
            puts opt
            exit
          end

        end.parse!(argv)

      end

      def configure(args)

        parse_args(args)

        @config[:environment] ||= "development"
        if @config[:config]
          @config.merge!(YAML::load_file(@config[:config]))
          @options = @config[:options]
        elsif @config[:yaml]
          @config.merge!(YAML::load_file(@config[:yaml]))
          @options = @config[@config[:environment]] || @config[@config[:environment].to_sym]
          raise "Options for environment '#{@config[:environment]}' are missing." if @options.nil?
        else
          @options = {
            :adapter  => @config[:adapter],
            :username => @config[:username],
            :password => @config[:password],
            :host     => @config[:host],
            :database => @config[:database]
          }
        end
        if !ARGV.empty?
          @config[:connection_string] = ARGV.shift
        end

      end

      def load_models
        Pathname.glob("#{config[:models]}/**/*.rb") { |file| load file }
      end

      def require_plugins
        # make sure we're loading dm plugins!
        plugins = config[:plugins].map {|p| (p =~ /^dm/) ? p : "dm-" + p }
        plugins.each do |plugin|
          begin
            require plugin
            puts "required #{plugin}."
          rescue LoadError => e
            puts "couldn't load #{plugin}."
          end
        end
      end

      def setup_logger
        if config[:log_file] =~ /^std(?:out|err)$/i
          log = Object.full_const_get(config[:log_file].upcase)
        else
          log = Pathname(config[:log_file])
        end

        DataMapper::Logger.new(log, :debug)
      end

      def start(argv = ARGV)
        if (ARGV.nil? || ARGV.empty?)
          puts DataMapper::CLI.usage
          exit 1
        end

        begin
          configure(argv)

          require_plugins if config[:plugins]

          setup_logger if config[:log_file]

          if config[:connection_string]
            DataMapper.setup(:default, config[:connection_string])
            puts "DataMapper has been loaded using '#{config[:connection_string]}'"
          else
            DataMapper.setup(:default, options.dup)
            puts "DataMapper has been loaded using the '#{options[:adapter] || options["adapter"]}' database '#{options[:database] || options["database"]}' on '#{options[:host] || options["host"]}' as '#{options[:username] || options["username"]}'"
          end
          load_models if config[:models]
          ENV["IRBRC"] ||= DataMapper::CLI::BinDir + "/.irbrc" # Do not change this please. This should NOT be DataMapper.root
          IRB.start
        rescue => error
          puts error.message
          exit
        end

      end
    end
  end # module CLI
end # module DataMapper
