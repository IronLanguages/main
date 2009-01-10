' ****************************************************************************
' 
'  Copyright (c) Microsoft Corporation. 
' 
'  This source code is subject to terms and conditions of the Microsoft Public License. A 
'  copy of the license can be found in the License.html file at the root of this distribution. If 
'  you cannot locate the  Microsoft Public License, please send an email to 
'  ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
'  by the terms of the Microsoft Public License.
' 
'  You must not remove this notice, or any other, from this software.
' 
' 
' ***************************************************************************

Namespace Merlin.Testing.BaseClass
  Public Interface IVbIndexer10
    Default Property IntProperty(ByVal index As Integer) As Integer
  End Interface

  Public Interface IVbIndexer11
    Default Property StrProperty(ByVal index1 As Integer, ByVal index2 As Integer) As String
  End Interface

  Public Interface IVbIndexer20
    Property DoubleProperty(ByVal index As Integer) As Double
  End Interface

  Public Class CVbIndexer30
    Public Overridable Property StrProperty(ByVal index1 As Integer, ByVal index2 As Integer) As String
      Get
        Return "abc"
      End Get
      Set(ByVal value As String)

      End Set
    End Property
  End Class

  Public Class VbCallback
    Public Shared Sub Act(ByVal arg As IVbIndexer10)
      arg.IntProperty(10) = arg.IntProperty(100) + 1000
    End Sub

    Public Shared Sub Act(ByVal arg As IVbIndexer20)
      arg.DoubleProperty(1) = arg.DoubleProperty(2) + 0.003
    End Sub
  End Class
End Namespace
