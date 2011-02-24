module FloatSpecs
  class CoerceToFloat
    def coerce(other)
      [other, 1.0]
    end
  end
end
