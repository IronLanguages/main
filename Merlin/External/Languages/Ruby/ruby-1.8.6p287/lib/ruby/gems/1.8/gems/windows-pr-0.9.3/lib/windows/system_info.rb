require 'windows/api'

module Windows
   module SystemInfo
      API.auto_namespace = 'Windows::SystemInfo'
      API.auto_constant  = true
      API.auto_method    = true
      API.auto_unicode   = true

      # Obsolete processor info constants
      PROCESSOR_INTEL_386     = 386
      PROCESSOR_INTEL_486     = 486
      PROCESSOR_INTEL_PENTIUM = 586
      PROCESSOR_INTEL_IA64    = 2200
      PROCESSOR_AMD_X8664     = 8664

      # Enum COMPUTER_NAME_FORMAT
      ComputerNameNetBIOS                    = 0
      ComputerNameDnsHostname                = 1
      ComputerNameDnsDomain                  = 2
      ComputerNameDnsFullyQualified          = 3
      ComputerNamePhysicalNetBIOS            = 4
      ComputerNamePhysicalDnsHostname        = 5
      ComputerNamePhysicalDnsDomain          = 6 
      ComputerNamePhysicalDnsFullyQualified  = 7
      ComputerNameMax                        = 8
      
      API.new('ExpandEnvironmentStrings', 'PPL', 'L')
      API.new('GetComputerName', 'PP', 'B')
      API.new('GetComputerNameEx', 'PPP', 'B')
      API.new('GetSystemInfo', 'P', 'V')
      API.new('GetUserName', 'PP', 'B', 'advapi32')
      API.new('GetUserNameEx', 'LPP', 'B', 'secur32')
      API.new('GetVersion', 'V', 'L')
      API.new('GetVersionEx', 'P', 'B')
      API.new('GetWindowsDirectory', 'PI', 'I')

      # XP or later
      begin
         API.new('GetSystemWow64Directory', 'PI', 'I')
      rescue Windows::API::Error
         # Do nothing. Users must check for function.
      end
   end
end
