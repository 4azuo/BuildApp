Imports System.Globalization

Public Class ValueToVisibility
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then Return Visibility.Collapsed
        If TypeOf value Is Boolean AndAlso Not value Then Return Visibility.Collapsed
        If TypeOf value Is String AndAlso String.IsNullOrEmpty(value) Then Return Visibility.Collapsed
        Return Visibility.Visible
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class