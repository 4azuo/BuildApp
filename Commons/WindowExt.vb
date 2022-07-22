Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Threading

Module WindowExt
    ''' <summary>
    ''' Consts
    ''' </summary>
    Private Const SYNC_PERIOD As Integer = 1
    Private Const MAX_LOAD_ASYNC As Integer = 10

    ''' <summary>
    ''' Load list async
    ''' </summary>
    ''' <typeparam name="A"></typeparam>
    ''' <typeparam name="B"></typeparam>
    ''' <param name="dstList"></param>
    ''' <param name="srcList"></param>
    ''' <param name="formatter"></param>
    Public Sub LoadAsync(Of A, B)(dstList As IList(Of A), srcList As IEnumerable(Of B), formatter As Func(Of B, A), Optional callback As Action = Nothing, Optional onceLoad As Integer = MAX_LOAD_ASYNC)
        Dim max As Integer = srcList.Count
        Dim cnt As Integer = 0
        Dim timer = New DispatcherTimer(TimeSpan.FromMilliseconds(SYNC_PERIOD), DispatcherPriority.Background,
                                        Sub(s, e)
                                            For cnt = cnt To Math.Min(max - 1, cnt + onceLoad)
                                                dstList.Add(formatter(srcList(cnt)))
                                            Next
                                            If cnt >= max Then
                                                s.Stop()
                                                callback?.Invoke
                                            End If
                                        End Sub, Application.Current.Dispatcher)
        timer.Start()
    End Sub

    ''' <summary>
    ''' Load list async
    ''' </summary>
    ''' <typeparam name="A"></typeparam>
    ''' <typeparam name="B"></typeparam>
    ''' <param name="dstList"></param>
    ''' <param name="src"></param>
    ''' <param name="formatter"></param>
    Public Sub LoadAsync(Of A, B)(dstList As IList(Of A), src As Func(Of IEnumerable(Of B)), formatter As Func(Of B, A), Optional callback As Action = Nothing, Optional onceLoad As Integer = MAX_LOAD_ASYNC)
        Dim srcList As IEnumerable(Of B) = Nothing
        Task.WhenAll(Task.Run(Sub()
                                  srcList = src.Invoke
                              End Sub)) _
            .ContinueWith(Sub()
                              Dim max As Integer = srcList.Count
                              Dim cnt As Integer = 0
                              Dim timer = New DispatcherTimer(TimeSpan.FromMilliseconds(SYNC_PERIOD), DispatcherPriority.Background,
                                                                                         Sub(s, e)
                                                                                             For cnt = cnt To Math.Min(max - 1, cnt + onceLoad)
                                                                                                 dstList.Add(formatter(srcList(cnt)))
                                                                                             Next
                                                                                             If cnt >= max Then
                                                                                                 s.Stop()
                                                                                                 callback?.Invoke
                                                                                             End If
                                                                                         End Sub, Application.Current.Dispatcher)
                              timer.Start()
                          End Sub)
    End Sub
End Module
