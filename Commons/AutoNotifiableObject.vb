Imports System.ComponentModel
Imports System.Reflection
Imports System.Windows.Threading
Imports System.CodeDom.Compiler
Imports Microsoft.CSharp

Public MustInherit Class AutoNotifiableObject
    Implements INotifyPropertyChanged

    ''' <summary>
    ''' Consts
    ''' </summary>
    Private Const AUTO_RENOTIFY_PERIOD As Integer = 100
    Private Const AUTO_RENOTIFY_MAX As Integer = 30

    ''' <summary>
    ''' Events
    ''' </summary>
    Private Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private AutoUpdateHandler As New EventHandler(AddressOf AutoUpdate)

    ''' <summary>
    ''' Variables
    ''' </summary>
    Private _isPaused As Integer = 0

    ''' <summary>
    ''' Properties
    ''' </summary>
    Private Shared ReadOnly AutoUpdateTimer As New DispatcherTimer(TimeSpan.FromMilliseconds(AUTO_RENOTIFY_PERIOD), DispatcherPriority.Background, Sub() Return, Application.Current.Dispatcher)
    Private Shared ReadOnly ListNotifyMethods As New Dictionary(Of PropertyInfo, MethodInfo())
    Private Shared ReadOnly LoadedTypes As New List(Of Type)
    Private ReadOnly Property PropertyOldValues As New Dictionary(Of PropertyInfo, Object)
    Private ReadOnly Property PausedProperties As New List(Of String)
    Private ReadOnly Property IsPaused As Boolean
        Get
            Return _isPaused > 0
        End Get
    End Property

    ''' <summary>
    ''' Methods
    ''' </summary>
    Public Sub Pause()
        _isPaused += 1
    End Sub

    Public Sub Unpause()
        If _isPaused > 0 Then _isPaused -= 1
    End Sub

    Public Sub Pause(propName As String)
        _PausedProperties.Add(propName)
    End Sub

    Public Sub Unpause(propName As String)
        _PausedProperties.Remove(propName)
    End Sub

    ''' <summary>
    ''' NotifyPropertyChanged
    ''' </summary>
    Private Function NotifyPropertyChanged(prop As PropertyInfo) As Boolean
        If Not prop.CanRead OrElse _PausedProperties.Contains(prop.Name) Then Return False
        Dim val = HashValue(prop.GetValue(Me))
        Dim befVal = Nothing
        PropertyOldValues.TryGetValue(prop, befVal)
        If Not Equals(val, befVal) Then
            'reassign value
            PropertyOldValues(prop) = val

            'self
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(prop.Name))

            'notify methods
            Dim notiMethods = Nothing
            ListNotifyMethods.TryGetValue(prop, notiMethods)
            If notiMethods IsNot Nothing Then
                For Each m As MethodInfo In notiMethods
                    m.Invoke(Me, New Object() {prop.Name})
                Next
            End If

            Return True
        End If
        Return False
    End Function

    Private Function HashValue(iValue As Object) As Object
        If iValue Is Nothing Then Return Nothing
        If GetType(IEnumerable).IsAssignableFrom(iValue.GetType) Then
            Dim sum As Long = 0
            Dim index As Long = 0
            For Each i As Object In iValue
                sum += If(i?.GetHashCode, index)
                index += 1
            Next
            Return sum
        Else
            Return iValue
        End If
    End Function

    ''' <summary>
    ''' Create AutoUpdateTimer
    ''' </summary>
    Private Sub CreateAutoUpdater()
        AddHandler AutoUpdateTimer.Tick, AutoUpdateHandler
        If Not AutoUpdateTimer.IsEnabled Then AutoUpdateTimer.Start()
    End Sub

    Private Sub AutoUpdate(sender As Object, e As EventArgs)
        If IsPaused Then Return
        Dim cnt As Integer = 0
        For Each p As PropertyInfo In Me.GetType.GetProperties
            If NotifyPropertyChanged(p) Then cnt += 1
            If cnt >= AUTO_RENOTIFY_MAX Then Exit For
        Next
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    Private Sub CheckNotifyMethods()
        Dim thisType = Me.GetType
        If LoadedTypes.Contains(thisType) Then Return
        LoadedTypes.Add(thisType)

        For Each p As PropertyInfo In Me.GetType.GetProperties
            Dim notiMethodAtt = p.GetCustomAttribute(Of NotifyMethodAttribute)
            If notiMethodAtt IsNot Nothing AndAlso notiMethodAtt.Methods?.Length > 0 Then
                Dim listMethods As New List(Of MethodInfo)
                For Each mName As String In notiMethodAtt.Methods
                    Dim runMethod = thisType.GetMethod(mName)
                    If runMethod.GetParameters.Length <> 1 AndAlso runMethod.GetParameters(0).ParameterType IsNot GetType(String) Then Throw New ArgumentException
                    listMethods.Add(runMethod)
                Next
                ListNotifyMethods.Add(p, listMethods.ToArray)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        CheckNotifyMethods()
        CreateAutoUpdater()
    End Sub

    ''' <summary>
    ''' Destructor
    ''' </summary>
    Protected Overrides Sub Finalize()
        RemoveHandler AutoUpdateTimer.Tick, AutoUpdateHandler
    End Sub

End Class
