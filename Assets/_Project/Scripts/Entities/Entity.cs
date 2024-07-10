using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.Entities {
    public interface IEntity {
        public abstract Planet GetPlanet();
        public abstract GameObject GetGameObject();
    }
}