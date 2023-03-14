using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network.MessageUtils {
    public static class PlayerDataMessage {
        /**<summary>
         * <param name="player">Jugador del que extraer los datos</param>
         * <returns>Datos en forma de NetworkMessage</returns>
         * <p>Extrae los datos del objeto <see cref="Player"/> y los convierte
         * en un NetworkMessage</p>
         * </summary>
         */
        public static PlayerDataMessageStruct getPlayerData(Player player){
            List<EquipmentMessageStruct> equipments = new List<EquipmentMessageStruct>();
            foreach (EquipmentDisplayer equipmentDisplayer in player.EquipmentHandler.EquipmentDisplayers) {
                equipments.Add(new EquipmentMessageStruct(equipmentDisplayer.CurrentEquipedItem, 
                    (int) equipmentDisplayer.GetBodyPart(), equipmentDisplayer.IsActive));
            }
            return new PlayerDataMessageStruct(equipments, NetworkManager.Singleton.Tick, player.Id);
        }
    }
}