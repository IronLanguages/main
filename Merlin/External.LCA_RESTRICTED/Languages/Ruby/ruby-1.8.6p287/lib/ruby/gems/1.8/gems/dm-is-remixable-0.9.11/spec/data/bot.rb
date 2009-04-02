require Pathname(__FILE__).dirname / "viewable"
require Pathname(__FILE__).dirname / "billable"
require Pathname(__FILE__).dirname / "addressable"
require Pathname(__FILE__).dirname / "rating"

class Bot
  include DataMapper::Resource

  property :id,             Integer,
    :key          => true,
    :serial       => true

  property :bot_name,     String,
    :nullable     => false,
    :length       => 2..50

  property :bot_version,      String,
    :nullable     => false,
    :length       => 2..50

end
