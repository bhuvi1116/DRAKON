# Шаг 18 — pointer-driven drag-and-drop

Добавлено перетаскивание узлов мышью прямо на canvas.

Что сделано:
- pointer pressed начинает drag-сессию по узлу;
- pointer moved обновляет координаты узла в модели;
- pointer released фиксирует результат в command-based Undo/Redo history;
- выбор узла остается синхронным между canvas и списком.

Текущее ограничение:
- snap-to-grid во время drag еще не применяется автоматически;
- визуальный preview совпадает с реальными координатами документа, отдельный overlay пока не нужен.
