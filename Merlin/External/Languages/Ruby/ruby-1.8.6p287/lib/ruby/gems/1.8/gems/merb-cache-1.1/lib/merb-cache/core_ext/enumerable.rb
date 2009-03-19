module Enumerable
  def capture_first
    each do |o|
      return yield(o) || next
    end

    nil
  end
end