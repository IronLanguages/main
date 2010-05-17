require 'System.Data'

module DBI

  PROVIDERS = {
      :odbc => "System.Data.Odbc",
      :oledb => "System.Data.OleDb",
      :oracle => "System.Data.OracleClient",
      :mssql => "System.Data.SqlClient",
      :sqlce => "System.Data.SqlServerCe.3.5",
      :mysql => "MySql.Data.MySqlClient",
      :sqlite => "System.Data.SQLite",
      :postgresql => "Npgsql"
    }

  # Implements the basic functionality that constitutes a Driver
  #
  # Drivers do not have a direct interface exposed to the user; these methods
  # are mostly for DBD authors.
  #
  # As with DBI::BaseDatabase, "DBD Required" and "DBD Optional" will be used
  # to explain the same requirements.
  #
  class BaseDriver < Base

    DEFAULT_PROVIDER = "System.Data.SqlServer"

    include System::Data::Common

    def initialize(dbi_version, key)
      major, minor = dbi_version.split(".").collect { |x| x.to_i }
      dbi_major, dbi_minor = DBI::VERSION.split(".").collect { |x| x.to_i }
      unless major == dbi_major and minor == dbi_minor
        raise InterfaceError, "Wrong DBD API version used"
      end
      @provider = PROVIDERS[key]
      load_factory @provider
    end

    # Connect to the database. DBD Required.
    def connect(dbname, user, auth, attr)
      connection = factory.create_connection
      connection.connection_string = dbname
      connection.open
      return create_database(connection, attr);
    rescue RuntimeError, System::Data::SqlClient::SqlException => err
      raise DBI::DatabaseError.new(err.message)
    end

    def create_database(connection, attr)
      raise NotImplementedError
    end

    def factory
      load_factory(@provider) unless defined? @factory and not @factory.nil?
      @factory      
    end

    # Default u/p information in an array.
    def default_user
      ['', '']
    end

    # Default attributes to set on the DatabaseHandle.
    def default_attributes
      {}
    end

    # Return the data sources available to this driver. Returns an empty
    # array per default.
    def data_sources
      []
    end

    # Disconnect all DatabaseHandles. DBD Required.
    def disconnect_all
      raise NotImplementedError
    end

    def load_factory(provider_name)
      return nil if defined? @factory

      provider = (provider_name.nil? || provider_name.empty?) ? DEFAULT_PROVIDER : provider_name
      @factory = DbProviderFactories.get_factory provider
    end

  end # class BaseDriver
end
