Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Windows.Threading

Public MustInherit Class CWindowData
    Inherits AutoNotifiableObject

    ''' <summary>
    ''' Window Values
    ''' </summary>
    Public ReadOnly Property Window As Window

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Set parent window
    ''' </summary>
    ''' <param name="iWindow"></param>
    Public Sub InitWindow(iWindow As Control)
        _Window = iWindow

        'load
        Pause()
        OnLoad()
        Unpause()
    End Sub

    ''' <summary>
    ''' Load data
    ''' </summary>
    Public Overridable Sub OnLoad()
    End Sub

End Class
