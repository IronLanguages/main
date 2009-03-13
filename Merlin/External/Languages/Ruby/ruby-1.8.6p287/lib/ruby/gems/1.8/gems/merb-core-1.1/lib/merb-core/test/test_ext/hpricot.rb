# http://yehudakatz.com/2007/01/27/a-better-assert_select-assert_elements/
# based on assert_elements
# Author: Yehuda Katz
# Email:  wycats @nospam@ gmail.com
# Web:    http://www.yehudakatz.com
#
# which was based on HpricotTestHelper
# Author: Luke Redpath
# Email: contact @nospam@ lukeredpath.co.uk
# Web: www.lukeredpath.co.uk / opensource.agileevolved.com

class Hpricot::Elem
  def contain?(value)
    self.inner_text.include?(value)
  end
  
  alias_method :contains?, :contain?

  def match?(regex)
    self.inner_text.match(regex)
  end
  
  alias_method :matches?, :match?
  
  # courtesy of 'thomas' from the comments
  # of _whys blog - get in touch if you want a better credit!
  def inner_text
    self.children.collect do |child|
      child.is_a?(Hpricot::Text) ? child.content : ((child.respond_to?("inner_text") && child.inner_text) || "")
    end.join.strip
  end
end
