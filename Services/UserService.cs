using Storage;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Services;

    public class UserService : IUserService
    {
        
        private readonly ApplicationContext _context;
        private readonly string BotToken; // Замените на ваш токен бота

        public UserService(ApplicationContext context, IConfiguration requiredService)
        {
            _context = context;
            BotToken = requiredService["Telegram:BotToken"];
        }

        public async Task<User> AuthenticationAsync(int id, string firstName, string lastName, string username, string hash, long authDate)
        {
            // Проверяем подлинность данных Telegram
            CheckTelegramAuthorizationAsync(id, firstName, lastName, username, authDate, hash);

            // Ищем пользователя по ID
            var _user = await GetUserById(id);
            if (_user != null)
            {
                if (username != _user.Username)
                {
                    _user.Username = username;
                    _context.Users.Update(_user);
                }

                if (firstName != _user.FirstName)
                {
                    _user.FirstName = firstName;
                    _context.Users.Update(_user);
                }

                if (lastName != _user.LastName)
                {
                    _user.LastName = lastName;
                    _context.Users.Update(_user);
                }
                return _user; // Пользователь существует, возвращаем его
            }
            else
            {
                // Создаём нового пользователя
                var newUser = new User
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username,
                    Hash = hash
                };
                await RegisterNewUser(newUser);
                return newUser;
            }
        }

        public async Task<User> GetUserById(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        private async Task RegisterNewUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        private async Task CheckTelegramAuthorizationAsync(int id, string firstName, string lastName, string username, long authDate, string hash)
        {
            // Формируем массив строк в формате "key=value"
            var dataCheckArr = new List<string>();
            dataCheckArr.Add($"id={id}");
            if (!string.IsNullOrEmpty(firstName)) dataCheckArr.Add($"first_name={firstName}");
            if (!string.IsNullOrEmpty(lastName)) dataCheckArr.Add($"last_name={lastName}");
            if (!string.IsNullOrEmpty(username)) dataCheckArr.Add($"username={username}");
            dataCheckArr.Add($"auth_date={authDate}");

            // Сортируем массив
            dataCheckArr.Sort();

            // Объединяем строки с разделителем \n
            string dataCheckString = string.Join("\n", dataCheckArr);

            // Генерируем секретный ключ из токена бота (SHA256)
            byte[] secretKey;
            using (var sha256 = SHA256.Create())
            {
                secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(BotToken));
            }

            // Вычисляем HMAC-SHA256
            byte[] hashBytes;
            using (var hmac = new HMACSHA256(secretKey))
            {
                hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
            }
            string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Сравниваем вычисленный hash с переданным
            if (computedHash != hash)
            {
                throw new Exception("Data is NOT from Telegram");
            }

            // Проверяем, что данные не устарели (24 часа = 86400 секунд)
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if ((currentTime - authDate) > 86400)
            {
                throw new Exception("Data is outdated");
            }
        }
    }

    
