# Описание
Приложение для обфускации пользовательских модификаций игры DayZ.
Компоненты:
* Библиотека классов для загрузки директории с исходными файлами модификации, преобразования модификации в формат .pbo (загружаемый игрой), добавления промежуточных операций в процесс преобразования.
* Парсер файлов конфигурации модификаций (config.cpp).
* Консольное приложение, реализующее процесс обфускации с использованием библиотеки классов.
**В репозитории отсутствует приватный код промежуточных операций, реализующих обфускацию.**
# Технологии
* .Net 7
* Newtonsoft.JSON
* MSTest
