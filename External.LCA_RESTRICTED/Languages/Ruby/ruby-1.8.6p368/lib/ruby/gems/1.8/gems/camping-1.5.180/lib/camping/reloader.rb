module Camping
# == The Camping Reloader
#
# Camping apps are generally small and predictable.  Many Camping apps are
# contained within a single file.  Larger apps are split into a handful of
# other Ruby libraries within the same directory.
#
# Since Camping apps (and their dependencies) are loaded with Ruby's require
# method, there is a record of them in $LOADED_FEATURES.  Which leaves a
# perfect space for this class to manage auto-reloading an app if any of its
# immediate dependencies changes.
#
# == Wrapping Your Apps
#
# Since bin/camping and the Camping::FastCGI class already use the Reloader,
# you probably don't need to hack it on your own.  But, if you're rolling your
# own situation, here's how.
#
# Rather than this:
#
#   require 'yourapp'
#
# Use this:
#
#   require 'camping/reloader'
#   Camping::Reloader.new('/path/to/yourapp.rb')
#
# The reloader will take care of requiring the app and monitoring all files
# for alterations.
class Reloader
    attr_accessor :klass, :mtime, :mount, :requires

    # Creates the reloader, assigns a +script+ to it and initially loads the
    # application.  Pass in the full path to the script, otherwise the script
    # will be loaded relative to the current working directory.
    def initialize(script)
        @script = File.expand_path(script)
        @mount = File.basename(script, '.rb')
        @requires = nil
        load_app
    end

    # Find the application, based on the script name.
    def find_app(title)
        @klass = Object.const_get(Object.constants.grep(/^#{title}$/i)[0]) rescue nil
    end

    # If the file isn't found, if we need to remove the app from the global
    # namespace, this will be sure to do so and set @klass to nil.
    def remove_app
        Object.send :remove_const, @klass.name if @klass
        @klass = nil
    end

    # Loads (or reloads) the application.  The reloader will take care of calling
    # this for you.  You can certainly call it yourself if you feel it's warranted.
    def load_app
        title = File.basename(@script)[/^([\w_]+)/,1].gsub /_/,'' 
        begin
            all_requires = $LOADED_FEATURES.dup
            load @script
            @requires = ($LOADED_FEATURES - all_requires).select do |req|
                req.index(File.basename(@script) + "/") == 0 || req.index(title + "/") == 0
            end
        rescue Exception => e
            puts "!! trouble loading #{title}: [#{e.class}] #{e.message}"
            puts e.backtrace.join("\n")
            find_app title
            remove_app
            return
        end

        @mtime = mtime
        find_app title
        unless @klass and @klass.const_defined? :C
            puts "!! trouble loading #{title}: not a Camping app, no #{title.capitalize} module found"
            remove_app
            return
        end
        
        Reloader.conditional_connect
        @klass.create if @klass.respond_to? :create
        @klass
    end

    # The timestamp of the most recently modified app dependency.
    def mtime
        ((@requires || []) + [@script]).map do |fname|
            fname = fname.gsub(/^#{Regexp::quote File.dirname(@script)}\//, '')
            begin
                File.mtime(File.join(File.dirname(@script), fname))
            rescue Errno::ENOENT
                remove_app
                @mtime
            end
        end.max
    end

    # Conditional reloading of the app.  This gets called on each request and
    # only reloads if the modification times on any of the files is updated.
    def reload_app 
        return if @klass and @mtime and mtime <= @mtime

        if @requires
            @requires.each { |req| $LOADED_FEATURES.delete(req) }
        end
        k = @klass
        Object.send :remove_const, k.name if k
        load_app
    end

    # Conditionally reloads (using reload_app.)  Then passes the request through
    # to the wrapped Camping app.
    def run(*a)
        reload_app
        if @klass
            @klass.run(*a) 
        else
            Camping.run(*a)
        end
    end

    # Returns source code for the main script in the application.
    def view_source
        File.read(@script)
    end

    class << self
        def database=(db)
            @database = db
        end
        def log=(log)
            @log = log
        end
        def conditional_connect
            # If database models are present, `autoload?` will return nil.
            unless Camping::Models.autoload? :Base
                require 'logger'
                require 'camping/session'
                Camping::Models::Base.establish_connection @database if @database

                case @log
                when Logger
                    Camping::Models::Base.logger = @log
                when String
                    Camping::Models::Base.logger = Logger.new(@log == "-" ? STDOUT : @log)
                end

                Camping::Models::Session.create_schema

                if @database and @database[:adapter] == 'sqlite3'
                    begin
                        require 'sqlite3_api'
                    rescue LoadError
                        puts "!! Your SQLite3 adapter isn't a compiled extension."
                        abort "!! Please check out http://code.whytheluckystiff.net/camping/wiki/BeAlertWhenOnSqlite3 for tips."
                    end
                end
            end
        end
    end
end
end
