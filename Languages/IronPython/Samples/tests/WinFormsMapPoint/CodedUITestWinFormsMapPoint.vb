Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Windows.Input
Imports Microsoft.VisualStudio.TestTools.UITest.Extension
Imports Microsoft.VisualStudio.TestTools.UITesting
Imports Microsoft.VisualStudio.TestTools.UITesting.Keyboard

<CodedUITest()>
Public Class CodedUITestWinFormsMapPoint

    <TestMethod()>
    Public Sub CodedUITestMethod1()
        '            
        ' To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        ' For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        '
        Me.UIMap.AssertFormV1()
        Me.UIMap.CloseFormV1()
        Me.UIMap.ClickButtonFormV2()
        Me.UIMap.AssertFormV2()
        Me.UIMap.CloseFormV2()
        Me.UIMap.InsertMessageFormV3()
        Me.UIMap.AssertFormV3()
        Me.UIMap.CloseFormV3()
        Me.UIMap.InsertMessageFormV4()
        Me.UIMap.AssertFormV4()
        Me.UIMap.CloseFormV4()
        Me.UIMap.InsertMessageFormV5()
        Me.UIMap.AssertFormV5()
        Me.UIMap.CloseFormV5()
        Me.UIMap.InsertMessageFormV6()
        Me.UIMap.AssertFormV6()
        Me.UIMap.CloseFormV6()
        Me.UIMap.GetMapFormV7()
        Me.UIMap.AssertFormV7()
        Me.UIMap.CloseFormV7()
        Me.UIMap.GetMapFormV8()
        Me.UIMap.AssertFormV8()
        Me.UIMap.CloseFormV8()
    End Sub

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    '' Use TestInitialize to run code before running each test
    '<TestInitialize()> Public Sub MyTestInitialize()
    '    '
    '    ' To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
    '    ' For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
    '    '
    'End Sub

    '' Use TestCleanup to run code after each test has run
    '<TestCleanup()> Public Sub MyTestCleanup()
    '    '
    '    ' To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
    '    ' For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
    '    '
    'End Sub

#End Region

    '''<summary>
    '''Gets or sets the test context which provides
    '''information about and functionality for the current test run.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property

    Private testContextInstance As TestContext

    Public ReadOnly Property UIMap As WinFormsMapPoint.UIMap
        Get
            If (Me.map Is Nothing) Then
                Me.map = New WinFormsMapPoint.UIMap()
            End If

            Return Me.map
        End Get
    End Property
    Private map As WinFormsMapPoint.UIMap
End Class
