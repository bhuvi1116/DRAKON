using DrakonNx.Core.Model;

namespace DrakonNx.Core.Services;

public static class DiagramFactory
{
    public static DiagramDocument CreateHelloWorldSample()
    {
        var document = new DiagramDocument("Hello World")
        {
            Profile = DiagramProfile.ExecutableV0,
            LayoutMode = DiagramLayoutMode.Primitive
        };

        document.Nodes.Add(new DiagramNode("start", NodeKind.Start, "Старт", 100, 40));
        document.Nodes.Add(new DiagramNode("print", NodeKind.Action, "print(\"Hello, DRAKON\")", 100, 140));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "Конец", 100, 240));

        document.Connections.Add(new DiagramConnection("c1", "start", PortKind.Out, "print", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "print", PortKind.Out, "end", PortKind.In));

        return document;
    }

    public static DiagramDocument CreateMinimalSample()
    {
        var document = new DiagramDocument("Minimal Sample")
        {
            Profile = DiagramProfile.ExecutableV0,
            LayoutMode = DiagramLayoutMode.Primitive
        };

        document.Nodes.Add(new DiagramNode("start", NodeKind.Start, "Старт", 100, 40));
        document.Nodes.Add(new DiagramNode("action1", NodeKind.Action, "x = 1", 100, 140));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "Конец", 100, 240));

        document.Connections.Add(new DiagramConnection("c1", "start", PortKind.Out, "action1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "action1", PortKind.Out, "end", PortKind.In));

        return document;
    }

    public static DiagramDocument CreateBranchSample()
    {
        var document = new DiagramDocument("Simple Branch Sample")
        {
            Profile = DiagramProfile.ExecutableV0,
            LayoutMode = DiagramLayoutMode.Primitive
        };

        document.Nodes.Add(new DiagramNode("start", NodeKind.Start, "Старт", 100, 40));
        document.Nodes.Add(new DiagramNode("setx", NodeKind.Action, "x = 5", 100, 140));
        document.Nodes.Add(new DiagramNode("cond", NodeKind.Condition, "x > 0", 100, 240));
        document.Nodes.Add(new DiagramNode("t1", NodeKind.Action, "print(1)", 30, 360, lane: 0));
        document.Nodes.Add(new DiagramNode("f1", NodeKind.Action, "print(0)", 220, 360, lane: 1));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "Конец", 100, 480));

        document.Connections.Add(new DiagramConnection("c1", "start", PortKind.Out, "setx", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "setx", PortKind.Out, "cond", PortKind.In));
        document.Connections.Add(new DiagramConnection("c3", "cond", PortKind.True, "t1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c4", "cond", PortKind.False, "f1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c5", "t1", PortKind.Out, "end", PortKind.In));
        document.Connections.Add(new DiagramConnection("c6", "f1", PortKind.Out, "end", PortKind.In));

        return document;
    }

    public static DiagramDocument CreateMaxOfTwoSample()
    {
        var document = new DiagramDocument("Max Of Two")
        {
            Profile = DiagramProfile.ExecutableV0,
            LayoutMode = DiagramLayoutMode.Primitive
        };

        document.Nodes.Add(new DiagramNode("start", NodeKind.Start, "Старт", 100, 40));
        document.Nodes.Add(new DiagramNode("a", NodeKind.Action, "a = 10", 100, 120));
        document.Nodes.Add(new DiagramNode("b", NodeKind.Action, "b = 20", 100, 200));
        document.Nodes.Add(new DiagramNode("cond", NodeKind.Condition, "a > b", 100, 280));
        document.Nodes.Add(new DiagramNode("trueAssign", NodeKind.Action, "result = a", 30, 380, lane: 0));
        document.Nodes.Add(new DiagramNode("falseAssign", NodeKind.Action, "result = b", 220, 380, lane: 1));
        document.Nodes.Add(new DiagramNode("print", NodeKind.Action, "print(result)", 100, 480));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "Конец", 100, 560));

        document.Connections.Add(new DiagramConnection("c1", "start", PortKind.Out, "a", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "a", PortKind.Out, "b", PortKind.In));
        document.Connections.Add(new DiagramConnection("c3", "b", PortKind.Out, "cond", PortKind.In));
        document.Connections.Add(new DiagramConnection("c4", "cond", PortKind.True, "trueAssign", PortKind.In));
        document.Connections.Add(new DiagramConnection("c5", "cond", PortKind.False, "falseAssign", PortKind.In));
        document.Connections.Add(new DiagramConnection("c6", "trueAssign", PortKind.Out, "print", PortKind.In));
        document.Connections.Add(new DiagramConnection("c7", "falseAssign", PortKind.Out, "print", PortKind.In));
        document.Connections.Add(new DiagramConnection("c8", "print", PortKind.Out, "end", PortKind.In));

        return document;
    }

    public static DiagramDocument CreateDrakonPrimitiveSpecSample()
    {
        var document = new DiagramDocument("ДРАКОН-примитив")
        {
            Profile = DiagramProfile.DrakonVisualSpec,
            LayoutMode = DiagramLayoutMode.Primitive
        };

        document.Nodes.Add(new DiagramNode("title", NodeKind.Title, "Поездка на автобусе", 110, 40));
        document.Nodes.Add(new DiagramNode("step1", NodeKind.Action, "Найти остановку", 110, 130));
        document.Nodes.Add(new DiagramNode("question", NodeKind.Question, "Автобус подошел?", 110, 220));
        document.Nodes.Add(new DiagramNode("wait", NodeKind.Action, "Ждать", 300, 220, lane: 1));
        document.Nodes.Add(new DiagramNode("board", NodeKind.Action, "Сесть в автобус", 110, 320));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "конец", 110, 410));

        document.Connections.Add(new DiagramConnection("c1", "title", PortKind.Out, "step1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "step1", PortKind.Out, "question", PortKind.In));
        document.Connections.Add(new DiagramConnection("c3", "question", PortKind.True, "board", PortKind.In));
        document.Connections.Add(new DiagramConnection("c4", "question", PortKind.False, "wait", PortKind.In));
        document.Connections.Add(new DiagramConnection("c5", "wait", PortKind.Out, "question", PortKind.In));
        document.Connections.Add(new DiagramConnection("c6", "board", PortKind.Out, "end", PortKind.In));

        return document;
    }

    public static DiagramDocument CreateDrakonSilhouetteSpecSample()
    {
        var document = new DiagramDocument("ДРАКОН-силуэт")
        {
            Profile = DiagramProfile.DrakonVisualSpec,
            LayoutMode = DiagramLayoutMode.Silhouette
        };

        document.Nodes.Add(new DiagramNode("title", NodeKind.Title, "Сборы в поездку", 70, 30));

        document.Nodes.Add(new DiagramNode("branch1", NodeKind.BranchStart, "Подъем и завтрак", 70, 120, lane: 0));
        document.Nodes.Add(new DiagramNode("b1a1", NodeKind.Action, "Встать пораньше", 70, 210, lane: 0));
        document.Nodes.Add(new DiagramNode("b1a2", NodeKind.Action, "Позавтракать", 70, 300, lane: 0));
        document.Nodes.Add(new DiagramNode("addr1", NodeKind.Address, "Укладка вещей", 70, 390, lane: 0));

        document.Nodes.Add(new DiagramNode("branch2", NodeKind.BranchStart, "Укладка вещей", 300, 120, lane: 1));
        document.Nodes.Add(new DiagramNode("b2a1", NodeKind.Action, "Собрать документы", 300, 210, lane: 1));
        document.Nodes.Add(new DiagramNode("b2a2", NodeKind.Action, "Уложить багаж", 300, 300, lane: 1));
        document.Nodes.Add(new DiagramNode("addr2", NodeKind.Address, "Поездка", 300, 390, lane: 1));

        document.Nodes.Add(new DiagramNode("branch3", NodeKind.BranchStart, "Поездка", 530, 120, lane: 2));
        document.Nodes.Add(new DiagramNode("b3q", NodeKind.Question, "Такси приехало?", 530, 210, lane: 2));
        document.Nodes.Add(new DiagramNode("b3wait", NodeKind.Action, "Подождать", 750, 210, lane: 3));
        document.Nodes.Add(new DiagramNode("b3a", NodeKind.Action, "Поехать в аэропорт", 530, 300, lane: 2));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "конец", 530, 390, lane: 2));

        document.Connections.Add(new DiagramConnection("c1", "title", PortKind.Out, "branch1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "branch1", PortKind.Out, "b1a1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c3", "b1a1", PortKind.Out, "b1a2", PortKind.In));
        document.Connections.Add(new DiagramConnection("c4", "b1a2", PortKind.Out, "addr1", PortKind.In));

        document.Connections.Add(new DiagramConnection("c5", "branch2", PortKind.Out, "b2a1", PortKind.In));
        document.Connections.Add(new DiagramConnection("c6", "b2a1", PortKind.Out, "b2a2", PortKind.In));
        document.Connections.Add(new DiagramConnection("c7", "b2a2", PortKind.Out, "addr2", PortKind.In));

        document.Connections.Add(new DiagramConnection("c8", "branch3", PortKind.Out, "b3q", PortKind.In));
        document.Connections.Add(new DiagramConnection("c9", "b3q", PortKind.True, "b3a", PortKind.In));
        document.Connections.Add(new DiagramConnection("c10", "b3q", PortKind.False, "b3wait", PortKind.In));
        document.Connections.Add(new DiagramConnection("c11", "b3wait", PortKind.Out, "b3q", PortKind.In));
        document.Connections.Add(new DiagramConnection("c12", "b3a", PortKind.Out, "end", PortKind.In));

        return document;
    }
}
