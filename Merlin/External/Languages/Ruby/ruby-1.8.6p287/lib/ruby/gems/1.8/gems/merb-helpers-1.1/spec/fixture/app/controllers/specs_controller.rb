# Helpers are tested going through the full stack.
# to keep things isolated each helper has its own controller subclass
# if you are working on a new helper, please consider creating 
# a new spec controller subclass
#
# Remember that your helper spec views
# will be located in a folder named after the controller you use

class SpecController < Merb::Controller
  layout nil
end
