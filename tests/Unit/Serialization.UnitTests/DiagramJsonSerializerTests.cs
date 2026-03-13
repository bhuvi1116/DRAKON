using DrakonNx.Core.Services;
using DrakonNx.Serialization.Json;

namespace DrakonNx.Tests.Unit.Serialization;

public sealed class DiagramJsonSerializerTests
{
    [Fact]
    public void SerializeThenDeserialize_PreservesBasicStructure()
    {
        var serializer = new DiagramJsonSerializer();
        var original = DiagramFactory.CreateMinimalSample();

        var json = serializer.Serialize(original);
        var restored = serializer.Deserialize(json);

        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Nodes.Count, restored.Nodes.Count);
        Assert.Equal(original.Connections.Count, restored.Connections.Count);
    }
}
