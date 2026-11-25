# Carsharing

Приложение для Windows на .NET 8 / MAUI 8.0.100. Откройте `CarsharingApp/CarsharingApp.sln` в Visual Studio с установленной рабочей нагрузкой .NET MAUI.

Windows-проект настроен на запуск без MSIX (`WindowsPackageType=None`, профиль запуска `Project`), поэтому установка пакета для запуска не требуется.

## Сборка и тесты

```powershell
dotnet workload install maui-windows
dotnet build CarsharingApp/CarsharingApp.sln
dotnet test CarsharingApp/CarsharingApp.sln --filter 'TestCategory!=Database'
```

Модели и репозитории компилируются в `CarsharingApp.Core`, поэтому тесты бизнес-логики не зависят от запуска MAUI. Исходные тестовые проекты сохранены в решении.

## Подключение к PostgreSQL

Все экраны используют одну переменную окружения `CARSHARING_CONNECTION_STRING`. Задайте её перед запуском Visual Studio или приложения, например:

```powershell
$env:CARSHARING_CONNECTION_STRING = 'Host=localhost;Port=5432;Database=Carsharing;Username=postgres;Password=<ваш пароль>;Timeout=5'
```

Без переменной используется локальный сервер, база `Carsharing`, пользователь `postgres`, без пароля в исходном коде. Для сервера в учебной сети укажите его адрес и свои реквизиты в этой переменной. Приложение не создаёт и не изменяет схему автоматически.

`База данных/Carsharing.sql` — архив PostgreSQL custom format, несмотря на расширение `.sql`; восстанавливайте его через `pg_restore`, а не открывайте как SQL-скрипт.

## Проверки с базой данных

Создайте отдельную тестовую БД и восстановите в неё схему:

```powershell
createdb -U postgres carsharing_test
pg_restore --exit-on-error --schema-only --no-owner --no-privileges -U postgres -d carsharing_test 'База данных/Carsharing.sql'
$env:CARSHARING_TEST_CONNECTION_STRING = 'Host=localhost;Database=carsharing_test;Username=postgres;Password=<тестовый пароль>'
dotnet test CarsharingApp/CarsharingApp.sln
```

Эти тесты создают и удаляют свои записи; используйте отдельную тестовую базу. Без переменной проверки категории `Database` пропускаются. GitHub Actions проверяет сборку Windows и операции с отдельным PostgreSQL 16, восстановленным из схемы репозитория.

## Работа приложения

После регистрации войдите в аккаунт. На странице автомобилей доступны поиск, бронирование и отзывы; в меню — профиль, история, текущая поездка и выход. Все операции используют вошедшего пользователя и выбранный автомобиль. Поездка начинается в пределах времени активной брони, завершение рассчитывает стоимость по тарифу автомобиля и закрывает бронь. Повторное завершение не создаёт второй платёж. Платёж записывается в учебную БД; интеграции с банковским сервисом нет.
