using RestaurantConsoleApp.Services;using RestaurantConsoleApp.Services;using RestaurantConsoleApp.Services;



namespace RestaurantConsoleApp.UI

{

    public static class MainMenunamespace RestaurantConsoleApp.UInamespace RestaurantConsoleApp.UI

    {

        public static void Show(){{

        {

            Console.WriteLine("\nГлавное меню");    public static class MainMenu    public static class MainMenu

            Console.WriteLine("0. Выйти");

            Console.Write("\nВыбор: ");    {    {

            

            var choice = Console.ReadLine();        public static void Show()        public static void Show()

            

            if (choice == "0")        {        {

            {

                AuthService.Logout();            Console.WriteLine("\nГлавное меню");            Console.WriteLine("

                Console.Clear();

            }            Console.WriteLine("1. Мои бронирования");=== Главное меню ===");

        }

    }            Console.WriteLine("2. Создать бронирование");            Console.WriteLine("1. Мои бронирования");

}

            Console.WriteLine("3. Просмотр столиков");            Console.WriteLine("2. Создать бронирование");

            Console.WriteLine("0. Выйти");            Console.WriteLine("3. Просмотр столиков");

            Console.Write("\nВыбор: ");            Console.WriteLine("0. Выйти");

                        Console.Write("

            var choice = Console.ReadLine();Выбор: ");

                        

            switch (choice)            var choice = Console.ReadLine();

            {            

                case "0":            switch (choice)

                    AuthService.Logout();            {

                    Console.Clear();                case "0":

                    break;                    AuthService.Logout();

                default:                    Console.Clear();

                    Console.WriteLine("\nФункция в разработке...");                    break;

                    Console.ReadKey();                default:

                    Console.Clear();                    Console.WriteLine("

                    break;Функция в разработке...");

            }                    Console.ReadKey();

        }                    Console.Clear();

    }                    break;

}            }

        }
    }
}
