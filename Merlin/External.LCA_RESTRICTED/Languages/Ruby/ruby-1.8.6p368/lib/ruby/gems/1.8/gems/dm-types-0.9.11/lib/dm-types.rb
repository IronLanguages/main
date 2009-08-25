require 'pathname'
require 'rubygems'

gem 'dm-core', '0.9.11'
require 'dm-core'

module DataMapper
  module Types
    dir = (Pathname(__FILE__).dirname.expand_path / 'dm-types').to_s

    autoload :BCryptHash, dir / 'bcrypt_hash'
    autoload :Csv,        dir / 'csv'
    autoload :Enum,       dir / 'enum'
    autoload :EpochTime,  dir / 'epoch_time'
    autoload :FilePath,   dir / 'file_path'
    autoload :Flag,       dir / 'flag'
    autoload :IPAddress,  dir / 'ip_address'
    autoload :Json,       dir / 'json'
    autoload :Regexp,     dir / 'regexp'
    autoload :Serial,     dir / 'serial'
    autoload :Slug,       dir / 'slug'
    autoload :URI,        dir / 'uri'
    autoload :UUID,       dir / 'uuid'
    autoload :Yaml,       dir / 'yaml'
  end
end
