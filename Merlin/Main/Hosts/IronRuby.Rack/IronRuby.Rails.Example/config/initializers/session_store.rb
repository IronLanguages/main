# Be sure to restart your server when you modify this file.

# Your secret key for verifying cookie session data integrity.
# If you change this key, all old sessions will become invalid!
# Make sure the secret is at least 30 characters and all random, 
# no regular words or you'll be exposed to dictionary attacks.
ActionController::Base.session = {
  :key         => '_IronRuby.Rails.Example_session',
  :secret      => '70d06ef87905b75bd3d94e7bf7bc3139078c9ba9de5967dc4af7fa3b682e0a301514dffe66bcc030230b91438d82e4e671be2da24f9b2a24d5b736b06f4606da'
}

# Use the database for sessions instead of the cookie-based default,
# which shouldn't be used to store highly confidential information
# (create the session table with "rake db:sessions:create")
# ActionController::Base.session_store = :active_record_store
