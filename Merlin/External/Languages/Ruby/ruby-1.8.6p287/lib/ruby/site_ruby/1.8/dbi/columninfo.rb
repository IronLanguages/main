class ColumnInfo < Hash

   # Creates and returns a ColumnInfo object.  This represents metadata for
   # columns within a given table, such as the data type, whether or not the
   # the column is a primary key, etc.
   #
   # ColumnInfo is a subclass of Hash.
   #
   def initialize(hash=nil)
      self.update(hash) if hash
   end
   
   # Returns the column's name.
   def name
      self['name']
   end
   
   # Sets the column's name.
   def name=(val)
      self['name'] = val
   end
   
   # Returns a portable integer representation of the column's type.  Here are
   # the constant names (under DBI) and their respective values:
   #
   # SQL_CHAR      = 1
   # SQL_NUMERIC   = 2
   # SQL_DECIMAL   = 3
   # SQL_INTEGER   = 4
   # SQL_SMALLINT  = 5
   # SQL_FLOAT     = 6
   # SQL_REAL      = 7
   # SQL_DOUBLE    = 8
   # SQL_DATE      = 9 
   # SQL_TIME      = 10
   # SQL_TIMESTAMP = 11
   # SQL_VARCHAR   = 12
   #
   # SQL_LONGVARCHAR   = -1
   # SQL_BINARY        = -2
   # SQL_VARBINARY     = -3
   # SQL_LONGVARBINARY = -4
   # SQL_BIGINT        = -5
   # SQL_BIT           = -7
   # SQL_TINYINT       = -6
   #
   def sql_type
      self['sql_type']
   end
   
   # Sets the integer representation for the column's type.
   def sql_type=(val)
      self['sql_type'] = val
   end
   
   # A string representation of the column's type, e.g. 'date'.
   def type_name
      self['type_name']
   end
   
   # Sets the representation for the column's type.
   def type_name=(val)
      self['type_name'] = val
   end
   
   # Returns the precision, i.e. number of bytes or digits.
   def precision
      self['precision']
   end
   
   # Sets the precision, i.e. number of bytes or digits.
   def precision=(val)
      self['precision'] = val
   end
   
   # Returns the number of digits from right.
   def scale
      self['scale']
   end
   
   # Sets the number of digits from right.
   def scale=(val)
      self['scale'] = val
   end
   
   # Returns the default value for the column, or nil if not set.
   def default
      self['default']
   end
   
   # Sets the default value for the column.
   def default=(val)
      self['default'] = val
   end
   
   # Returns whether or not the column is may contain a NULL.
   def nullable
      self['nullable']
   end
   
   # Sets whether or not the column may contain a NULL.
   def nullable=(val)
      self['nullable'] = val
   end
   
   # Returns whether or not the column is indexed.
   def indexed
      self['indexed']
   end
   
   # Sets whether or not the column is indexed.
   def indexed=(val)
      self['indexed'] = 'val'
   end
   
   # Returns whether or not the column is a primary key.
   def primary
      self['primary']
   end
   
   # Sets whether or not the column is a primary key.
   def primary=(val)
      self['primary'] = val
   end
   
   # Returns whether or not data in the column must be unique.
   def unique
      self['unique']
   end
   
   # Sets whether or not data in the column must be unique.
   def unique=(val)
      self['unique'] = val
   end

   # Aliases
   alias nullable? nullable
   alias is_nullable? nullable

   alias indexed? indexed
   alias is_indexed? indexed

   alias primary? primary
   alias is_primary? primary

   alias unique? unique
   alias is_unique unique

   alias size precision
   alias size= precision=
   alias length precision
   alias length= precision=

   alias decimal_digits scale
   alias decimal_digits= scale=
end


