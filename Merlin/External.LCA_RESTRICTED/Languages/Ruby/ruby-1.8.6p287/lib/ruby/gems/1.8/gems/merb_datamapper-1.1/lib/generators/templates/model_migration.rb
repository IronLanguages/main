migration <%= current_migration_nr + 1 %>, :<%= migration_name %>  do
  up do
<% unless properties.empty? -%>
    create_table :<%= table_name %> do
  <% properties.each do |p| -%>
    column :<%= p.name -%>, <%= p.type %>
  <% end -%>
  end
<% end -%>
  end

  down do
    drop_table :<%= table_name %>
  end
end
