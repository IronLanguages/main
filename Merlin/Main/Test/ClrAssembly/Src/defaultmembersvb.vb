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

Imports System.Reflection

Namespace Merlin.Testing.DefaultMemberSample
    '
    ' you have to define both indexers as default, 
    ' otherwise "cannot overload each other because only one is declared 'Default'"
    '
    ' 'Default' can be applied to only one property name in a class.	
    '
    Public Class ClassWithOverloadDefaultIndexer
        Private _property1(20) As Integer
        Private _property2(20, 30) As Integer

        Default Public Property MyProperty(ByVal arg As Integer) As Integer
            Get
                Return _property1(arg)
            End Get
            Set(ByVal value As Integer)
                _property1(arg) = value
            End Set
        End Property

        Default Public Property MyProperty(ByVal arg1 As Integer, ByVal arg2 As Integer) As Integer
            Get
                Return _property2(arg1, arg2)
            End Get
            Set(ByVal value As Integer)
                _property2(arg1, arg2) = value
            End Set
        End Property
    End Class

    ' does the derived class keep the default member thing? guess no
    Public Class DerivedClass
        Inherits ClassWithOverloadDefaultIndexer

    End Class

    ' value type
    Public Structure StructWithDefaultIndexer
        Private _property As Integer()

        Public Sub Init()
            _property = New Integer(3) {1, 2, 3, 4}
        End Sub
        Default Public Property MyProperty(ByVal arg As Integer) As Integer
            Get
                Return _property(arg)
            End Get
            Set(ByVal value As Integer)
                _property(arg) = value
            End Set
        End Property
    End Structure

    ' the specified default member does not exist
    <DefaultMember("NotExisting")> _
    Public Class ClassWithNotExistingMember
        Private _property1(20) As Integer

        Public Property MyProperty(ByVal arg As Integer) As Integer
            Get
                Return _property1(arg)
            End Get
            Set(ByVal value As Integer)
                _property1(arg) = value
            End Set
        End Property

    End Class

    ' interface declared with default property 
    Public Interface IDefaultIndexer
        Default Property MyProperty(ByVal arg As Integer) As Integer
    End Interface

    Public Structure StructImplementsIDefaultIndexer
        Implements IDefaultIndexer
        Private _dummy As Integer
        Default Public Property MyProperty(ByVal arg As Integer) As Integer Implements IDefaultIndexer.MyProperty
            Get
                Return arg
            End Get
            Set(ByVal value As Integer)
                Flag.Set(arg + value)
            End Set
        End Property
    End Structure

    Public Class ClassImplementsIDefaultIndexer
        Implements IDefaultIndexer

        Default Public Property MyProperty(ByVal arg As Integer) As Integer Implements IDefaultIndexer.MyProperty
            Get
                Return arg
            End Get
            Set(ByVal value As Integer)
                Flag.Set(arg + value)
            End Set
        End Property
    End Class

    '
    ' can not create shared/Static default indexer; otherwise try indexing on type, like Type[i, j] 
    '

End Namespace
