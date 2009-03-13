module Merb
  module Generators
    class MerbStackGenerator < AppGenerator
      #
      # ==== Paths
      #

      def self.source_root
        File.join(super, 'application', 'merb_stack')
      end

      def self.common_templates_dir
        File.expand_path(File.join(File.dirname(__FILE__), '..',
                                   'templates', 'application', 'common'))
      end

      def destination_root
        File.join(@destination_root, base_name)
      end

      def common_templates_dir
        self.class.common_templates_dir
      end
      
      def testing_framework
        :rspec
      end
      
      def orm
        :datamapper
      end

      #
      # ==== Generator options
      #

      option :template_engine, :default => :erb,
      :desc => 'Template engine to prefer for this application (one of: erb, haml).'

      desc <<-DESC
      Generates a new "jump start" Merb application with support for DataMapper,
      helpers, assets, mailer, caching, slices and merb-auth.
    DESC

      first_argument :name, :required => true, :desc => "Application name"

      #
      # ==== Common directories & files
      #

      empty_directory :gems, 'gems'
      template :rakefile do |template|
        template.source = File.join(common_templates_dir, "Rakefile")
        template.destination = "Rakefile"
      end

      file :gitignore do |file|
        file.source = File.join(common_templates_dir, 'dotgitignore')
        file.destination = ".gitignore"
      end

      file :htaccess do |file|
        file.source = File.join(common_templates_dir, 'dothtaccess')
        file.destination = 'public/.htaccess'
      end
      
      file :doctask do |file|
        file.source = File.join(common_templates_dir, 'doc.thor')
        file.destination = 'tasks/doc.thor'
      end
      
      file :jquery do |file|
        file.source = File.join(common_templates_dir, 'jquery.js')
        file.destination = 'public/javascripts/jquery.js'
      end

      directory :thor_file do |directory|
        directory.source = File.join(common_templates_dir, "merb_thor")
        directory.destination = File.join("tasks", "merb.thor")
      end

      directory :test_dir do |directory|
        dir = testing_framework == :rspec ? "spec" : "test"

        directory.source      = File.join(source_root, dir)
        directory.destination = dir
      end

      #
      # ==== Layout specific things
      #

      def merb_gems_version
        Merb::VERSION
      end

      def dm_gems_version
        Merb::DM_VERSION
      end
      
      def do_gems_version
        Merb::DO_VERSION
      end
      
      # empty array means all files are considered to be just
      # files, not templates
      glob! "app"
      glob! "autotest"
      glob! "config"
      glob! "doc",      []
      glob! "public"
      glob! "lib"
      glob! "merb"

      invoke :layout do |generator|
        generator.new(destination_root, options, 'application')
      end
    end

    add :app,   MerbStackGenerator
  end
end
