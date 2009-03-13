require Pathname(__FILE__).dirname / "viewable"
require Pathname(__FILE__).dirname / "billable"
require Pathname(__FILE__).dirname / "addressable"
require Pathname(__FILE__).dirname / "rating"

class User
  include DataMapper::Resource

  property :id,             Integer,
    :key          => true,
    :serial       => true

  property :first_name,     String,
    :nullable     => false,
    :length       => 2..50

  property :last_name,      String,
    :nullable     => false,
    :length       => 2..50

  remix n, :viewables

  remix n, :billables, :class_name => "Account"

  remix n, :addressables

  remix n, :commentables, :as => "comments", :for => "User", :via => "commentor"

  remix n, "My::Nested::Remixable::Rating"

  enhance :addressables do
    property :label, Enum.new('home','work')
  end
end
