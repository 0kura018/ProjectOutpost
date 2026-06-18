# Система Производства Ресурсов

## Обзор

Система позволяет зданиям производить **множество ресурсов** одновременно с гибкой настройкой.

## Ключевые особенности

? **Множественное производство** - одно здание может производить любое количество разных ресурсов
? **Два режима расчета** - за цикл работы или за сутки (24 часа)
? **Автоматические расчеты** - система сама рассчитывает пропорции
? **Валидация** - редактор предупреждает об ошибках
? **Визуальная сводка** - в инспекторе видно всю информацию о производстве

## Настройка здания

### 1. Основные параметры

В `RoomConfig`:
- **CanProvideWork** = `true` - здание может предоставлять работу
- **WorkTimeHours** - длительность одного рабочего цикла (в игровых часах)
- **Production** - массив производимых ресурсов

### 2. Настройка производства ресурса

Для каждого ресурса в массиве `Production`:

#### Режим "За цикл" (Per Cycle)
```
Resource Type: Gold
Amount Per Cycle: 50
Use Daily Production: false
```

**Результат:**
- Каждый цикл работы выдаёт 50 Gold
- Независимо от WorkTimeHours

#### Режим "За сутки" (Per Day)
```
Resource Type: Wood  
Amount Per Day (24h): 600
Use Daily Production: true
```

**Результат при разных WorkTimeHours:**
- WorkTimeHours = 2: выдаёт 50 Wood за цикл (600 / 12 циклов)
- WorkTimeHours = 3: выдаёт 75 Wood за цикл (600 / 8 циклов)
- WorkTimeHours = 4: выдаёт 100 Wood за цикл (600 / 6 циклов)

## Примеры зданий

### Лесопилка
```
WorkTimeHours: 4
Production:
  [0] Wood: 100 per cycle
  [1] Stone: 20 per cycle
```
**За сутки:** 600 Wood, 120 Stone

### Золотая Шахта
```
WorkTimeHours: 3
Production:
  [0] Gold: 480 per day (Use Daily Production)
  [1] Stone: 240 per day (Use Daily Production)
```
**За цикл:** 60 Gold, 30 Stone
**За сутки:** 480 Gold, 240 Stone

### Универсальное производство
```
WorkTimeHours: 2
Production:
  [0] Gold: 50 per cycle
  [1] Wood: 30 per cycle
  [2] Stone: 20 per cycle
  [3] Energy: 10 per cycle
```
**За сутки:** 600 Gold, 360 Wood, 240 Stone, 120 Energy

## Визуальный редактор

При открытии `RoomConfig` в инспекторе, внизу автоматически показывается:

```
Production Info
Work Cycle Time: 4 hours

• Gold: 100/cycle (~600/day, 6.0 cycles/day)
• Wood: 50/cycle (~300/day, 6.0 cycles/day)

Daily Summary:
  Gold: 600 per day
  Wood: 300 per day
```

## Валидация

Редактор автоматически проверяет:

? **Ошибка:** `Gold: AmountPerDay должен быть больше 0 при включенном UseDailyProduction`
- Исправление: укажите AmountPerDay > 0

? **Ошибка:** `Wood: Amount должен быть больше 0`
- Исправление: укажите Amount > 0 или включите UseDailyProduction

?? **Предупреждение:** `Нет настроенного производства! Добавьте ресурсы в массив Production.`
- Исправление: добавьте хотя бы один элемент в Production

## API для программистов

### Получение производства

```csharp
// Получить все производимые ресурсы
ResourceProduction[] productions = roomConfig.GetProduction();

// Итерация
foreach (var prod in productions)
{
    ResourceType type = prod.Type;
    int amountPerCycle = prod.GetAmountForWorkTime(roomConfig.WorkTimeHours);
    int amountPerDay = prod.GetDailyAmount(roomConfig.WorkTimeHours);
    
    Debug.Log($"{type}: {amountPerCycle}/cycle, {amountPerDay}/day");
}
```

### Получение итоговой суточной сводки

```csharp
// Получить сумму по каждому типу ресурса за сутки
Dictionary<ResourceType, int> dailyTotals = roomConfig.GetTotalDailyProduction();

foreach (var kvp in dailyTotals)
{
    Debug.Log($"{kvp.Key}: {kvp.Value} per day");
}
```

### Валидация конфигурации

```csharp
if (!roomConfig.ValidateProduction(out string[] errors))
{
    Debug.LogError("Production configuration has errors:");
    foreach (var error in errors)
    {
        Debug.LogError($"  - {error}");
    }
}
```

### Методы ResourceProduction

```csharp
ResourceProduction prod = productions[0];

// Количество за цикл
int amount = prod.GetAmountForWorkTime(4f); // для цикла 4 часа

// Суточное производство
int daily = prod.GetDailyAmount(4f);

// Количество циклов в сутки
float cycles = prod.GetCyclesPerDay(4f); // = 6 циклов

// Валидация
if (!prod.IsValid(out string error))
{
    Debug.LogError(error);
}
```

## Как работает система в игре

1. **Игрок назначает юнита на работу** ? `RoomWorkHandler.AssignWorker(unit)`
2. **Система запускает цикл работы** ? `_hoursRemaining = WorkTimeHours`
3. **Время идёт** ? часы уменьшаются
4. **Цикл завершен** ? `RoomWorkHandler.CompleteWork()`
5. **Производство ресурсов** ? вызывается `ProduceResources()`
6. **Каждый ресурс выдаётся** ? `ResourceManager.AddResource(type, amount)`

```csharp
// Внутри RoomWorkHandler.ProduceResources()
var productions = _room.Config.GetProduction();
foreach (var production in productions)
{
    int amount = production.GetAmountForWorkTime(_room.Config.WorkTimeHours);
    if (amount > 0)
    {
        ResourceManager.Instance.AddResource(production.Type, amount);
    }
}
```

## Legacy совместимость

Старая система с одним ресурсом:
```csharp
ProducedResource = ResourceType.Gold
ProducedAmount = 100
```

Автоматически работает через `GetProduction()`:
```csharp
// Внутренне преобразуется в:
Production[0] = new ResourceProduction 
{ 
    Type = ResourceType.Gold,
    Amount = 100,
    UseDailyProduction = false
}
```

## Советы по использованию

### Когда использовать "Per Cycle"
- Фиксированное количество за цикл
- Простые здания
- Когда время цикла редко меняется

### Когда использовать "Per Day"
- Нужен точный баланс производства за сутки
- Время цикла может меняться (апгрейды, баффы)
- Легче балансировать экономику игры

### Пример стратегии баланса

**Ранняя игра:**
- Базовые здания: Per Cycle, короткие циклы (1-2 часа)
- Простое производство 1-2 ресурсов

**Поздняя игра:**
- Продвинутые здания: Per Day, длинные циклы (4-6 часов)
- Множественное производство 3-5 ресурсов
- Точный баланс через AmountPerDay

## Часто задаваемые вопросы

**Q: Можно ли производить один и тот же ресурс дважды?**
A: Да! Система суммирует их в Daily Summary.

**Q: Что произойдет если Amount и AmountPerDay оба = 0?**
A: Редактор покажет ошибку валидации.

**Q: Влияет ли скорость игры на производство?**
A: Нет, производство зависит только от игрового времени, а не реального.

**Q: Можно ли изменить Production во время игры?**
A: Лучше не надо - Production настраивается в ScriptableObject, изменения затронут все экземпляры.

**Q: Как добавить новый тип ресурса?**
A: 1) Добавьте в ResourceType enum, 2) Создайте ResourceData, 3) Добавьте в ResourcesConfig
