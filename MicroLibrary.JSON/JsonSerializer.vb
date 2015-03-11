Imports MicroLibrary.Networking
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' The JSON serialization engine is slower than MessagePack, but supports a wider range of objects.
''' </summary>
Public Class JsonSerializer
        Implements ISerializationProtocol

        Public Function Serialize(obj As Object) As Byte() Implements ISerializationProtocol.Serialize
        Dim JsonString As String = JsonConvert.SerializeObject(New DeserializationWrapper(obj, Me))
        Return System.Text.UnicodeEncoding.UTF8.GetBytes(JsonString)
        End Function

        Public Function Deserialize(Data As Byte()) As Object Implements ISerializationProtocol.Deserialize
            Dim JsonString As String = System.Text.UnicodeEncoding.UTF8.GetString(Data)
            Dim ObjWrapper As DeserializationWrapper = JsonConvert.DeserializeObject(Of DeserializationWrapper)(JsonString)
            Return ObjWrapper.GetInitializedObject(Me)
        End Function

        Public Function GetPropertyValue(PropertyName As String, PropertyIndex As Integer, ByRef ObjectData As Object) As Object Implements ISerializationProtocol.GetPropertyValue
            Dim v As JValue = DirectCast(ObjectData, JObject)(PropertyName)
            Return v.Value
        End Function
    End Class

