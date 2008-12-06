class R < Range
  def exclude_end?
    p "foooo"
    true
  end
end

/(a)(b)(c)(d)(e)?(f)?(g)?(h)?/ =~ "abcd"
p $1, $2, $3, $4, $5, $6, $7, $8, $9, $10
p $+
p $~[R.new(1,-1)]


x = [1,2,3,4,5,6]
p x[R.new(1,-1)]