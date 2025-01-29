﻿using System;
using System.Collections.Generic;

namespace ATMConsoleApp
{
    public class Program
    {
        private static Dictionary<string, User> Users = new Dictionary<string, User>
        {
            { "12345678", new User("12345678", "1234", 1000) },
            { "87654321", new User("87654321", "4321", 2000) }
        };
        public delegate void OperationHandler(string message);
        public static event OperationHandler OnAuthentication;
        public static event OperationHandler OnBalanceCheck;
        public static event OperationHandler OnWithdrawal;
        public static event OperationHandler OnFundsTransfer;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Ласкаво просимо до банкомату!");
            if (!AuthenticateUser(out string cardNumber))
            {
                return;
            }

            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();
                if (!HandleUserChoice(choice, cardNumber))
                {
                    break;
                }
            }
        }

        private static bool AuthenticateUser(out string cardNumber)
        {
            Console.Write("Введіть номер картки: ");
            cardNumber = Console.ReadLine();

            if (!IsCardNumberValid(cardNumber))
            {
                return false;
            }

            Console.Write("Введіть PIN-код: ");
            string pin = Console.ReadLine();

            if (!Authenticate(cardNumber, pin))
            {
                Console.WriteLine("Невірний PIN-код.");
                return false;
            }

            return true;
        }

        private static void ShowMenu()
        {
            Console.WriteLine("\nОберіть операцію:");
            Console.WriteLine("1. Переглянути баланс");
            Console.WriteLine("2. Зняти кошти");
            Console.WriteLine("3. Зарахувати кошти");
            Console.WriteLine("4. Перерахувати кошти");
            Console.WriteLine("5. Вихід");
        }

        private static bool HandleUserChoice(string choice, string cardNumber)
        {
            switch (choice)
            {
                case "1":
                    CheckBalance(cardNumber);
                    break;
                case "2":
                    Withdraw(cardNumber);
                    break;
                case "3":
                    Deposit(cardNumber);
                    break;
                case "4":
                    TransferFunds(cardNumber);
                    break;
                case "5":
                    Console.WriteLine("Дякуємо за використання банкомату!");
                    return false;
                default:
                    Console.WriteLine("Невірний вибір, спробуйте ще раз.");
                    break;
            }
            return true;
        }

        private static bool IsCardNumberValid(string cardNumber)
        {
            if (!Users.ContainsKey(cardNumber))
            {
                Console.WriteLine("Картка не знайдена.");
                return false;
            }
            return true;
        }

        private static bool Authenticate(string cardNumber, string pin)
        {
            bool isAuthenticated = Users[cardNumber].Pin == pin;
            OnAuthentication?.Invoke($"Аутентифікація {(isAuthenticated ? "успішна" : "неуспішна")}.");
            return isAuthenticated;
        }

        private static void CheckBalance(string cardNumber)
        {
            Console.WriteLine($"Ваш баланс: {Users[cardNumber].Balance} грн.");
            OnBalanceCheck?.Invoke("Перегляд балансу виконано.");
        }

        private static void Withdraw(string cardNumber)
        {
            Console.Write("Введіть суму для зняття: ");
            if (!TryGetValidAmount(Console.ReadLine(), out decimal amount)) return;

            if (Users[cardNumber].Withdraw(amount))
            {
                Console.WriteLine($"Ви зняли {amount} грн. Ваш залишок: {Users[cardNumber].Balance} грн.");
                OnWithdrawal?.Invoke("Зняття коштів успішне.");
            }
            else
            {
                Console.WriteLine("Недостатньо коштів.");
            }
        }

        private static void Deposit(string cardNumber)
        {
            Console.Write("Введіть суму для зарахування: ");
            if (!TryGetValidAmount(Console.ReadLine(), out decimal amount)) return;

            Users[cardNumber].Deposit(amount);
            Console.WriteLine($"Ви зарахували {amount} грн. Ваш новий баланс: {Users[cardNumber].Balance} грн.");
        }

        private static void TransferFunds(string fromCardNumber)
        {
            Console.Write("Введіть номер картки для переказу: ");
            string toCardNumber = Console.ReadLine();

            if (!IsCardNumberValid(toCardNumber)) return;

            Console.Write("Введіть суму для переказу: ");
            if (!TryGetValidAmount(Console.ReadLine(), out decimal amount)) return;

            if (Users[fromCardNumber].Transfer(amount, Users[toCardNumber]))
            {
                Console.WriteLine($"Переказ {amount} грн. успішно виконано. Ваш залишок: {Users[fromCardNumber].Balance} грн.");
                OnFundsTransfer?.Invoke("Переказ коштів успішний.");
            }
            else
            {
                Console.WriteLine("Недостатньо коштів для переказу.");
            }
        }


        private static bool TryGetValidAmount(string input, out decimal amount)
        {
            if (decimal.TryParse(input, out amount) && amount > 0)
            {
                return true;
            }
            Console.WriteLine("Невірна сума.");
            return false;
        }
    }

    public class User
    {
        public string CardNumber { get; set; }
        public string Pin { get; set; }
        private decimal _balance;

        public decimal Balance => _balance;

        public User(string cardNumber, string pin, decimal balance)
        {
            CardNumber = cardNumber;
            Pin = pin;
            _balance = balance;
        }

        public bool Withdraw(decimal amount)
        {
            if (_balance >= amount)
            {
                _balance -= amount;
                return true;
            }
            return false;
        }

        public void Deposit(decimal amount)
        {
            _balance += amount;
        }

        public bool Transfer(decimal amount, User toUser)
        {
            if (_balance >= amount)
            {
                _balance -= amount;
                toUser.Deposit(amount);
                return true;
            }
            return false;
        }
    }
}
