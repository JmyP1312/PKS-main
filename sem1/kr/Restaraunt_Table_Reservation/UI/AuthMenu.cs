using RestaurantConsoleApp.Services;using RestaurantConsoleApp.Services;



namespace RestaurantConsoleApp.UInamespace RestaurantConsoleApp.UI

{{

    public static class AuthMenu    public static class AuthMenu

    {    {

        public static void Show()        public static void Show()

        {        {

            Console.WriteLine("\n1. Войти");            while (!AuthService.IsLoggedIn)

            Console.WriteLine("2. Регистрация");            {

            Console.WriteLine("0. Выход");                Console.WriteLine("\n┌─────────────────────────────────────┐");

            Console.Write("\nВыбор: ");                Console.WriteLine("│         МЕНЮ АВТОРИЗАЦИИ           │");

                            Console.WriteLine("├─────────────────────────────────────┤");

            var choice = Console.ReadLine();                Console.WriteLine("│ 1. Войти                           │");

                            Console.WriteLine("│ 2. Зарегистрироваться              │");

            switch (choice)                Console.WriteLine("│ 0. Выход                           │");

            {                Console.WriteLine("└─────────────────────────────────────┘");

                case "1":                Console.Write("\nВыберите действие: ");

                    Login();

                    break;                var choice = Console.ReadLine();

                case "2":

                    Console.WriteLine("Функция регистрации в разработке");                switch (choice)

                    Console.ReadKey();                {

                    Console.Clear();                    case "1":

                    break;                        Login();

                case "0":                        break;

                    Environment.Exit(0);                    case "2":

                    break;                        Register();

            }                        break;

        }                    case "0":

                                Console.WriteLine("\nДо свидания!");

        private static void Login()                        Environment.Exit(0);

        {                        break;

            Console.Clear();                    default:

            Console.WriteLine("=== ВХОД ===\n");                        Console.WriteLine("\n❌ Неверный выбор!");

            Console.Write("Логин: ");                        break;

            var username = Console.ReadLine();                }

            Console.Write("Пароль: ");            }

            var password = Console.ReadLine();        }

            

            if (AuthService.Login(username, password))        private static void Login()

            {        {

                Console.WriteLine("\nВход выполнен успешно!");            Console.Clear();

                Console.ReadKey();            Console.WriteLine("╔════════════════════════════════════════════════╗");

                Console.Clear();            Console.WriteLine("║                    ВХОД                        ║");

            }            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            else

            {            Console.Write("Логин: ");

                Console.WriteLine("\nОшибка входа!");            var username = Console.ReadLine()?.Trim();

                Console.ReadKey();

                Console.Clear();            Console.Write("Пароль: ");

            }            var password = ReadPassword();

        }

    }            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))

}            {

                Console.WriteLine("\n❌ Логин и пароль не могут быть пустыми!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            if (AuthService.Login(username, password))
            {
                Console.WriteLine($"\n✅ Добро пожаловать, {AuthService.CurrentUser?.FullName ?? username}!");
                Console.WriteLine($"Роль: {GetRoleName(AuthService.CurrentUser?.Role)}");
                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                Console.WriteLine("\n❌ Неверный логин или пароль!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static void Register()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║                РЕГИСТРАЦИЯ                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");

            Console.Write("Логин (мин. 3 символа): ");
            var username = Console.ReadLine()?.Trim();

            Console.Write("Email: ");
            var email = Console.ReadLine()?.Trim();

            Console.Write("Полное имя (необязательно): ");
            var fullName = Console.ReadLine()?.Trim();

            Console.Write("Пароль (мин. 6 символов): ");
            var password = ReadPassword();

            Console.Write("\nПодтвердите пароль: ");
            var confirmPassword = ReadPassword();

            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                Console.WriteLine("\n❌ Логин должен содержать минимум 3 символа!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                Console.WriteLine("\n❌ Неверный формат email!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                Console.WriteLine("\n❌ Пароль должен содержать минимум 6 символов!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            if (password != confirmPassword)
            {
                Console.WriteLine("\n❌ Пароли не совпадают!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            if (AuthService.Register(username, password, email, string.IsNullOrWhiteSpace(fullName) ? null : fullName))
            {
                Console.WriteLine("\n✅ Регистрация успешна! Теперь вы можете войти.");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                Console.WriteLine("\n❌ Пользователь с таким логином или email уже существует!");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private static string GetRoleName(Models.UserRole? role)
        {
            return role switch
            {
                Models.UserRole.Admin => "Администратор",
                Models.UserRole.Manager => "Менеджер",
                Models.UserRole.Client => "Клиент",
                _ => "Неизвестно"
            };
        }
    }
}
