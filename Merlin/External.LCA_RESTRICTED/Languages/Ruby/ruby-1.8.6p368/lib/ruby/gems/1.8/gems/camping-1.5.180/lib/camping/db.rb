class MissingLibrary < Exception #:nodoc: all
end
begin
    require 'active_record'
rescue LoadError => e
    raise MissingLibrary, "ActiveRecord could not be loaded (is it installed?): #{e.message}"
end

$AR_EXTRAS = %{
  Base = ActiveRecord::Base unless const_defined? :Base

  def Y; ActiveRecord::Base.verify_active_connections!; self; end

  class SchemaInfo < Base
  end

  def self.V(n)
    @final = [n, @final.to_i].max
    m = (@migrations ||= [])
    Class.new(ActiveRecord::Migration) do
      meta_def(:version) { n }
      meta_def(:inherited) { |k| m << k }
    end
  end

  def self.create_schema(opts = {})
    opts[:assume] ||= 0
    opts[:version] ||= @final
    if @migrations
      unless SchemaInfo.table_exists?
        ActiveRecord::Schema.define do
          create_table SchemaInfo.table_name do |t|
            t.column :version, :float
          end
        end
      end

      si = SchemaInfo.find(:first) || SchemaInfo.new(:version => opts[:assume])
      if si.version < opts[:version]
        @migrations.each do |k|
          k.migrate(:up) if si.version < k.version and k.version <= opts[:version]
          k.migrate(:down) if si.version > k.version and k.version > opts[:version]
        end
        si.update_attributes(:version => opts[:version])
      end
    end
  end
}

module Camping
  module Models
    A = ActiveRecord
    # Base is an alias for ActiveRecord::Base.  The big warning I'm going to give you
    # about this: *Base overloads table_name_prefix.*  This means that if you have a
    # model class Blog::Models::Post, it's table name will be <tt>blog_posts</tt>.
    #
    # ActiveRecord is not loaded if you never reference this class.  The minute you
    # use the ActiveRecord or Camping::Models::Base class, then the ActiveRecord library
    # is loaded.
    Base = A::Base

    # The default prefix for Camping model classes is the topmost module name lowercase
    # and followed with an underscore.
    #
    #   Tepee::Models::Page.table_name_prefix
    #     #=> "tepee_pages"
    #
    def Base.table_name_prefix
        "#{name[/\w+/]}_".downcase.sub(/^(#{A}|camping)_/i,'')
    end
    module_eval $AR_EXTRAS
  end
end
Camping::S.sub! "autoload:Base,'camping/db'", ""
Camping::S.sub! "def Y;self;end", $AR_EXTRAS
Camping::Apps.each do |app|
    app::Models.module_eval $AR_EXTRAS
end
