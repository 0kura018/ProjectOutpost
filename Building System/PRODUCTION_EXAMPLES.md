# Примеры Конфигураций Зданий

## Пример 1: Лесопилка (Простое производство)

```
RoomName: Лесопилка
CanProvideWork: true
WorkTimeHours: 3

Production:
  [0]
    Resource Type: Wood
    Amount Per Cycle: 75
    Use Daily Production: false
```

**Результат:**
- Каждые 3 часа: +75 Wood
- За сутки (8 циклов): 600 Wood

---

## Пример 2: Золотая Шахта (Суточное производство)

```
RoomName: Золотая Шахта
CanProvideWork: true
WorkTimeHours: 4

Production:
  [0]
    Resource Type: Gold
    Amount Per Day (24h): 480
    Use Daily Production: true
    
  [1]
    Resource Type: Stone
    Amount Per Day (24h): 240
    Use Daily Production: true
```

**Результат:**
- Каждые 4 часа: +80 Gold, +40 Stone
- За сутки (6 циклов): 480 Gold, 240 Stone

---

## Пример 3: Универсальная Фабрика (Множественное производство)

```
RoomName: Универсальная Фабрика
CanProvideWork: true
WorkTimeHours: 2

Production:
  [0]
    Resource Type: Gold
    Amount Per Cycle: 30
    Use Daily Production: false
    
  [1]
    Resource Type: Wood
    Amount Per Cycle: 25
    Use Daily Production: false
    
  [2]
    Resource Type: Stone
    Amount Per Cycle: 20
    Use Daily Production: false
    
  [3]
    Resource Type: Iron
    Amount Per Cycle: 15
    Use Daily Production: false
    
  [4]
    Resource Type: Energy
    Amount Per Cycle: 10
    Use Daily Production: false
```

**Результат:**
- Каждые 2 часа: +30 Gold, +25 Wood, +20 Stone, +15 Iron, +10 Energy
- За сутки (12 циклов): 360 Gold, 300 Wood, 240 Stone, 180 Iron, 120 Energy

---

## Пример 4: Продвинутая Шахта (Смешанный режим)

```
RoomName: Продвинутая Шахта
CanProvideWork: true
WorkTimeHours: 3

Production:
  [0]
    Resource Type: Stone
    Amount Per Day (24h): 800
    Use Daily Production: true
    
  [1]
    Resource Type: Iron
    Amount Per Day (24h): 400
    Use Daily Production: true
    
  [2]
    Resource Type: Gold
    Amount Per Cycle: 20
    Use Daily Production: false
```

**Результат:**
- Каждые 3 часа: +100 Stone, +50 Iron, +20 Gold
- За сутки (8 циклов): 800 Stone, 400 Iron, 160 Gold

---

## Пример 5: Энергостанция (Фокус на одном ресурсе)

```
RoomName: Энергостанция
CanProvideWork: true
WorkTimeHours: 1

Production:
  [0]
    Resource Type: Energy
    Amount Per Day (24h): 1200
    Use Daily Production: true
```

**Результат:**
- Каждый час: +50 Energy
- За сутки (24 цикла): 1200 Energy

---

## Пример 6: Торговая Палата (Быстрые циклы)

```
RoomName: Торговая Палата
CanProvideWork: true
WorkTimeHours: 0.5

Production:
  [0]
    Resource Type: Gold
    Amount Per Cycle: 10
    Use Daily Production: false
```

**Результат:**
- Каждые 30 минут: +10 Gold
- За сутки (48 циклов): 480 Gold

---

## Пример 7: Мультиресурсная Ферма (Баланс)

```
RoomName: Ферма
CanProvideWork: true
WorkTimeHours: 6

Production:
  [0]
    Resource Type: Wood
    Amount Per Day (24h): 200
    Use Daily Production: true
    
  [1]
    Resource Type: Stone
    Amount Per Day (24h): 100
    Use Daily Production: true
```

**Результат:**
- Каждые 6 часов: +50 Wood, +25 Stone
- За сутки (4 цикла): 200 Wood, 100 Stone

---

## Сравнение стратегий

### Per Cycle (за цикл)
**Плюсы:**
- Простота настройки
- Фиксированное количество
- Понятно сколько за один раз

**Минусы:**
- Сложнее балансировать при разных WorkTimeHours
- Нужно пересчитывать суточное производство вручную

**Когда использовать:**
- Ранняя игра
- Простые здания
- Одинаковое время циклов

### Per Day (за сутки)
**Плюсы:**
- Точный контроль суточного баланса
- Автоматический пересчет при изменении WorkTimeHours
- Легче балансировать экономику

**Минусы:**
- Менее очевидно сколько за цикл
- Нужно думать в рамках суток

**Когда использовать:**
- Поздняя игра
- Продвинутые здания
- Разное время циклов
- Важен суточный баланс

---

## Советы по балансу

### Правило 1: Масштабирование времени
Чем дольше цикл, тем больше должно производиться:
- 1 час: 40-50 ресурса
- 2 часа: 80-100 ресурса
- 4 часа: 160-200 ресурса
- 6 часов: 240-300 ресурса

### Правило 2: Редкость ресурсов
- Обычные (Wood, Stone): 200-600/день
- Редкие (Iron): 100-300/день
- Очень редкие (Gold, Energy): 50-200/день

### Правило 3: Специализация
- Простые здания: 1-2 ресурса
- Средние здания: 2-3 ресурса
- Продвинутые здания: 3-5 ресурсов

### Правило 4: Эффективность
Продвинутые здания должны быть эффективнее:
- Базовая лесопилка: 400 Wood/день
- Продвинутая лесопилка: 800 Wood/день + 200 Stone/день

---

## Как создать баланс экономики

1. **Определите суточные потребности**
   - Строительство: 500 Wood, 300 Stone
   - Улучшения: 200 Iron, 100 Gold
   - Энергия: 400 Energy

2. **Рассчитайте нужное производство**
   - +30% запас: 650 Wood, 390 Stone, 260 Iron, 130 Gold, 520 Energy

3. **Распределите по зданиям**
   - 2x Лесопилка: 800 Wood
   - 2x Шахта: 480 Stone, 320 Iron
   - 1x Золотая шахта: 480 Gold
   - 2x Энергостанция: 600 Energy

4. **Тестируйте и корректируйте**
   - Логи производства
   - Статистика потребления
   - Обратная связь игроков
