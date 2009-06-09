#
# Adds some methods to FOX's FXSettings class
#
module Fox
  class FXSettings
    #
    # Iterate over sections (where each section is a dictionary).
    #
    def each_section
      pos = first
      while pos < getTotalSize()
        yield data(pos)
        pos = self.next(pos)
      end
    end
  end
end

