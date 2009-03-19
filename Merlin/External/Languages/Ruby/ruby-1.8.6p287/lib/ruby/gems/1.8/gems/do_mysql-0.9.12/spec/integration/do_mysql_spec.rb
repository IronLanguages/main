# encoding: utf-8

require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataObjects::Mysql do
  include MysqlSpecHelpers

  before :each do
    setup_test_environment
  end

  after :each do
    teardown_test_environment
  end

  it "should expose the proper DataObjects classes" do
    DataObjects::Mysql.const_get('Connection').should_not be_nil
    DataObjects::Mysql.const_get('Command').should_not be_nil
    DataObjects::Mysql.const_get('Result').should_not be_nil
    DataObjects::Mysql.const_get('Reader').should_not be_nil
  end

  it "should connect successfully via TCP" do
    connection = DataObjects::Connection.new("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.host}:#{MYSQL.port}/#{MYSQL.database}")
    connection.should_not be_using_socket
    connection.close
  end

  it "should be able to send queries asynchronously in parallel" do
    threads = []

    start = Time.now
    4.times do |i|
      threads << Thread.new do
        connection = DataObjects::Connection.new("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.host}:#{MYSQL.port}/#{MYSQL.database}")
        command = connection.create_command("SELECT sleep(1)")
        result = command.execute_non_query
      end
    end

    threads.each{|t| t.join }
    finish = Time.now
    (finish - start).should < 2
  end

#
#  I comment this out partly to raise the issue for discussion. Socket files are afaik not supported under windows. Does this
#  mean that we should test for it on unix boxes but not on windows boxes? Or does it mean that it should not be speced at all?
#  It's not really a requirement, since all architectures that support MySQL also supports TCP connectsion, ne?
#
#  it "should connect successfully via the socket file" do
#    @connection = DataObjects::Mysql::Connection.new("mysql://#{MYSQL.user}@#{MYSQL.hostname}:#{MYSQL.port}/#{MYSQL.database}/?socket=#{SOCKET_PATH}")
#    @connection.should be_using_socket
#  end

  it "should return the current character set" do
    connection = DataObjects::Connection.new("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.hostname}:#{MYSQL.port}/#{MYSQL.database}")
    connection.character_set.should == "utf8"
    connection.close
  end

  it "should support changing the character set" do
    pending "JDBC API does not provide an easy way to get the current character set" if JRUBY
    # current character set can be retrieved with the following query:
    # "SHOW VARIABLES LIKE character_set_database"

    connection = DataObjects::Connection.new("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.hostname}:#{MYSQL.port}/#{MYSQL.database}?charset=latin1")
                 # N.B. query parameter after forward slash causes problems with JDBC
    connection.character_set.should == "latin1"
    connection.close

    connection = DataObjects::Connection.new("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.hostname}:#{MYSQL.port}/#{MYSQL.database}?charset=utf8")
    connection.character_set.should == "utf8"
    connection.close
  end

  it "should raise an error when opened with an invalid server uri" do
    pending 'causing a hang in JRuby' if JRUBY

    def connecting_with(uri)
      lambda { DataObjects::Connection.new(uri) }
    end

    unless JRUBY  ## FIXME in JRuby
      # Missing database name
      connecting_with("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.hostname}:#{MYSQL.port}/").should raise_error(MysqlError)
    end

    # Wrong port
    connecting_with("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.hostname}:666/").should raise_error(MysqlError)

    unless JRUBY  ## FIXME in JRuby
      # Bad Username
      connecting_with("mysql://baduser@#{MYSQL.hostname}:#{MYSQL.port}/").should raise_error(MysqlError)
    end

    # Bad Password
    connecting_with("mysql://#{MYSQL.user}:wrongpassword@#{MYSQL.hostname}:#{MYSQL.port}/").should raise_error(MysqlError)

    # Bad Database Name
    connecting_with("mysql://#{MYSQL.user}:#{MYSQL.pass}@#{MYSQL.hostname}:#{MYSQL.port}/bad_database").should raise_error(MysqlError)

    #
    # Again, should socket even be speced if we don't support it across all platforms?
    #
    # Invalid Socket Path
    #connecting_with("mysql://#{MYSQL.user}@#{MYSQL.hostname}:#{MYSQL.port}/MYSQL.database/?socket=/invalid/path/mysql.sock").should raise_error(MysqlError)
  end
end

