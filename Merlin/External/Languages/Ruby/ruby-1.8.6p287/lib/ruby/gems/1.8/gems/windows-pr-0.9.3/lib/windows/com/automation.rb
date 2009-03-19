require 'windows/api'

module Windows
   module COM
      module Automation
         API.auto_namespace = 'Windows::COM::Automation'
         API.auto_constant  = true
         API.auto_method    = true
         API.auto_unicode   = false

         API.new('BstrFromVector', 'PP', 'L', 'oleaut32')
         API.new('CreateErrorInfo', 'P', 'L', 'oleaut32')
         API.new('CreateTypeLib2', 'PPP', 'L', 'oleaut32')
         API.new('DispGetIDsOfNames', 'PPLP', 'L', 'oleaut32')
         API.new('DispGetParam', 'PLLPP', 'L', 'oleaut32')
         API.new('DispInvoke', 'PPPLPPPP', 'L', 'oleaut32')
         API.new('GetActiveObject', 'PPP', 'L', 'oleaut32')
         API.new('LoadRegTypeLib', 'PLLLP', 'L', 'oleaut32')
         API.new('LoadTypeLib', 'PPP', 'L', 'oleaut32')
         API.new('LoadTypeLibEx', 'PPP', 'L', 'oleaut32')
         API.new('RegisterActiveObject', 'PPLP', 'L', 'oleaut32')
         API.new('RevokeActiveObject', 'LP', 'L', 'oleaut32')       
         API.new('RegisterTypeLib', 'PPP', 'L', 'oleaut32')
         API.new('SafeArrayAccessData', 'PP', 'L', 'oleaut32')
         API.new('SafeArrayAllocData', 'P', 'L', 'oleaut32')
         API.new('SafeArrayAllocDescriptor', 'LP', 'L', 'oleaut32')
         API.new('SafeArrayCopy', 'PP', 'L', 'oleaut32')
         API.new('SafeArrayCopyData', 'PP', 'L', 'oleaut32')
         API.new('SafeArrayCreate', 'LLP', 'P', 'oleaut32')
         API.new('SafeArrayCreateVector', 'LLL', 'P', 'oleaut32')
         API.new('SafeArrayDestroy', 'P', 'L', 'oleaut32')
         API.new('SafeArrayDestroyData', 'P', 'L', 'oleaut32')
         API.new('SafeArrayDestroyDescriptor', 'P', 'L', 'oleaut32')
         API.new('SafeArrayGetDim', 'P', 'L', 'oleaut32')
         API.new('SafeArrayGetElement', 'PLP', 'L', 'oleaut32')
         API.new('SafeArrayGetElemsize', 'P', 'L', 'oleaut32')
         API.new('SafeArrayGetLBound', 'PLP', 'L', 'oleaut32')
         API.new('SafeArrayGetUBound', 'PLP', 'L', 'oleaut32')
         API.new('SafeArrayLock', 'P', 'L', 'oleaut32')
         API.new('SafeArrayPtrOfIndex', 'PPP', 'L', 'oleaut32')
         API.new('SafeArrayPutElement', 'PPP', 'L', 'oleaut32')
         API.new('SafeArrayRedim', 'PP', 'L', 'oleaut32')
         API.new('SafeArrayUnaccessData', 'P', 'L', 'oleaut32')
         API.new('SafeArrayUnlock', 'P', 'L', 'oleaut32')
         API.new('SetErrorInfo', 'LP', 'L', 'oleaut32')
         API.new('SysAllocString', 'P', 'P', 'oleaut32')
         API.new('SysAllocStringByteLen', 'PI', 'P', 'oleaut32')
         API.new('SysFreeString', 'P', 'L', 'oleaut32')
         API.new('SysReAllocString', 'PP', 'L', 'oleaut32')
         API.new('SysReAllocStringLen', 'PPI', 'P', 'oleaut32')
         API.new('SysStringByteLen', 'P', 'L', 'oleaut32')
         API.new('SysStringLen', 'P', 'L', 'oleaut32')
         API.new('SystemTimeToVariantTime', 'PP', 'I', 'oleaut32')
         API.new('UnRegisterTypeLib', 'PLLLL', 'I', 'oleaut32')
         API.new('VectorFromBstr', 'PP', 'L', 'oleaut32')
      end
   end
end
