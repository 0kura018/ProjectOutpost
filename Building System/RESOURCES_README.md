# Система Ресурсов на ScriptableObjects

## Обзор
Система ресурсов переведена на ScriptableObjects для удобного менеджмента и настройки в редакторе Unity.

## Структура файлов

### 1. ResourceType.cs
Enum с типами ресурсов (Gold, Wood, Stone, Iron, Energy)

### 2. ResourceData.cs
ScriptableObject для каждого типа ресурса. Содержит:
- Название ресурса
- Иконку
- Описание
- Тип (ResourceType)
- Цвет для UI
- Начальное количество
- Максимальное количество (опционально)

### 3. ResourcesConfig.cs
Главный конфигурационный файл системы. Содержит:
- Массив всех ResourceData
- Методы для получения данных по типу
- Метод для получения начальных ресурсов

### 4. ResourceManager.cs
Singleton-менеджер для управления ресурсами во время игры:
- Использует ResourcesConfig для инициализации
- Управляет текущим количеством ресурсов
- События изменения ресурсов
- Методы Add/Spend/Get/Set ресурсов

### 5. ResourceDisplay.cs
UI компонент для отображения ресурса:
- Автоматически загружает иконку из ResourceData
- Поддерживает override иконки
- Применяет цвет из конфига

### 6. ResourceCost.cs (в RoomConfig.cs)
Простой класс для стоимости (Type + Amount)

### 7. ResourceCostAdvanced.cs
Расширенный класс с прямой ссылкой на ResourceData

### 8. ResourceProduction.cs
Класс для описания производства ресурсов зданиями:
- Тип ресурса
- Количество за цикл работы
- Опциональное производство за сутки (24 часа)

## Производство Ресурсов Зданиями

### Настройка производства в RoomConfig

Здания могут производить **несколько типов ресурсов** одновременно!

```
Production Array:
  ?? [0] Gold: 100/цикл (или 500/день)
  ?? [1] Wood: 50/цикл
  ?? [2] Stone: 25/цикл
```

### Два режима производства

**1. За цикл работы (Per Cycle)**
- Указываете `Amount` - количество за один цикл
- Пример: WorkTimeHours = 2, Amount = 100 ? каждые 2 часа выдаётся 100 ресурса

**2. За сутки (Per Day)**
- Включите `UseDailyProduction`
- Указываете `AmountPerDay` - количество за 24 игровых часа
- Система автоматически рассчитывает количество за цикл
- Пример: WorkTimeHours = 2, AmountPerDay = 1200 ? каждые 2 часа выдаётся 100 ресурса (1200/12 циклов)

### Пример настройки

**Лесопилка:**
- WorkTimeHours: 3 часа
- Production:
  - Wood: 150/цикл (1200/день)
  - Stone: 30/цикл (240/день)

**Шахта:**
- WorkTimeHours: 4 часа  
- Production:
  - Stone: 200/день (UseDailyProduction = true)
  - Iron: 100/день (UseDailyProduction = true)

## Как использовать

### Создание ресурсов
1. В Unity: ПКМ ? Create ? Resources ? Resource Data
2. Настройте параметры ресурса
3. Создайте ResourceData для каждого типа ресурса

### Настройка конфига
1. ПКМ ? Create ? Resources ? Resources Config
2. Добавьте все созданные ResourceData в массив AllResources
3. Назначьте конфиг в ResourceManager на сцене

### Настройка производства здания
1. Откройте RoomConfig в инспекторе
2. Включите `CanProvideWork = true`
3. Установите `WorkTimeHours` (время одного цикла)
4. В массиве `Production` добавьте ресурсы:
   - Выберите тип ресурса
   - Укажите Amount Per Cycle ИЛИ включите Use Daily Production и укажите Amount Per Day
5. Внизу инспектора будет показана сводка производства

### Использование в коде

```csharp
// Получить количество ресурса
int gold = ResourceManager.Instance.GetResource(ResourceType.Gold);

// Добавить ресурс
ResourceManager.Instance.AddResource(ResourceType.Wood, 100);

// Потратить ресурс
bool success = ResourceManager.Instance.SpendResource(ResourceType.Stone, 50);

// Получить данные о ресурсе
ResourceData data = ResourceManager.Instance.GetResourceData(ResourceType.Gold);
Sprite icon = data.Icon;
string name = data.ResourceName;

// Получить производство здания
ResourceProduction[] productions = roomConfig.GetProduction();
foreach (var prod in productions)
{
    int amount = prod.GetAmountForWorkTime(roomConfig.WorkTimeHours);
    Debug.Log($"Produces {amount} {prod.ResourceType}");
}
```

## Преимущества новой системы

? Централизованная настройка всех ресурсов
? Легко добавлять новые типы ресурсов
? Визуальная настройка в редакторе Unity
? Автоматическая загрузка иконок и цветов
? Валидация дубликатов в редакторе
? Гибкая система стоимости (простая и расширенная)
? **Множественное производство ресурсов одним зданием**
? **Два режима: за цикл или за сутки**
? **Автоматический расчёт производства в инспекторе**

## Миграция старого кода

Старый код с прямым использованием `ProducedResource` и `ProducedAmount` продолжит работать!

Система автоматически использует legacy поля через метод `GetProduction()`:

```csharp
// Старая конфигурация (все еще работает):
ProducedResource = ResourceType.Gold
ProducedAmount = 100

// Автоматически преобразуется в:
Production[0] = { ResourceType.Gold, Amount = 100 }
```

Для новых зданий рекомендуется использовать массив `Production`.

## Editor Extensions

### ResourceProductionDrawer
Кастомный редактор для удобной настройки производства в инспекторе

### RoomConfigEditor  
Показывает сводку производства внизу инспектора RoomConfig:
- Время цикла
- Список всех производимых ресурсов
- Количество за цикл и за день
- Количество циклов в день
