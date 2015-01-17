Imports System.Reflection
Imports MsgPack.Serialization
Imports Newtonsoft.Json.Linq
Imports System.Linq

Namespace Networking
    ''' <summary>
    ''' This protocol allow methods that deserialize and serialize object to use the same language. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface ISerializationProtocol

        Function Serialize(Obj As Object) As Byte()
        Function Deserialize(Data As Byte()) As Object
        Function GetPropertyValue(PropertyName As String, PropertyIndex As Integer, ByRef ObjectData As Object) As Object

    End Interface

    ''' <summary>
    ''' The JSON serialization engine is slower than MessagePack, but supports a wider range of objects.
    ''' </summary>
    Public Class JsonSerializerEngine
        Implements ISerializationProtocol

        Public Function Serialize(obj As Object) As Byte() Implements ISerializationProtocol.Serialize
            Dim JsonString As String = Newtonsoft.Json.JsonConvert.SerializeObject(New DeserializationWrapper(obj))
            Return System.Text.UnicodeEncoding.UTF8.GetBytes(JsonString)
        End Function

        Public Function Deserialize(Data As Byte()) As Object Implements ISerializationProtocol.Deserialize
            Dim JsonString As String = System.Text.UnicodeEncoding.UTF8.GetString(Data)
            Dim ObjWrapper As DeserializationWrapper = Newtonsoft.Json.JsonConvert.DeserializeObject(Of DeserializationWrapper)(JsonString)
            Return ObjWrapper.GetInitializedObject(Me)
        End Function

        Public Function GetPropertyValue(PropertyName As String, PropertyIndex As Integer, ByRef ObjectData As Object) As Object Implements ISerializationProtocol.GetPropertyValue
            Dim v As JValue = DirectCast(ObjectData, JObject)(PropertyName)
            Return v.Value
        End Function
    End Class

    Public Class BinaryFormatterSerializerEngine
        Implements ISerializationProtocol

        Public Function Deserialize(Data() As Byte) As Object Implements ISerializationProtocol.Deserialize
            Dim bfTemp As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Using sStream As New IO.MemoryStream(Data)
                Dim Obj As Object = bfTemp.Deserialize(sStream, Nothing)
                Return Obj
            End Using
        End Function

        Public Function GetPropertyValue(PropertyName As String, PropertyIndex As Integer, ByRef ObjectData As Object) As Object Implements ISerializationProtocol.GetPropertyValue
            Return ObjectData(PropertyName)
        End Function

        Public Function Serialize(Obj As Object) As Byte() Implements ISerializationProtocol.Serialize
            Dim sStream As New IO.MemoryStream()
            Dim bfTemp As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
            bfTemp.AssemblyFormat = Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            bfTemp.Serialize(sStream, Obj)
            Return sStream.ToArray
        End Function
    End Class

    ''' <summary>
    ''' The Message Pack Serializer is faster and slimmer than JSON, but not without limitations. 
    ''' https://github.com/msgpack/msgpack/blob/master/spec.md#limitation
    ''' </summary>
    Public Class MessagePackSerializerEngine
        Implements ISerializationProtocol

        Dim serializer As IMessagePackSingleObjectSerializer = SerializationContext.Default.GetSerializer(Of DeserializationWrapper)

        Private Function ISerializationProtocol_Serialize(Obj As Object) As Byte() Implements ISerializationProtocol.Serialize
            Dim WrapperObject As New DeserializationWrapper(Obj)
            Return serializer.PackSingleObject(WrapperObject)
        End Function

        Public Function Deserialize(Data As Byte()) As Object Implements ISerializationProtocol.Deserialize
            Dim ObjWrapper As DeserializationWrapper = serializer.UnpackSingleObject(Data)
            Return ObjWrapper.GetInitializedObject(Me)
        End Function

        Public Function GetPropertyValue(PropertyName As String, PropertyIndex As Integer, ByRef ObjectData As Object) As Object Implements ISerializationProtocol.GetPropertyValue
            Dim MsgPackObj As IList(Of MsgPack.MessagePackObject) = DirectCast(ObjectData, MsgPack.MessagePackObject).AsList
            Return MsgPackObj(PropertyIndex).ToObject
        End Function

    End Class

    <Serializable>
    Public Class DeserializationWrapper

        Public Sub New()
        End Sub

        Public Sub New(Obj As Object)
            Me.Type = Obj.GetType.FullName
            Me.Data = Obj
        End Sub

        Public Property Type As String
        Public Property Data As Object

        Public Function GetInitializedObject(sender As ISerializationProtocol) As Object
            Dim theType As Type = System.Type.GetType(Type)
            Dim instance As Object = Activator.CreateInstance(theType)

            DeserializationWrapper.IterateProperties(instance,
            Sub(p, i)
                If p.CanWrite Then
                    Dim v = sender.GetPropertyValue(p.Name, i, Data)
                    p.SetValue(instance, v, Nothing)
                End If
            End Sub)

            Return instance
        End Function

        Public Shared Sub IterateProperties(target As Object, Operation As Action(Of PropertyInfo, Integer))
            Dim TargetType As Type = target.GetType
            Dim objProperties As PropertyInfo() = TargetType.GetProperties(BindingFlags.Instance Or BindingFlags.[Public])
            Dim instance As Object = Activator.CreateInstance(TargetType)
            Dim index As Integer = 0
            For Each p As PropertyInfo In objProperties
                Operation.Invoke(p, index)
                index += 1
            Next
        End Sub

    End Class

End Namespace