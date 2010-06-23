class TableWillNeverBeCreated < ActiveRecord::Migration
  
  def self.up
    create_table(:sqlserver_trans_table1) {  }
    create_table(:sqlserver_trans_table2) { raise ActiveRecord::StatementInvalid }
  end
  
  def self.down
  end
  
end
