require 'digest'

class Hash
  
  def to_sha2
    string = ""
    keys.sort_by{|k| k.to_s}.each do |k| 
      case self[k]
      when Array
        string << self[k].join
      when Hash
        string << self[k].to_sha2
      else
        string << self[k].to_s
      end
    end
    Digest::SHA2.hexdigest(string)
  end
  
end