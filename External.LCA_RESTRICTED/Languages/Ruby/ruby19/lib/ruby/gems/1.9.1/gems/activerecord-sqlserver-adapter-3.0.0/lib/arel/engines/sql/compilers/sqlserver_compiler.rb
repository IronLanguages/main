module Arel
  class Lock < Compound
    def initialize(relation, locked)
      super(relation)
      @locked = true == locked ? "WITH(HOLDLOCK, ROWLOCK)" : locked
    end
  end
end

module Arel
  module SqlCompiler
    class SQLServerCompiler < GenericCompiler
      
      def select_sql
        if complex_count_sql?
          select_sql_with_complex_count
        elsif relation.skipped
          select_sql_with_skipped
        else
          select_sql_without_skipped
        end
      end
      
      def delete_sql
        build_query \
          "DELETE #{taken_clause if relation.taken.present?}".strip,
          "FROM #{relation.table_sql}",
          ("WHERE #{relation.wheres.collect(&:to_sql).join(' AND ')}" unless relation.wheres.blank? )
      end
      
      
      protected
      
      def complex_count_sql?
        projections = relation.projections
        projections.first.is_a?(Arel::Count) && projections.size == 1 &&
          (relation.taken.present? || relation.wheres.present?) && relation.joins(self).blank?
      end
      
      def taken_only?
        relation.taken.present? && relation.skipped.blank?
      end
      
      def taken_clause
        "TOP (#{relation.taken.to_i}) "
      end
      
      def expression_select?
        relation.select_clauses.any? { |sc| sc.match /(COUNT|SUM|MAX|MIN|AVG)\s*\(.*\)/ }
      end
      
      def eager_limiting_select?
        single_distinct_select? && taken_only? && relation.group_clauses.blank?
      end
      
      def single_distinct_select?
        relation.select_clauses.size == 1 && relation.select_clauses.first.to_s.include?('DISTINCT')
      end
      
      def all_select_clauses_aliased?
        relation.select_clauses.all? do |sc|
          sc.split(',').all? { |c| c.include?(' AS ') }
        end
      end
      
      def select_sql_with_complex_count
        joins   = correlated_safe_joins
        wheres  = relation.where_clauses
        groups  = relation.group_clauses
        havings = relation.having_clauses
        orders  = relation.order_clauses
        taken   = relation.taken.to_i
        skipped = relation.skipped.to_i
        top_clause = "TOP (#{taken+skipped}) " if relation.taken.present?
        build_query \
          "SELECT COUNT([count]) AS [count_id]",
          "FROM (",
            "SELECT #{top_clause}ROW_NUMBER() OVER (ORDER BY #{unique_orders(rowtable_order_clauses).join(', ')}) AS [__rn],",
            "1 AS [count]",
            "FROM #{relation.from_clauses}",
            (locked unless locked.blank?),
            (joins unless joins.blank?),
            ("WHERE #{wheres.join(' AND ')}" unless wheres.blank?),
            ("GROUP BY #{groups.join(', ')}" unless groups.blank?),
            ("HAVING #{havings.join(' AND ')}" unless havings.blank?),
            ("ORDER BY #{unique_orders(orders).join(', ')}" unless orders.blank?),
          ") AS [__rnt]",
          "WHERE [__rnt].[__rn] > #{relation.skipped.to_i}"
      end
      
      def select_sql_without_skipped(windowed=false)
        selects = relation.select_clauses
        joins   = correlated_safe_joins
        wheres  = relation.where_clauses
        groups  = relation.group_clauses
        havings = relation.having_clauses
        orders  = relation.order_clauses
        if windowed
          selects = expression_select? ? selects : selects.map{ |sc| clause_without_expression(sc) }          
        elsif eager_limiting_select?
          groups = selects.map { |sc| clause_without_expression(sc) }
          selects = selects.map { |sc| "#{taken_clause}#{clause_without_expression(sc)}" }
          orders = orders.map do |oc|
            oc.split(',').reject(&:blank?).map do |c|
              max = c =~ /desc\s*/i
              c = clause_without_expression(c).sub(/(asc|desc)/i,'').strip
              max ? "MAX(#{c})" : "MIN(#{c})"
            end.join(', ')
          end
        elsif taken_only?
          fsc = "#{taken_clause}#{selects.first}"
          selects = selects.tap { |sc| sc.shift ; sc.unshift(fsc) }
        end
        build_query(
          (windowed ? selects.join(', ') : "SELECT #{selects.join(', ')}"),
          "FROM #{relation.from_clauses}",
          (locked unless locked.blank?),
          (joins unless joins.blank?),
          ("WHERE #{wheres.join(' AND ')}" unless wheres.blank?),
          ("GROUP BY #{groups.join(', ')}" unless groups.blank?),
          ("HAVING #{havings.join(' AND ')}" unless havings.blank?),
          ("ORDER BY #{unique_orders(orders).join(', ')}" if orders.present? && !windowed))
      end
      
      def select_sql_with_skipped
        tc = taken_clause if relation.taken.present? && !single_distinct_select?
        build_query \
          "SELECT #{tc}#{rowtable_select_clauses.join(', ')}",
          "FROM (",
            "SELECT ROW_NUMBER() OVER (ORDER BY #{unique_orders(rowtable_order_clauses).join(', ')}) AS [__rn],",
            select_sql_without_skipped(true),
          ") AS [__rnt]",
          "WHERE [__rnt].[__rn] > #{relation.skipped.to_i}"
      end
      
      def rowtable_select_clauses
        if single_distinct_select?
          ::Array.wrap(relation.select_clauses.first.dup.tap do |sc|
            sc.sub! 'DISTINCT', "DISTINCT #{taken_clause if relation.taken.present?}".strip
            sc.sub! table_name_from_select_clause(sc), '__rnt'
            sc.strip!
          end)
        elsif relation.join? && all_select_clauses_aliased?
          relation.select_clauses.map do |sc|
            sc.split(',').map { |c| c.split(' AS ').last.strip  }.join(', ')
          end
        elsif expression_select?
          ['*']
        else
          relation.select_clauses.map do |sc|
            sc.gsub /\[#{relation.table.name}\]\./, '[__rnt].'
          end
        end
      end
      
      def rowtable_order_clauses
        orders = relation.order_clauses
        if orders.present?
          orders
        elsif relation.join?
          table_names_from_select_clauses.map { |tn| quote("#{tn}.#{pk_for_table(tn)}") }
        else
          [quote("#{relation.table.name}.#{relation.primary_key}")]
        end
      end
      
      def limited_update_conditions(conditions,taken)
        quoted_primary_key = engine.connection.quote_column_name(relation.primary_key)
        conditions = " #{conditions}".strip
        build_query \
          "WHERE #{quoted_primary_key} IN",
          "(SELECT #{taken_clause if relation.taken.present?}#{quoted_primary_key} FROM #{engine.connection.quote_table_name(relation.table.name)}#{conditions})"
      end
      
      def quote(value)
        engine.connection.quote_column_name(value)
      end
      
      def pk_for_table(table_name)
        engine.connection.primary_key(table_name)
      end
      
      def clause_without_expression(clause)
        clause.to_s.split(',').map do |c|
          c.strip!
          c.sub!(/^(COUNT|SUM|MAX|MIN|AVG)\s*(\((.*)\))?/,'\3')
          c.sub!(/^DISTINCT\s*/,'')
          c.sub!(/TOP\s*\(\d+\)\s*/i,'')
          c.strip
        end.join(', ')
      end
      
      def unqualify_table_name(table_name)
        table_name.to_s.split('.').last.tr('[]','')
      end
      
      def table_name_from_select_clause(sc)
        parts = clause_without_expression(sc).split('.')
        tn = parts.third ? parts.second : (parts.second ? parts.first : nil)
        tn ? tn.tr('[]','') : nil
      end

      def table_names_from_select_clauses
        relation.select_clauses.map do |sc|
          sc.split(',').map { table_name_from_select_clause(sc) }
        end.flatten.compact.uniq
      end
      
      def unique_orders(orders)
        existing_columns = {}
        orders.inject([]) do |queued_orders, order|
          table_column, dir = clause_without_expression(order).split
          table_column = table_column.tr('[]','').split('.')
          table, column = table_column.size == 2 ? table_column : table_column.unshift('')
          existing_columns[table] ||= []
          unless existing_columns[table].include?(column)
            existing_columns[table] << column
            queued_orders << order 
          end
          queued_orders
        end
      end
      
      def correlated_safe_joins
        joins = relation.joins(self)
        if joins.present?
          find_and_fix_uncorrelated_joins
          relation.joins(self)
        else
          joins
        end
      end
      
      def find_and_fix_uncorrelated_joins
        join_relation = relation
        while join_relation.present?
          return join_relation if uncorrelated_inner_join_relation?(join_relation)
          join_relation = join_relation.relation rescue nil
        end
      end

      def uncorrelated_inner_join_relation?(r)
        if r.is_a?(Arel::StringJoin) && r.relation1.is_a?(Arel::OuterJoin) && 
            r.relation2.is_a?(String) && r.relation2.starts_with?('INNER JOIN')
          outter_join_table1 = r.relation1.relation1.table
          outter_join_table2 = r.relation1.relation2.table
          string_join_table_info = r.relation2.split(' ON ').first.sub('INNER JOIN ','')
          return nil if string_join_table_info.include?(' AS ') # Assume someone did something right.
          string_join_table_name = unqualify_table_name(string_join_table_info)
          uncorrelated_table1 = string_join_table_name == outter_join_table1.name && string_join_table_name == outter_join_table1.alias.name
          uncorrelated_table2 = string_join_table_name == outter_join_table2.name && string_join_table_name == outter_join_table2.alias.name
          if uncorrelated_table1 || uncorrelated_table2
            on_index = r.relation2.index(' ON ')
            r.relation2.insert on_index, " AS [#{string_join_table_name}_crltd]"
            r.relation2.sub! "[#{string_join_table_name}].", "[#{string_join_table_name}_crltd]."
            return r
          else
            return nil
          end
        end
      rescue
        nil
      end
      
    end
  end
end