describe DataObjects::Mysql::Connection do
  include MysqlSpecHelpers

  before :each do
    setup_test_environment
  end

  after :each do
    teardown_test_environment
  end

  it "should raise an error when attempting to execute a bad query" do
    lambda { @connection.create_command("INSERT INTO non_existant_table (tester) VALUES (1)").execute_non_query }.should raise_error(MysqlError)
  end

  it "should raise an error when executing a bad reader" do
    lambda { @connection.create_command("SELECT * FROM non_existant_table").execute_reader }.should raise_error(MysqlError)
  end

  it "should not raise a connection closed error after an incorrect query" do
    lambda { @connection.create_command("INSERT INTO non_existant_table (tester) VALUES (1)").execute_non_query }.should raise_error(MysqlError)
    lambda { @connection.create_command("INSERT INTO non_existant_table (tester) VALUES (1)").execute_non_query }.should_not raise_error(MysqlError, "This connection has already been closed.")
  end

  it "should not raise a connection closed error after an incorrect reader" do
    lambda { @connection.create_command("SELECT * FROM non_existant_table").execute_reader }.should raise_error(MysqlError)
    lambda { @connection.create_command("SELECT * FROM non_existant_table").execute_reader }.should_not raise_error(MysqlError, "This connection has already been closed.")
  end

end

