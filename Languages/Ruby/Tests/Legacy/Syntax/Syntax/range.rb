## inclusive range

10..30
10.1..20
10..20.5
-10..30
10..-40
-40..-50
-60..-10

'a'..'z'
'Z'..'A'

x = 9
x..x
x..-x
# Scenario: valid
# Default: pass

10
..15
# Scenario: line starts with ..
# Default: syntax error

10..
20
10 ..
20
# Scenario: new line
# Default: pass

10. .20
# Scenario: space inside
# Default: syntax error

10 .. 30
10.. 40
10 ..50
# Scenario: space
# Default: pass

x = []
x..10
# Scenario: array as range start
# Default: ArgumentError
# ParseOnly: pass

## exclusive range

10...20
20...10
10.20...-433.03
# Scenario: valid
# Default: pass

10
...20
# Scenario: newline
# Default: syntax error

10 ... 
-30
# Scenario: newline
# Default: pass

10.. .20
# Scenario: space inside
# Default: syntax error

10. ..20
# Scenario: space inside
# Default: syntax error

10 ... 20
10... 30
10 ...40
# Scenario: space
# Default: pass

10..20.30
# Scenario: valid
# Default: pass

10..20..30
# Scenario: invalid
# Default: pass

