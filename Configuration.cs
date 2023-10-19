using Rocket.API;
using System;
namespace ExampleUnturnedPlugin
{
    // класс конфига вашего плагина, значения которые тут есть можно заменить на свои
    public class Configuration : IRocketPluginConfiguration
    {
        // определение переменной
        public float NockedTime;


        // метод который вызывается при создании файла конфига (при первом запуске), удалять нельзя
        public void LoadDefaults()
        {
            NockedTime = 60;
        }
    }
}