describe DataObjects::Mysql::Reader do
  include MysqlSpecHelpers

  before :each do
    setup_test_environment
  end

  after :each do
    teardown_test_environment
  end

  it "should raise an error when you pass too many or too few types for the expected result set" do
    lambda { select("SELECT name, fired_at FROM users", [String, DateTime, Integer]) }.should raise_error(MysqlError)
  end

  it "shouldn't raise an error when you pass NO types for the expected result set" do
    lambda { select("SELECT name, fired_at FROM users", nil) }.should_not raise_error(MysqlError)
  end

  it "should return the proper number of fields" do
    id = insert("INSERT INTO users (name) VALUES ('Billy Bob')")

    select("SELECT id, name, fired_at FROM users WHERE id = ?", nil, id) do |reader|
      reader.fields.size.should == 3
    end
  end

  it "should return proper number of rows and fields using row_count and field_count" do
    pending "C-extension doesn't return row_count correctly at the moment" unless JRUBY
    command = @connection.create_command("SELECT * FROM widgets WHERE id = (SELECT max(id) FROM widgets)")
    reader = command.execute_reader
    reader.field_count.should == 21
    reader.row_count.should == 1
    reader.close
  end

  it "should raise an exception if .values is called after reading all available rows" do

    select("SELECT * FROM widgets LIMIT 2") do |reader|
      # select already calls next once for us
      reader.next!
      reader.next!

      lambda { reader.values }.should raise_error(MysqlError)
    end
  end

  it "should fetch the proper number of rows" do
    ids = [
      insert("INSERT INTO users (name) VALUES ('Slappy Wilson')"),
      insert("INSERT INTO users (name) VALUES ('Jumpy Jones')")
    ]
                                            # do_jdbc rewrites "?" as "(?,?)"
                                            # to correspond to the JDBC API

    select("SELECT * FROM users WHERE id IN ?", nil, ids) do |reader|
      # select already calls next once for us
      reader.next!.should == true
      reader.next!.should be_nil
    end
  end

  it "should contain tainted strings" do
    id = insert("INSERT INTO users (name) VALUES ('Cuppy Canes')")

    select("SELECT name FROM users WHERE id = ?", nil, id) do |reader|
      reader.values.first.should be_tainted
    end

  end

  it "should return DB nulls as nil" do
    id = insert("INSERT INTO users (name) VALUES (NULL)")
    select("SELECT name from users WHERE name is null") do |reader|
      reader.values[0].should == nil
    end
  end

  it "should not convert empty strings to null" do
    id = insert("INSERT INTO users (name) VALUES ('')")
    select("SELECT name FROM users WHERE id = ?", [String], id) do |reader|
      reader.values.first.should == ''
    end
  end

  it "should correctly work with default utf8 character set" do
    name = "Билли Боб"
    id = insert("INSERT INTO users (name) VALUES ('#{name}')")

    select("SELECT name from users WHERE id = ?", [String], id) do |reader|
      reader.values[0].should == name
    end
  end

  it "should correctly interpret extended characters in sql statements" do
    name = "Билли Боб"
    id = insert("INSERT INTO users (name) VALUES ('#{name}')")
    select("SELECT name from users WHERE id = ?", [String], id) do |reader|
      reader.values[0].should == name
    end
  end

  it "should correctly interpret extended characters in args of sql statements" do
    name = "Билли Боб"
    id = insert("INSERT INTO users (name) VALUES (?)", name)
    select("SELECT name from users WHERE id = ?", [String], id) do |reader|
      reader.values[0].should == name
    end
  end

  describe "Date, Time, and DateTime" do

    it "should return nil when the time is 0" do
      pending "We need to introduce something like Proxy for typeasting where each SQL type will have _rules_ of casting" if JRUBY
      # skip the test if the strict dates/times setting is turned on
      strict_time = select("SHOW VARIABLES LIKE 'sql_mode'") do |reader|
        reader.values.last.split(',').any? do |mode|
          %w[ NO_ZERO_IN_DATE NO_ZERO_DATE ].include?(mode.strip.upcase)
        end
      end

      unless strict_time
        id = insert("INSERT INTO users (name, fired_at) VALUES ('James', 0);")
        select("SELECT fired_at FROM users WHERE id = ?", [Time], id) do |reader|
          reader.values.last.should be_nil
        end
        exec("DELETE FROM users WHERE id = ?", id)
      end
    end

    it "should return DateTimes using the current locale's Time Zone" do
      date = DateTime.now
      id = insert("INSERT INTO users (name, fired_at) VALUES (?, ?)", 'Sam', date)

      select("SELECT fired_at FROM users WHERE id = ?", [DateTime], id) do |reader|
        reader.values.last.to_s.should == date.to_s
      end

      exec("DELETE FROM users WHERE id = ?", id)
    end

    now = DateTime.now

    dates = [
      now.new_offset( (-11 * 3600).to_r / 86400), # GMT -11:00
      now.new_offset( (-9 * 3600 + 10 * 60).to_r / 86400), # GMT -9:10, contrived
      now.new_offset( (-8 * 3600).to_r / 86400), # GMT -08:00
      now.new_offset( (+3 * 3600).to_r / 86400), # GMT +03:00
      now.new_offset( (+5 * 3600 + 30 * 60).to_r / 86400)  # GMT +05:30 (New Delhi)
    ]

    dates.each do |date|
      it "should return #{date.to_s} offset to the current locale's Time Zone if they were inserted using a different timezone" do
        pending "We don't support non-local date input yet"

        dates.each do |date|
          id = insert("INSERT INTO users (name, fired_at) VALUES (?, ?)", 'Sam', date)

          select("SELECT fired_at FROM users WHERE id = ?", [DateTime], id) do |reader|
            reader.values.last.to_s.should == now.to_s
          end

          exec("DELETE FROM users WHERE id = ?", id)
        end
      end
    end

  end

  describe "executing a non-query" do
    it "should return a Result" do
      command = @connection.create_command("INSERT INTO invoices (invoice_number) VALUES ('1234')")
      result = command.execute_non_query
      result.should be_kind_of(DataObjects::Mysql::Result)
    end

    it "should be able to determine the affected_rows" do
      command = @connection.create_command("INSERT INTO invoices (invoice_number) VALUES ('1234')")
      result = command.execute_non_query
      result.to_i.should == 1
    end

    it "should yield the last inserted id" do
      @connection.create_command("TRUNCATE TABLE invoices").execute_non_query

      result = @connection.create_command("INSERT INTO invoices (invoice_number) VALUES ('1234')").execute_non_query
      result.insert_id.should == 1

      result = @connection.create_command("INSERT INTO invoices (invoice_number) VALUES ('3456')").execute_non_query
      result.insert_id.should == 2
    end

    it "should be able to determine the affected_rows" do
      [
        "TRUNCATE TABLE invoices",
        "INSERT INTO invoices (invoice_number) VALUES ('1234')",
        "INSERT INTO invoices (invoice_number) VALUES ('1234')"
      ].each { |q| @connection.create_command(q).execute_non_query }

      result = @connection.create_command("UPDATE invoices SET invoice_number = '3456'").execute_non_query
      result.to_i.should == 2
    end

    it "should raise an error when executing an invalid query" do
      command = @connection.create_command("UPDwhoopsATE invoices SET invoice_number = '3456'")

      lambda { command.execute_non_query }.should raise_error(Exception)
    end

    # it "should raise an error when inserting the wrong typed data" do
    #   command = @connection.create_command("UPDATE invoices SET invoice_number = ?")
    #   command.execute_non_query(1)
    # end

  end

end
