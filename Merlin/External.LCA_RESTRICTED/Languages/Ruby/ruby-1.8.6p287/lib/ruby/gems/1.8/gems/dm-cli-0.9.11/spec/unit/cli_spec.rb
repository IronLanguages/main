require 'rubygems'

gem 'dm-core', '~>0.9.11'
require 'dm-core'

require File.expand_path(File.join(File.dirname(__FILE__), "..", "..", "lib", "dm-cli", "cli"))

describe DataMapper::CLI do

  describe "connection string" do
  end

  describe "CLI options" do

    # Entire configuration structure, useful for testing scenarios.
    describe "-c or --config" do
      it "should load config file" do
        pending
#        arg = ["c"]
#        cli = DataMapper::CLI.new
#        cli.parse_args(arg)

      end
    end

    describe "-m or --models" do
      it "should set options[:models]" do
        pending
      end
    end

    # database connection configuration yaml file.
    describe "-y or --yaml" do
      it "should set options[:yaml]" do
        pending
      end
    end

    # logfile
    describe "-l or --log" do
      it "should set options[:log_file]" do
        pending
      end
    end

    # environment to use with database yaml file.
    describe "-e, --environment" do
      it "should set options[:environment]" do
        pending
      end
    end

    # Loads Merb app settings: config/database.yml, app/models
    # Loads Rails app settings: config/database.yml, app/models
    describe "--merb, --rails" do
      it "should set options[:models]" do
        pending
      end

      it "should set options[:yaml]" do
        pending
      end
    end

    describe "database options" do

      # adapter {mysql, pgsql, etc...}
      describe "-a, --adapter" do
        it "should support mysql" do
          pending
        end

        it "should support pgsql" do
          pending
        end

        it "should support sqlite" do
          pending
        end
      end

      # database name
      describe "-d, --database" do
        it "should set options[:database]" do
          pending
        end
      end

      # user name
      describe "-u, --username" do
        it "should set options[:username]" do
          pending
        end
      end

      # password
      describe "-p, --password" do
        it "should set options[:password]" do
          pending
        end
      end

      # host
      describe "-h, --host" do
        it "should set options[:host]" do
          pending
        end
      end

      # socket
      describe "-s, --socket" do
        it "should set options[:socket]" do
          pending
        end
      end

      # port
      describe "-o, --port" do
        it "should set options[:port]" do
          pending
        end
      end

    end

  end

end
