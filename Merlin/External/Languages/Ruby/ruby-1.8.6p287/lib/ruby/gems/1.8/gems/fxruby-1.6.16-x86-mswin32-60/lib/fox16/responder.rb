# Enumeration
def enum(start, count)
  (start...(start+count)).to_a
end

module Responder
  # Generates identifiers as class constants. Originally submitted by
  # Sean O'Halpin, slightly modified by Lyle.
  def identifier(*ids)
    ids << :ID_LAST
    base = self.class.superclass::ID_LAST
    vals = enum(base, ids.size)
    ids.each_index do |i|
      unless self.class.const_defined?(ids[i])
        self.class.class_eval("#{ids[i].id2name} = #{vals[i]}")
      end
    end
  end

  # Return the array of (selector -> func) associations
  def messageMap
    unless instance_variables.include? "@assocs"
      @assocs = []
    end
    @assocs
  end

  # Look up array index of this message map entry
  def assocIndex(lo, hi)
    currIndex = -1
    assocs = messageMap
    assocs.each_index do |i|
      if assocs[i][0] == lo && assocs[i][1] == hi
        currIndex = i
      end
    end
    currIndex
  end

  # Add new or replace existing map entry
  def addMapEntry(lo, hi, func)
    func = func.intern if func.is_a? String
    currIndex = assocIndex(lo, hi)
    if currIndex < 0
      messageMap.push([lo, hi, func])
    else
      messageMap[currIndex] = [lo, hi, func]
    end
  end

  # Define range of function types
  def FXMAPTYPES(typelo, typehi, func)
    addMapEntry(Fox.MKUINT(Fox::MINKEY, typelo), Fox.MKUINT(Fox::MAXKEY, typehi), func)
  end

  # Define one function type
  def FXMAPTYPE(type, func)
    addMapEntry(Fox.MKUINT(Fox::MINKEY, type), Fox.MKUINT(Fox::MAXKEY, type), func)
  end

  # Define range of functions
  def FXMAPFUNCS(type, keylo, keyhi, func)
    addMapEntry(Fox.MKUINT(keylo, type), Fox.MKUINT(keyhi, type), func)
  end

  # Define one function
  def FXMAPFUNC(type, id, func)
    addMapEntry(Fox.MKUINT(id, type), Fox.MKUINT(id, type), func)
  end
end
