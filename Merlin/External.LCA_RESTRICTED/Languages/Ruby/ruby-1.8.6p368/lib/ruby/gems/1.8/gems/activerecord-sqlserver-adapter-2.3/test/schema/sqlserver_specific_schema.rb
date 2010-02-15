ActiveRecord::Schema.define do
  
  create_table :table_with_real_columns, :force => true do |t|
    t.column :real_number, :real
  end
  
  create_table :defaults, :force => true do |t|
    t.column :positive_integer, :integer, :default => 1
    t.column :negative_integer, :integer, :default => -1
    t.column :decimal_number, :decimal, :precision => 3, :scale => 2, :default => 2.78
  end
  
  create_table :string_defaults, :force => true do |t|
    t.column :string_with_null_default, :string, :default => nil
    t.column :string_with_pretend_null_one, :string, :default => 'null'
    t.column :string_with_pretend_null_two, :string, :default => '(null)'
    t.column :string_with_pretend_null_three, :string, :default => 'NULL'
    t.column :string_with_pretend_null_four, :string, :default => '(NULL)'
    t.column :string_with_pretend_paren_three, :string, :default => '(3)'
    t.column :string_with_multiline_default, :string, :default => "Some long default with a\nnew line."
  end
  
  create_table :sql_server_chronics, :force => true do |t|
    t.column :date,       :date
    t.column :time,       :time
    t.column :datetime,   :datetime
    t.column :timestamp,  :timestamp
    t.column :smalldatetime, :smalldatetime
  end
  
  create_table(:fk_test_has_fks, :force => true) { |t| t.column(:fk_id, :integer, :null => false) }
  create_table(:fk_test_has_pks, :force => true) { }
  execute <<-ADDFKSQL
    ALTER TABLE fk_test_has_fks 
    ADD CONSTRAINT FK__fk_test_has_fk_fk_id
    FOREIGN KEY (#{quote_column_name('fk_id')}) 
    REFERENCES #{quote_table_name('fk_test_has_pks')} (#{quote_column_name('id')})
  ADDFKSQL
  
  create_table :sql_server_unicodes, :force => true do |t|
    t.column :nchar,          :nchar
    t.column :nvarchar,       :nvarchar
    t.column :ntext,          :ntext
    t.column :ntext_10,       :ntext,     :limit => 10
    t.column :nchar_10,       :nchar,     :limit => 10
    t.column :nvarchar_100,   :nvarchar,  :limit => 100
    if ActiveRecord::Base.connection.sqlserver_2005? || ActiveRecord::Base.connection.sqlserver_2008?
      t.column :nvarchar_max,     :nvarchar_max 
      t.column :nvarchar_max_10,  :nvarchar_max, :limit => 10
    end
  end
  
  create_table :sql_server_strings, :force => true do |t|
    t.column :char,     :char
    t.column :char_10,  :char,  :limit => 10
    if ActiveRecord::Base.connection.sqlserver_2005? || ActiveRecord::Base.connection.sqlserver_2008?
      t.column :varchar_max,     :varchar_max 
      t.column :varchar_max_10,  :varchar_max, :limit => 10
    end
  end
  
  create_table :sql_server_binary_types, :force => true do |t|
    # TODO: Add some different native binary types and test.
  end
  
  create_table :sql_server_edge_schemas, :force => true do |t|
    t.string :description
    t.column :bigint, :bigint
    t.column :tinyint, :tinyint
  end
  
  execute "IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'customers_view') DROP VIEW customers_view"
  execute <<-CUSTOMERSVIEW
    CREATE VIEW customers_view AS
      SELECT id, name, balance
      FROM customers
  CUSTOMERSVIEW

  execute "IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'string_defaults_view') DROP VIEW string_defaults_view"
  execute <<-STRINGDEFAULTSVIEW
    CREATE VIEW string_defaults_view AS
      SELECT id, string_with_pretend_null_one as pretend_null
      FROM string_defaults
  STRINGDEFAULTSVIEW
  
  execute "IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'string_defaults_big_view') DROP VIEW string_defaults_big_view"
  execute <<-STRINGDEFAULTSBIGVIEW
    CREATE VIEW string_defaults_big_view AS
      SELECT id, string_with_pretend_null_one as pretend_null
      /*#{'x'*4000}}*/
      FROM string_defaults
  STRINGDEFAULTSBIGVIEW
  
end
