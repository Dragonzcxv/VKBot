using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using VkNet.Model;

namespace VKBot.Models
{
    public static class WeatherAPI
    {
        private static string descriptionWeather;
        private static int tempWeather;
        private static int tempFeelsWeather;
        private static string cityName;
        public static string iconResponse;
        



        /// <summary>
        /// Обновление информации о погоде
        /// </summary>
        /// <param name="lat">Широта</param>
        /// <param name="lon">Долгота</param>
        /// <param name="_cityName">Имя города</param>
        /// <param name="uploadServer">Ссылка для загрузки картинки</param>
        public static void UpdateWeather(double lat, double lon, string _cityName, UploadServerInfo uploadServer)
        {
            string adress = $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&lang=ru&appid=0e8751055ef3bd151909c6423d520232";
            using (WebClient client = new WebClient())
            {
                var JData = client.DownloadString(adress);
                var data = JObject.Parse(JData);
                descriptionWeather = (string)data["weather"][0]["description"];
                tempWeather = (int)data["main"]["temp"];
                tempFeelsWeather = (int)data["main"]["feels_like"];
                cityName = _cityName;
                string iconName = (string)data["weather"][0]["icon"];
                client.DownloadFile($"http://openweathermap.org/img/wn/{iconName}@2x.png", "icon.png");
                iconResponse = Encoding.UTF8.GetString(client.UploadFile(uploadServer.UploadUrl, "icon.png"));
            }
        }





        public static string GetTextWeather()
        {
            return $"В твоём мухосранске, под названием {cityName} сейчас {tempWeather} по цельсию!\n" +
                   $"Ощущается это как {tempFeelsWeather} по цельсию.\nВ целом у тебя сейчас {descriptionWeather}!\n" +
                   $" Надеюсь это информация будет тебе полезна, а не то полезай на ножик";
        }
    }
}
