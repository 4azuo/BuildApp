<AttributeUsage(AttributeTargets.Property)>
Public Class NotifyMethodAttribute
    Inherits Attribute

    Public ReadOnly Property Methods As String()

    Public Sub New(ParamArray iMethods As String())
        Methods = iMethods
    End Sub
End Class
