using DrakonNx.Core.Model;

namespace DrakonNx.Editor.Models;

public sealed class NodeItemViewModel
{
    public NodeItemViewModel(string id, NodeKind kind, string text, double x, double y, int lane = 0)
    {
        Id = id;
        Kind = kind;
        Text = text;
        X = x;
        Y = y;
        Lane = lane;
    }

    public string Id { get; }
    public NodeKind Kind { get; }
    public string Text { get; }
    public double X { get; }
    public double Y { get; }
    public int Lane { get; }
    public string PositionDisplay => $"({X:0.##}, {Y:0.##})";
    public string DisplayName => $"{KindDisplayName} [{Id}] — {Text} @ {PositionDisplay}, lane={Lane}";

    public string KindDisplayName => Kind switch
    {
        NodeKind.Start => "Старт",
        NodeKind.Action => "Действие",
        NodeKind.Condition => "Условие",
        NodeKind.End => "Конец",
        NodeKind.Title => "Заголовок",
        NodeKind.BranchStart => "Имя ветки",
        NodeKind.Address => "Адрес",
        NodeKind.Question => "Вопрос",
        NodeKind.Select => "Выбор",
        NodeKind.Case => "Вариант",
        NodeKind.LoopStart => "Начало цикла",
        NodeKind.LoopEnd => "Конец цикла",
        NodeKind.Procedure => "Процедура",
        NodeKind.Insert => "Вставка",
        _ => Kind.ToString()
    };
}
