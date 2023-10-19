using System.Collections;
using System.Collections.Generic;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace ExampleUnturnedPlugin
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Instance;

        public static List<CSteamID> Nocked;
        
        protected override void Load()
        {
            Instance = this;
            // Делегируем метод
            DamageTool.damagePlayerRequested += DamageToolOndamagePlayerRequested;
            // Создаем новый лист для нокнутых игроков
            Nocked = new List<CSteamID>();
        }

        // Получаем запрос на дамаг 
        private void DamageToolOndamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldallow)
        {
            // Получаем класс игрока
            var player = UnturnedPlayer.FromPlayer(parameters.player);
            // Если уже нокнут, то отклоняем запрос
            if (Nocked.Contains(player.CSteamID))
            {
                // Ставим множитель кровотоечения на 0
                parameters.bleedingModifier = 0;
                // Ставим получаемый урон на 0
                parameters.damage = 0;
                // Отменяем урон по игроку
                shouldallow = false;
                return;
            }
            // Если хп игрока меньше чем урон, то добавляем в нокнутых
            if (player.Health < parameters.damage)
            {
                // Добавление игрока в список нокнутых
                Nocked.Add(player.CSteamID);
                // Если у игрока имеется оружие в слотах
                if (player.Player.equipment != null)
                {
                    // Убираем из рук оружие
                    player.Player.equipment.dequip();
                    // Выбрасываем экипированное оружие из инвентаря
                    player.Player.inventory.sendDropItem(player.Player.equipment.equippedPage,
                        player.Player.equipment.equipped_x, player.Player.equipment.equipped_y);
                }
                // Таймер до смерти игрока
                StartCoroutine(Kill(player, parameters));
                // Меняем положение игрока, его возможность передвижижения
                StartCoroutine(check(player));

                // Ставим множитель кровотоечения на 0
                parameters.bleedingModifier = 0;
                // Ставим получаемый урон на 0
                parameters.damage = 0;
                // Отменяем урон по игроку
                shouldallow = false;
            }
        }


        private IEnumerator check(UnturnedPlayer player)
        {
            // Когда игрок находится в списке нокнутых
            while (Nocked.Contains(player.CSteamID))
            {
                // Убираем возможность прыгать
                player.Player.movement.sendPluginJumpMultiplier(0f);
                // Убираем возможность бегать
                player.Player.movement.sendPluginSpeedMultiplier(0f);
                // Ставим положение игрока лёжа
                player.Player.stance.stance = EPlayerStance.PRONE;
                // Проверяем положение игрока
                player.Player.stance.checkStance(EPlayerStance.PRONE);
                yield return new WaitForSeconds(0.2f);
            }
        }

        private IEnumerator Kill(UnturnedPlayer player, DamagePlayerParameters parameters)
        {
            // Время ожидания до того как игрок умрет
            yield return new WaitForSeconds(Configuration.Instance.NockedTime);

            // Если по окончанию таймера игрок не находится в списке нокнутых, то его смерть отменяется
            if (!Nocked.Contains(player.CSteamID)) yield break;
            // Убираем игрока из списка нокнутых
            Nocked.Remove(player.CSteamID);
            // Наносим игроку урон с параметрами
            DamageTool.damagePlayer(parameters, out var kill);
            // Возвращаем возможность прыжков
            player.Player.movement.sendPluginJumpMultiplier(1f);
            // Возвращаем возможность бега
            player.Player.movement.sendPluginSpeedMultiplier(1f);
        }

        
        protected override void Unload()
        {
            DamageTool.damagePlayerRequested -= DamageToolOndamagePlayerRequested;
            Nocked = null;
            Instance = null;
        }
    }
}