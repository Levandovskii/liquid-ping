package LiquidPing {
    doc /* Модель системы Liquid Ping: структура, варианты использования и ключевые требования. */

    private import ScalarValues::*;

    // Участники и данные

    part def User {
        doc /* Пользователь приложения. */
    }

    part def MonitoredHost {
        doc /* Узел, который проверяет приложение: домен или IP-адрес. */
        attribute name : String;
        attribute address : String;
        attribute tcpPort : Natural = 443;
    }

    enum def HostStatus {
        doc /* Общий статус узла (REQ-FUNC-006). */
        enum available;
        enum unstable;
        enum unavailable;
    }

    enum def PortState {
        doc /* Состояние TCP-порта (REQ-FUNC-010). */
        enum open;
        enum closed;
        enum filtered;
    }

    // Структура приложения

    part def Monitor {
        doc /* Асинхронно выполняет ICMP-, TCP- и HTTP-проверки. */
        attribute intervalSeconds : Natural = 5;
        attribute status : HostStatus;
    }

    part def PortScanner {
        doc /* Сканирует TCP-порты узла только по запросу пользователя. */
        attribute portTimeoutSeconds : Natural = 1;
    }

    part def UserInterface {
        doc /* Интерфейс в стиле liquid glass, светлая и тёмная темы. */
    }

    part def SettingsStorage {
        doc /* Хранение списка узлов и настроек между запусками. */
    }

    part def LiquidPingApp {
        doc /* Настольное приложение Liquid Ping для Windows 11. */
        part monitor : Monitor;
        part scanner : PortScanner;
        part ui : UserInterface;
        part storage : SettingsStorage;

        connect ui to monitor;
        connect ui to scanner;
        connect monitor to storage;
    }

    part liquidPing : LiquidPingApp;

    // Варианты использования

    use case def MonitorHosts {
        doc /* Пользователь запускает мониторинг и видит статус, задержку, потери и IP узлов. */
        subject app : LiquidPingApp;
        actor user : User;
    }

    use case def ScanPorts {
        doc /* Пользователь выбирает узел и запускает сканирование портов (до 2 кликов). */
        subject app : LiquidPingApp;
        actor user : User;
    }

    use case def ManageHosts {
        doc /* Пользователь добавляет, изменяет и удаляет узлы. */
        subject app : LiquidPingApp;
        actor user : User;
    }

    // Требования

    package Requirements {

        requirement def MemoryLimit {
            doc /* Приложение использует не более 1 ГБ оперативной памяти. */
            attribute maxMemoryMB : Natural = 1024;
        }

        requirement def GlassStyle {
            doc /* Панели полупрозрачные, с размытием фона (liquid glass). */
        }

        requirement def AsyncPolling {
            doc /* Проверки всех узлов выполняются асинхронно и не блокируют интерфейс. */
        }

        requirement def TwoClicks {
            doc /* Просмотр портов или нужного узла доступен не более чем за 2 клика. */
            attribute maxClicks : Natural = 2;
        }

        requirement def DefaultHosts {
            doc /* По умолчанию: Yandex, Google, Steam, Minsktrans, Ozon, Wildberries. */
        }

        requirement <'REQ-PERF-001'> memoryLimit : MemoryLimit;
        requirement <'REQ-INT-001'> glassStyle : GlassStyle;
        requirement <'REQ-FUNC-009'> asyncPolling : AsyncPolling;
        requirement <'REQ-FUNC-013'> twoClicks : TwoClicks;
        requirement <'REQ-FUNC-001'> defaultHosts : DefaultHosts;
    }
}
