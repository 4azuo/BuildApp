Imports System.Globalization

Public Class TextToRowNum
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing OrElse String.IsNullOrEmpty(value) Then
            Return 0
        End If
        Return CType(value, String).Split(New String() {vbNewLine, vbLf}, StringSplitOptions.RemoveEmptyEntries).Length
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class