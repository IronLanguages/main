
def m
    1, 
end 

# Scenario: comma at the end 
# Default: syntax error

def m
    (1, )
end 

# Scenario: parenthesis around; simply (1) is ok
# Default: syntax error

def m
  1, 2
end 

# Scenario: return a python-like tuple
# Default: syntax error

def m
    (1, 2)
end 

# Scenario: return a python-like tuple
# Default: syntax error